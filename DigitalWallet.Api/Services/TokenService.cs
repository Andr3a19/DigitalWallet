using DigitalWallet.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalWallet.Api.Services
{
    // Service responsible for creating, signing, and configuring JSON Web Tokens
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration;

        // Generates a cryptographically signed JWT containing user identity claims and a validity window
        public string GenerateToken(User user)
        {
            // 1. Retrieve the secret signing key and construct HMAC-SHA256 cryptographic credentials
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("La chiave segreta JWT non è configurata.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. Embed user claims (UserId, Username, Role for RBAC, and a unique token identifier)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 3. Configure token descriptor with issuer, audience, and UTC expiration
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            // 4. Serialize the security token to its compact string representation
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}