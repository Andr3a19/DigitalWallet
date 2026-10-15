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

            if (request.UserId <= 0)
                return BadRequest("L'ID utente non è valido");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound($"Utente con ID {request.UserId} non trovato");

            var wallet = new Wallet(request.UserId, request.OwnerName);
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
        }

        // Asynchronously retrieves all wallets
        [HttpGet]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets = await _context.Wallets.ToListAsync();
            return Ok(wallets);
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

            // Begin explicit database transaction for ACID compliance
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Update account balances
                sourceWallet.Balance -= request.Amount;
                destinationWallet.Balance += request.Amount;

                string description = string.IsNullOrWhiteSpace(request.Description) ? "Bonifico P2P" : request.Description;

                // 2. Record debit transaction for the sender
                _context.Transactions.Add(new Transaction(
                    sourceWallet.Id,
                    request.Amount,
                    TransactionType.BonificoInUscita,
                    $"Inviato a {destinationWallet.OwnerName} (ID {destinationWallet.Id}): {description}"));

                // 3. Record credit transaction for the recipient
                _context.Transactions.Add(new Transaction(
                    destinationWallet.Id,
                    request.Amount,
                    TransactionType.BonificoInEntrata,
                    $"Ricevuto da {sourceWallet.OwnerName} (ID {sourceWallet.Id}): {description}"));

                // 4. Persist pending changes to database
                await _context.SaveChangesAsync();

                // 5. Commit transaction atomically
                await transaction.CommitAsync();

                return Ok(new { message = "Bonifico eseguito con successo", sourceBalance = sourceWallet.Balance });
            }
            catch (Exception)
            {
                // Rollback all database modifications if any step fails
                await transaction.RollbackAsync();
                return StatusCode(500, "Errore interno durante il trasferimento. Operazione annullata");
            }
        }

        // Asynchronously retrieves all transactions for a specific wallet ordered by timestamp descending
        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> GetTransactions(int id)
        {
            var wallet = await _context.Wallets.FindAsync(id);

            if (wallet == null)
                return NotFound($"Conto con ID {id} non trovato");

            var transactions = await _context.Transactions.Where(t => t.WalletId == id).OrderByDescending(t => t.TimeStamp).ToListAsync();
            return Ok(transactions);
        }
    }
}