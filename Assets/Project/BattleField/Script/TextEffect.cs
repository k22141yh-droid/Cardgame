using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class TextEffect : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private TextMeshProUGUI _damageText;
    
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // matchup: 1 = ばつぐん / 0 = ふつう / -1 = いまひとつ(DamageValue.Matchup の値)
    public void WinDamaged(int x, int matchup = 0) {
        _damageText.color = ColorCodeExchanger("#FF2700");
        _damageText.outlineColor = ColorCodeExchanger("#915B00");
        _damageText.text = "相手に\n" + x.ToString() + "ダメージ";
        ShowMatchup(matchup);
    }

    public void LoseDamaged(int x, int matchup = 0) {
        _damageText.color = ColorCodeExchanger("#1200FF");
        _damageText.outlineColor = ColorCodeExchanger("#CDCDCD");
        _damageText.text = "味方に\n" + x.ToString() + "ダメージ";
        ShowMatchup(matchup);
    }

    // ダメージ表示の上に相性を出す。
    // ダメージの Timeline が終わると DamageText ごと非表示になるので、相性の文字は DamageText の子にして一緒に消えるようにする。
    // アニメーションも文字自身に持たせ、DamageText 側のコルーチンが止まっても残らないようにする
    private void ShowMatchup(int matchup) {
        if (matchup == 0) return;

        var go = new GameObject("MatchupText", typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(_damageText.rectTransform, false);
        rect.sizeDelta = new Vector2(_damageText.rectTransform.rect.width * 1.5f, _damageText.rectTransform.rect.height * 0.6f);
        rect.anchoredPosition = new Vector2(0f, _damageText.rectTransform.rect.height * 0.75f);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.font = _damageText.font;
        label.fontSharedMaterial = _damageText.fontSharedMaterial;
        label.fontStyle = _damageText.fontStyle;
        label.fontSize = _damageText.fontSize * 0.7f;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        label.text = matchup > 0 ? "ばつぐん！ ×1.25" : "いまひとつ… ×0.75";
        label.color = ColorCodeExchanger(matchup > 0 ? "#FFD400" : "#A9C4DC");
        label.outlineColor = ColorCodeExchanger(matchup > 0 ? "#8A1A00" : "#1E2C4A");

        go.AddComponent<MatchupPop>();
        Destroy(go, MatchupPop.TotalSeconds);
    }

    // 相性の文字を大きい状態から縮めて出し、しばらくしたら薄くする
    private class MatchupPop : MonoBehaviour {
        private const float PopSeconds = 0.25f;
        private const float HoldSeconds = 1.6f;
        private const float FadeSeconds = 0.3f;
        public const float TotalSeconds = PopSeconds + HoldSeconds + FadeSeconds;

        private TextMeshProUGUI _label;
        private float _elapsed;

        private void Awake() {
            _label = GetComponent<TextMeshProUGUI>();
        }

        private void Update() {
            _elapsed += Time.deltaTime;
            if (_elapsed < PopSeconds) {
                float k = _elapsed / PopSeconds;
                transform.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, 1f - (1f - k) * (1f - k));
                _label.alpha = k;
            } else if (_elapsed < PopSeconds + HoldSeconds) {
                transform.localScale = Vector3.one;
                _label.alpha = 1f;
            } else {
                _label.alpha = Mathf.Clamp01(1f - (_elapsed - PopSeconds - HoldSeconds) / FadeSeconds);
            }
        }
    }

    private Color ColorCodeExchanger(string colorCode) {
        Color color;
        ColorUtility.TryParseHtmlString(colorCode, out color);
        return color;
    }
}
