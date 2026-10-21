using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R3;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace Battle.View {

    public sealed class Battle_View : MonoBehaviour
    {
        [SerializeField] private GameObject mainCamera;

        [SerializeField] private PlayableDirector director;
        // Start is called before the first frame update
        void Start()
        {
            TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
            director.Stop();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void PlayRound() {
            director.Play();
        }
    }
}
