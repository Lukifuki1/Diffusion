namespace AuraFlow.Core.Models.Api.AuraCloud;

public class RecommendedModelsV2Response
{
    public Dictionary<string, List<CivitModel>> RecommendedModelsByCategory { get; set; } = new();
}
