using System;
using MainLevel.TetrisElements;
using Modules.BlockObjectPool;
using Smooth.Algebraics;
using Smooth.Slinq;
using UniRx;
using UnityEngine;

namespace MainLevel
{
    public class GameGrid
    {
        public BlockItem[,] GridArea { get; }

        public IObservable<BlockItem[,]> ONGridChangedSubject => _onGridChangedSubject;

        public Vector2Int PlayAreaBounds => new Vector2Int(
            GridArea.GetLength(0), GridArea.GetLength(1));

        private IBlockItemsPoolModule _blockItemsPoolPool;
        
        private Subject<BlockItem[,]> _onGridChangedSubject = new Subject<BlockItem[,]>();

        public GameGrid(Vector2Int playAreaBounds, Option<BlockItem[]> blockItems, 
            IBlockItemsPoolModule blockItemsPoolPoolPool)
        {
            GridArea = new BlockItem[playAreaBounds.x, playAreaBounds.y];
            _blockItemsPoolPool = blockItemsPoolPoolPool;
            
            if (blockItems.isSome)
                InitBlocks(blockItems.value);
        }

        public void RegisterBlocks(BlockItem[] items)
        {
            items.Slinq()
                .ForEach(block => GridArea[block.Position.x, block.Position.y] = block);

            ValidateBlocksMatch();
        }

        public bool InteractWithExisting(Vector2Int[] positions)
            => positions.Slinq().Any((block, _this)
                => _this.InteractsWithAnyInDirection(block, Option<Vector2Int>.None), this);

        public bool ItemsInteractInDirection(BlockItem[] items, Vector2Int direction)
            => items.Slinq()
                .Select(item => item.Position)
                .Any((item, _context) =>
                    _context.Item1.InteractsWithAnyInDirection(item, _context.direction.ToSome()), (this, direction));

        private void InitBlocks(BlockItem[] blockItems)
            => blockItems.Slinq()
                .ForEach(GridArea, (block, _grid) => { _grid[block.Position.x, block.Position.y] = block; });

        private bool InteractsWithAnyInDirection(Vector2Int position, Option<Vector2Int> direction)
        {
            var targetXCoord = position.x + direction.Cata(value => value.x, 0);
            var targetYCoord = position.y + direction.Cata(value => value.y, 0);

            if (targetXCoord < 0 || targetXCoord > PlayAreaBounds.x - 1 || targetYCoord < 0)
                return true;

            return GridArea[targetXCoord, targetYCoord] != null;
        }

        private void ValidateBlocksMatch()
        {
            for (int row = 0; row < GridArea.GetLength(1); row++)
            {
                int existCount = 0;
                for (int column = 0; column < GridArea.GetLength(0); column++)
                    if (GridArea[column, row] != null)
                        existCount++;

                if (GridArea.GetLength(0) == existCount)
                {
                    RemoveRowFromGrid(row);
                    MoveUpperRowElementsDown(row);
                    row--;
                }
            }

            _onGridChangedSubject.OnNext(GridArea);
        }

        private void RemoveRowFromGrid(int rowIndex)
        {
            for (int column = 0; column < GridArea.GetLength(0); column++)
            {
                _blockItemsPoolPool.Return(GridArea[column, rowIndex]);
                GridArea[column, rowIndex] = null;
            }
        }

        private void MoveUpperRowElementsDown(int startFromRowIndex)
        {
            for (int row = startFromRowIndex; row < PlayAreaBounds.y - 1; row++)
            for (int column = 0; column < PlayAreaBounds.x; column++)
            {
                if (row == PlayAreaBounds.y - 1)
                    GridArea[column, row] = null;
                else
                {
                    var targetBlock = GridArea[column, row + 1];
                    if (targetBlock == null)
                        continue;

                    GridArea[column, row] = targetBlock;
                    targetBlock.Position = new Vector2Int(column, row);
                    GridArea[column, row + 1] = null;
                }
            }
        }
    }
}