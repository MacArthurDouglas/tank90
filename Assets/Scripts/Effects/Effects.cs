using UnityEngine;

namespace Tank90
{
    /// <summary>Spawns transient visual effects (explosions, score popups).</summary>
    public static class Effects
    {
        static GameObject MakeSprite(string name, Vector3 pos, int order)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return go;
        }

        /// <summary>Small 3-frame burst (bullet hitting a wall).</summary>
        public static void SmallExplosion(Vector3 pos)
        {
            var go = MakeSprite("Explosion", pos, GameConfig.OrderEffect);
            var a = go.AddComponent<SpriteAnim>();
            a.frames = new[]
            {
                SpriteLibrary.SmallExplosion(0),
                SpriteLibrary.SmallExplosion(1),
                SpriteLibrary.SmallExplosion(2),
            };
            a.frameTime = 0.05f;
        }

        /// <summary>Big burst for a destroyed tank (small frames + scaled blast).</summary>
        public static void BigExplosion(Vector3 pos)
        {
            var go = MakeSprite("BigExplosion", pos, GameConfig.OrderEffect);
            var a = go.AddComponent<SpriteAnim>();
            a.frames = new[]
            {
                SpriteLibrary.SmallExplosion(0),
                SpriteLibrary.SmallExplosion(1),
                SpriteLibrary.SmallExplosion(2),
                SpriteLibrary.SmallExplosion(2),
                SpriteLibrary.SmallExplosion(1),
            };
            a.frameTime = 0.06f;
            go.transform.localScale = new Vector3(2f, 2f, 1f);
        }

        /// <summary>4-frame spawn star shown before a tank materialises.</summary>
        public static GameObject SpawnStar(Vector3 pos, System.Action onDone)
        {
            var go = MakeSprite("SpawnStar", pos, GameConfig.OrderEffect);
            var a = go.AddComponent<SpriteAnim>();
            a.frames = new[]
            {
                SpriteLibrary.SpawnStar(0), SpriteLibrary.SpawnStar(1),
                SpriteLibrary.SpawnStar(2), SpriteLibrary.SpawnStar(3),
                SpriteLibrary.SpawnStar(2), SpriteLibrary.SpawnStar(1),
                SpriteLibrary.SpawnStar(0),
            };
            a.frameTime = 0.08f;
            a.onEnd = onDone;
            return go;
        }

        /// <summary>Floating score sprite. value is 100..500.</summary>
        public static void ScorePopup(Vector3 pos, int value)
        {
            int idx = Mathf.Clamp(value / 100 - 1, 0, 4);
            var go = MakeSprite("Score", pos, GameConfig.OrderEffect);
            go.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.Score(idx);
            Object.Destroy(go, 0.6f);
        }
    }
}
