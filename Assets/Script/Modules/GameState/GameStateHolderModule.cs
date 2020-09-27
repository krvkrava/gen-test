using System.Linq;
using System.Threading.Tasks;
using MainLevel.Data;
using MainLevel.Models;
using MainLevel.TetrisElements;
using MainMenu.UiElements.LevelSelector;
using Modules.RequestProcessing;
using Newtonsoft.Json;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Smooth.Slinq;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Extensions.Collections;
using Strx.Expansions.Modules.ModulesManagement;
using Strx.Expansions.Modules.RequestProcessing.Data;
using UnityEditor;
using UnityEngine;

namespace Modules.GameState
{
    public class GameStateHolderModule : IGameStateModule
    {
        private const string GET_STATE_ENDPOINT = "userState/info";
        private const string SAVE_STATE_ENDPOINT = "userState/save";
        private const string LOAD_STATE_ENDPOINT = "userState/load";
        private const string DELETE_STATE_ENDPOINT = "userState/delete";
        private const string PARAM_USER_ID_KEY = "userId";
        private const string STORAGE_USER_ID_KEY = "user_id";

        public string UserId => GetOrCreateUserId();
        public Option<StartGameData> StartGameData { get; set; }

        private IUnityNetworkClientProvider _networkClient;

        public static Result<GameStateHolderModule> Create()
        {
            var module = new GameStateHolderModule();
            return ModulesRegister.Get<IUnityNetworkClientProvider>()
                .Specify($"Cannot get {nameof(IUnityNetworkClientProvider)}")
                .Then(module, (networkClient, _this) => _this._networkClient = networkClient)
                .Then(_ => module);
        }

        public Task<Result<LevelSelectionModel>> FetchLevelInfos()
            => FetchData<LevelSelectionModel>(GET_STATE_ENDPOINT);

        public async Task<Result<Unit>> SaveLevelState(LevelSaveData levelSaveData)
        {
            var gridBlocksSequenceModel = levelSaveData.GameGrid.Cast<BlockItem>().Slinq()
                .Where(block => block != null)
                .Select(CreateGridBlockModel)
                .ToArray();

            var shapeBlockModel = levelSaveData.ShapeBlocks.Slinq()
                .Select(CreateGridBlockModel)
                .ToArray();

            var gameStateModule = new GameStateModel {GridBlocks = gridBlocksSequenceModel, ShapeBlocks = shapeBlockModel};
            var serializedState = JsonConvert.SerializeObject(gameStateModule);

            var request = RequestData.CreateWithLink(SAVE_STATE_ENDPOINT, RequestMethod.POST)
                .SetHeader(PARAM_USER_ID_KEY, UserId)
                .SetBody(serializedState);
            
            var result = await _networkClient.SendPost(request);

            return result.ToUnit(() => "Unable to save user state");
        }

        public Task<Result<GameStateModel>> LoadLevelState()
            => FetchData<GameStateModel>(LOAD_STATE_ENDPOINT);
        
        public async Task<Result<Unit>> DeleteLevelState()
        {
            var request = RequestData.CreateWithLink(DELETE_STATE_ENDPOINT, RequestMethod.DELETE)
                .SetHeader(PARAM_USER_ID_KEY, UserId);
            var result = await _networkClient.Send(request);

            return result.ToUnit(() => "Unable to delete user level state");
        }

        private async Task<Result<T>> FetchData<T>(string endpoint)
        {
            var request = RequestData.CreateWithLink(endpoint, RequestMethod.GET)
                .SetHeader(PARAM_USER_ID_KEY, UserId);
            var result = await _networkClient.SendGet(request);
            if (result.IsError)
                return result.ConvertErrorTo<T>($"Unable to fetch from: {endpoint}");

            var response = result.Value;
            if (!response.IsSuccess)
                return Result<T>.FromError($"{endpoint} endpoint returned an error: {response.Error}");

            return JsonConvert.DeserializeObject<T>(response.RawText.value).ToValue();
        }

        private GridBlockInfo CreateGridBlockModel(BlockItem blockItem)
            => new GridBlockInfo
            {
                XPos = blockItem.Position.x, YPos = blockItem.Position.y, 
                Color = blockItem.ColorName
            };

        private string GetOrCreateUserId()
        {
            var value = PlayerPrefs.GetString(STORAGE_USER_ID_KEY);
            var recordExists = value != string.Empty;
            if (recordExists)
                return value;

            var userId = GUID.Generate().ToString();
            PlayerPrefs.SetString(STORAGE_USER_ID_KEY, userId);

            return userId;
        }
    }
}