using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Project.BattleField.Script.TimelineTracks.Text
{
    [Serializable]
    public class TextClip : PlayableAsset, ITimelineClipAsset
    {
        public TextBehaviour template = new TextBehaviour ();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TextBehaviour>.Create (graph, template);
            return playable;
        }
    }
}
