using DigitalWallet.Api.Data;
using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController(WalletDbContext context) : ControllerBase
    {
        private readonly WalletDbContext _context = context;

        // Validates input, persists the new wallet, and returns 201 Created with a Location header
        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OwnerName))
                return BadRequest("Il nome del titolare non può essere vuoto");

            var wallet = new Wallet(request.OwnerName);
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
        }

        // Asynchronously retrieves a wallet by its primary key, returning 404 NotFound if missing
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWallet(int id)
        {
            var wallet = await _context.Wallets.FindAsync(id);

            if (wallet == null)
                return NotFound();

            return Ok(wallet);
        }

        // Asynchronously retrieves all wallets
        [HttpGet]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets = await _context.Wallets.ToListAsync();
            return Ok(wallets);
        }

        // Deposits funds into a wallet, updates balance, and records a deposit transaction
        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> Deposit(int id, [FromBody] DepositRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("L'importo del deposito deve essere maggiore di zero");

            var wallet = await _context.Wallets.FindAsync(id);

            if (wallet == null)
                return NotFound();

            wallet.Balance += request.Amount;

            string description = string.IsNullOrWhiteSpace(request.Description) ? "Deposito fondi" : request.Description;
            _context.Transactions.Add(new Transaction(id, request.Amount, TransactionType.Deposito, description));

            await _context.SaveChangesAsync();
            return Ok(wallet);
        }

        // Executes an atomic peer-to-peer transfer between two wallets within an ACID database transaction
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

            if (sourceWallet.Balance < request.Amount)
                return BadRequest("Saldo insufficiente per completare il bonifico");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                sourceWallet.Balance -= request.Amount;
                destinationWallet.Balance += request.Amount;

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
            } catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Errore interno durante il trasferimento, Operazione annullata");
            }
        }
    }
}