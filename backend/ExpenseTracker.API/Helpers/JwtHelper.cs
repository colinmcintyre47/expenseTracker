using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.API.Helpers;

/// <summary>
/// Generates and validates JSON Web Tokens (JWTs) for API authentication.
///
/// How JWT auth works in this app:
/// 1. User logs in → server creates a signed JWT containing userId and email
/// 2. Client stores token in localStorage
/// 3. Client sends token in every request: "Authorization: Bearer {token}"
/// 4. ASP.NET Core middleware validates the signature and expiry automatically
/// 5. Controllers read userId from HttpContext.User.Claims
/// </summary>
public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Creates a signed JWT token for the given user.
    /// The token contains claims (userId, email) that identify the user on subsequent requests.
    /// </summary>
    public string GenerateToken(User user)
    {
        // Read JWT configuration from appsettings.json
        var secret = _config["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var issuer = _config["JwtSettings:Issuer"] ?? "ExpenseTrackerAPI";
        var audience = _config["JwtSettings:Audience"] ?? "ExpenseTrackerClient";
        var expiryMinutes = int.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "1440");

        // Claims are the "payload" of the JWT — data readable by the server after validation
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
        };

        // Sign the token using HMAC-SHA256 with the secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
