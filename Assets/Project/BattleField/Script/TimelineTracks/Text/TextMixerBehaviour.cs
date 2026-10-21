using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Project.BattleField.Script.TimelineTracks.Text
{
    public class TextMixerBehaviour : PlayableBehaviour
    {
        private string m_AssignedText;

        private TextMeshProUGUI m_TrackBinding;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            m_TrackBinding = playerData as TextMeshProUGUI;

            if (m_TrackBinding == null)
                return;

            int inputCount = playable.GetInputCount();

            float totalWeight = 0f;
            float greatestWeight = 0f;
            int currentInputs = 0;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<TextBehaviour> inputPlayable = (ScriptPlayable<TextBehaviour>)playable.GetInput(i);
                TextBehaviour input = inputPlayable.GetBehaviour();

                totalWeight += inputWeight;

                if (inputWeight > greatestWeight)
                {
                    m_AssignedText = input.text;
                    m_TrackBinding.text = m_AssignedText;
                    greatestWeight = inputWeight;
                }

                if (!Mathf.Approximately(inputWeight, 0f))
                {
                    currentInputs++;
                }
            }

            if (currentInputs != 1 && 1f - totalWeight > greatestWeight)
            {
                m_TrackBinding.text = "";
            }
        }

        public override void OnGraphStop(Playable playable)
        {
            m_TrackBinding.text = "";
        }
    }
}