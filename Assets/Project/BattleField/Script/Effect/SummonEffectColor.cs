using Common.MasterData;
using UnityEngine;

namespace BattleField.Script.Effect
{
    // カードが出るときの光のエフェクト(PlEffect / EnEffect)の色と形を、カードの属性に合わせる。
    // エフェクトは Timeline が表示するので、Timeline を始める前に設定だけ変えておく。
    //   火: 細く立ち上る炎 / 水: 渦を巻いて昇る水流 / 草: ふわふわ舞う葉 / その他: 外へはじける光
    // エフェクトは X に -90 度回してあるので、ローカルの Z が真上になる。
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
                    ApplyShape(particles, type);
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

        // 前の召喚の形が残らないよう、属性ごとに毎回すべて設定し直す
        private static void ApplyShape(ParticleSystem particles, CardType type)
        {
            var main = particles.main;
            var shape = particles.shape;
            var emission = particles.emission;
            var velocity = particles.velocityOverLifetime;
            var noise = particles.noise;
            var size = particles.sizeOverLifetime;
            var trails = particles.trails;

            shape.enabled = true;
            shape.radiusThickness = 1f;
            shape.arc = 360f;
            velocity.enabled = false;
            velocity.space = ParticleSystemSimulationSpace.Local;
            noise.enabled = false;
            size.enabled = true;

            switch (type)
            {
                case CardType.Fire:
                    // 細い円すいから勢いよく昇り、揺らぎながら小さくなって消える
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = 8f;
                    shape.radius = 0.5f;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
                    main.gravityModifier = -0.3f;
                    emission.rateOverTime = 90f;
                    noise.enabled = true;
                    noise.strength = 0.4f;
                    noise.frequency = 2f;
                    noise.scrollSpeed = 1.5f;
                    size.size = Curve((0f, 1f), (1f, 0f));
                    SetTrails(trails, true, 0.25f);
                    break;

                case CardType.Water:
                    // 輪の上から出て、カードの周りを回りながら昇る
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = 1f;
                    shape.radiusThickness = 0f;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
                    main.startSpeed = 0f;
                    main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
                    main.gravityModifier = 0f;
                    emission.rateOverTime = 70f;
                    SetVelocity(velocity, up: 0.6f, swirl: 3f);
                    size.size = Curve((0f, 0.6f), (0.3f, 1f), (1f, 0.3f));
                    SetTrails(trails, true, 0.6f);
                    break;

                case CardType.Grass:
                    // 周りに広く出て、ゆっくり回りながらふわふわ漂う(尾は付けない)
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 1.2f;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
                    main.gravityModifier = 0f;
                    emission.rateOverTime = 45f;
                    SetVelocity(velocity, up: 0.25f, swirl: 1.2f);
                    noise.enabled = true;
                    noise.strength = 0.5f;
                    noise.frequency = 0.6f;
                    noise.scrollSpeed = 0.5f;
                    size.size = Curve((0f, 0.3f), (0.2f, 1f), (1f, 0.6f));
                    SetTrails(trails, false, 0f);
                    break;

                default:
                    // 中心から全方向へはじけ、尾を引いて小さくなる
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.2f;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 3.5f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
                    main.gravityModifier = 0f;
                    emission.rateOverTime = 80f;
                    size.size = Curve((0f, 1f), (1f, 0f));
                    SetTrails(trails, true, 0.4f);
                    break;
            }
        }

        // up は真上への速さ、swirl は真上の軸の周りを回る速さ
        private static void SetVelocity(ParticleSystem.VelocityOverLifetimeModule velocity, float up, float swirl)
        {
            velocity.enabled = true;
            velocity.x = 0f;
            velocity.y = 0f;
            velocity.z = up;
            velocity.orbitalX = 0f;
            velocity.orbitalY = 0f;
            velocity.orbitalZ = swirl;
        }

        private static void SetTrails(ParticleSystem.TrailModule trails, bool enabled, float lifetime)
        {
            trails.enabled = enabled;
            if (enabled) trails.lifetime = lifetime;
        }

        private static ParticleSystem.MinMaxCurve Curve(params (float time, float value)[] keys)
        {
            var curve = new AnimationCurve();
            foreach (var (time, value) in keys) curve.AddKey(time, value);
            return new ParticleSystem.MinMaxCurve(1f, curve);
        }
    }
}
