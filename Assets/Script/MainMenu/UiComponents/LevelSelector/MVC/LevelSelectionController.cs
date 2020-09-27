using System;
using System.Threading.Tasks;
using MainLevel.Data;
using Modules.GameState;
using Patterns.MVC;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Smooth.Slinq;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Extensions.Primitive;
using Strx.Expansions.Modules.ModulesManagement;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unit = Smooth.Algebraics.Unit;

namespace MainMenu.UiElements.LevelSelector
{
    public class LevelSelectionController : ControllerBase<LevelSelectionModel, LevelSelectionView>
    {
        private const string LEVEL_SELECTOR_PATH = "MainMenu/LevelSelection";

        private GameObject _contentRoot;
        private IGameStateModule _gameStateModule;

        public static Task<Result<LevelSelectionController>> Create(GameObject contentRoot)
            => ControllerBase<LevelSelectionModel, LevelSelectionView>
                .Create<LevelSelectionController>(false)
                .ThenContextAsync(contentRoot, (controller, _contentroot) =>
                {
                    controller._contentRoot = _contentroot;
                    return controller.InitAsync()
                        .ThenContextAsync(_ => controller);
                })
                .SpecifyAsync(() => $"Cannot create {nameof(LevelSelectionController)}");

        protected override Task<Result<Unit>> SetInitialAsync()
            => ModulesRegister.Get<IGameStateModule>()
                .Then(this, (gameStateModule, _this) => _this._gameStateModule = gameStateModule)
                .ToUnit()
                .ToTask();

        protected override Task<Result<LevelSelectionModel>> LoadModelAsync()
            => _gameStateModule.FetchLevelInfos();

        protected override Task<Result<LevelSelectionView>> LoadViewAsync()
            => GetLevelSelectorView()
                .Then(this, (viewComponent, _this) =>
                {
                    SubscribeForViewSelectorEvents(viewComponent, _this);
                    return viewComponent;
                })
                .ToTask();

        private Result<LevelSelectionView> GetLevelSelectorView()
            => Resources.Load<GameObject>(LEVEL_SELECTOR_PATH)
                .ToResult(() => "The prefab is missing ")
                .Then(this, (prefab, _this)
                    => MonoBehaviour.Instantiate(prefab, _this._contentRoot.transform)
                        .GetComponent<LevelSelectionView>()
                );

        private static void SubscribeForViewSelectorEvents(LevelSelectionView viewComponent, LevelSelectionController _this)
            => viewComponent.OnLevelSelected.First().Subscribe(_this.LaunchGameLevel);

        private void LaunchGameLevel(string sceneName)
        {
            var levelInfo = Model.LevelInfos.Slinq()
                .First(info => info.DisplayName == sceneName);

            if (!levelInfo.IsNewGame && levelInfo.ShapeType.isNone)
                throw new InvalidOperationException("Shape type is not setup for continuation game mode");

            _gameStateModule.StartGameData = new StartGameData
                {ElementsShapeType = levelInfo.ShapeType.value, NewGame = levelInfo.IsNewGame}.ToSome();

            SceneManager.LoadScene("Game");
        }
    }
}