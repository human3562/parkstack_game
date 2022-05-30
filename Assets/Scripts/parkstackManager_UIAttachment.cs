using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class parkstackManager_UIAttachment : MonoBehaviour
{
    private parkstackManager manager;
    private PlayableDirector director;
    public TextMeshProUGUI ui_timeText;
    public TextMeshProUGUI ui_recText;
    public TextMeshProUGUI ui_carcountText;
    public Image ui_recCircle;
    public Image ui_rewindIcon;

    //onEndRecodring
    public RectTransform ui_restartMenuHolder;
    public RectTransform ui_replaySliderHolder;
    public Slider ui_replaySlider;

    //buttons
    public Button ui_restart_button;
    public Button ui_mainmenu_button;
    public Button ui_nextlevel_button;

    public TextMeshProUGUI ui_countDownText;

    public float rewindFlashTime = 0.1f;
    public float recCirclePulseTime = 0.5f;


    private bool waitForInput = false;
    private void Awake() {

        director = GetComponent<PlayableDirector>();
        manager = GetComponent<parkstackManager>();

        //binding
        manager.OnRewind.AddListener(RewindAnim);
        manager.OnStartRecording.AddListener(PulseRecordCircleAnim);
        manager.OnStartRecording.AddListener(HideEndroundUI);
        manager.OnEndRecording.AddListener(StopPulseRecordCircleAnim);
        manager.OnEndRecording.AddListener(ShowEndroundUI);
        manager.OnCarParked.AddListener(UpdateCarsLeftCounter);
        manager.OnGameOver.AddListener(ShowGameOverUI);
    }

    private void Start() {
        Time.timeScale = 1;
        StartGame(3);
    }

    private void Update() {
        ui_timeText.SetText("T: " + TimeSpan.FromSeconds(manager.rec_time).ToString(@"mm\:ss\.ff"));
        if (manager.totalRecTime > 0 && !waitForInput)
            ui_replaySlider.value = (float)(manager.rec_time / manager.totalRecTime);
        
    }

    public void StartGame(int count) {
        int currentCount = count;

        Sequence countDownSequence = DOTween.Sequence();
        ui_countDownText.text = currentCount.ToString();

        countDownSequence.Append(ui_countDownText.rectTransform.DOScale(2.0f, 0.1f));
        countDownSequence.Append(ui_countDownText.rectTransform.DOScale(1.0f, 0.9f)).AppendCallback(() => {
            currentCount--;
            if (currentCount == 0) ui_countDownText.text = "Go!";
            else ui_countDownText.text = currentCount.ToString();
        });

        countDownSequence.SetLoops(count, LoopType.Restart);
        countDownSequence.OnComplete(()=> {
            ui_countDownText.DOFade(0, 1.0f);
            manager.StartGame();
        });

        countDownSequence.Play();
    }


    public void UpdateCarsLeftCounter() {
        ui_carcountText.rectTransform.DOScale(1.8f, 0.2f).OnComplete(()=>ui_carcountText.rectTransform.DOScale(1.0f, 1.0f)).SetUpdate(true);
        ui_carcountText.text = manager.carsLeft.ToString();
        
    }

    public void SliderDown() {
        waitForInput = true;
        ui_replaySlider.onValueChanged.AddListener((float time)=> {
            manager.PlayOnTime(time);
        });
    }

    public void SliderUp() {
        waitForInput = false;
        ui_replaySlider.onValueChanged.RemoveAllListeners();
    }

    public void RewindAnim() {
        ui_rewindIcon.DOFade(1.0f, rewindFlashTime).SetLoops(3, LoopType.Yoyo).OnComplete(() => ui_rewindIcon.DOFade(0.0f, rewindFlashTime)).SetUpdate(true);
    }

    public void PulseRecordCircleAnim() {
        ui_recCircle.DOFade(0.6f, recCirclePulseTime).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }
    public void StopPulseRecordCircleAnim() {
        ui_recCircle.DOKill();
        ui_recCircle.enabled = false;
    }

    public void HideEndroundUI() {
        ui_restart_button.interactable = false;
        ui_mainmenu_button.interactable = false;
        if(ui_nextlevel_button != null) ui_nextlevel_button.interactable = false;
        ui_restartMenuHolder.DOAnchorPosX(240, 1.0f).SetUpdate(true);  
        ui_replaySliderHolder.DOMoveY(-60, 1.0f).SetUpdate(true);
        ui_recText.text = "REC";
    }

    public void ShowEndroundUI() {
        ui_restart_button.interactable = true;
        ui_mainmenu_button.interactable = true;
        if (ui_nextlevel_button != null) ui_nextlevel_button.interactable = true;
        ui_restartMenuHolder.DOAnchorPosX(0, 1.0f).SetUpdate(true);
        ui_replaySliderHolder.DOMoveY(50, 1.0f).SetUpdate(true);
        ui_recText.text = "REP";
    }

    public void ShowGameOverUI() {
        StopPulseRecordCircleAnim();
        ui_rewindIcon.DOKill();
        ui_restart_button.interactable = true;
        ui_mainmenu_button.interactable = true;
        if (ui_nextlevel_button != null) ui_nextlevel_button.interactable = true;
        ui_restartMenuHolder.DOAnchorPosX(0, 1.0f).SetUpdate(true);
        ui_recText.text = "RIP";
    }

    public void RestartLevel() {
        ui_restart_button.interactable = false;
        ui_mainmenu_button.interactable = false;
        if (ui_nextlevel_button != null) ui_nextlevel_button.interactable = false;
        ui_restartMenuHolder.DOAnchorPosX(240, 1.0f).SetUpdate(true);
        ui_replaySliderHolder.DOMoveY(-60, 1.0f).OnComplete(() => manager.restartLevel()).SetUpdate(true);
    }

    public void ReturnToMenu() {
        ui_restart_button.interactable = false;
        ui_mainmenu_button.interactable = false;
        if (ui_nextlevel_button != null) ui_nextlevel_button.interactable = false;
        ui_restartMenuHolder.DOAnchorPosX(240, 1.0f).SetUpdate(true);
        ui_replaySliderHolder.DOMoveY(-60, 1.0f).OnComplete(() => manager.loadMainMenu()).SetUpdate(true);
    }
}
