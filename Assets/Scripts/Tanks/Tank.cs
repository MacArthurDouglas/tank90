using System;
using System.Collections;
using UnityEngine;

namespace Tank90
{
    public enum Team { Player, Enemy }

    /// <summary>Identity, health and firing for a tank. Movement lives in TankMotor.</summary>
    [RequireComponent(typeof(TankMotor))]
    public class Tank : MonoBehaviour
    {
        public Team team = Team.Player;
        public int maxBullets = 1;
        public bool poweredBullets = false;          // can break steel
        public float fireCooldown = 0.35f;
        public float bulletSpeed = GameConfig.BulletSpeed;
        public bool invulnerable = false;

        /// <summary>Raised when this tank is destroyed (after the explosion is spawned).</summary>
        public event Action<Tank> Destroyed;

        protected TankMotor motor;
        float lastFire = -99f;
        int liveBullets = 0;
        GameObject shieldGo;
        Coroutine shieldCo;

        public TankMotor Motor => motor;

        protected virtual void Awake()
        {
            motor = GetComponent<TankMotor>();
        }

        public bool CanFire => liveBullets < maxBullets && Time.time - lastFire >= fireCooldown;

        public void Fire()
        {
            if (!CanFire) return;
            lastFire = Time.time;
            liveBullets++;

            var go = new GameObject("Bullet");
            go.tag = GameConfig.TagBullet;
            go.layer = LayerMask.NameToLayer("Bullet");
            float ahead = 0.55f;
            go.transform.position = transform.position + (Vector3)(motor.Facing.ToVector() * ahead);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = GameConfig.OrderBullet;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.25f, 0.25f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;

            var b = go.AddComponent<Bullet>();
            b.speed = bulletSpeed;
            b.Init(motor.Facing, team, poweredBullets, () => { liveBullets = Mathf.Max(0, liveBullets - 1); });
        }

        /// <summary>Return true if the tank was destroyed by this hit.</summary>
        public virtual bool TakeHit()
        {
            if (invulnerable) return false; // shield absorbs the shot
            Die();
            return true;
        }

        protected virtual void Die()
        {
            Effects.BigExplosion(transform.position);
            RaiseDestroyed();
            Destroy(gameObject);
        }

        protected void RaiseDestroyed() => Destroyed?.Invoke(this);

        // --- Shield ---------------------------------------------------------
        public void AddShield(float seconds)
        {
            invulnerable = true;
            if (shieldGo == null)
            {
                shieldGo = new GameObject("Shield");
                shieldGo.transform.SetParent(transform, false);
                var sr = shieldGo.AddComponent<SpriteRenderer>();
                sr.sortingOrder = GameConfig.OrderEffect;
                var a = shieldGo.AddComponent<SpriteAnim>();
                a.frames = new[] { SpriteLibrary.Shield(0), SpriteLibrary.Shield(1) };
                a.frameTime = 0.05f; a.loop = true; a.destroyOnEnd = false;
            }
            shieldGo.SetActive(true);
            if (shieldCo != null) StopCoroutine(shieldCo);
            shieldCo = StartCoroutine(ShieldTimer(seconds));
        }

        IEnumerator ShieldTimer(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            invulnerable = false;
            if (shieldGo != null) shieldGo.SetActive(false);
            shieldCo = null;
        }
    }
}
