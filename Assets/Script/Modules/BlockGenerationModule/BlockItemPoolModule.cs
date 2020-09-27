using MainLevel.Models;
using MainLevel.TetrisElements;
using Smooth.Slinq;
using Strx.Expansions.Extensions.Collections;
using UniRx.Toolkit;
using UnityEngine;

namespace Modules.BlockObjectPool
{
    public class BlockItemPoolModule : ObjectPool<BlockItem>, IBlockItemsPoolModule
    {
        private const string BLOCK_ITEM_PATH = "MainLevel/BlockItem";
        private GameObject _blockItemPrefab;

        public BlockItemPoolModule()
        {   
            _blockItemPrefab = Resources.Load<GameObject>(BLOCK_ITEM_PATH);
        }   
        
        public BlockItem[] GetBlockItems(GridBlockInfo[] models)
        {
            var blocks = GetBlockItems(models.Length, false)
                .SlinqWithIndex()
                .Select(blockAndIndex =>
                {
                    var model = models[blockAndIndex.Item2];
                    blockAndIndex.Item1.Position = new Vector2Int(model.XPos, model.YPos);
                    
                    return blockAndIndex.Item1;
                })
                .ToArray();
            ApplyColors(models, blocks);

            return blocks;
        }

        public BlockItem[] GetBlockItems(int quantity, bool applyRandomColor)
        {
            var blocks = new BlockItem[quantity];
            for (int i = 0; i < quantity; i++)
                blocks[i] = Rent(); 

            if (applyRandomColor)
                FillWithRandomColor(blocks);

            return blocks;
        }

        public new void Return(BlockItem block) => base.Return(block);
        
        protected override BlockItem CreateInstance()
            => MonoBehaviour.Instantiate(_blockItemPrefab).GetComponent<BlockItem>();         

        private void FillWithRandomColor(BlockItem[] blocks)
        {
            var colorAndName = BlockColorPalette.GetRandomNameAndColor();
            blocks.Slinq().ForEach(colorAndName, (block, _colorAndName) =>
                block.SetColor(_colorAndName.Item1, _colorAndName.Item2));
        }

        private void ApplyColors(GridBlockInfo[] models, BlockItem[] blocks)
            => models.SlinqWithIndex()
                .ForEach(modelAndIndex =>
                {
                    var color = BlockColorPalette.GetColor(modelAndIndex.Item1.Color);
                    blocks[modelAndIndex.Item2].SetColor(modelAndIndex.Item1.Color, color);
                });

    }
}