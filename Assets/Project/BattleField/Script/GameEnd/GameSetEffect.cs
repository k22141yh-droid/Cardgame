#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.BattleField.Script.GameEnd
{
    // 勝敗が決まったときの「GAME SET」演出。
    // 画面中央に黒い帯が開く → GAME SET が大きく叩きつけられる(白く光って少し揺れる)→ 少し見せて消える。
    // シーンに配置しなくてよいよう、呼び出し時に UI を実行時に組み立てて、終わったら破棄する。
    public class GameSetEffect : MonoBehaviour
    {
        private const float HoldSeconds = 1.2f;
        private const float BandHeight = 330f;

        public static async UniTask PlayAsync(CancellationToken ct)
        {
            var effect = new GameObject("GameSetEffect").AddComponent<GameSetEffect>();
            try
            {
                await effect.Play(ct);
            }
            finally
            {
                if (effect != null) Destroy(effect.gameObject);
            }
        }

        private async UniTask Play(CancellationToken ct)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy());
            ct = linked.Token;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var group = gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var root = (RectTransform)transform;

            var dim = CreateImage("Dim", root, new Color(0f, 0f, 0f, 0f));
            Stretch(dim.rectTransform);

            var band = CreateImage("Band", root, new Color(0.02f, 0.03f, 0.08f, 0.85f));
            band.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            band.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            band.rectTransform.sizeDelta = new Vector2(0f, 0f);

            // 帯の上下の光る線
            var lines = new Image[2];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = CreateImage("Line", band.rectTransform, new Color(1f, 0.85f, 0.3f, 1f));
                var rect = lines[i].rectTransform;
                rect.anchorMin = new Vector2(0f, i == 0 ? 1f : 0f);
                rect.anchorMax = new Vector2(1f, i == 0 ? 1f : 0f);
                rect.sizeDelta = new Vector2(0f, 6f);
            }

            var title = CreateTitle(root);

            var flash = CreateImage("Flash", root, new Color(1f, 1f, 1f, 0f));
            Stretch(flash.rectTransform);

            // 暗くしながら帯を開く
            await Tween(0.25f, t =>
            {
                dim.color = new Color(0f, 0f, 0f, 0.35f * t);
                band.rectTransform.sizeDelta = new Vector2(0f, BandHeight * EaseOutCubic(t));
            }, ct);

            // 文字を叩きつける
            await Tween(0.22f, t =>
            {
                title.rectTransform.localScale = Vector3.one * Mathf.Lerp(3f, 1f, EaseInQuad(t));
                title.alpha = Mathf.Clamp01(t * 1.5f);
            }, ct);

            // 着地の瞬間に白く光らせて揺らす
            await UniTask.WhenAll(
                Tween(0.35f, t => flash.color = new Color(1f, 1f, 1f, 0.7f * (1f - t)), ct),
                Shake(title.rectTransform, 0.3f, 22f, ct));

            await UniTask.Delay(TimeSpan.FromSeconds(HoldSeconds), cancellationToken: ct);

            await Tween(0.35f, t => group.alpha = 1f - t, ct);
        }

        private static TextMeshProUGUI CreateTitle(RectTransform parent)
        {
            var rect = CreateRect("Title", parent);
            rect.sizeDelta = new Vector2(1800f, 300f);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = "GAME SET";
            text.fontSize = 210f;
            text.fontStyle = FontStyles.Bold | FontStyles.Italic;
            text.characterSpacing = 6f;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.enableVertexGradient = true;
            text.colorGradient = new VertexGradient(
                Color.white, Color.white,
                new Color(1f, 0.78f, 0.2f), new Color(1f, 0.78f, 0.2f));
            text.outlineWidth = 0.25f;
            text.outlineColor = new Color32(40, 15, 0, 255);
            text.alpha = 0f;
            return text;
        }

        private static async UniTask Shake(RectTransform rect, float duration, float strength, CancellationToken ct)
        {
            var origin = rect.anchoredPosition;
            await Tween(duration, t =>
            {
                var power = strength * (1f - t);
                rect.anchoredPosition = origin + new Vector2(UnityEngine.Random.Range(-power, power), UnityEngine.Random.Range(-power, power));
            }, ct);
            rect.anchoredPosition = origin;
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

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private static float EaseInQuad(float t) => t * t;

        private static RectTransform CreateRect(string objectName, RectTransform parent)
        {
            var rect = (RectTransform)new GameObject(objectName, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(string objectName, RectTransform parent, Color color)
        {
            var image = CreateRect(objectName, parent).gameObject.AddComponent<Image>();
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
    }
}
