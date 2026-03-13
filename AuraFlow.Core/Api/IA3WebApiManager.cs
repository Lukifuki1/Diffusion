using Refit;

namespace AuraFlow.Core.Api;

public interface IA3WebApiManager
{
    IA3WebApi Client { get; }
    RefitSettings? RefitSettings { get; init; }
    string? BaseUrl { get; set; }
    void ResetClient();
}
