using System;
using UnityEngine;

namespace BattleField.Script.Background
{
    // バトルの背景(溶岩島・密林・データ空間など)を、試合ごとにランダムで 1 つ選んで表示する。
    // 背景ごとに「地形やモデルをまとめた親オブジェクト」と、空・霧の設定を登録しておく。
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
        }

        [SerializeField] private BackgroundSet[] backgrounds = Array.Empty<BackgroundSet>();

        [Tooltip("-1 ならランダム。0 以上にすると、その番号の背景に固定する(動作確認用)")]
        [SerializeField] private int forcedIndex = -1;

        private void Awake()
        {
            var usable = 0;
            foreach (var set in backgrounds)
            {
                if (set != null && set.root != null) usable++;
            }
            if (usable == 0) return;

            var selected = SelectIndex();
            for (int i = 0; i < backgrounds.Length; i++)
            {
                var set = backgrounds[i];
                if (set == null || set.root == null) continue;
                set.root.SetActive(i == selected);
            }

            Apply(backgrounds[selected]);
            Debug.Log($"背景: {backgrounds[selected].name}");
        }

        private int SelectIndex()
        {
            if (forcedIndex >= 0 && forcedIndex < backgrounds.Length && backgrounds[forcedIndex]?.root != null)
            {
                return forcedIndex;
            }

            // root が未設定の枠は選ばない
            var candidates = new System.Collections.Generic.List<int>();
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i] != null && backgrounds[i].root != null) candidates.Add(i);
            }
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private static void Apply(BackgroundSet set)
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
        }
    }
}
