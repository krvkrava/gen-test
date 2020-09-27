using System;
using System.Linq;
using Helpers;
using MainLevel.TetrisElements.Input;
using Modules.BlockObjectPool;
using Smooth.Algebraics;
using Smooth.Slinq;
using Strx.Expansions.Extensions.Collections;
using UniRx;
using UnityEngine;
using Unit = Smooth.Algebraics.Unit;

namespace MainLevel.TetrisElements
{
    public class ShapeElement : MonoBehaviour, IDisposable
    {
        public IObservable<Unit> OnGameGridIsFull => _onGameGridIsFull;

        public Option<BlockItem[]> BlockItems { get; set; }

        private GameGrid GameGrid { get; set; }
        private IBlockItemsPoolModule _blockItemsPoolModule;

        private Subject<Unit> _onGameGridIsFull = new Subject<Unit>();

        private IDisposable _disposable;

        public static ShapeElement Create(InputReceiver inputReceiver, GameGrid gameGrid,
            IBlockItemsPoolModule blockItemsPoolModule, Option<BlockItem[]> blockItems)
        {
            var shapeGameObject = new GameObject("Shape");
            var blockGroups = shapeGameObject.AddComponent<ShapeElement>();
            blockGroups.Initialize(inputReceiver, gameGrid, blockItemsPoolModule, blockItems);

            return blockGroups;
        }

        public void Run(ShapeType elementsShapeType)
        {
            SetBlocksInitialState(elementsShapeType);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private void SetBlocksInitialState(ShapeType elementsShapeType)
        {
            if (BlockItems.isNone)
            {
                var blocks = ShapeElementBuilder.Create(elementsShapeType, _blockItemsPoolModule);
                BlockItems = blocks.ToSome();
            }

            MoveBlocksToTheTop();
            CheckForGameOver();
        }

        private void AttemptToRotate()
        {
            var sumX = BlockItems.value.Sum(block => block.Position.x);
            var sumY = BlockItems.value.Sum(block => block.Position.y);
            var pivotX = Mathf.RoundToInt(sumX / BlockItems.value.Length);
            var pivotY = Mathf.RoundToInt(sumY / BlockItems.value.Length);

            var rotatedPositions = BlockItems.value.Slinq()
                .Select(block => Vector3Utils.RotateAroundPivot(
                    new Vector3(block.Position.x, block.Position.y, 0),
                    new Vector3(pivotX, pivotY, 0),
                    Vector3.forward * 90))
                .Select(vector3Pos => new Vector2Int(
                    Mathf.RoundToInt(vector3Pos.x),
                    Mathf.RoundToInt(vector3Pos.y)))
                .ToArray();

            var doInteract = GameGrid.InteractWithExisting(rotatedPositions);
            if (doInteract)
                return;

            for (int i = 0; i < rotatedPositions.Length; i++)
                BlockItems.value[i].Position = rotatedPositions[i];
        }

        private void Initialize(InputReceiver inputReceiver, GameGrid gameGrid,
            IBlockItemsPoolModule blockItemsPoolModule, Option<BlockItem[]> blockItems)
        {
            GameGrid = gameGrid;
            BlockItems = blockItems;
            _blockItemsPoolModule = blockItemsPoolModule;

            SubscribeToObservables(inputReceiver);
        }

        private void SubscribeToObservables(InputReceiver inputReceiver)
        {
            var horizontalInputDisposable = inputReceiver.HorizontalButtonClicked.Subscribe(MovementButtonClicked);
            var rotationInputDisposable = inputReceiver.RotateButtonClicked.Subscribe(_ => AttemptToRotate());
            var moveItemDownDisposable = inputReceiver.MoveDownButtonClicked.Subscribe(_ => AttemptMoveItemsDown());
            var shapeMovementDisposable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ => AttemptMoveItemsDown());

            _disposable = new CompositeDisposable(horizontalInputDisposable,
                rotationInputDisposable, moveItemDownDisposable, shapeMovementDisposable);
        }


        private void CheckForGameOver()
        {
            if (!GameGrid.ItemsInteractInDirection(BlockItems.value, Vector2Int.down))
                return;

            _onGameGridIsFull.OnNext(Unit.Default);
            Dispose();
        }

        private void MovementButtonClicked(bool isLeft)
        {
            if (GameGrid.ItemsInteractInDirection(BlockItems.value, isLeft ? Vector2Int.left : Vector2Int.right))
                return;

            MoveItemsHorizontally(isLeft);
        }

        private void MoveItemsHorizontally(bool left)
            => BlockItems.value.Slinq()
                .ForEach(left, (item, _left) =>
                    item.Move(_left ? Vector2Int.left : Vector2Int.right));

        private void MoveBlocksToTheTop()
        {
            var highestItemYPos = BlockItems.value.Max(item => item.Position.y);
            BlockItems.value.Slinq()
                .ForEach(highestItemYPos, (item, _highestItemYPos) =>
                    item.Move(Vector2Int.up * (GameGrid.PlayAreaBounds.y - 1 - _highestItemYPos)));
            
        }

        private void AttemptMoveItemsDown()
        {
            var interacts = GameGrid.ItemsInteractInDirection(BlockItems.value, Vector2Int.down);
            if (interacts)
            {
                var blocks = BlockItems.value;
                BlockItems = Option<BlockItem[]>.None;
                GameGrid.RegisterBlocks(blocks);
                return;
            }

            BlockItems.value.Slinq()
                .ForEach(item => item.Move(Vector2Int.down));
        }


        private void OnDestroy()
        {
            Dispose();
        }
    }
}