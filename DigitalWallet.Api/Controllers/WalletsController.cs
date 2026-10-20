using DigitalWallet.Api.Data;
using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController(WalletDbContext context) : ControllerBase
{
    private readonly WalletDbContext _context = context;

    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OwnerName))
            return BadRequest("Il nome del titolare non può essere vuoto");

        if (request.UserId <= 0)
            return BadRequest("L'ID utente non è valido");

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            return NotFound($"Utente con ID {request.UserId} non trovato");

        var temporaryIban = $"IT00DWT{Random.Shared.NextInt64(100000000000, 999999999999)}";
        var wallet = new Wallet(request.UserId, request.OwnerName, temporaryIban, Currency.EUR);
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWallets()
    {
        var wallets = await _context.Wallets.AsNoTracking().ToListAsync();
        return Ok(wallets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWallet(int id)
    {
        var wallet = await _context.Wallets.AsNoTracking().SingleOrDefaultAsync(w => w.Id == id);

        if (wallet == null)
            return NotFound();

        return Ok(wallet);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(int id, [FromBody] DepositRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("L'importo del deposito deve essere maggiore di zero");

        var wallet = await _context.Wallets.FindAsync(id);

        if (wallet == null)
            return NotFound();

        wallet.Credit(request.Amount);

        string description = string.IsNullOrWhiteSpace(request.Description) ? "Deposito fondi" : request.Description;
        _context.Transactions.Add(new Transaction(id, request.Amount, TransactionType.Deposito, description));

        await _context.SaveChangesAsync();
        return Ok(wallet);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("L'importo del bonifico deve essere maggiore di zero");

        if (request.SourceWalletId == request.DestinationWalletId)
            return BadRequest("Non puoi effettuare un bonifico verso lo stesso conto");

        var sourceWallet = await _context.Wallets.FindAsync(request.SourceWalletId);
        var destinationWallet = await _context.Wallets.FindAsync(request.DestinationWalletId);

        if (sourceWallet == null)
            return NotFound($"Conto mittente con ID {request.SourceWalletId} non trovato");

        if (destinationWallet == null)
            return NotFound($"Conto destinatario con ID {request.DestinationWalletId} non trovato");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            sourceWallet.Debit(request.Amount);
            destinationWallet.Credit(request.Amount);

            string description = string.IsNullOrWhiteSpace(request.Description) ? "Bonifico P2P" : request.Description;

            _context.Transactions.Add(new Transaction(
                sourceWallet.Id,
                request.Amount,
                TransactionType.BonificoInUscita,
                $"Inviato a {destinationWallet.OwnerName} (ID {destinationWallet.Id}): {description}"));

            _context.Transactions.Add(new Transaction(
                destinationWallet.Id,
                request.Amount,
                TransactionType.BonificoInEntrata,
                $"Ricevuto da {sourceWallet.OwnerName} (ID {sourceWallet.Id}): {description}"));

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new { message = "Bonifico eseguito con successo", sourceBalance = sourceWallet.Balance });
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        var wallet = await _context.Wallets.FindAsync(id);

        if (wallet == null)
            return NotFound($"Conto con ID {id} non trovato");

        var transactions = await _context.Transactions.AsNoTracking().Where(t => t.WalletId == id).OrderByDescending(t => t.TimeStamp).ToListAsync();
        return Ok(transactions);
    }
}