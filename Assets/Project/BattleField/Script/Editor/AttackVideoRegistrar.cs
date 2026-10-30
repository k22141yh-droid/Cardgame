using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleField.Script.Effect;
using Common.MasterData;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Video;

namespace BattleField.Script.Editor
{
    // Timeline/mp4 にある ID〇.mp4 を、card.csv の属性・手・威力と結び付けて、
    // BattleField シーンの全 EffectPresenter(Player / Enemy)の攻撃動画リストに登録し直す。
    // メニュー「Tools > Cardgame > 攻撃動画をまとめて登録する」から実行する(何回実行しても同じ結果になる)。
    public static class AttackVideoRegistrar
    {
        private const string ScenePath = "Assets/Project/BattleField/Scene/BattleField.unity";
        private const string VideoDirectory = "Assets/Project/BattleField/Timeline/mp4";
        private const string CardCsvPath = "Assets/Project/Common/Program/MasterData/Csv/card.csv";

        [MenuItem("Tools/Cardgame/攻撃動画をまとめて登録する")]
        public static void Register()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var entries = BuildEntries(out var registeredIds);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int presenters = 0;
            foreach (var presenter in UnityEngine.Object.FindObjectsOfType<EffectPresenter>(true))
            {
                var so = new SerializedObject(presenter);
                var list = so.FindProperty("_attackVideos");
                list.ClearArray();
                for (int i = 0; i < entries.Count; i++)
                {
                    list.InsertArrayElementAtIndex(i);
                    var element = list.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Type").enumValueIndex = (int)entries[i].Type;
                    element.FindPropertyRelative("Hand").enumValueIndex = (int)entries[i].Hand;
                    element.FindPropertyRelative("Level").intValue = entries[i].Level;
                    element.FindPropertyRelative("Clip").objectReferenceValue = entries[i].Clip;
                }
                so.ApplyModifiedProperties();
                presenters++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            var message = $"{presenters} か所の EffectPresenter に、{entries.Count} 本の動画を登録しました。\n" +
                          $"登録した ID: {string.Join(", ", registeredIds)}";
            Debug.Log(message);
            EditorUtility.DisplayDialog("攻撃動画の登録", message, "OK");
        }

        private static List<AttackVideoEntry> BuildEntries(out List<int> registeredIds)
        {
            var entries = new List<AttackVideoEntry>();
            registeredIds = new List<int>();

            // Id,Power,Hand,Type,ImageId
            foreach (var line in File.ReadAllLines(CardCsvPath).Skip(1))
            {
                var cols = line.Split(',');
                if (cols.Length < 4 || !int.TryParse(cols[0], out var id)) continue;

                var clip = AssetDatabase.LoadAssetAtPath<VideoClip>($"{VideoDirectory}/ID{id}.mp4");
                if (clip == null) continue;

                entries.Add(new AttackVideoEntry
                {
                    Type = (CardType)Enum.Parse(typeof(CardType), cols[3].Trim()),
                    Hand = (CardHand)Enum.Parse(typeof(CardHand), cols[2].Trim()),
                    Level = PowerToLevel(int.Parse(cols[1])),
                    Clip = clip,
                });
                registeredIds.Add(id);
            }
            return entries;
        }

        // EffectPresenter と同じ対応(2000 → 3 / 1000 → 2 / 500 → 1 / それ以外 0)
        private static int PowerToLevel(int power) => power switch
        {
            2000 => 3,
            1000 => 2,
            500 => 1,
            _ => 0
        };
    }
}
