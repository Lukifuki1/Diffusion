using System.Text.Json;
using System.Text.Json.Nodes;
using AuraFlow.Core.Api;
using AuraFlow.Core.Models.Api.OpenArt;
using AuraFlow.Core.Models.Progress;
using AuraFlow.Core.Services;

namespace AuraFlow.Core.Models.PackageModification;

public class DownloadOpenArtWorkflowStep(
    IOpenArtApi openArtApi,
    OpenArtSearchResult workflow,
    ISettingsManager settingsManager
) : IPackageStep
{
    public async Task ExecuteAsync(IProgress<ProgressReport>? progress = null)
    {
        var workflowData = await openArtApi
            .DownloadWorkflowAsync(new OpenArtDownloadRequest { WorkflowId = workflow.Id })
            .ConfigureAwait(false);

        var workflowJson = JsonSerializer.SerializeToNode(workflow);

        Directory.CreateDirectory(settingsManager.WorkflowDirectory);
        var filePath = Path.Combine(settingsManager.WorkflowDirectory, $"{workflowData.Filename}.json");

        var jsonObject = JsonNode.Parse(workflowData.Payload) as JsonObject;
        jsonObject?.Add("sm_workflow_data", workflowJson);

        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(jsonObject)).ConfigureAwait(false);

        progress?.Report(new ProgressReport(1f, "Downloaded OpenArt Workflow"));
    }

    public string ProgressTitle => "Downloading OpenArt Workflow";
}
