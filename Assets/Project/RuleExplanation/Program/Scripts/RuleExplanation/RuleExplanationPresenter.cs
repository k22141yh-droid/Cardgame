using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Project.RuleExplanation.Program.Scripts.RuleExplanation
{
    public class RuleExplanationPresenter : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;

        public void Initialize()
        {
            videoPlayer.loopPointReached += OnVideoComplete;
        }

        private static void OnVideoComplete(VideoPlayer _)
        {
            SceneManager.LoadScene("BattleField");
        }
    }
}