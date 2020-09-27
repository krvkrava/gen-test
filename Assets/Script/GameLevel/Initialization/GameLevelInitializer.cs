using System;
using System.Threading.Tasks;
using MainLevel.Data;
using MainLevel.Models;
using MainLevel.TetrisElements;
using MainLevel.TetrisElements.Input;
using MainLevel.UI;
using Modules.BlockObjectPool;
using Modules.GameState;
using Smooth.Algebraics;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Modules.ModulesManagement;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainLevel
{
    public class GameLevelInitializer : MonoBehaviour
    {
        [SerializeField] private NavigationPanel _navigationPanel;
        [SerializeField] private PopupController _popupController;

        private readonly Vector2Int _playFieldAreaBounds = new Vector2Int(9, 14);
        
        private ShapeElement _shapeElement;
        private GameGrid _gameGrid;
        
        private IGameStateModule _gameStateModule;
        private IBlockItemsPoolModule _blockItemsPoolModule;
        
        private StartGameData _startGameData;
        
        private bool _isGameFinished;

        private IDisposable _disposable;

        private async void Awake()
        {
            InitModuleReferences();
            SetGameState();
            await InitGridAndShapeElementAsync();
            SubscribeForEvents();
            StartIteration();
        }

        private void SetGameState()
        {
            _startGameData = _gameStateModule.StartGameData.value;
            _gameStateModule.StartGameData = Option<StartGameData>.None;
        }

        private void InitModuleReferences()
        {
            _blockItemsPoolModule = ModulesRegister.Get<IBlockItemsPoolModule>()
                .ThrowIfError(() => $"Cannot initialize module references at {nameof(GameLevelInitializer)}")
                .Value;

            _gameStateModule = ModulesRegister.Get<IGameStateModule>()
                .ThrowIfError(() => $"Cannot initialize module references at {nameof(GameLevelInitializer)}")
                .Value;
        }

        private void SubscribeForEvents()
        {
            _disposable = new CompositeDisposable(
                _shapeElement.OnGameGridIsFull.Subscribe(_ => StopGame()),
                _gameGrid.ONGridChangedSubject.Subscribe(grid => OnIterationCompleted(grid)));

            _navigationPanel.OnBackButtonClicked.Subscribe(_ => OnBackButtonClicked())
                .AddTo(this);
        }

        private async void StopGame()
        {
            _disposable?.Dispose();
            _isGameFinished = true;
            await _gameStateModule.DeleteLevelState();
            _popupController.ShowGameOverPopup();
        }

        private async Task OnIterationCompleted(BlockItem[,] grid)
        {
            StartIteration();
            if (!_isGameFinished)
                await SaveGameState(grid.ToSome());
        }

        private async void OnBackButtonClicked()
        {
            if (!_isGameFinished)
                await SaveGameState(Option<BlockItem[,]>.None);

            SceneManager.LoadScene("Menu");
        }

        private Task SaveGameState(Option<BlockItem[,]> newGridStateOption)
        {
            var newGridState = newGridStateOption.ValueOr(_gameGrid.GridArea);
            return _gameStateModule.SaveLevelState(new LevelSaveData
                    {GameGrid = newGridState, ShapeBlocks = _shapeElement.BlockItems.value})
                .ThrowIfErrorAsync("Unable to save game state");
        }

        private async Task InitGridAndShapeElementAsync()
        {
            var blockItemsData = _startGameData.NewGame
                ? Option<GameStateModel>.None
                : (await _gameStateModule.LoadLevelState()
                    .ThrowIfErrorAsync("Cannot load level data")).ToOption();

            var gridData = blockItemsData.Cata(
                v => _blockItemsPoolModule.GetBlockItems(v.GridBlocks).ToSome(),
                () => Option<BlockItem[]>.None);

            var shapeBlocksData = blockItemsData.Cata(
                v => _blockItemsPoolModule.GetBlockItems(v.ShapeBlocks).ToSome(),
                () => Option<BlockItem[]>.None);

            _gameGrid = new GameGrid(_playFieldAreaBounds, gridData, _blockItemsPoolModule);
            var inputReceiver = InputReceiver.Create();
            _shapeElement = ShapeElement.Create(inputReceiver, _gameGrid, _blockItemsPoolModule, shapeBlocksData);
        }

        private void StartIteration()
        {
            _shapeElement.Run(_startGameData.ElementsShapeType);
        }
    }
}