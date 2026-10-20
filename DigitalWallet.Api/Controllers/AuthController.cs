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
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Tutti i campi sono obbligatori");
        }

        if (request.Password.Length < 6)
            return BadRequest("La password deve contenere almeno 6 caratteri");

        var usernameNormalized = request.Username.Trim().ToLowerInvariant();
        var userExists = await context.Users.AsNoTracking().AnyAsync(u => u.Username == usernameNormalized);
        if (userExists)
            return Conflict("Lo username inserito è già in uso");

        var passwordHash = passwordService.HashPassword(request.Password);

        // Restricts public self-registration strictly to Customer role to prevent privilege escalation attacks
        var user = new User(usernameNormalized, request.FullName, passwordHash, Role.Customer);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Utente registrato con successo",
            userId = user.Id,
            username = user.Username
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Credenziali non valide");

        var usernameNormalized = request.Username.Trim().ToLowerInvariant();
        var user = await context.Users.SingleOrDefaultAsync(u => u.Username == usernameNormalized);

        if (user is null || !passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Credenziali non valide");

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Username, user.FullName, user.Role.ToString()));
    }
}