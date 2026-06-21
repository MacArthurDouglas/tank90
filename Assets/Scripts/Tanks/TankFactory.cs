using UnityEngine;

namespace Tank90
{
    /// <summary>Builds fully-wired player and enemy tank GameObjects from code.</summary>
    public static class TankFactory
    {
        const float ColliderSize = 0.75f;  // 1-unit cell with generous clearance to pass 1-unit gaps

        static GameObject BaseTank(string name, Vector2 pos, int sortLayer)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Tank");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = GameConfig.OrderTank;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(ColliderSize, ColliderSize);

            go.AddComponent<Rigidbody2D>(); // TankMotor configures it as kinematic
            return go;
        }

        public static Tank CreatePlayer(Vector2 pos)
        {
            var go = BaseTank("Player", pos, GameConfig.OrderTank);
            go.tag = GameConfig.TagPlayer;

            var motor = go.AddComponent<TankMotor>();
            motor.blockBaseCol = 0;   // yellow block
            motor.blockBaseRow = 0;
            motor.designRow = 0;
            motor.speed = GameConfig.PlayerMoveSpeed;

            var tank = go.AddComponent<Tank>();
            tank.team = Team.Player;

            go.AddComponent<PlayerController>();
            return tank;
        }

        public static EnemyTank CreateEnemy(Vector2 pos, EnemyType type, bool bonus)
        {
            var go = BaseTank("Enemy", pos, GameConfig.OrderTank);
            go.tag = GameConfig.TagEnemy;

            var motor = go.AddComponent<TankMotor>();
            motor.blockBaseCol = 8;   // enemy block
            motor.blockBaseRow = 0;
            motor.designRow = (int)type;
            motor.speed = GameConfig.EnemyMoveSpeed;

            var enemy = go.AddComponent<EnemyTank>();
            enemy.Setup(type, bonus);

            go.AddComponent<EnemyAI>();
            return enemy;
        }
    }
}
