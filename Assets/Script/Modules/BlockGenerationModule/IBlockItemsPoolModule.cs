using MainLevel.Models;
using MainLevel.TetrisElements;
using Strx.Expansions.Modules.ModulesManagement.Attributes;

namespace Modules.BlockObjectPool
{
    [ModuleProvider]
    public interface IBlockItemsPoolModule
    {
        BlockItem[] GetBlockItems(int quantity, bool applyRandomColor);
        BlockItem[] GetBlockItems(GridBlockInfo[] models);

        void Return(BlockItem block);
    }
}