using UnityEngine;

namespace Tank90
{
    /// <summary>A 0.5-unit terrain tile. Brick is destroyed by any bullet; steel only by powered bullets.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DestructibleTile : MonoBehaviour
    {
        public bool steel = false;

        public void Hit(bool powered)
        {
            if (steel && !powered) return; // shrug off normal bullets
            Destroy(gameObject);
        }
    }
}
