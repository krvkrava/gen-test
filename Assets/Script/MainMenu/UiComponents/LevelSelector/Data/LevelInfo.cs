using Helpers;
using MainLevel.TetrisElements;
using Newtonsoft.Json;
using Smooth.Algebraics;

namespace MainMenu.UiElements.LevelSelector.Data
{
    public class LevelInfo
    {
        [JsonProperty("enabled")] public bool Enabled { get; set; }
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        [JsonProperty("shape"), JsonConverter(typeof(JsonOptionNullConverter<ShapeType>))]
        public Option<ShapeType> ShapeType { get; set; }
        [JsonProperty("newGame")] public bool IsNewGame { get; set; }
    }
}