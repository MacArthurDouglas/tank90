using UnityEngine;
using UnityEngine.UI;

namespace Tank90
{
    /// <summary>Screen-space HUD: score, stage, a centre message, and icon grids for remaining enemies / lives.</summary>
    public class Hud : MonoBehaviour
    {
        const int MaxEnemyIcons = 20;
        const int MaxLifeIcons = 9;
        const float IconSize = 20f;
        const float IconGap = 24f;

        Text scoreText, livesText, stageText, enemyCaption, centerText;
        Image titleImage;
        RectTransform enemyRoot, lifeRoot;
        Image[] enemyIcons, lifeIcons;
        bool blinkPrompt;
        float blinkTimer;

        public static Hud Create()
        {
            var go = new GameObject("HUD");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800, 600);
            go.AddComponent<GraphicRaycaster>();

            var hud = go.AddComponent<Hud>();
            hud.scoreText = hud.MakeText("Score", new Vector2(0, 1), new Vector2(20, -20), TextAnchor.UpperLeft, 26);
            hud.stageText = hud.MakeText("Stage", new Vector2(1, 1), new Vector2(-20, -20), TextAnchor.UpperRight, 26);
            hud.livesText = hud.MakeText("Lives", new Vector2(0, 0), new Vector2(20, 20), TextAnchor.LowerLeft, 26);

            // Remaining-enemy icon grid (2 columns) in the top-right margin, with an "ENEMY" caption.
            hud.enemyCaption = hud.MakeText("EnemyCap", new Vector2(1, 1), new Vector2(-20, -56), TextAnchor.UpperRight, 20);
            hud.enemyCaption.text = "ENEMY";
            hud.BuildEnemyIcons();
            hud.BuildLifeIcons();

            hud.titleImage = hud.MakeTitleImage();
            hud.centerText = hud.MakeText("Center", new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 48);
            hud.centerText.rectTransform.sizeDelta = new Vector2(760, 200);
            return hud;
        }

        void BuildEnemyIcons()
        {
            enemyRoot = MakeRoot("EnemyIcons", new Vector2(1, 1));
            var sprite = SpriteLibrary.Tank(8, 0, Direction.Down, 0); // enemy tank, facing down
            enemyIcons = new Image[MaxEnemyIcons];
            for (int i = 0; i < MaxEnemyIcons; i++)
            {
                int col = i % 2, row = i / 2;
                var pos = new Vector2(-(20 + col * IconGap), -(86 + row * IconGap));
                enemyIcons[i] = MakeIcon(enemyRoot, sprite, pos, new Vector2(1, 1), Color.white);
            }
        }

        void BuildLifeIcons()
        {
            lifeRoot = MakeRoot("LifeIcons", new Vector2(0, 0));
            var sprite = SpriteLibrary.Tank(0, 0, Direction.Up, 0); // player tank, facing up
            lifeIcons = new Image[MaxLifeIcons];
            for (int i = 0; i < MaxLifeIcons; i++)
            {
                var pos = new Vector2(70 + i * IconGap, 26);
                lifeIcons[i] = MakeIcon(lifeRoot, sprite, pos, new Vector2(0, 0), Color.white);
            }
        }

        RectTransform MakeRoot(string name, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        Image MakeIcon(RectTransform parent, Sprite sprite, Vector2 pos, Vector2 anchor, Color color)
        {
            var go = new GameObject("Icon");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = new Vector2(IconSize, IconSize);
            rt.anchoredPosition = pos;
            return img;
        }

        Image MakeTitleImage()
        {
            var go = new GameObject("Title");
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();

            // The asset's bottom half is a sprite-reference chart; crop to just the title region (top half).
            var full = Resources.Load<Sprite>("Miscellaneous");
            var tex = full != null ? full.texture : Resources.Load<Texture2D>("Miscellaneous");
            if (tex != null)
            {
                int yBottom = Mathf.RoundToInt(tex.height * 0.50f); // keep top 50%
                var rect = new Rect(0, yBottom, tex.width, tex.height - yBottom);
                img.sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 16f);
            }
            img.preserveAspect = true;

            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(430, 360);
            rt.anchoredPosition = new Vector2(0, 60);
            go.SetActive(false);
            return img;
        }

        Text MakeText(string name, Vector2 anchor, Vector2 offset, TextAnchor align, int size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;

            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(360, 40);
            return t;
        }

        public void SetStats(int score, int lives, int stage, int remaining)
        {
            scoreText.text = "SCORE  " + score;
            livesText.text = "IP";
            stageText.text = "STAGE  " + stage;

            for (int i = 0; i < enemyIcons.Length; i++)
                enemyIcons[i].enabled = i < remaining;
            for (int i = 0; i < lifeIcons.Length; i++)
                lifeIcons[i].enabled = i < Mathf.Max(0, lives);
        }

        public void ShowStats(bool visible)
        {
            scoreText.enabled = visible;
            livesText.enabled = visible;
            stageText.enabled = visible;
            enemyCaption.enabled = visible;
            enemyRoot.gameObject.SetActive(visible);
            lifeRoot.gameObject.SetActive(visible);
        }

        public void ShowMessage(string msg)
        {
            centerText.text = msg;
            centerText.enabled = true;
        }

        public void HideMessage() => centerText.enabled = false;

        public void ShowTitle()
        {
            if (titleImage != null) titleImage.gameObject.SetActive(true);
            ShowStats(false);
            // "PRESS ENTER" sits below the title art and blinks.
            centerText.rectTransform.anchoredPosition = new Vector2(0, -200);
            centerText.fontSize = 32;
            ShowMessage("PRESS ENTER");
            blinkPrompt = true;
            blinkTimer = 0f;
        }

        public void HideTitle()
        {
            if (titleImage != null) titleImage.gameObject.SetActive(false);
            ShowStats(true);
            centerText.rectTransform.anchoredPosition = Vector2.zero;
            centerText.fontSize = 48;
            blinkPrompt = false;
            HideMessage();
        }

        void Update()
        {
            if (!blinkPrompt) return;
            blinkTimer += Time.unscaledDeltaTime;
            if (blinkTimer >= 0.5f)
            {
                blinkTimer = 0f;
                centerText.enabled = !centerText.enabled;
            }
        }
    }
}
