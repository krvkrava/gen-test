using Modules.CoroutineRunner;
using Modules.RequestProcessing.Abstractions;
using Modules.RequestProcessing.Middlewares;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Modules.ModulesManagement;
using Strx.Expansions.Modules.RequestProcessing;
using Strx.Expansions.Modules.RequestProcessing.Manifests;

namespace Modules.RequestProcessing
{
    public class UnityNetworkClientPipelineBuilder<T> : NetworkClientPipelineBuilder<T>
    where T : IUnityNetworkMiddlewareSchema
    {
        public Result<IUnityNetworkMiddlewareSchema> BuildUnityNetworkClientPipeline(
            NetworkClientMiddlewareManifests manifest)
        {
            var coroutineRunner = ModulesRegister.Get<ICoroutineRunner>();
            if(coroutineRunner.IsError)
                return Result<IUnityNetworkMiddlewareSchema>.FromError(
                    $"Unable to get {nameof(coroutineRunner)} at {nameof(UnityNetworkClientPipelineBuilder<T>)}");

            var compositionManifest = manifest.RequestComposition;
            
            var requestProcessor = UnityWebRequestProcessorMiddleware<T>.Create(Option<T>.None,
                coroutineRunner.Value, manifest.RequestProcessor);
            var statusCodeValidator = StatusCodeValidationMiddleware<T>.Create(
                ((T)(IUnityNetworkMiddlewareSchema)requestProcessor).ToSome());
            var compositionMiddleware = UnityRequestCompositionMiddleware<T>.Create(
                ((T)(IUnityNetworkMiddlewareSchema)statusCodeValidator).ToSome(), compositionManifest);

            return ((IUnityNetworkMiddlewareSchema)compositionMiddleware).ToValue();
        }
    }
}