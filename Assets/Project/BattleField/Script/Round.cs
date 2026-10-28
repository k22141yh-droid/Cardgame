using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Timeline;
using UnityEngine.Playables;
public class Round : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]private TextMeshProUGUI _round;
    private int _summonCount = 0;
    private string _fullText = "ROUND";
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Timeline のシグナルから 6 回呼ばれ、ラウンド表示を右から少しずつ消していく。
    // 文字数に関係なく 6 回で消えきるよう、残す文字数を割合で決める
    public void RoundString() {
        int length = _fullText.Length;
        int keep = length * (5 - _summonCount) / 6;
        _round.text = _fullText.Substring(0, Mathf.Max(keep, 0));
        _summonCount++;
        if (_summonCount > 5) _summonCount = 0;
    }

    public void RoundCount(int turn, bool isFinalRound = false) {
        _fullText = isFinalRound ? "FINAL ROUND" : "ROUND" + turn;
        _summonCount = 0;

        // 「FINAL ROUND」は長いので、折り返さずに枠の幅へ収める
        _round.enableWordWrapping = !isFinalRound;
        _round.enableAutoSizing = isFinalRound;
        _round.text = _fullText;
    }


    
    

}
