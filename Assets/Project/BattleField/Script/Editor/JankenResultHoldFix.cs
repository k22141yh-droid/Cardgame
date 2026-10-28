using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace BattleField.Script.Editor
{
    // ジャンケンの「Win / Lose / Draw」の文字が約 0.5 秒で消えてしまうのを、長めに残す。
    // 1.65 秒前後で手・名前札・文字がまとめて消え、1.65〜1.82 秒で背景(JankenPose)が上へはけるので、
    // それ以降の出来事をすべて HoldExtraSeconds だけ後ろへずらす。
    // メニュー「Tools > Cardgame > ジャンケンの勝敗表示を長めに残す」から 1 回実行する。
    public static class JankenResultHoldFix
    {
        private static readonly string[] TimelinePaths =
        {
            "Assets/Project/BattleField/Timeline/JankenWin.playable",
            "Assets/Project/BattleField/Timeline/JankenLose.playable",
            "Assets/Project/BattleField/Timeline/JankenDraw.playable",
        };

        private const double HoldExtraSeconds = 1.0;

        // この時刻以降に終わるクリップ・キーを「勝敗表示のあと」とみなして後ろへずらす
        private const double ResultEndTime = 1.6;

        // 勝敗の文字を出すトラック。今の終了時刻で、すでに延ばしたかどうかを判断する
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
                if (resultClip == null || System.Math.Abs(resultClip.end - OriginalResultEnd) > 0.05)
                {
                    Debug.LogWarning($"{path} は変更済みか想定と違う形なので、そのままにします");
                    continue;
                }

                foreach (var track in timeline.GetOutputTracks())
                {
                    // 手・名前札・勝敗の文字・背景の表示期間を延ばす
                    if (track is ActivationTrack)
                    {
                        foreach (var clip in track.GetClips())
                        {
                            if (clip.end >= ResultEndTime) clip.duration += HoldExtraSeconds;
                        }
                    }

                    // 背景が上へはけるキーなど、勝敗表示のあとのキーを後ろへずらす
                    if (track is AnimationTrack animationTrack && animationTrack.infiniteClip != null)
                    {
                        ShiftLateKeys(animationTrack.infiniteClip);
                    }
                }

                EditorUtility.SetDirty(timeline);
                Debug.Log($"{path} の勝敗表示を {HoldExtraSeconds} 秒延ばしました");
            }

            AssetDatabase.SaveAssets();
        }

        private static void ShiftLateKeys(AnimationClip clip)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                var keys = curve.keys;
                bool changed = false;

                // 後ろのキーから動かして、途中でキーの順番が入れ替わらないようにする
                for (int i = keys.Length - 1; i >= 0; i--)
                {
                    if (keys[i].time < ResultEndTime) continue;
                    keys[i].time += (float)HoldExtraSeconds;
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
