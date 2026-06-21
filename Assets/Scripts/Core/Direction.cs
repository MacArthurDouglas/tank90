using UnityEngine;

namespace Tank90
{
    public enum Direction { Up = 0, Left = 1, Down = 2, Right = 3 }

    public static class DirectionExt
    {
        public static Vector2 ToVector(this Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Vector2.up;
                case Direction.Down: return Vector2.down;
                case Direction.Left: return Vector2.left;
                default: return Vector2.right;
            }
        }

        // Sprites are drawn facing UP; rotate the renderer/transform by this Z angle.
        public static float ToZAngle(this Direction d)
        {
            switch (d)
            {
                case Direction.Up: return 0f;
                case Direction.Left: return 90f;
                case Direction.Down: return 180f;
                default: return -90f; // Right
            }
        }

        public static Direction Opposite(this Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                default: return Direction.Left;
            }
        }
    }
}
