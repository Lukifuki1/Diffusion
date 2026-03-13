using System.Text.Json;
using AuraFlow.Core.Api;
using AuraFlow.Core.Models.Api.Comfy;
using AuraFlow.Core.Models.Api.Comfy.Nodes;

namespace AuraFlow.Infrastructure.Engines.Comfy;

public class ComfyWorkflowGenerator : IComfyWorkflowGenerator
{
    private readonly IComfyApi _comfyApi;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicies.SnakeCaseLower,
        WriteIndented = true
    };

    public ComfyWorkflowGenerator(IComfyApi comfyApi)
    {
        _comfyApi = comfyApi;
    }

    public async Task<string> GenerateWorkflowAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        var workflow = new Dictionary<string, ComfyNode>();

        // Load model based on selected type
        if (request.Type == "Image")
        {
            await BuildImageWorkflowAsync(workflow, request);
        }
        else if (request.Type == "Video")
        {
            await BuildVideoWorkflowAsync(workflow, request);
        }

        var promptRequest = new ComfyPromptRequest
        {
            ClientId = Guid.NewGuid().ToString(),
            Prompt = workflow
        };

        return JsonSerializer.Serialize(promptRequest, _jsonOptions);
    }

    private async Task BuildImageWorkflowAsync(Dictionary<string, ComfyNode> workflow, GenerationRequest request)
    {
        // Get available models
        var modelNames = await _comfyApi.GetModelNamesAsync();
        var modelName = modelNames?.FirstOrDefault() ?? "Flux Dev.safetensors";

        // CLIP Text Encode (Prompt)
        workflow["CLIPTextEncode"] = new ComfyNode
        {
            ClassType = "CLIPTextEncode",
            Inputs = new Dictionary<string, object?>
            {
                ["clip"] = new[] { "1", 0 },
                ["text"] = request.Prompt
            }
        };

        // CLIP Text Encode (Negative)
        workflow["CLIPTextEncode2"] = new ComfyNode
        {
            ClassType = "CLIPTextEncode",
            Inputs = new Dictionary<string, object?>
            {
                ["clip"] = new[] { "1", 0 },
                ["text"] = "low quality, blurry, distorted"
            }
        };

        // Empty Latent Image
        workflow["EmptyLatentImage"] = new ComfyNode
        {
            ClassType = "EmptyLatentImage",
            Inputs = new Dictionary<string, object?>
            {
                ["width"] = request.Width,
                ["height"] = request.Height,
                ["batch_size"] = 1
            }
        };

        // KSampler
        workflow["KSampler"] = new ComfyNode
        {
            ClassType = "KSampler",
            Inputs = new Dictionary<string, object?>
            {
                ["seed"] = request.Seed ?? 42,
                ["steps"] = request.Steps,
                ["cfg"] = request.GuidanceScale,
                ["sampler_name"] = "euler",
                ["scheduler"] = "normal",
                ["denoise"] = 1.0,
                ["model"] = new[] { "CheckpointLoaderSimple", 0 },
                ["positive"] = new[] { "CLIPTextEncode", 0 },
                ["negative"] = new[] { "CLIPTextEncode2", 0 },
                ["latent_image"] = new[] { "EmptyLatentImage", 0 }
            }
        };

        // Checkpoint Loader Simple
        workflow["CheckpointLoaderSimple"] = new ComfyNode
        {
            ClassType = "CheckpointLoaderSimple",
            Inputs = new Dictionary<string, object?>
            {
                ["ckpt_name"] = modelName
            }
        };

        // VAE Decode
        workflow["VAEDecode"] = new ComfyNode
        {
            ClassType = "VAEDecode",
            Inputs = new Dictionary<string, object?>
            {
                ["samples"] = new[] { "KSampler", 0 },
                ["vae"] = new[] { "CheckpointLoaderSimple", 1 }
            }
        };

        // Save Image
        workflow["SaveImage"] = new ComfyNode
        {
            ClassType = "SaveImage",
            Inputs = new Dictionary<string, object?>
            {
                ["images"] = new[] { "VAEDecode", 0 }
            }
        };
    }

    private async Task BuildVideoWorkflowAsync(Dictionary<string, ComfyNode> workflow, GenerationRequest request)
    {
        // Similar structure but with video-specific nodes (Wan2GP or SVD)
        // This is a placeholder that can be expanded based on actual ComfyUI nodes
        
        workflow["CLIPTextEncode"] = new ComfyNode
        {
            ClassType = "CLIPTextEncode",
            Inputs = new Dictionary<string, object?>
            {
                ["clip"] = new[] { "1", 0 },
                ["text"] = request.Prompt
            }
        };

        workflow["EmptyLatentVideo"] = new ComfyNode
        {
            ClassType = "EmptyLatentVideo",
            Inputs = new Dictionary<string, object?>
            {
                ["width"] = request.Width,
                ["height"] = request.Height,
                ["length"] = 24 // Default video length in frames
            }
        };

        workflow["KSamplerVideo"] = new ComfyNode
        {
            ClassType = "KSamplerVideo",
            Inputs = new Dictionary<string, object?>
            {
                ["seed"] = request.Seed ?? 42,
                ["steps"] = request.Steps,
                ["cfg"] = request.GuidanceScale,
                ["sampler_name"] = "euler",
                ["scheduler"] = "normal",
                ["denoise"] = 0.8,
                ["model"] = new[] { "CheckpointLoaderSimple", 0 },
                ["positive"] = new[] { "CLIPTextEncode", 0 },
                ["negative"] = new[] { "CLIPTextEncode2", 0 },
                ["latent_video"] = new[] { "EmptyLatentVideo", 0 }
            }
        };

        workflow["VAEDecodeVideo"] = new ComfyNode
        {
            ClassType = "VAEDecodeVideo",
            Inputs = new Dictionary<string, object?>
            {
                ["samples"] = new[] { "KSamplerVideo", 0 },
                ["vae"] = new[] { "CheckpointLoaderSimple", 1 }
            }
        };

        workflow["SaveVideo"] = new ComfyNode
        {
            ClassType = "SaveVideo",
            Inputs = new Dictionary<string, object?>
            {
                ["images"] = new[] { "VAEDecodeVideo", 0 }
            }
        };
    }
}
