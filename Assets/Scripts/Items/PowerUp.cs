using UnityEngine;

namespace Tank90
{
    public enum PowerUpType { Helmet = 0, Timer = 1, Shovel = 2, Star = 3, Grenade = 4, Life = 5, Gun = 6 }

    /// <summary>A collectible dropped by flashing enemies. Picked up by the player tank.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PowerUp : MonoBehaviour
    {
        public PowerUpType type;

        SpriteRenderer sr;
        float blink;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            blink += Time.deltaTime;
            if (blink >= 0.25f) { blink = 0f; sr.enabled = !sr.enabled; }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var tank = other.GetComponent<Tank>();
            if (tank == null || tank.team != Team.Player) return;
            Apply(tank);
            Destroy(gameObject);
        }

        void Apply(Tank player)
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.AddScore(500);
            var pc = player.GetComponent<PlayerController>();
            switch (type)
            {
                case PowerUpType.Helmet: player.AddShield(10f); break;
                case PowerUpType.Timer: if (gm != null) gm.FreezeEnemies(8f); break;
                case PowerUpType.Shovel: if (gm != null) gm.FortifyBase(10f); break;
                case PowerUpType.Star: if (pc != null) pc.Upgrade(); break;
                case PowerUpType.Grenade: if (gm != null) gm.KillAllEnemies(); break;
                case PowerUpType.Life: if (gm != null) gm.AddLife(); break;
                case PowerUpType.Gun: if (pc != null) pc.MaxLevel(); break;
            }
        }

        /// <summary>Drop a random power-up at a random interior cell of the field.</summary>
        public static PowerUp SpawnRandom()
        {
            var type = (PowerUpType)Random.Range(0, 7);
            int x = Random.Range(1, GameConfig.FieldCells - 1);
            int y = Random.Range(1, GameConfig.FieldCells - 1);
            return Spawn(type, new Vector2(x, y));
        }

        public static PowerUp Spawn(PowerUpType type, Vector2 pos)
        {
            var go = new GameObject("PowerUp");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.Item((int)type);
            sr.sortingOrder = GameConfig.OrderEffect - 1;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            var pu = go.AddComponent<PowerUp>();
            pu.type = type;
            return pu;
        }
    }
}
