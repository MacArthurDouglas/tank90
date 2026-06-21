using System;
using UnityEngine;

namespace Tank90
{
    /// <summary>The player's home base. Destroying it ends the game (wired up in Phase 4).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BaseEagle : MonoBehaviour
    {
        public bool Destroyed { get; private set; }
        public event Action OnDestroyed;

        SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null) sr.sprite = SpriteLibrary.BaseEagle;
        }

        public void DestroyBase()
        {
            if (Destroyed) return;
            Destroyed = true;
            sr.sprite = SpriteLibrary.BaseDead;
            OnDestroyed?.Invoke();
        }
    }
}
