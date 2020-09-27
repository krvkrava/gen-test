using System.ComponentModel;
using Modules.BlockObjectPool;
using UnityEngine;

namespace MainLevel.TetrisElements
{
    public static class ShapeElementBuilder
    {
        public static BlockItem[] Create(ShapeType type, IBlockItemsPoolModule blockItemsPoolModule)
        {
            if (type == ShapeType.Combined)
                type = Random.Range(0, 2) == 0 ? ShapeType.TType : ShapeType.Corner;

            var itemsQuantity = type == ShapeType.Corner ? 5 : 4;
            var blocks = blockItemsPoolModule.GetBlockItems(itemsQuantity, true);

            if (type == ShapeType.TType)
                SetItemsTTypeItemsPosition(blocks);
            else
                SetItemsCornerItemsPosition(blocks);

            return blocks;
        }

        public static void DisposeBlockItem(BlockItem block)
        {
            block.gameObject.SetActive(false);
        }

        private static void SetItemsTTypeItemsPosition(BlockItem[] blockItems)
        {
            blockItems[0].Position = new Vector2Int(0, 0);
            blockItems[1].Position = new Vector2Int(1, 0);
            blockItems[2].Position = new Vector2Int(2, 0);
            blockItems[3].Position = new Vector2Int(1, 1);
        }

        private static void SetItemsCornerItemsPosition(BlockItem[] blockItems)
        {
            blockItems[0].Position = new Vector2Int(0, 0);
            blockItems[1].Position = new Vector2Int(1, 0);
            blockItems[2].Position = new Vector2Int(2, 0);
            blockItems[3].Position = new Vector2Int(3, 0);
            blockItems[4].Position = new Vector2Int(3, 1);
        }
    }
}