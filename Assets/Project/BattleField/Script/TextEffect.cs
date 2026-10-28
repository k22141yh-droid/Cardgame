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

    // ダメージ表示の上に相性を出す。見た目をそろえるため、ダメージの文字を複製して使う
    private void ShowMatchup(int matchup) {
        if (matchup == 0) return;

        var label = Instantiate(_damageText, _damageText.transform.parent);
        label.name = "MatchupText";
        label.gameObject.SetActive(true);
        label.text = matchup > 0 ? "ばつぐん！ ×1.25" : "いまひとつ… ×0.75";
        label.color = ColorCodeExchanger(matchup > 0 ? "#FFD400" : "#A9C4DC");
        label.outlineColor = ColorCodeExchanger(matchup > 0 ? "#8A1A00" : "#1E2C4A");
        label.fontSize = _damageText.fontSize * 0.7f;

        var rect = label.rectTransform;
        rect.anchoredPosition = _damageText.rectTransform.anchoredPosition
            + new Vector2(0f, _damageText.rectTransform.rect.height * 0.75f);

        StartCoroutine(PopMatchup(label));
    }

    private IEnumerator PopMatchup(TextMeshProUGUI label) {
        const float popSeconds = 0.25f;
        const float holdSeconds = 1.6f;
        const float fadeSeconds = 0.3f;

        for (float t = 0f; t < popSeconds; t += Time.deltaTime) {
            float k = t / popSeconds;
            label.transform.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, 1f - (1f - k) * (1f - k));
            label.alpha = k;
            yield return null;
        }
        label.transform.localScale = Vector3.one;
        label.alpha = 1f;

        yield return new WaitForSeconds(holdSeconds);

        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime) {
            label.alpha = 1f - t / fadeSeconds;
            yield return null;
        }
        Destroy(label.gameObject);
    }

    private Color ColorCodeExchanger(string colorCode) {
        Color color;
        ColorUtility.TryParseHtmlString(colorCode, out color);
        return color;
    }
}
