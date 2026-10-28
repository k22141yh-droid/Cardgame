using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace BattleField.Script.Editor
{
    // ジャンケンの手がぶつかる場面で「You」「Enemy」の札が重なるのを直す。
    // 札(PlayerNameBase / EnemyNameBase)は Timeline の録画アニメーションで中央 ±50 まで寄るため、
    // 寄せる位置を外側に広げる。メニュー「Tools > Cardgame > ジャンケンの名前札の重なりを直す」から 1 回実行する。
    public static class JankenNameSpacingFix
    {
        private static readonly string[] TimelinePaths =
        {
            "Assets/Project/BattleField/Timeline/JankenWin.playable",
            "Assets/Project/BattleField/Timeline/JankenLose.playable",
            "Assets/Project/BattleField/Timeline/JankenDraw.playable",
        };

        // シーン側で PlayerNameBase / EnemyNameBase に割り当てられているトラック
        private static readonly string[] NameTrackNames = { "Animation Track (3)", "Animation Track (4)" };

        private const float OldClashX = 50f;   // 今の札の位置(手と同じ)
        private const float NewClashX = 260f;  // 札どうしが重ならない位置
        private const float NewPassX = 220f;   // 飛び込んできた札が一瞬通る位置(元は中央 0)
        private const float PassTime = 0.4f;

        [MenuItem("Tools/Cardgame/ジャンケンの名前札の重なりを直す")]
        public static void Apply()
        {
            int changedCurves = 0;
            foreach (var path in TimelinePaths)
            {
                var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
                if (timeline == null)
                {
                    Debug.LogError($"Timeline が見つかりません: {path}");
                    continue;
                }

                foreach (var track in timeline.GetOutputTracks().OfType<AnimationTrack>())
                {
                    if (!NameTrackNames.Contains(track.name) || track.infiniteClip == null) continue;
                    if (FixClip(track.infiniteClip)) changedCurves++;
                }

                EditorUtility.SetDirty(timeline);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"ジャンケンの名前札の位置を直しました(変更したカーブ: {changedCurves})");
        }

        private static bool FixClip(AnimationClip clip)
        {
            var binding = AnimationUtility.GetCurveBindings(clip)
                .FirstOrDefault(b => b.propertyName == "m_AnchoredPosition.x");
            if (binding.propertyName == null) return false;

            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            var keys = curve.keys;
            bool changed = false;

            // 札が手と一緒に寄る値(±50)と、その直前に中央を通るキーだけを外側へずらす
            float side = keys.Select(k => k.value).FirstOrDefault(v => Mathf.Abs(Mathf.Abs(v) - OldClashX) < 0.5f);
            if (side == 0f) return false;
            side = Mathf.Sign(side);

            for (int i = 0; i < keys.Length; i++)
            {
                if (Mathf.Abs(keys[i].value - side * OldClashX) < 0.5f)
                {
                    keys[i].value = side * NewClashX;
                    changed = true;
                }
                else if (Mathf.Abs(keys[i].time - PassTime) < 0.01f && Mathf.Abs(keys[i].value) < 0.5f)
                {
                    keys[i].value = side * NewPassX;
                    changed = true;
                }
            }

            if (!changed) return false;

            curve.keys = keys;
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            EditorUtility.SetDirty(clip);
            return true;
        }
    }
}
