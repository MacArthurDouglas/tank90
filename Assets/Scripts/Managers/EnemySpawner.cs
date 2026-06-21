using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tank90
{
    /// <summary>Feeds enemies onto the field from the three top spawn points, capped at a max concurrent count.</summary>
    public class EnemySpawner : MonoBehaviour
    {
        public int maxConcurrent = 4;
        public float spawnInterval = 2.0f;

        readonly List<EnemyTank> live = new List<EnemyTank>();
        Queue<EnemyType> queue;
        bool[] bonusFlags;
        int totalToSpawn;
        int spawnedCount;
        Vector2[] spawnPoints;
        int spawnPtr;

        public int RemainingToSpawn => totalToSpawn - spawnedCount;
        public int LiveCount => live.Count;
        public bool StageCleared => RemainingToSpawn == 0 && LiveCount == 0;

        public void Begin(Vector2[] points, int stageIndex)
        {
            spawnPoints = points;
            BuildWave(stageIndex);
            totalToSpawn = queue.Count;
            spawnedCount = 0;
            spawnPtr = 0;
            live.Clear();
            StopAllCoroutines();
            StartCoroutine(SpawnLoop());
        }

        public void StopAll()
        {
            StopAllCoroutines();
            foreach (var e in live) if (e != null) Destroy(e.gameObject);
            live.Clear();
        }

        void BuildWave(int stageIndex)
        {
            // 20 enemies, getting tougher on later stages.
            int basic = Mathf.Max(2, 10 - stageIndex * 2);
            int fast = 6;
            int power = 4 + stageIndex;
            int armor = 20 - basic - fast - power;
            if (armor < 0) { armor = 0; basic = 20 - fast - power; }

            var list = new List<EnemyType>();
            for (int i = 0; i < basic; i++) list.Add(EnemyType.Basic);
            for (int i = 0; i < fast; i++) list.Add(EnemyType.Fast);
            for (int i = 0; i < power; i++) list.Add(EnemyType.Power);
            for (int i = 0; i < armor; i++) list.Add(EnemyType.Armor);

            // Shuffle.
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            queue = new Queue<EnemyType>(list);
            bonusFlags = new bool[list.Count];
            foreach (int idx in new[] { 3, 10, 17 })
                if (idx < bonusFlags.Length) bonusFlags[idx] = true;
        }

        IEnumerator SpawnLoop()
        {
            while (spawnedCount < totalToSpawn)
            {
                while (live.Count >= maxConcurrent) yield return null;

                EnemyType type = queue.Dequeue();
                bool bonus = bonusFlags[spawnedCount];
                Vector2 pos = spawnPoints[spawnPtr % spawnPoints.Length];
                spawnPtr++;
                spawnedCount++;

                bool spawned = false;
                Effects.SpawnStar(pos, () => spawned = true);
                while (!spawned) yield return null;

                var enemy = TankFactory.CreateEnemy(pos, type, bonus);
                live.Add(enemy);
                enemy.Destroyed += OnEnemyDestroyed;

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        void OnEnemyDestroyed(Tank t)
        {
            live.Remove(t as EnemyTank);
        }

        /// <summary>Destroy every enemy currently on the field (grenade power-up).</summary>
        public void KillAllLive()
        {
            var copy = new List<EnemyTank>(live);
            foreach (var e in copy) if (e != null) e.ForceKill();
        }
    }
}
