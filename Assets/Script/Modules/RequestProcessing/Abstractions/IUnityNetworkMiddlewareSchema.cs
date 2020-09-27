using System.Threading.Tasks;
using Modules.RequestProcessing.Data;
using Smooth.Algebraics.Results;
using Strx.Expansions.Modules.RequestProcessing.Abstractions;
using Strx.Expansions.Modules.RequestProcessing.Data;

namespace Modules.RequestProcessing.Abstractions
{
    public interface IUnityNetworkMiddlewareSchema : INetworkMiddlewareSchema
    {
        Task<Result<ResponseBundleData>> RequestBundle(RequestData requestData);
    }
}