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

    public void WinDamaged(int x) {
        _damageText.color = ColorCodeExchanger("#FF2700");
        _damageText.outlineColor = ColorCodeExchanger("#915B00");
        _damageText.text = "相手に\n" + x.ToString() + "ダメージ";
    }

    public void LoseDamaged(int x) {
        _damageText.color = ColorCodeExchanger("#1200FF");
        _damageText.outlineColor = ColorCodeExchanger("#CDCDCD");
        _damageText.text = "味方に\n" + x.ToString() + "ダメージ";
    }

    private Color ColorCodeExchanger(string colorCode) {
        Color color;
        ColorUtility.TryParseHtmlString(colorCode, out color);
        return color;
    }
}
