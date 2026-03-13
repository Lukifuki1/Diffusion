using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuraFlow.Core.Common.Auth;

/// <summary>
/// JWT-based authentication service implementation
/// </summary>
public class JwtAuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthService> _logger;
    private readonly SymmetricSecurityKey _key;

    public JwtAuthService(IConfiguration configuration, 
        ILogger<JwtAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var secretKey = _configuration["Auth:JwtSecret"] ?? "default-secret-key-12345";
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public async Task<string> GenerateTokenAsync(AuthUser user, 
        CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, string.Join(",", user.Roles))
        };

        // Add custom claims
        foreach (var claim in user.Claims)
        {
            claims.Add(new Claim(claim.Key, claim.Value?.ToString() ?? ""));
        }

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Auth:Issuer"],
            audience: _configuration["Auth:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Generated JWT token for user: {Username}", user.Username);

        return tokenString;
    }

    public async Task<AuthResult> ValidateTokenAsync(string token, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Auth:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Auth:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var user = new AuthUser
            {
                Id = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""),
                Username = principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? ""
            };

            user.Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

            return new AuthResult 
            { 
                IsValid = true, 
                User = user 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            
            return new AuthResult 
            { 
                IsValid = false, 
                ErrorMessage = "Invalid token" 
            };
        }
    }

    public async Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        // In a real implementation, validate refresh token from database
        // For now, generate new tokens
        
        var accessToken = await GenerateTokenAsync(
            new AuthUser { Username = "user", Email = "user@example.com" }, 
            cancellationToken);

        return new TokenRefreshResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
}
