using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Project.Common.Program.Scripts.UIAnimation
{
    public class Fade : MonoBehaviour
    {
        public float speed = 0.5f;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RawImage img;
        private float time;

        private void FixedUpdate()
        {
            text.color = GetAlphaColor(text.color);
            img.color = GetAlphaColor(img.color);
        }

        Color GetAlphaColor(Color color)
        {
            time += Time.deltaTime * 3.0f * speed;
            color.a = Mathf.Sin(time);

            return color;
        }
    }
}
