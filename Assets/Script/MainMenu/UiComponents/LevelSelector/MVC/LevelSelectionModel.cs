using MainMenu.UiElements.LevelSelector.Data;
using Newtonsoft.Json;
using Patterns.MVC;

namespace MainMenu.UiElements.LevelSelector
{
    public class LevelSelectionModel : ModelBase
    {
        [JsonProperty("levelInfos")] public LevelInfo[] LevelInfos { get; set; }
    }
}