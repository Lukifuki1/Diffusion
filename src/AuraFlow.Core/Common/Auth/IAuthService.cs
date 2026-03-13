namespace AuraFlow.Core.Common.Auth;

/// <summary>
/// Authentication service interface for JWT and OAuth
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Generate JWT token for authenticated user
    /// </summary>
    Task<string> GenerateTokenAsync(AuthUser user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate JWT token and return user info
    /// </summary>
    Task<AuthResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refresh expired JWT token
    /// </summary>
    Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Authenticated user information
/// </summary>
public class AuthUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    public Dictionary<string, object?> Claims { get; set; } = new();
}

/// <summary>
/// Authentication result
/// </summary>
public class AuthResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthUser? User { get; set; }
}

/// <summary>
/// Token refresh result
/// </summary>
public class TokenRefreshResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
