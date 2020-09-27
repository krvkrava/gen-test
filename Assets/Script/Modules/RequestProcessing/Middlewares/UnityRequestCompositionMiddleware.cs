using System.Threading.Tasks;
using Modules.RequestProcessing.Abstractions;
using Modules.RequestProcessing.Data;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Strx.Expansions.Modules.RequestProcessing.Data;
using Strx.Expansions.Modules.RequestProcessing.Manifests;
using Strx.Expansions.Modules.RequestProcessing.Middlewares;

namespace Modules.RequestProcessing.Middlewares
{
    public class UnityRequestCompositionMiddleware<T> : RequestCompositionMiddleware<T>, 
        IUnityNetworkMiddlewareSchema where T : IUnityNetworkMiddlewareSchema
    {   
        public static UnityRequestCompositionMiddleware<T> Create(Option<T> nextMiddleware, 
            RequestCompositionMiddlewareManifest manifest)
        {
            var middleware = new UnityRequestCompositionMiddleware<T>
            {
                NextMiddleware = nextMiddleware, _manifest = manifest
            };

            return middleware;
        }
        
        public Task<Result<ResponseBundleData>> RequestBundle(RequestData requestData)
        {
            requestData = CompositeRequestData(requestData);

            return NextMiddleware.value.RequestBundle(requestData);
        }
    }
}