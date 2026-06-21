using System;
using UnityEngine;

namespace Tank90
{
    /// <summary>Plays a frame sequence on a SpriteRenderer, optionally looping or self-destructing.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnim : MonoBehaviour
    {
        public Sprite[] frames;
        public float frameTime = 0.07f;
        public bool loop = false;
        public bool destroyOnEnd = true;

        SpriteRenderer sr;
        float t;
        int idx;

        public Action onEnd;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        void OnEnable()
        {
            idx = 0; t = 0f;
            if (frames != null && frames.Length > 0) sr.sprite = frames[0];
        }

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            t += Time.deltaTime;
            if (t < frameTime) return;
            t -= frameTime;
            idx++;
            if (idx >= frames.Length)
            {
                if (loop) { idx = 0; }
                else
                {
                    onEnd?.Invoke();
                    if (destroyOnEnd) Destroy(gameObject);
                    return;
                }
            }
            sr.sprite = frames[idx];
        }
    }
}
