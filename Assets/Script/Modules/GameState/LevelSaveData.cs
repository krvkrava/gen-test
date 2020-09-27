using MainLevel.TetrisElements;

namespace Modules.GameState
{
    public struct LevelSaveData
    {
        public BlockItem[] ShapeBlocks { get; set; }
        public BlockItem[,] GameGrid { get; set; }
    }
}