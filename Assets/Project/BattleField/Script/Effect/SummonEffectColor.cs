using Common.MasterData;
using UnityEngine;

namespace BattleField.Script.Effect
{
    // カードが出るときの光のエフェクト(PlEffect / EnEffect)の色を、カードの属性に合わせる。
    // エフェクトは Timeline が表示するので、Timeline を始める前に色だけ変えておく。
    public static class SummonEffectColor
    {
        public static void Apply(string effectObjectName, CardType type)
        {
            var (a, b) = GetColors(type);

            // エフェクトは召喚の前は非表示なので、非表示のものも含めて探す
            foreach (var root in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (root.name != effectObjectName || !root.gameObject.scene.IsValid()) continue;

                foreach (var particles in root.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var main = particles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(a, b);
                }
            }
        }

        // 2 色の間でランダムに粒の色が決まる
        private static (Color, Color) GetColors(CardType type)
        {
            return type switch
            {
                CardType.Fire => (new Color(1f, 0.15f, 0.05f), new Color(1f, 0.7f, 0.1f)),
                CardType.Water => (new Color(0f, 0.2f, 1f), new Color(0f, 0.95f, 0.9f)),
                CardType.Grass => (new Color(0.1f, 0.8f, 0.1f), new Color(0.75f, 1f, 0.3f)),
                _ => (new Color(1f, 0.8f, 0.1f), new Color(1f, 1f, 1f)),
            };
        }
    }
}
