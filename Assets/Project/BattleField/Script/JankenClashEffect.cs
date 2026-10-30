#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Common.MasterData;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BattleField.Script.Janken
{
    // ジャンケンの手がぶつかる場面の火花と画面揺れ。
    // JankenWin / Lose / Draw の Timeline では、手が 0.4 秒で中央に着いて 0.88 秒まで押し合い、そこで勝負が決まる。
    //   0.4 秒: 大きな火花・白い光・強い揺れ / 押し合いの間: バチバチと小さな火花と細かい揺れ / 0.88 秒: もう一度大きな火花
    // シーンに配置しなくてよいよう、呼び出し時に UI を実行時に組み立てて、終わったら破棄する。
    public class JankenClashEffect : MonoBehaviour
    {
        private const float ClashTime = 0.4f;
        private const float BreakTime = 0.88f;
        private const float CrackleInterval = 0.05f;

        private RectTransform _root = null!;
        private Sprite _sparkSprite = null!;
        private Sprite _glowSprite = null!;
        private Color[] _colors = Array.Empty<Color>();
        private readonly List<UnityEngine.Object> _generated = new();

        public static void Play(Transform playerHand, Transform enemyHand, CardType playerType, CardType enemyType, CancellationToken ct)
        {
            var effect = new GameObject("JankenClashEffect").AddComponent<JankenClashEffect>();
            effect.Run(playerHand, enemyHand, playerType, enemyType, ct).Forget();
        }

        private async UniTaskVoid Run(Transform playerHand, Transform enemyHand, CardType playerType, CardType enemyType, CancellationToken ct)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy());
            ct = linked.Token;
            try
            {
                Build(playerType, enemyType);
                var shakeTarget = FindShakeTarget(playerHand);

                await UniTask.Delay(TimeSpan.FromSeconds(ClashTime), cancellationToken: ct);
                var point = ClashPoint(playerHand, enemyHand);
                Burst(point, 28, 900f, 1600f, ct);
                Glow(point, 520f, 0.3f, ct);
                Flash(0.45f, ct);
                Shake(shakeTarget, 0.25f, 18f, ct).Forget();

                // 押し合いの間、バチバチと火花を散らし続ける
                float elapsed = 0f;
                while (elapsed < BreakTime - ClashTime)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(CrackleInterval), cancellationToken: ct);
                    elapsed += CrackleInterval;
                    point = ClashPoint(playerHand, enemyHand);
                    Burst(point, 4, 400f, 900f, ct);
                    if (UnityEngine.Random.value < 0.35f) Glow(point, 220f, 0.12f, ct);
                    Shake(shakeTarget, CrackleInterval, 5f, ct).Forget();
                }

                point = ClashPoint(playerHand, enemyHand);
                Burst(point, 22, 700f, 1400f, ct);
                Glow(point, 420f, 0.25f, ct);
                await Shake(shakeTarget, 0.2f, 12f, ct);

                // 飛んでいる火花が消えるのを待つ
                await UniTask.Delay(TimeSpan.FromSeconds(0.6f), cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (this != null) Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            foreach (var obj in _generated)
            {
                if (obj != null) Destroy(obj);
            }
        }

        // ---------- 組み立て ----------

        private void Build(CardType playerType, CardType enemyType)
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1500;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _root = (RectTransform)transform;

            _sparkSprite = CreateSprite(CreateSparkTexture(64, 8));
            _glowSprite = CreateSprite(CreateGlowTexture(128));

            // 白と黄色を中心に、両者の属性の色を混ぜる
            _colors = new[]
            {
                Color.white, new Color(1f, 0.95f, 0.5f), new Color(1f, 0.8f, 0.2f),
                TypeColor(playerType), TypeColor(enemyType),
            };
        }

        private static Color TypeColor(CardType type)
        {
            return type switch
            {
                CardType.Fire => new Color(1f, 0.35f, 0.1f),
                CardType.Water => new Color(0.3f, 0.75f, 1f),
                CardType.Grass => new Color(0.45f, 1f, 0.35f),
                _ => new Color(1f, 0.85f, 0.3f),
            };
        }

        // 両手の間(スクリーン座標)。手が見つからなければ画面中央
        private static Vector2 ClashPoint(Transform playerHand, Transform enemyHand)
        {
            if (playerHand == null || enemyHand == null) return new Vector2(Screen.width / 2f, Screen.height / 2f);
            return (ToScreen(playerHand) + ToScreen(enemyHand)) / 2f;
        }

        private static Vector2 ToScreen(Transform target)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : canvas == null ? Camera.main : null;
            return RectTransformUtility.WorldToScreenPoint(camera, target.position);
        }

        // 手の親(Timeline で動かされていない入れ物)を揺らす。Canvas そのものは動かせないので、その場合はカメラを揺らす
        private static Transform? FindShakeTarget(Transform playerHand)
        {
            var parent = playerHand != null ? playerHand.parent : null;
            if (parent != null && parent.GetComponent<Canvas>() == null) return parent;
            return Camera.main != null ? Camera.main.transform : null;
        }

        // ---------- 演出 ----------

        private void Burst(Vector2 screenPoint, int count, float minSpeed, float maxSpeed, CancellationToken ct)
        {
            for (int i = 0; i < count; i++)
            {
                var angle = UnityEngine.Random.Range(0f, 360f);
                var speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                var color = _colors[UnityEngine.Random.Range(0, _colors.Length)];
                Spark(screenPoint, angle, speed, color, ct).Forget();
            }
        }

        private async UniTaskVoid Spark(Vector2 screenPoint, float angle, float speed, Color color, CancellationToken ct)
        {
            var image = CreateImage("Spark", _sparkSprite, color);
            var rect = image.rectTransform;
            rect.sizeDelta = new Vector2(UnityEngine.Random.Range(40f, 90f), UnityEngine.Random.Range(4f, 9f));
            rect.position = screenPoint;

            var velocity = (Vector2)(Quaternion.Euler(0f, 0f, angle) * Vector3.right) * speed;
            var life = UnityEngine.Random.Range(0.25f, 0.5f);
            var elapsed = 0f;
            try
            {
                while (elapsed < life)
                {
                    var dt = Time.deltaTime;
                    velocity *= 1f - 4f * dt;          // 空気抵抗で減速
                    velocity.y -= 1800f * dt;          // 少し下に落ちる
                    rect.position += (Vector3)(velocity * dt);
                    rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
                    var t = elapsed / life;
                    image.color = new Color(color.r, color.g, color.b, 1f - t * t);
                    rect.localScale = new Vector3(1f - 0.5f * t, 1f, 1f);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    elapsed += dt;
                }
            }
            finally
            {
                if (image != null) Destroy(image.gameObject);
            }
        }

        private void Glow(Vector2 screenPoint, float size, float duration, CancellationToken ct)
        {
            var image = CreateImage("Glow", _glowSprite, new Color(1f, 0.95f, 0.7f, 1f));
            image.rectTransform.position = screenPoint;
            image.rectTransform.sizeDelta = Vector2.one * size;
            Tween(duration, t =>
            {
                image.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.3f, t);
                image.color = new Color(1f, 0.95f, 0.7f, 1f - t);
            }, () => DestroyImage(image), ct).Forget();
        }

        private static void DestroyImage(Image image)
        {
            if (image != null) Destroy(image.gameObject);
        }

        private void Flash(float alpha, CancellationToken ct)
        {
            var image = CreateImage("Flash", null, new Color(1f, 1f, 1f, alpha));
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Tween(0.18f, t => image.color = new Color(1f, 1f, 1f, alpha * (1f - t)), () => DestroyImage(image), ct).Forget();
        }

        // 揺れが重なってもずれが残らないよう、元の位置は最初に 1 回だけ覚えておく
        private Transform? _shakeTarget;
        private Vector3 _shakeOrigin;

        private async UniTask Shake(Transform? target, float duration, float strength, CancellationToken ct)
        {
            if (target == null) return;
            if (_shakeTarget != target)
            {
                _shakeTarget = target;
                _shakeOrigin = target.localPosition;
            }
            // カメラはワールド単位なので弱めにする
            if (target.GetComponent<Camera>() != null) strength *= 0.01f;

            var origin = _shakeOrigin;
            var elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    var power = strength * (1f - elapsed / duration);
                    target.localPosition = origin + new Vector3(UnityEngine.Random.Range(-power, power), UnityEngine.Random.Range(-power, power), 0f);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    elapsed += Time.deltaTime;
                }
            }
            finally
            {
                if (target != null) target.localPosition = origin;
            }
        }

        private static async UniTaskVoid Tween(float duration, Action<float> step, Action onComplete, CancellationToken ct)
        {
            try
            {
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    step(elapsed / duration);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    elapsed += Time.deltaTime;
                }
            }
            finally
            {
                onComplete();
            }
        }

        // ---------- UI 部品 ----------

        private Image CreateImage(string objectName, Sprite? sprite, Color color)
        {
            var rect = (RectTransform)new GameObject(objectName, typeof(RectTransform)).transform;
            rect.SetParent(_root, false);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Sprite CreateSprite(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            _generated.Add(texture);
            _generated.Add(sprite);
            return sprite;
        }

        // 先端が明るく後ろへ細く消える横長の筋
        private static Texture2D CreateSparkTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < height; y++)
            {
                float v = 1f - Mathf.Abs(y - (height - 1) / 2f) / ((height - 1) / 2f);
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / (width - 1);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(u * u * v * 1.6f)));
                }
            }
            texture.Apply();
            return texture;
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
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(1f - d, 2f)));
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
