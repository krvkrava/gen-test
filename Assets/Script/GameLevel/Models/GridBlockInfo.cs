using Newtonsoft.Json;

namespace MainLevel.Models
{
    public class GridBlockInfo
    {
        [JsonProperty("x")] public int XPos { get; set; }
        [JsonProperty("y")] public int YPos { get; set; }
        [JsonProperty("color")] public string Color { get; set; }
    }
}