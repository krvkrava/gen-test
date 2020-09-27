using System.Threading.Tasks;
using MainLevel.Data;
using MainLevel.Models;
using MainMenu.UiElements.LevelSelector;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Strx.Expansions.Modules.ModulesManagement.Attributes;

namespace Modules.GameState
{
    [ModuleProvider]
    public interface IGameStateModule
    {
        Option<StartGameData> StartGameData { get; set; }
        Task<Result<LevelSelectionModel>> FetchLevelInfos();
        Task<Result<Unit>> SaveLevelState(LevelSaveData levelData);
        Task<Result<GameStateModel>> LoadLevelState();
        Task<Result<Unit>> DeleteLevelState();
    }
}