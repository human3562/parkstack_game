using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;

public class moveRecorder : MonoBehaviour {
    [HideInInspector]
    private AnimationClip clip;
    //public AnimationClip recordClip;

    //private GameObjectRecorder m_Recorder;
    private bool recording = false;

    private float time = 0f;

    private AnimationCurve[] carPos;

    void Start() {
        clip = new AnimationClip();
        clip.legacy = true;

        // Create recorder and record the script GameObject.
        //m_Recorder = new GameObjectRecorder(gameObject);

        // Bind all the Transforms on the GameObject and all its children.
        //m_Recorder.BindComponentsOfType<Transform>(gameObject, true);
        //StartCoroutine(recordShit());
    }

    //private void OnDestroy() {
    // FinishedRecording();
    //}

    public void StartRecording() {
        time = 0f;
        recording = true;
        carPos = new AnimationCurve[6];
        for (int i = 0; i < carPos.Length; i++)
            carPos[i] = new AnimationCurve();
    }

    public AnimationClip StopRecording() {
        recording = false;
        time = 0f;
        //GetComponent<Collider2D>().enabled = false;
        FinishedRecording();
        GetComponent<Animation>().AddClip(clip, "recordedClip");
        GetComponent<Animation>().Play("recordedClip");
        foreach (AnimationState state in GetComponent<Animation>()) state.speed = 0;

        return clip;
    }

    //public void PlayAnimation() {
    // Animation anim = GetComponent<Animation>();
    // anim.AddClip(clip, "test");
    // anim.Play("test");
    //}

    void LateUpdate() {
        if (clip == null)
            return;

        // Take a snapshot and record all the bindings values for this frame.
        if (recording) {
            carPos[0].AddKey(time, transform.position.x);
            carPos[1].AddKey(time, transform.position.y);
            carPos[2].AddKey(time, transform.rotation.x);
            carPos[3].AddKey(time, transform.rotation.y);
            carPos[4].AddKey(time, transform.rotation.z);
            carPos[5].AddKey(time, transform.rotation.w);
            time += Time.deltaTime;
        }

        //m_Recorder.TakeSnapshot(Time.deltaTime);
    }

    void FinishedRecording() {
        clip.SetCurve("", typeof(Transform), "localPosition.x", carPos[0]);
        clip.SetCurve("", typeof(Transform), "localPosition.y", carPos[1]);
        clip.SetCurve("", typeof(Transform), "localRotation.x", carPos[2]);
        clip.SetCurve("", typeof(Transform), "localRotation.y", carPos[3]);
        clip.SetCurve("", typeof(Transform), "localRotation.z", carPos[4]);
        clip.SetCurve("", typeof(Transform), "localRotation.w", carPos[5]);
        //if (m_Recorder.isRecording) {
        // // Save the recorded session to the clip.
        // m_Recorder.SaveToClip(clip);
        // //m_Recorder.SaveToClip(recordClip);
        //}
    }
}