using Modules.RequestProcessing.Abstractions;
using Modules.RequestProcessing.Manifests;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Modules.RequestProcessing;
using Strx.Expansions.Modules.RequestProcessing.Manifests;

namespace Modules.RequestProcessing
{
    public class UnityNetworkClientProvider : NetworkClient<IUnityNetworkMiddlewareSchema>, IUnityNetworkClientProvider
    {
        public static Result<UnityNetworkClientProvider> Create(Option<UnityNetworkClientManifest> overrideManifest)
        {   
            var client = new UnityNetworkClientProvider();
            var pipelineBuildResult = client.MergeManifest(UnityNetworkClientManifest.MANIFEST_NAME, overrideManifest)
                .Specify($"Unable to register manifest: {UnityNetworkClientManifest.MANIFEST_NAME}")
                .ThenAndSpecify(manifest => client.BuildPipeline(manifest.NetworkClientMiddlewares), 
                    $"Cannot build pipeline at {nameof(UnityNetworkClientProvider)}");

            if (pipelineBuildResult.IsError)
                return pipelineBuildResult.ConvertErrorTo<UnityNetworkClientProvider>();
            
            client.NetworkPipeline = pipelineBuildResult.Value;

            return client.ToValue();
        }
        
        protected override Result<IUnityNetworkMiddlewareSchema> BuildPipeline(NetworkClientMiddlewareManifests manifests)
        {
            return new UnityNetworkClientPipelineBuilder<IUnityNetworkMiddlewareSchema>()
                .BuildUnityNetworkClientPipeline(manifests);
        }
    }
}