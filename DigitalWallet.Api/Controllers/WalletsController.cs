using DigitalWallet.Api.Data;
using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;
using DigitalWallet.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigitalWallet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WalletsController(WalletDbContext context, ICurrencyExchangeService exchangeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerName))
            return BadRequest("Il nome del titolare non può essere vuoto");

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized();

        // Generates a synthetic IBAN not compliant with ISO 13616, acceptable for demo purposes only
        string iban;
        do
        {
            iban = GenerateIban();
        } while (await context.Wallets.AsNoTracking().AnyAsync(w => w.Iban == iban));

        var wallet = new Wallet(currentUserId.Value, request.OwnerName, iban, request.Currency);

        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        var response = ToResponseDto(wallet);
        return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWallets()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized();

        var isUserAdmin = User.IsInRole(nameof(Role.Admin));

        var query = isUserAdmin
            ? context.Wallets.AsNoTracking()
            : context.Wallets.AsNoTracking().Where(w => w.UserId == currentUserId.Value);

        var wallets = await query
            .Select(w => new WalletResponse(w.Id, w.UserId, w.OwnerName, w.Iban, w.Currency, w.Balance, w.CreatedAt))
            .ToListAsync();

        return Ok(wallets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWallet(int id)
    {
        var wallet = await context.Wallets.AsNoTracking().SingleOrDefaultAsync(w => w.Id == id);
        if (wallet is null)
            return NotFound("Conto non trovato");

        if (!CanReadWallet(wallet.UserId))
            return Forbid();

        return Ok(ToResponseDto(wallet));
    }

    [HttpPost("{id:int}/deposit")]
    public async Task<IActionResult> Deposit(int id, [FromBody] DepositRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("L'importo del deposito deve essere maggiore di zero");

        var wallet = await context.Wallets.FindAsync(id);
        if (wallet is null)
            return NotFound("Conto non trovato");

        if (!IsAuthorizedForWallet(wallet.UserId))
            return Forbid();

        wallet.Credit(request.Amount);

        var description = string.IsNullOrWhiteSpace(request.Description) ? "Deposito fondi" : request.Description.Trim();
        context.Transactions.Add(new Transaction(id, request.Amount, TransactionType.Deposito, description));

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Il conto è stato modificato contemporaneamente da un'altra operazione, riprova");
        }
        catch (Exception)
        {
            return StatusCode(500, "Errore interno durante il deposito, operazione annullata");
        }

        return Ok(ToResponseDto(wallet));
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("L'importo del bonifico deve essere maggiore di zero");

        if (request.SourceWalletId == request.DestinationWalletId)
            return BadRequest("Non puoi effettuare un bonifico verso lo stesso conto");

        // Enforces deterministic lock ordering by primary key to prevent database deadlocks during concurrent transfers
        var firstId = Math.Min(request.SourceWalletId, request.DestinationWalletId);
        var secondId = Math.Max(request.SourceWalletId, request.DestinationWalletId);

        var firstWallet = await context.Wallets.FindAsync(firstId);
        var secondWallet = await context.Wallets.FindAsync(secondId);

        var sourceWallet = request.SourceWalletId == firstId ? firstWallet : secondWallet;
        var destinationWallet = request.DestinationWalletId == firstId ? firstWallet : secondWallet;

        if (sourceWallet is null)
            return NotFound($"Conto mittente con ID {request.SourceWalletId} non trovato");

        if (destinationWallet is null)
            return NotFound($"Conto destinatario con ID {request.DestinationWalletId} non trovato");

        // Verifies wallet ownership via caller identity claims before performing balance mutations
        if (!IsAuthorizedForWallet(sourceWallet.UserId))
            return Forbid();

        ExchangeResult exchange;
        try
        {
            exchange = exchangeService.Convert(request.Amount, sourceWallet.Currency, destinationWallet.Currency);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(ex.Message);
        }

        // Cross-currency conversion can round down to zero for very small amounts
        if (exchange.ConvertedAmount <= 0)
            return BadRequest("L'importo convertito è troppo piccolo per essere accreditato");

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            sourceWallet.Debit(request.Amount);
            destinationWallet.Credit(exchange.ConvertedAmount);

            var description = string.IsNullOrWhiteSpace(request.Description) ? "Bonifico P2P" : request.Description.Trim();

            context.Transactions.Add(new Transaction(
                sourceWallet.Id,
                request.Amount,
                TransactionType.BonificoInUscita,
                $"Inviato a {destinationWallet.OwnerName} (ID {destinationWallet.Id}): {description}"));

            context.Transactions.Add(new Transaction(
                destinationWallet.Id,
                exchange.ConvertedAmount,
                TransactionType.BonificoInEntrata,
                $"Ricevuto da {sourceWallet.OwnerName} (ID {sourceWallet.Id}): {description}"));

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new TransferResponse(
                "Bonifico eseguito con successo",
                sourceWallet.Balance,
                request.Amount,
                sourceWallet.Currency,
                exchange.ConvertedAmount,
                destinationWallet.Currency,
                exchange.ExchangeRate));
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Conflict("Conflitto di concorrenza rilevato durante il trasferimento, riprova");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Errore interno durante il trasferimento, operazione annullata");
        }
    }

    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        var wallet = await context.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
        if (wallet is null)
            return NotFound($"Conto con ID {id} non trovato");

        if (!CanReadWallet(wallet.UserId))
            return Forbid();

        var transactions = await context.Transactions
            .AsNoTracking()
            .Where(t => t.WalletId == id)
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new
            {
                t.Id,
                t.WalletId,
                t.Amount,
                t.Type,
                t.Timestamp,
                t.Description
            })
            .ToListAsync();

        return Ok(transactions);
    }

    private int? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }

    private bool IsAuthorizedForWallet(int walletOwnerUserId)
    {
        var currentUserId = GetCurrentUserId();
        return currentUserId == walletOwnerUserId;
    }

    private bool CanReadWallet(int walletOwnerUserId)
    {
        var currentUserId = GetCurrentUserId();
        return currentUserId == walletOwnerUserId || User.IsInRole(nameof(Role.Admin));
    }

    private static WalletResponse ToResponseDto(Wallet wallet) =>
        new(wallet.Id, wallet.UserId, wallet.OwnerName, wallet.Iban, wallet.Currency, wallet.Balance, wallet.CreatedAt);

    private static string GenerateIban()
    {
        var randomNumeric = Random.Shared.NextInt64(100000000000, 999999999999);
        return $"IT60X0542811101{randomNumeric}";
    }
}