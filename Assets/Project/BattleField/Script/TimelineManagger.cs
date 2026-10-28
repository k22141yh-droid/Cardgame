using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

namespace BattleField.Script.TimelineManagge {
    public class TimelineManagger : MonoBehaviour
    {
        [SerializeField] private PlayableDirector director;
        [SerializeField] private GameObject[] _gameobjects;
        private bool _isdone;
        
        // Start is called before the first frame update
        void Awake()
        {
            director.stopped += DirectorStopped;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public bool IsDone() {
            return _isdone;
        }

        public void TimelinePlay() {
            _isdone = false;
            director.Play();
        }

        void DirectorStopped(PlayableDirector _director) {
            if (_director == director) {
                _isdone = true;
                // シーンを抜けるとき(再生停止・シーン遷移)は、対象がすでに破棄されていることがある
                foreach(GameObject gameobject in _gameobjects) {
                    if (gameobject != null) gameobject.SetActive(false);
                }
            }
        }
    }
}

