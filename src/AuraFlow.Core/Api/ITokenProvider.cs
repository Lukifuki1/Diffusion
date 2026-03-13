namespace AuraFlow.Core.Api;

public interface ITokenProvider
{
    Task<string> GetAccessTokenAsync();
    Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync();
}
