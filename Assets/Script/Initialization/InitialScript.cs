using MainLevel.TetrisElements;
using Modules.BlockObjectPool;
using Modules.Builders;
using Modules.CoroutineRunner;
using Modules.GameState;
using Modules.RequestProcessing;
using Modules.RequestProcessing.Manifests;
using Smooth.Algebraics;
using Strx.Expansions.Modules.ManifestManagement;
using Strx.Expansions.Modules.ModulesManagement;
using UniRx.Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Initialization
{
    public class InitialScript : MonoBehaviour
    {
        private void Awake()
        {
            SetupAppSettings();
            InitModules();
            LaunchMenu();
        }

        private void SetupAppSettings()
        {
            Application.targetFrameRate = 20;
        }
    
        private void InitModules()
        {
            ModulesRegister.AddSingleton(CoroutineRunner.Create())
                .Then(_ => ManifestRegisterBuilder.BuildAndFillEnvDependant()
                    .Specify($"Cannot build {nameof(ManifestRegister)} using builder")
                    .Then(ModulesRegister.AddSingleton))
                .Then(_ => UnityNetworkClientProvider.Create(Option<UnityNetworkClientManifest>.None)
                    .Then(ModulesRegister.AddSingleton))
                .Then(_ => GameStateHolderModule.Create()
                    .Then(ModulesRegister.AddSingleton))
                .Then(_ => ModulesRegister.AddSingleton(new BlockItemPoolModule()))
                .ThrowIfError();
        }

        private void LaunchMenu()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}