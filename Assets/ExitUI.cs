using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ExitUI : MonoBehaviour
{
    public PlayableDirector timeLine;
    public Transform pos;

    private void Awake()
    {
        timeLine = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        timeLine.played += OnTimeLineStart;
        timeLine.stopped += OnTimeLineStop;
    }
    private void OnDisable()
    {
        timeLine.played -= OnTimeLineStart;
        timeLine.stopped -= OnTimeLineStop;
    }
    
    private void OnTimeLineStart(PlayableDirector playableDirector)
    {
        Debug.Log("OnTimeLineStart");
        
    }

    private void OnTimeLineStop(PlayableDirector playableDirector)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
