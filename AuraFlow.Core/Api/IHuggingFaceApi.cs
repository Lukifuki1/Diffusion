using System.Threading.Tasks;
using Refit;
using AuraFlow.Core.Models.Api.HuggingFace;

namespace AuraFlow.Core.Api;

public interface IHuggingFaceApi
{
    [Get("/api/whoami-v2")]
    Task<IApiResponse<HuggingFaceUser>> GetCurrentUserAsync([Header("Authorization")] string authorization);
}
