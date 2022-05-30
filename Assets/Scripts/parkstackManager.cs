using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Experimental.Rendering.Universal;

public class parkstackManager : MonoBehaviour
{

    private PlayableDirector director;
    //private parkstackManager_UIAttachment ui;

    public Transform spawnPoint;
    public List<GameObject> carsToPark;
    public int freeSpaces = 13;
    public float maxTimePerCar = 5.0f;
    public bool lightsOn = false;
    public float reliefTime = 3.0f;
    public float startVelocity = 10.0f;
    private int currentCar = 0;

    [HideInInspector]
    public int carsLeft;

    [HideInInspector]
    public float rec_time;
    [HideInInspector]
    public bool isRecording = false;
    [HideInInspector]
    public float car_time = 0;
    [HideInInspector]
    public float totalRecTime = 0;

    //EVENTS
    public UnityEvent OnRewind;
    public UnityEvent OnStartRecording;
    public UnityEvent OnEndRecording;
    public UnityEvent OnCarParked;
    public UnityEvent OnGameOver;

    private List<string> recordedAssetNames = new List<string>();
    private List<GameObject> spawnedCars = new List<GameObject>();

    public void carParked() => b_isCarParked = true;
    private bool b_isCarParked = false;
    private bool isCarParked() {
        return b_isCarParked;
    }

    private void Awake() {
        director = GetComponent<PlayableDirector>();
        //ui = GetComponent<parkstackManager_UIAttachment>();
    }

    private void Start() {
        director.RebuildGraph();
        director.time = 0;
        director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        director.Play();
    }

    public void StartGame() {
        StartCoroutine(startGameLoop());
    }

    private void Update() {
        if (isRecording) {
            rec_time += Time.deltaTime;
            car_time += Time.deltaTime;
            if (car_time > maxTimePerCar) GameOver();

            if (rec_time > totalRecTime) totalRecTime = rec_time;
        }
        else {
            rec_time += Time.deltaTime;
            if (rec_time > totalRecTime) rec_time = 0;
            director.time = rec_time;
        }

        for (int i = 0; i < spawnedCars.Count; i++) {
            GameObject go = spawnedCars[i];
            foreach (AnimationState state in go.GetComponent<Animation>()) {
                go.GetComponent<Animation>().Play("recordedClip");
                state.time = rec_time - reliefTime * i;
            }
        }
    }

    private IEnumerator startGameLoop() {
        director.RebuildGraph();
        director.time = 0;
        director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        director.Play();

        carsLeft = carsToPark.Count;
        isRecording = true;
        OnStartRecording?.Invoke();
        while (currentCar < carsToPark.Count) {
            car_time = 0;
            GameObject newCar = Instantiate(carsToPark[currentCar], spawnPoint.position, spawnPoint.rotation);
            spawnedCars.Add(newCar);
            
            newCar.GetComponent<Rigidbody2D>().velocity = newCar.transform.right * startVelocity;
            newCar.GetComponent<moveRecorder>().StartRecording();
            newCar.GetComponent<carController>().OnCarCrashed.AddListener(GameOver);

            //start timeline on appropriate time
            
            yield return new WaitForSeconds(0.5f);
            newCar.GetComponent<PlayerInput>().enabled = true;
            if (lightsOn) foreach (Light2D light in newCar.GetComponentsInChildren<Light2D>()) light.enabled = true;

            if (freeSpaces <= 0) GameOver();

            b_isCarParked = false;
            yield return new WaitUntil(isCarParked);
            b_isCarParked = false;

            car_time = 0;

            newCar.GetComponent<carController>().enabled = false;

            carsLeft--;
            OnCarParked?.Invoke();


            Debug.Log("saved");

            newCar.GetComponent<moveRecorder>().StopRecording();
            //AddClipToTimeline(newCar.GetComponent<moveRecorder>().StopRecording(), newCar.GetComponent<Animator>(), reliefTime * currentCar);

            director.Stop();

            director.RebuildGraph();
            director.time = rec_time;

            //director.time = reliefTime * (currentCar+1);
            //rec_time = reliefTime * (currentCar + 1);


            if (currentCar + 1 != carsToPark.Count) {
                OnRewind?.Invoke();

                DOTween.To(() => rec_time, x => rec_time = x, reliefTime * (currentCar + 1), 1f).OnUpdate(() => { director.time = rec_time; director.Play(); director.playableGraph.GetRootPlayable(0).SetSpeed(0); });
                yield return new WaitForSeconds(1f);

                director.playableGraph.GetRootPlayable(0).SetSpeed(1);

                director.Play();
            }


            newCar.GetComponent<PlayerInput>().enabled = false;

            currentCar++;
        }

        foreach (carController c in FindObjectsOfType<carController>())
            c.OnCarCrashed.RemoveAllListeners();
        
        director.time = 0;
        director.Play();
        //rec_time = (float)director.duration;

        isRecording = false;
        OnEndRecording?.Invoke();
    }

    void AddClipToTimeline(AnimationClip clip, Animator objectToAnimate, float timeOffset) {
        TimelineAsset asset = director.playableAsset as TimelineAsset;

        foreach (TrackAsset track in asset.GetOutputTracks())
            if (track.name == clip.name)
                asset.DeleteTrack(track);
        AnimationTrack newTrack = asset.CreateTrack<AnimationTrack>(clip.GetInstanceID().ToString());
        recordedAssetNames.Add(clip.GetInstanceID().ToString());
        //Debug.Log(clip.GetInstanceID().ToString());
        
        newTrack.infiniteClipOffsetPosition = spawnPoint.transform.position;
        TimelineClip tclip = newTrack.CreateClip(clip);
        tclip.start = timeOffset;
        (tclip.asset as AnimationPlayableAsset).removeStartOffset = false;
        director.SetGenericBinding(newTrack, objectToAnimate);
    }

    public void PlayOnTime(float value) {
        //Debug.Log("this shouldnt show up unless you click something...");
        rec_time = value * totalRecTime;
        director.time = rec_time;
        director.Play();
    }

    public void restartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void loadMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy() {
        TimelineAsset asset = director.playableAsset as TimelineAsset;
        foreach (TrackAsset track in asset.GetOutputTracks())
                if(recordedAssetNames.Contains(track.name)) asset.DeleteTrack(track);
        recordedAssetNames.Clear();
        director.RebuildGraph();
        
    }


    public void GameOver() {
        StopAllCoroutines();
        isRecording = false;
        Camera.main.DOShakePosition(0.5f, 0.1f, 10, 90, true).SetUpdate(true);
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 2f).OnComplete(()=>OnGameOver?.Invoke()).SetUpdate(true);
    }

    public void LoadLevel(string levelName) {
        Debug.Log($"Should load level: {levelName}");
        SceneManager.LoadScene(levelName);
    }

    //private void Update() {
    //    if (Input.GetKeyDown(KeyCode.Space)) {
    //        finish_recording();
    //    }
    //}
}
