using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalWallet.Api.Data;
using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;

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
            if(string.IsNullOrWhiteSpace(request.OwnerName))
            {
                return BadRequest("Il nome del titolare non può essere vuoto");
            }

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
            {
                return NotFound();
            }
            return Ok(wallet);
        }

        // Asynchronously retrieves all wallets
        [HttpGet]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets = await _context.Wallets.ToListAsync();
            return Ok(wallets);
        }
    }
}