using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleField.Script.Background
{
    // バトルの背景(草原・溶岩島・森の中・データ空間など)を、試合ごとにランダムで 1 つ選んで表示する。
    // 背景ごとに「地形やモデルをまとめた親オブジェクト」と、空・霧・光の設定を登録しておく。
    public class BackgroundSelector : MonoBehaviour
    {
        [Serializable]
        public class BackgroundSet
        {
            [Tooltip("管理用の名前(例: 溶岩島)")]
            public string name = "";

            [Tooltip("この背景の地形・モデル・ライトなどをまとめた親オブジェクト。選ばれたときだけ表示される")]
            public GameObject root = null;

            [Tooltip("空の見た目(Skybox マテリアル)。空にしておくと今のまま")]
            public Material skybox = null;

            [Tooltip("霧を使うか")]
            public bool useFog = false;
            public Color fogColor = Color.gray;
            [Min(0f)] public float fogDensity = 0.01f;

            [Tooltip("太陽の光(Directional Light)の色と強さを変えるか")]
            public bool overrideLight = false;
            public Color lightColor = Color.white;
            [Min(0f)] public float lightIntensity = 1f;
        }

        [SerializeField] private BackgroundSet[] backgrounds = Array.Empty<BackgroundSet>();

        [Tooltip("背景ごとに色と強さを変える太陽の光")]
        [SerializeField] private Light mainLight = null;

        [Tooltip("-1 ならランダム。0 以上にすると、その番号の背景に固定する(動作確認用)")]
        [SerializeField] private int forcedIndex = -1;

        private void Awake()
        {
            var candidates = new List<int>();
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i] != null && backgrounds[i].root != null) candidates.Add(i);
            }
            if (candidates.Count == 0) return;

            var selected = SelectIndex(candidates);
            foreach (var i in candidates)
            {
                backgrounds[i].root.SetActive(i == selected);
            }

            Apply(backgrounds[selected]);
            Debug.Log($"背景: {backgrounds[selected].name}");
        }

        private int SelectIndex(List<int> candidates)
        {
            if (candidates.Contains(forcedIndex)) return forcedIndex;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private void Apply(BackgroundSet set)
        {
            if (set.skybox != null)
            {
                RenderSettings.skybox = set.skybox;
                DynamicGI.UpdateEnvironment();
            }

            RenderSettings.fog = set.useFog;
            if (set.useFog)
            {
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = set.fogColor;
                RenderSettings.fogDensity = set.fogDensity;
            }

            if (set.overrideLight && mainLight != null)
            {
                mainLight.color = set.lightColor;
                mainLight.intensity = set.lightIntensity;
            }
        }
    }
}
