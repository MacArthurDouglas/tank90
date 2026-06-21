using UnityEngine;

namespace Tank90
{
    /// <summary>Classic-style enemy AI: roam with a bias toward the base, turn when blocked, fire periodically.</summary>
    [RequireComponent(typeof(Tank))]
    [RequireComponent(typeof(TankMotor))]
    public class EnemyAI : MonoBehaviour
    {
        Tank tank;
        TankMotor motor;

        float dirTimer;
        float fireTimer;
        float stuckTimer;

        void Awake()
        {
            tank = GetComponent<Tank>();
            motor = GetComponent<TankMotor>();
            ScheduleDirChange();
            fireTimer = Random.Range(0.3f, 1.2f);
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.EnemiesFrozen)
            {
                motor.StopMove();
                return;
            }

            float dt = Time.deltaTime;
            dirTimer -= dt;
            fireTimer -= dt;

            // Turn if the scheduled timer elapsed or we've been wedged against something.
            if (!motor.IsMoving) stuckTimer += dt; else stuckTimer = 0f;
            if (dirTimer <= 0f || stuckTimer > 0.25f)
            {
                PickDirection();
                ScheduleDirChange();
                stuckTimer = 0f;
            }

            if (fireTimer <= 0f)
            {
                tank.Fire();
                fireTimer = Random.Range(0.4f, 1.4f);
            }
        }

        void ScheduleDirChange() => dirTimer = Random.Range(0.4f, 1.6f);

        void PickDirection()
        {
            // Weighted bag: bias downward (toward the eagle), some horizontal, rare up.
            float r = Random.value;
            Direction d;
            if (r < 0.45f) d = Direction.Down;
            else if (r < 0.70f) d = Direction.Left;
            else if (r < 0.95f) d = Direction.Right;
            else d = Direction.Up;

            // Avoid immediately reversing into where we just came from too often.
            if (d == motor.Facing.Opposite() && Random.value < 0.5f)
                d = (Random.value < 0.5f) ? Direction.Left : Direction.Right;

            motor.SetMove(d);
        }
    }
}
