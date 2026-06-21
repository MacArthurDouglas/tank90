using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tank90
{
    /// <summary>Top-level game flow: title, stage loading, lives/score, spawning, win/lose.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        enum State { Title, Playing, StageClear, GameOver }

        [Header("Tuning")]
        public int startLives = 3;
        public int enemiesPerStage = 20;
        public float respawnDelay = 1.2f;
        public float playerShieldTime = 3f;
        [Tooltip("Seconds a dropped power-up stays on the field before disappearing (0 = never).")]
        public float powerUpLifetime = 15f;

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int StageNumber => stageIndex + 1;
        public Tank Player { get; private set; }
        public bool EnemiesFrozen { get; private set; }

        State state;
        int stageIndex;
        LevelBuilder builder;
        EnemySpawner spawner;
        Hud hud;
        bool playerAlive;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            SpriteLibrary.EnsureLoaded();
            SetupCamera();
            builder = new LevelBuilder();
            spawner = gameObject.AddComponent<EnemySpawner>();
            hud = Hud.Create();
            EnterTitle();
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 7.2f;
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(GameConfig.FieldCenter, GameConfig.FieldCenter, -10f);
        }

        // ---- State: Title -------------------------------------------------
        void EnterTitle()
        {
            state = State.Title;
            hud.SetStats(0, startLives, 1, enemiesPerStage);
            hud.ShowTitle();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (state == State.Title || state == State.GameOver)
            {
                if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    StartNewGame();
                return;
            }

            if (state == State.Playing)
            {
                hud.SetStats(Score, Lives, StageNumber, spawner.RemainingToSpawn + spawner.LiveCount);

                if (playerAlive && Player == null)
                {
                    playerAlive = false;
                    OnPlayerDied();
                }

                if (spawner.StageCleared)
                    StartCoroutine(StageClearRoutine());
            }
        }

        // ---- State: Playing ----------------------------------------------
        void StartNewGame()
        {
            Score = 0;
            Lives = startLives;
            stageIndex = 0;
            hud.HideTitle();
            LoadStage();
        }

        void LoadStage()
        {
            EnemiesFrozen = false;
            CleanupField();
            string[] map = LevelData.Stages[stageIndex % LevelData.Stages.Length];
            builder.Build(map);
            if (builder.Eagle != null) builder.Eagle.OnDestroyed += OnBaseDestroyed;

            spawner.Begin(builder.EnemySpawns, stageIndex);
            SpawnPlayer();
            state = State.Playing;
        }

        // Destroy every transient actor so a new stage / restart never leaves a stray (e.g. a second
        // controllable player tank surviving from a base-destroyed game over).
        void CleanupField()
        {
            spawner.StopAll();
            if (Player != null) { Destroy(Player.gameObject); Player = null; }
            playerAlive = false;
            foreach (var e in FindObjectsByType<EnemyTank>(FindObjectsSortMode.None)) Destroy(e.gameObject);
            foreach (var p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None)) Destroy(p.gameObject);
            foreach (var b in FindObjectsByType<Bullet>(FindObjectsSortMode.None)) Destroy(b.gameObject);
            foreach (var pu in FindObjectsByType<PowerUp>(FindObjectsSortMode.None)) Destroy(pu.gameObject);
        }

        void SpawnPlayer()
        {
            Vector2 pos = builder.PlayerSpawns[0];
            Effects.SpawnStar(pos, null);
            Player = TankFactory.CreatePlayer(pos);
            Player.Destroyed += _ => { }; // player destruction detected via null check in Update
            Player.AddShield(playerShieldTime);
            playerAlive = true;
        }

        void OnPlayerDied()
        {
            Lives--;
            if (Lives <= 0) { StartCoroutine(GameOverRoutine()); return; }
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            if (state == State.Playing) SpawnPlayer();
        }

        IEnumerator StageClearRoutine()
        {
            state = State.StageClear;
            hud.ShowMessage("STAGE " + StageNumber + " CLEAR");
            yield return new WaitForSeconds(2.5f);
            stageIndex++;
            hud.HideMessage();
            LoadStage();
        }

        IEnumerator GameOverRoutine()
        {
            state = State.GameOver;
            spawner.StopAll();
            hud.ShowMessage("GAME OVER\n\nPRESS ENTER");
            yield return new WaitForSeconds(0.5f);
        }

        void OnBaseDestroyed()
        {
            if (state != State.Playing) return;
            StartCoroutine(GameOverRoutine());
        }

        // ---- Power-up effects --------------------------------------------
        public void AddScore(int pts) => Score += pts;
        public void AddLife() => Lives++;

        public void FreezeEnemies(float seconds) => StartCoroutine(FreezeRoutine(seconds));
        IEnumerator FreezeRoutine(float seconds)
        {
            EnemiesFrozen = true;
            yield return new WaitForSeconds(seconds);
            EnemiesFrozen = false;
        }

        public void FortifyBase(float seconds) => StartCoroutine(FortifyRoutine(seconds));
        IEnumerator FortifyRoutine(float seconds)
        {
            builder.FortifyBase(true);
            yield return new WaitForSeconds(seconds);
            builder.FortifyBase(false);
        }

        public void KillAllEnemies() => spawner.KillAllLive();
    }
}
