using UnityEngine;

namespace Tank90
{
    /// <summary>
    /// Grid-aligned tank movement (kinematic + shapecast blocking, classic Battle City feel).
    /// Swaps directional sprites and animates treads while moving. Self-corrects if it ever
    /// wedges into a wall so the player can never get permanently stuck.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
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
        Transform vis;          // child holding the sprite; rotated to face a direction
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
            vis = transform.Find("Visual");
            sr = vis != null ? vis.GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
            if (vis == null && sr != null) vis = sr.transform;
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
            float step = speed * Time.fixedDeltaTime;

            // Free movement: face the pressed direction instantly and move exactly that way. No grid
            // snapping, no alignment correction — the tank goes precisely where you point and walls
            // simply block it. (Entering a 1-cell corridor needs the player to line up themselves.)
            if (d != Facing) { Facing = d; UpdateSprite(); }

            IsMoving = TryMove(d.ToVector(), step) > 0.0001f;
        }

        // How far the tank can travel along dir before hitting a wall/tank, WITHOUT moving it.
        float CastAllowed(Vector2 dir, float dist)
        {
            if (dist <= 0f) return 0f;
            int n = rb.Cast(dir, filter, hits, dist);
            float allowed = dist;
            for (int i = 0; i < n; i++)
            {
                // A tank flush against a wall makes rb.Cast report that wall at distance 0 in EVERY
                // direction — which would freeze it even when driving AWAY. Only treat a hit as
                // blocking if its surface actually faces against our motion (normal opposes dir).
                if (Vector2.Dot(hits[i].normal, dir) > -0.5f) continue;
                if (hits[i].distance < allowed) allowed = hits[i].distance;
            }
            return allowed < 0f ? 0f : allowed;
        }

        // Move up to dist along dir, stopping short of walls/tanks. Returns the distance actually moved.
        float TryMove(Vector2 dir, float dist)
        {
            float allowed = CastAllowed(dir, dist);
            if (allowed > 0.0001f) rb.MovePosition(rb.position + dir * allowed);
            return allowed;
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

        // One up-facing sprite (2-frame tread anim) rotated to the facing direction. Because every
        // direction is the same image rotated about its centre, all four are perfectly centred on the
        // collider — no per-direction art offset that could make the tank look/feel wedged in a wall.
        void UpdateSprite()
        {
            sr.sprite = SpriteLibrary.Tank(blockBaseCol, blockBaseRow + designRow, Direction.Up, frame);
            if (vis != null) vis.localRotation = Quaternion.Euler(0f, 0f, Facing.ToZAngle());
        }

        public void RefreshSprite() => UpdateSprite();

        /// <summary>The tank's sprite renderer (on the rotating Visual child) for tinting.</summary>
        public SpriteRenderer Renderer => sr;
    }
}
