using System;
using UnityEngine;

namespace Tank90
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        public Direction dir;
        public Team team;
        public bool powered;
        public float speed = GameConfig.BulletSpeed;

        Rigidbody2D rb;
        Collider2D col;
        Action onDestroyed;
        bool dead;
        bool spawnChecked;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        public void Init(Direction d, Team t, bool pow, Action onDestroyedCb)
        {
            dir = d; team = t; powered = pow; onDestroyed = onDestroyedCb;
            GetComponent<SpriteRenderer>().sprite = SpriteLibrary.Bullet(d);
        }

        void FixedUpdate()
        {
            // A bullet can spawn already overlapping a wall (tank firing flush against it);
            // OnTriggerEnter2D never fires for pre-existing overlaps, so scan once on the first step.
            if (!spawnChecked)
            {
                spawnChecked = true;
                if (ScanSpawnOverlap()) return;
            }
            rb.MovePosition(rb.position + (Vector2)dir.ToVector() * speed * Time.fixedDeltaTime);

            // Safety net: a bullet that somehow misses every wall must still die so it frees the tank's slot.
            Vector2 p = rb.position;
            if (p.x < GameConfig.FieldMin - 1f || p.x > GameConfig.FieldMax + 1f ||
                p.y < GameConfig.FieldMin - 1f || p.y > GameConfig.FieldMax + 1f)
                Kill();
        }

        bool ScanSpawnOverlap()
        {
            var results = new Collider2D[8];
            var f = new ContactFilter2D { useTriggers = true };
            f.useLayerMask = false;
            int n = col.Overlap(f, results);
            for (int i = 0; i < n; i++)
                if (HandleHit(results[i])) return true;
            return false;
        }

        void OnTriggerEnter2D(Collider2D other) => HandleHit(other);

        /// <summary>Returns true if the bullet was consumed.</summary>
        bool HandleHit(Collider2D other)
        {
            if (dead || other == null) return true;

            var ob = other.GetComponent<Bullet>();
            if (ob != null)
            {
                if (ob.team != team) { ob.Kill(); Kill(); return true; }
                return false;
            }

            // Bullets fly over water (tanks are blocked by it via the Wall layer).
            if (other.GetComponent<WaterTile>() != null) return false;

            var tile = other.GetComponent<DestructibleTile>();
            if (tile != null)
            {
                tile.Hit(powered);
                Kill();
                return true;
            }

            var tank = other.GetComponent<Tank>();
            if (tank != null)
            {
                if (tank.team != team) { tank.TakeHit(); Kill(); return true; }
                return false;
            }

            var bas = other.GetComponent<BaseEagle>();
            if (bas != null) { bas.DestroyBase(); Kill(); return true; }

            if (other.gameObject.layer == LayerMask.NameToLayer("Wall")) { Kill(); return true; }
            return false;
        }

        public void Kill()
        {
            if (dead) return;
            dead = true;
            Effects.SmallExplosion(transform.position);
            onDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}
