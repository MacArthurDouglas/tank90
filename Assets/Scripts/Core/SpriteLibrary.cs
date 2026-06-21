using System.Collections.Generic;
using UnityEngine;

namespace Tank90
{
    /// <summary>
    /// Loads all sub-sprites of Resources/GeneralSprites and indexes them by name.
    /// Cell naming: gs_c{col}_r{rowFromTop}. See docs/SPRITE_MAP.md.
    /// </summary>
    public static class SpriteLibrary
    {
        static Dictionary<string, Sprite> _map;

        public static void EnsureLoaded()
        {
            if (_map != null) return;
            _map = new Dictionary<string, Sprite>(512);
            foreach (var s in Resources.LoadAll<Sprite>("GeneralSprites"))
                _map[s.name] = s;
            if (_map.Count == 0)
                Debug.LogError("SpriteLibrary: no sprites loaded from Resources/GeneralSprites");
        }

        public static Sprite Get(string name)
        {
            EnsureLoaded();
            Sprite s;
            return _map.TryGetValue(name, out s) ? s : null;
        }

        /// <summary>Sprite at grid cell (col, rowFromTop).</summary>
        public static Sprite Cell(int col, int rowFromTop) => Get("gs_c" + col + "_r" + rowFromTop);

        /// <summary>
        /// Tank sprite. baseCol = 0 (yellow/green left block) or 8 (enemy right block).
        /// baseRow = the tank's row in the sheet. dir/frame select sub-cell.
        /// Layout per row: [Up0 Up1 Left0 Left1 Down0 Down1 Right0 Right1].
        /// </summary>
        public static Sprite Tank(int baseCol, int baseRow, Direction dir, int frame)
        {
            int col = baseCol + (int)dir * 2 + (frame & 1);
            return Cell(col, baseRow);
        }

        public static Sprite Bullet(Direction d)
        {
            switch (d)
            {
                case Direction.Up: return Get("bullet_up");
                case Direction.Down: return Get("bullet_down");
                case Direction.Left: return Get("bullet_left");
                default: return Get("bullet_right");
            }
        }

        // Terrain (see SPRITE_MAP.md)
        public static Sprite Brick => Cell(16, 0);
        public static Sprite Steel => Cell(16, 1);
        public static Sprite Water0 => Cell(16, 2);
        public static Sprite Water1 => Cell(16, 3);
        public static Sprite Trees => Cell(17, 2);
        public static Sprite Ice => Cell(18, 2);
        public static Sprite BaseEagle => Cell(19, 2);
        public static Sprite BaseDead => Cell(20, 2);

        // 8px (0.5-unit) terrain tiles
        public static Sprite Brick8 => Get("brick8");
        public static Sprite Steel8 => Get("steel8");

        // Effects / overlays
        public static Sprite SpawnStar(int frame) => Cell(16 + Mathf.Clamp(frame, 0, 3), 6);
        public static Sprite SmallExplosion(int frame) => Cell(16 + Mathf.Clamp(frame, 0, 2), 8);
        public static Sprite Shield(int frame) => Cell(16 + (frame & 1), 9);

        // Power-up icon. 0 helmet,1 timer,2 shovel,3 star,4 grenade,5 tank,6 gun.
        public static Sprite Item(int idx) => Cell(16 + Mathf.Clamp(idx, 0, 6), 7);

        // Score popup. 0:100 .. 4:500.
        public static Sprite Score(int idx) => Cell(18 + Mathf.Clamp(idx, 0, 4), 10);
    }
}
