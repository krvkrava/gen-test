using Newtonsoft.Json;

namespace MainLevel.Models
{
    public class GameStateModel
    {
        [JsonProperty("gridBlocks")]
        public GridBlockInfo[] GridBlocks { get; set; }
        [JsonProperty("shapeBlocks")]
        public GridBlockInfo[] ShapeBlocks { get; set; }
    }
}