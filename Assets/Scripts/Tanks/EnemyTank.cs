using UnityEngine;

namespace Tank90
{
    public enum EnemyType { Basic = 0, Fast = 1, Power = 2, Armor = 3 }

    /// <summary>Enemy tank: type stats, armor health, bonus blink + power-up drop, scoring.</summary>
    public class EnemyTank : Tank
    {
        public EnemyType type = EnemyType.Basic;
        public bool bonus = false;     // flashing enemy that drops a power-up
        public int points = 100;
        public int health = 1;

        SpriteRenderer sr;
        float blinkTimer;
        bool blinkOn;

        protected override void Awake()
        {
            base.Awake();
            team = Team.Enemy;
            sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>Configure this enemy from its type. Call right after creation.</summary>
        public void Setup(EnemyType t, bool isBonus)
        {
            type = t;
            bonus = isBonus;
            switch (t)
            {
                case EnemyType.Basic: motor.speed = 1.8f; bulletSpeed = 8f; fireCooldown = 0.9f; maxBullets = 1; health = 1; points = 100; break;
                case EnemyType.Fast:  motor.speed = 3.6f; bulletSpeed = 8f; fireCooldown = 0.9f; maxBullets = 1; health = 1; points = 200; break;
                case EnemyType.Power: motor.speed = 2.2f; bulletSpeed = 13f; fireCooldown = 0.5f; maxBullets = 1; health = 1; points = 300; break;
                case EnemyType.Armor: motor.speed = 2.0f; bulletSpeed = 9f; fireCooldown = 0.8f; maxBullets = 1; health = 4; points = 400; break;
            }
            motor.designRow = (int)t;        // enemy block rows 0-3 = Basic/Fast/Power/Armor
            motor.RefreshSprite();
            ApplyTint();
        }

        void Update()
        {
            if (!bonus) return;
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= 0.12f)
            {
                blinkTimer = 0f;
                blinkOn = !blinkOn;
                sr.color = blinkOn ? new Color(1f, 0.5f, 0.5f) : Color.white;
            }
        }

        public override bool TakeHit()
        {
            if (invulnerable) return false;
            health--;
            if (health > 0) { ApplyTint(); return false; }
            Die();
            return true;
        }

        // Armor tank fades through colours as it loses health.
        void ApplyTint()
        {
            if (bonus) return; // bonus blink owns the colour
            if (type == EnemyType.Armor)
            {
                switch (health)
                {
                    case 4: sr.color = Color.white; break;
                    case 3: sr.color = new Color(0.7f, 0.9f, 1f); break;
                    case 2: sr.color = new Color(1f, 0.85f, 0.5f); break;
                    default: sr.color = new Color(1f, 0.6f, 0.6f); break;
                }
            }
            else sr.color = Color.white;
        }

        /// <summary>Force-destroy this enemy (grenade power-up).</summary>
        public void ForceKill() => Die();

        protected override void Die()
        {
            Effects.BigExplosion(transform.position);
            Effects.ScorePopup(transform.position, points);
            if (GameManager.Instance != null) GameManager.Instance.AddScore(points);
            if (bonus) PowerUp.SpawnRandom();
            RaiseDestroyed();
            Destroy(gameObject);
        }
    }
}
