using UnityEngine;

namespace MainLevel.TetrisElements
{
    public class BlockItem : MonoBehaviour
    {
        public string ColorName { get; private set; }

        public Vector2Int Position
        {
            get => _position;
            set
            {
                _position = value;
                transform.position = new Vector2(value.x, value.y);
            }
        }

        private Vector2Int _position;

        public void Move(Vector2Int direction)
        {
            Position += direction;
        }

        public void SetColor(string colorName, Color value)
        {
            ColorName = colorName;
            GetComponent<SpriteRenderer>().color = value;
        }
    }
}