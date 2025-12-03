using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace service_csharp.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string? role = null);
}

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService()
    {
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? throw new InvalidOperationException("JWT_SECRET no está configurado");
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "Paradox";
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ParadoxAPI";
        _expirationMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "60");
    }

    public string GenerateToken(Guid userId, string email, string? role = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
