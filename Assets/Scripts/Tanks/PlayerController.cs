using UnityEngine;
using UnityEngine.InputSystem;

namespace Tank90
{
    /// <summary>Player input via the new Input System. Arrows/WASD move, Space/J fire. Handles upgrade levels.</summary>
    [RequireComponent(typeof(Tank))]
    [RequireComponent(typeof(TankMotor))]
    public class PlayerController : MonoBehaviour
    {
        public int Level { get; private set; }

        Tank tank;
        TankMotor motor;

        void Awake()
        {
            tank = GetComponent<Tank>();
            motor = GetComponent<TankMotor>();
            ApplyLevel();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            Direction? d = null;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) d = Direction.Up;
            else if (kb.downArrowKey.isPressed || kb.sKey.isPressed) d = Direction.Down;
            else if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) d = Direction.Left;
            else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) d = Direction.Right;

            if (d.HasValue) motor.SetMove(d.Value);
            else motor.StopMove();

            // Hold to keep firing; Tank.Fire() self-limits via fireCooldown + maxBullets.
            if (kb.spaceKey.isPressed || kb.jKey.isPressed)
                tank.Fire();
        }

        public void Upgrade() { Level = Mathf.Min(Level + 1, 3); ApplyLevel(); }
        public void MaxLevel() { Level = 3; ApplyLevel(); }

        void ApplyLevel()
        {
            motor.designRow = Level;
            motor.RefreshSprite();
            tank.maxBullets = Level >= 2 ? 2 : 1;
            tank.poweredBullets = Level >= 3;
            tank.bulletSpeed = Level >= 1 ? 12f : GameConfig.BulletSpeed;
        }
    }
}
