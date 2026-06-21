using System.Collections.Generic;
using UnityEngine;

namespace Tank90
{
    /// <summary>Instantiates a stage's terrain from its map text and tracks the base + spawn points.</summary>
    public class LevelBuilder
    {
        const int OrderTerrainMid = 5;   // bricks / steel (block layer, same plane as tanks)

        public Transform Root { get; private set; }
        public BaseEagle Eagle { get; private set; }
        public Vector2 BasePos { get; private set; }
        public readonly Vector2[] EnemySpawns =
        {
            new Vector2(0, 12), new Vector2(6, 12), new Vector2(12, 12)
        };
        public readonly Vector2[] PlayerSpawns =
        {
            new Vector2(4, 0), new Vector2(8, 0)
        };

        readonly List<Vector2Int> guardCells = new List<Vector2Int>();

        public void Build(string[] map)
        {
            if (Root != null) Object.Destroy(Root.gameObject);
            Root = new GameObject("Level").transform;
            guardCells.Clear();

            BuildBorders();

            Vector2Int eagleCell = new Vector2Int(6, 0);
            for (int line = 0; line < map.Length; line++)
            {
                string row = map[line];
                int worldRow = (map.Length - 1) - line;
                for (int col = 0; col < row.Length; col++)
                {
                    char c = row[col];
                    if (c == 'E') eagleCell = new Vector2Int(col, worldRow);
                    BuildCell(col, worldRow, c);
                }
            }

            // Base + guard ring (cells around the eagle, clipped to the field).
            BasePos = new Vector2(eagleCell.x, eagleCell.y);
            int[,] ring = { { -1, 0 }, { 1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 } };
            for (int i = 0; i < ring.GetLength(0); i++)
            {
                int gx = eagleCell.x + ring[i, 0];
                int gy = eagleCell.y + ring[i, 1];
                if (gx >= 0 && gx < GameConfig.FieldCells && gy >= 0 && gy < GameConfig.FieldCells)
                    guardCells.Add(new Vector2Int(gx, gy));
            }
        }

        // Invisible solid border just outside the 13x13 field. Stops tanks leaving and kills stray bullets.
        // Inner faces sit at the field edge (+/-0.5), leaving a ~0.1 gap to a flush tank so it never spawns
        // touching the wall (a touching kinematic collider makes rb.Cast return 0 in every direction = frozen).
        void BuildBorders()
        {
            float c = GameConfig.FieldCenter; // 6
            float span = GameConfig.FieldCells + 2; // 15
            AddBorder(new Vector2(-1f, c), new Vector2(1f, span));      // left  (right edge -0.5)
            AddBorder(new Vector2(GameConfig.FieldCells, c), new Vector2(1f, span)); // right (left edge 12.5)
            AddBorder(new Vector2(c, -1f), new Vector2(span, 1f));      // bottom (top edge -0.5)
            AddBorder(new Vector2(c, GameConfig.FieldCells), new Vector2(span, 1f)); // top (bottom edge 12.5)
        }

        void AddBorder(Vector2 center, Vector2 size)
        {
            var go = new GameObject("Border");
            go.transform.SetParent(Root, false);
            go.transform.position = center;
            go.layer = LayerMask.NameToLayer("Wall");
            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        void BuildCell(int col, int row, char c)
        {
            Vector2 center = new Vector2(col, row);
            switch (c)
            {
                case 'B': BuildQuarters(center, false); break;
                case 'S': BuildQuarters(center, true); break;
                case 'W': BuildWater(center); break;
                case 'T': BuildOverlay(center, SpriteLibrary.Trees, GameConfig.OrderTerrainHigh); break;
                case 'I': BuildOverlay(center, SpriteLibrary.Ice, GameConfig.OrderTerrainLow); break;
                case 'E': BuildEagle(center); break;
            }
        }

        void BuildQuarters(Vector2 center, bool steel)
        {
            float q = 0.25f;
            Vector2[] offs = { new Vector2(-q, q), new Vector2(q, q), new Vector2(-q, -q), new Vector2(q, -q) };
            foreach (var o in offs)
            {
                var go = new GameObject(steel ? "Steel" : "Brick");
                go.transform.SetParent(Root, false);
                go.transform.position = center + o;
                go.layer = LayerMask.NameToLayer("Wall");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = steel ? SpriteLibrary.Steel8 : SpriteLibrary.Brick8;
                sr.sortingOrder = OrderTerrainMid;
                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(GameConfig.TileSize, GameConfig.TileSize);
                go.AddComponent<DestructibleTile>().steel = steel;
            }
        }

        void BuildWater(Vector2 center)
        {
            var go = new GameObject("Water");
            go.transform.SetParent(Root, false);
            go.transform.position = center;
            go.layer = LayerMask.NameToLayer("Wall"); // blocks tanks; bullets ignore via WaterTile
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = GameConfig.OrderTerrainLow;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            go.AddComponent<WaterTile>();
            var anim = go.AddComponent<SpriteAnim>();
            anim.frames = new[] { SpriteLibrary.Water0, SpriteLibrary.Water1 };
            anim.frameTime = 0.4f; anim.loop = true; anim.destroyOnEnd = false;
        }

        void BuildOverlay(Vector2 center, Sprite sprite, int order)
        {
            var go = new GameObject("Terrain");
            go.transform.SetParent(Root, false);
            go.transform.position = center;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
        }

        void BuildEagle(Vector2 center)
        {
            var go = new GameObject("Base");
            go.transform.SetParent(Root, false);
            go.transform.position = center;
            go.tag = GameConfig.TagBase;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.BaseEagle;
            sr.sortingOrder = OrderTerrainMid;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            Eagle = go.AddComponent<BaseEagle>();
        }

        // --- Shovel power-up: swap the guard ring between brick and steel ---
        public void FortifyBase(bool steel)
        {
            foreach (var cell in guardCells)
            {
                ClearCell(cell.x, cell.y);
                BuildQuarters(new Vector2(cell.x, cell.y), steel);
            }
        }

        void ClearCell(int col, int row)
        {
            if (Root == null) return;
            Vector2 center = new Vector2(col, row);
            for (int i = Root.childCount - 1; i >= 0; i--)
            {
                Transform t = Root.GetChild(i);
                if (t.GetComponent<DestructibleTile>() == null) continue;
                if (((Vector2)t.position - center).sqrMagnitude <= 0.30f) // within this cell
                    Object.Destroy(t.gameObject);
            }
        }
    }
}
