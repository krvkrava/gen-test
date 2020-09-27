using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Smooth.Slinq;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Extensions.Collections;
using Strx.Expansions.Modules.ManifestManagement;
using UnityEngine;

namespace Modules.Builders
{
    public static class ManifestRegisterBuilder
    {
        private const string DEFAULT_MANIFESTS_LOCATION_PATH = "Manifests/Default";
        private const string TEST_MANIFESTS_LOCATION_PATH = "Manifests/Default";
        private const string PROD_MANIFESTS_LOCATION_PATH = "Manifests/Default";
        
        public static Result<ManifestRegister> BuildAndFillEnvDependant()
        {
            var manifestRegister = new ManifestRegister();
            var envRelatedManifestsLocation =
#if UNITY_EDITOR
                Option<string>.None;
#elif RELEASE
                PROD_MANIFESTS_LOCATION_PATH.ToSome();
#elif DEBUG
                TEST_MANIFESTS_LOCATION_PATH.ToSome();
#else
                return Result<Unit>.FromError("Not supported environment or environment is not defined");
#endif

            var addDefaultManifestsResult = Resources.LoadAll<TextAsset>(DEFAULT_MANIFESTS_LOCATION_PATH).Slinq()
                .Select(textAsset => textAsset.text)
                .ToArray()
                .ToValue()
                .Then(manifestRegister.AddManyAsJsons);

            if (addDefaultManifestsResult.IsError)
                return addDefaultManifestsResult.ConvertErrorTo<ManifestRegister>($"Cannot add default" +
                                                                      $" manifests from: {DEFAULT_MANIFESTS_LOCATION_PATH}");

            if (envRelatedManifestsLocation.isSome)
            {
                var addEnvManifestsResult = Resources.LoadAll<TextAsset>(envRelatedManifestsLocation.value).Slinq()
                    .Select(textAsset => textAsset.text)
                    .ToArray()
                    .ToValue()
                    .Then(manifestRegister.AddManyAsJsons);

                if (addEnvManifestsResult.IsError)
                    return addEnvManifestsResult.ConvertErrorTo<ManifestRegister>($"Cannot add env related" +
                                                                      $" manifests from: {envRelatedManifestsLocation}");
            }

            return manifestRegister.ToValue();
        }
    }
}