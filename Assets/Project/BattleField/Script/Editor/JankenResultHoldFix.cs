using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace BattleField.Script.Editor
{
    // ジャンケンの「Win / Lose / Draw」の文字が約 0.5 秒で消えてしまうのを、長めに残す。
    // 1.65 秒前後で手・名前札・文字がまとめて消え、1.65〜1.82 秒で背景(JankenPose)が上へはけるので、
    // それ以降の出来事をすべて後ろへずらし、元より HoldExtraSeconds 長く表示する。
    // メニュー「Tools > Cardgame > ジャンケンの勝敗表示を長めに残す」から実行する。
    // 何度実行しても「元 + HoldExtraSeconds」になるので、秒数を変えたら実行し直せばよい。
    public static class JankenResultHoldFix
    {
        private static readonly string[] TimelinePaths =
        {
            "Assets/Project/BattleField/Timeline/JankenWin.playable",
            "Assets/Project/BattleField/Timeline/JankenLose.playable",
            "Assets/Project/BattleField/Timeline/JankenDraw.playable",
        };

        private const double HoldExtraSeconds = 0.7;

        // この時刻以降に終わるクリップ・キーを「勝敗表示のあと」とみなして後ろへずらす
        private const double ResultEndTime = 1.6;

        // 勝敗の文字を出すトラック。今の終了時刻から、あと何秒ずらせばよいかを求める
        private const string ResultTextTrackName = "Activation Track (5)";
        private const double OriginalResultEnd = 1.65;

        [MenuItem("Tools/Cardgame/ジャンケンの勝敗表示を長めに残す")]
        public static void Apply()
        {
            foreach (var path in TimelinePaths)
            {
                var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
                if (timeline == null)
                {
                    Debug.LogError($"Timeline が見つかりません: {path}");
                    continue;
                }

                var resultTrack = timeline.GetOutputTracks().FirstOrDefault(t => t.name == ResultTextTrackName);
                var resultClip = resultTrack?.GetClips().FirstOrDefault();
                if (resultClip == null || resultClip.end < ResultEndTime)
                {
                    Debug.LogWarning($"{path} は想定と違う形なので、そのままにします");
                    continue;
                }

                double shift = OriginalResultEnd + HoldExtraSeconds - resultClip.end;
                if (System.Math.Abs(shift) < 0.001)
                {
                    Debug.Log($"{path} はすでに {HoldExtraSeconds} 秒延ばしてあります");
                    continue;
                }

                foreach (var track in timeline.GetOutputTracks())
                {
                    // 手・名前札・勝敗の文字・背景の表示期間を延ばす
                    if (track is ActivationTrack)
                    {
                        foreach (var clip in track.GetClips())
                        {
                            if (clip.end >= ResultEndTime) clip.duration += shift;
                        }
                    }

                    // 背景が上へはけるキーなど、勝敗表示のあとのキーを後ろへずらす
                    if (track is AnimationTrack animationTrack && animationTrack.infiniteClip != null)
                    {
                        ShiftLateKeys(animationTrack.infiniteClip, shift);
                    }
                }

                EditorUtility.SetDirty(timeline);
                Debug.Log($"{path} の勝敗表示を元より {HoldExtraSeconds} 秒長くしました");
            }

            AssetDatabase.SaveAssets();
        }

        private static void ShiftLateKeys(AnimationClip clip, double shift)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var keys = curve.keys;
                bool changed = false;

                // 勝敗表示のあとのキーだけを同じ量ずらすので、キーの順番は変わらない
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].time < ResultEndTime) continue;
                    keys[i].time += (float)shift;
                    changed = true;
                }

                if (!changed) continue;
                curve.keys = keys;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
            EditorUtility.SetDirty(clip);
        }
    }
}
