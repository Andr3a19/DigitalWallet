using DigitalWallet.Api.Data;
using DigitalWallet.Api.DTOs;
using DigitalWallet.Api.Models;
using DigitalWallet.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(WalletDbContext context, IPasswordService passwordService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Tutti i campi sono obbligatori");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Tutti i campi sono obbligatori");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Tutti i campi sono obbligatori");

        if (request.Password.Length < 6)
            return BadRequest("La password deve contenere almeno 6 caratteri");

        var usernameNormalized = request.Username.Trim().ToLowerInvariant();
        var userExists = await context.Users.AsNoTracking().AnyAsync(u => u.Username == usernameNormalized);
        if (userExists)
            return Conflict("Lo username inserito è già in uso");

        var passwordHash = passwordService.HashPassword(request.Password);

        User user = new(usernameNormalized, request.FullName, passwordHash, request.Role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Utente registrato con successo",
            userId = user.Id,
            username = user.Username
        });
    }
}