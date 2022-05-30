using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class animationController : MonoBehaviour
{
    PlayableDirector director;
    private void Awake() {
        director = GetComponent<PlayableDirector>();
    }
}
