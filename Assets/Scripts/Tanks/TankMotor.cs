using UnityEngine;

namespace Tank90
{
    /// <summary>
    /// Grid-aligned tank movement (kinematic + shapecast blocking, classic Battle City feel).
    /// Swaps directional sprites and animates treads while moving. Self-corrects if it ever
    /// wedges into a wall so the player can never get permanently stuck.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class TankMotor : MonoBehaviour
    {
        [Header("Sprite block (see SPRITE_MAP.md)")]
        public int blockBaseCol = 0;   // 0 = yellow/green, 8 = enemy
        public int blockBaseRow = 0;   // 0 = top colour blocks, 8 = bottom colour blocks
        public int designRow = 0;      // 0-3 player levels, 4-7 enemy types

        public float speed = GameConfig.PlayerMoveSpeed;
        public Direction Facing { get; private set; } = Direction.Up;
        public bool IsMoving { get; private set; }

        Rigidbody2D rb;
        SpriteRenderer sr;
        BoxCollider2D box;
        ContactFilter2D filter;
        readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        int wallMask;
        Vector2 colSize;

        Direction? desired;
        float animTimer;
        int frame;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            box = GetComponent<BoxCollider2D>();
            colSize = box.size;
            wallMask = LayerMask.GetMask("Wall");

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.SetLayerMask(LayerMask.GetMask("Wall", "Tank"));
            filter.useTriggers = false;

            sr.sortingOrder = GameConfig.OrderTank;
            UpdateSprite();
        }

        public void SetMove(Direction d) => desired = d;
        public void StopMove() => desired = null;

        void FixedUpdate()
        {
            Unstick();

            if (!desired.HasValue) { IsMoving = false; return; }

            Direction d = desired.Value;
            if (d != Facing)
            {
                Facing = d;
                AlignPerpendicular(d);
                UpdateSprite();
            }

            float dist = speed * Time.fixedDeltaTime;
            Vector2 dir = d.ToVector();
            int n = rb.Cast(dir, filter, hits, dist);
            float allowed = dist;
            for (int i = 0; i < n; i++)
                if (hits[i].distance < allowed) allowed = hits[i].distance;
            if (allowed < 0f) allowed = 0f;

            if (allowed > 0.0001f)
            {
                rb.MovePosition(rb.position + dir * allowed);
                IsMoving = true;
            }
            else IsMoving = false;
        }

        void Update()
        {
            if (IsMoving)
            {
                animTimer += Time.deltaTime;
                if (animTimer >= 0.08f) { animTimer = 0f; frame ^= 1; UpdateSprite(); }
            }
        }

        // True if the tank's body would overlap a wall at this centre (box shrunk so a flush
        // edge-touch doesn't count as overlap — otherwise a tank parked against a wall would
        // be considered "stuck" forever).
        bool OverlapsWall(Vector2 center, float shrink)
        {
            return Physics2D.OverlapBox(center, colSize * shrink, 0f, wallMask) != null;
        }

        // Snap the off-axis coordinate to the 1-unit cell grid so the tank lines up with corridors.
        // Walls sit on full cells, so cell-aligned movement is what lets tanks fit through gaps.
        void AlignPerpendicular(Direction d)
        {
            Vector2 p = rb.position;
            if (d == Direction.Up || d == Direction.Down) p.x = SnapCell(p.x);
            else p.y = SnapCell(p.y);

            // Only snap if the destination is genuinely clear (real collider footprint), so the
            // align step can never push the body into a wall.
            if (!OverlapsWall(p, 0.95f)) rb.position = p;
        }

        // Safety net: if the tank is somehow overlapping a wall (float drift, a tile spawned/cleared
        // on top of it, a snap edge case), shove it back to the nearest clear spot so it never locks up.
        void Unstick()
        {
            if (!OverlapsWall(rb.position, 0.8f)) return;

            // 1) Try the nearest whole-cell centre.
            Vector2 cur = rb.position;
            Vector2 cell = new Vector2(Mathf.Round(cur.x), Mathf.Round(cur.y));
            if (!OverlapsWall(cell, 0.85f)) { rb.position = cell; return; }

            // 2) Search outward along the four axes for the closest clear cell-aligned position.
            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (float r = 0.25f; r <= 2f; r += 0.25f)
                foreach (var dir in dirs)
                {
                    Vector2 p = cell + dir * r;
                    if (!OverlapsWall(p, 0.85f)) { rb.position = p; return; }
                }
        }

        static float SnapCell(float v) => Mathf.Round(v / GameConfig.CellSize) * GameConfig.CellSize;

        void UpdateSprite()
        {
            sr.sprite = SpriteLibrary.Tank(blockBaseCol, blockBaseRow + designRow, Facing, frame);
        }

        public void RefreshSprite() => UpdateSprite();
    }
}
