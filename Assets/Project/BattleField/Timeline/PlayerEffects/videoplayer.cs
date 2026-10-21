using UnityEngine;
using UnityEngine.Playables;

public class TimelineControl : MonoBehaviour
{
    public PlayableDirector director;

    void Start()
    {
        director.Play(); // Timeline再生
    }
}