using Refit;
using AuraFlow.Core.Models.Api.Pypi;

namespace AuraFlow.Core.Api;

[Headers("User-Agent: StabilityMatrix/2.x")]
public interface IPyPiApi
{
    [Get("/pypi/{packageName}/json")]
    Task<PyPiResponse> GetPackageInfo(string packageName);
}
