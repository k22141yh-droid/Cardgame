#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Common.MasterData;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleField.Script.Victory
{
    // 勝利時の「YOU WIN」演出。
    // 光の筋 → 自分が出したキャラクターが右から登場 → 金色の YOU WIN が 1 文字ずつ出る。
    // シーンに配置しなくてよいよう、Play の呼び出し時に UI を実行時に組み立てて、終わったら破棄する。
    public class VictoryEffect : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float HoldSeconds = 1.6f;

        private RectTransform _root = null!;
        private CanvasGroup _group = null!;
        private Sprite _glowSprite = null!;
        private Sprite _streakSprite = null!;
        private readonly List<UnityEngine.Object> _generated = new();
        private bool _streaming;

        public static async UniTask PlayAsync(IReadOnlyList<Card> playerCards, CancellationToken ct)
        {
            var effect = new GameObject("VictoryEffect").AddComponent<VictoryEffect>();
            try
            {
                await effect.Play(playerCards, ct);
            }
            finally
            {
                if (effect != null) Destroy(effect.gameObject);
            }
        }

        private async UniTask Play(IReadOnlyList<Card> playerCards, CancellationToken ct)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy());
            ct = linked.Token;
            BuildCanvas();

            var background = CreateImage("Background", null, new Color(0.02f, 0.06f, 0.22f, 0f));
            Stretch(background.rectTransform);

            var streakLayer = CreateRect("Streaks", _root);
            Stretch(streakLayer);
            _streaming = true;
            RunStreaks(streakLayer, ct).Forget();

            var characters = CreateCharacters(playerCards);
            var (letters, glow) = CreateTitle();

            var flash = CreateImage("Flash", null, new Color(1f, 1f, 1f, 0f));
            Stretch(flash.rectTransform);

            await UniTask.WhenAll(
                Tween(0.25f, t => background.color = WithAlpha(background.color, 0.85f * t), ct),
                Flash(flash, ct),
                SlideInCharacters(characters, ct),
                ShowTitle(letters, glow, ct));

            await UniTask.Delay(TimeSpan.FromSeconds(HoldSeconds), cancellationToken: ct);

            await Tween(0.4f, t => _group.alpha = 1f - t, ct);
            _streaming = false;
        }

        private void OnDestroy()
        {
            foreach (var obj in _generated)
            {
                if (obj != null) Destroy(obj);
            }
        }

        // ---------- 組み立て ----------

        private void BuildCanvas()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            _group = gameObject.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _root = (RectTransform)transform;

            _glowSprite = CreateSprite(CreateGlowTexture(128));
            _streakSprite = CreateSprite(CreateStreakTexture(256, 16));
        }

        private List<RectTransform> CreateCharacters(IReadOnlyList<Card> playerCards)
        {
            var sprites = new List<Sprite>();
            foreach (var card in playerCards)
            {
                var sprite = Resources.Load<Sprite>("Images/" + card.ImageId);
                if (sprite != null) sprites.Add(sprite);
            }

            // (x, y, 大きさ)。後ろの要素ほど手前に描画される
            Vector3[] slots = sprites.Count switch
            {
                1 => new[] { new Vector3(-420f, -40f, 1.05f) },
                2 => new[] { new Vector3(-620f, -30f, 0.9f), new Vector3(-300f, -50f, 0.95f) },
                _ => new[] { new Vector3(-700f, -20f, 0.8f), new Vector3(-170f, -20f, 0.8f), new Vector3(-440f, -60f, 1f) },
            };

            var characters = new List<RectTransform>();
            for (int i = 0; i < sprites.Count && i < slots.Length; i++)
            {
                var slot = slots[i];
                var size = new Vector2(620f, 820f) * slot.z;
                var container = CreateRect("Character" + (i + 1), _root);
                container.sizeDelta = size;
                container.anchoredPosition = new Vector2(slot.x, slot.y);

                // キャラクターの背後に淡い光を置いて背景から浮かせる
                var halo = CreateImage("Halo", _glowSprite, new Color(0.55f, 0.85f, 1f, 0.55f), container);
                halo.rectTransform.sizeDelta = size * 0.9f;

                var image = CreateImage("Image", sprites[i], Color.white, container);
                image.preserveAspect = true;
                image.rectTransform.sizeDelta = size;

                characters.Add(container);
            }
            return characters;
        }

        private (List<TextMeshProUGUI> letters, Image glow) CreateTitle()
        {
            var title = CreateRect("Title", _root);
            title.anchoredPosition = new Vector2(430f, 20f);

            var glow = CreateImage("TitleGlow", _glowSprite, new Color(1f, 0.85f, 0.35f, 0f), title);
            glow.rectTransform.sizeDelta = new Vector2(1100f, 700f);

            var letters = new List<TextMeshProUGUI>();
            letters.AddRange(CreateWord("YOU", title, new Vector2(-60f, 110f)));
            letters.AddRange(CreateWord("WIN", title, new Vector2(60f, -110f)));
            return (letters, glow);
        }

        private List<TextMeshProUGUI> CreateWord(string word, RectTransform parent, Vector2 center)
        {
            const float fontSize = 230f;
            var letters = new List<TextMeshProUGUI>();
            var widths = new List<float>();
            float total = 0f;

            foreach (var c in word)
            {
                var text = CreateLetter(c.ToString(), parent, fontSize);
                float width = text.GetPreferredValues(c.ToString()).x * 0.92f;
                letters.Add(text);
                widths.Add(width);
                total += width;
            }

            float x = center.x - total / 2f;
            for (int i = 0; i < letters.Count; i++)
            {
                letters[i].rectTransform.anchoredPosition = new Vector2(x + widths[i] / 2f, center.y);
                x += widths[i];
            }
            return letters;
        }

        private TextMeshProUGUI CreateLetter(string letter, RectTransform parent, float fontSize)
        {
            var rect = CreateRect("Letter_" + letter, parent);
            rect.sizeDelta = new Vector2(fontSize, fontSize * 1.2f);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = letter;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold | FontStyles.Italic;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.enableVertexGradient = true;
            text.colorGradient = new VertexGradient(
                new Color(1f, 0.97f, 0.65f), new Color(1f, 0.97f, 0.65f),
                new Color(0.95f, 0.55f, 0.05f), new Color(0.95f, 0.55f, 0.05f));
            text.outlineWidth = 0.22f;
            text.outlineColor = new Color32(70, 25, 0, 255);
            text.alpha = 0f;
            return text;
        }

        // ---------- アニメーション ----------

        private static async UniTask Flash(Image flash, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: ct);
            await Tween(0.08f, t => flash.color = WithAlpha(flash.color, 0.9f * t), ct);
            await Tween(0.35f, t => flash.color = WithAlpha(flash.color, 0.9f * (1f - t)), ct);
        }

        private static async UniTask SlideInCharacters(List<RectTransform> characters, CancellationToken ct)
        {
            var tasks = new List<UniTask>();
            for (int i = 0; i < characters.Count; i++)
            {
                tasks.Add(SlideIn(characters[i], 0.2f + 0.12f * i, ct));
            }
            await UniTask.WhenAll(tasks);
        }

        private static async UniTask SlideIn(RectTransform character, float delay, CancellationToken ct)
        {
            var target = character.anchoredPosition;
            var start = target + new Vector2(1500f, 0f);
            character.anchoredPosition = start;

            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
            await Tween(0.45f, t => character.anchoredPosition = Vector2.LerpUnclamped(start, target, EaseOutBack(t)), ct);
        }

        private static async UniTask ShowTitle(List<TextMeshProUGUI> letters, Image glow, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f), cancellationToken: ct);

            var tasks = new List<UniTask>
            {
                Tween(0.5f, t =>
                {
                    glow.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.15f, EaseOutCubic(t));
                    glow.color = WithAlpha(glow.color, Mathf.Lerp(0f, 0.75f, t));
                }, ct),
            };
            for (int i = 0; i < letters.Count; i++)
            {
                tasks.Add(PopLetter(letters[i], 0.07f * i, ct));
            }
            await UniTask.WhenAll(tasks);
        }

        private static async UniTask PopLetter(TextMeshProUGUI letter, float delay, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
            await Tween(0.28f, t =>
            {
                letter.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(2.4f, 1f, EaseOutBack(t));
                letter.alpha = Mathf.Clamp01(t * 2f);
            }, ct);
        }

        // 斜めの光の筋を、演出が終わるまで流し続ける
        private async UniTaskVoid RunStreaks(RectTransform layer, CancellationToken ct)
        {
            const int count = 12;
            const float angle = -12f;
            var streaks = new List<(RectTransform rect, float speed)>();
            for (int i = 0; i < count; i++)
            {
                var color = i % 3 == 0 ? new Color(1f, 1f, 1f, 0.9f) : new Color(0.45f, 0.85f, 1f, 0.75f);
                var image = CreateImage("Streak", _streakSprite, color, layer);
                var rect = image.rectTransform;
                rect.sizeDelta = new Vector2(UnityEngine.Random.Range(500f, 1100f), UnityEngine.Random.Range(4f, 14f));
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                rect.anchoredPosition = new Vector2(UnityEngine.Random.Range(-1800f, 1800f), UnityEngine.Random.Range(-560f, 560f));
                streaks.Add((rect, UnityEngine.Random.Range(2600f, 4200f)));
            }

            var direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * Vector3.right);
            while (_streaming && !ct.IsCancellationRequested)
            {
                foreach (var (rect, speed) in streaks)
                {
                    rect.anchoredPosition += direction * speed * Time.deltaTime;
                    if (rect.anchoredPosition.x > ReferenceWidth)
                    {
                        rect.anchoredPosition = new Vector2(-ReferenceWidth, UnityEngine.Random.Range(-560f, 760f));
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            }
        }

        private static async UniTask Tween(float duration, Action<float> step, CancellationToken ct)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                step(elapsed / duration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                elapsed += Time.deltaTime;
            }
            step(1f);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        // ---------- UI 部品 ----------

        private RectTransform CreateRect(string objectName, RectTransform parent)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private Image CreateImage(string objectName, Sprite? sprite, Color color, RectTransform? parent = null)
        {
            var rect = CreateRect(objectName, parent != null ? parent : _root);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

        private Sprite CreateSprite(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            _generated.Add(texture);
            _generated.Add(sprite);
            return sprite;
        }

        // 中心が明るく外側へ透明になる円
        private static Texture2D CreateGlowTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float half = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Clamp01(new Vector2(x - half, y - half).magnitude / half);
                    float a = Mathf.Pow(1f - d, 2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            texture.Apply();
            return texture;
        }

        // 両端と上下がぼけた横長の帯
        private static Texture2D CreateStreakTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < height; y++)
            {
                float v = 1f - Mathf.Abs(y - (height - 1) / 2f) / ((height - 1) / 2f);
                for (int x = 0; x < width; x++)
                {
                    float u = Mathf.Sin(Mathf.PI * x / (width - 1));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(u * v * 1.4f)));
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
