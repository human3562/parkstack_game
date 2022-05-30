using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    public RectTransform lvlSelectHolder;

    private void Awake() {
        Time.timeScale = 1;
    }

    public void StartGameClicked() {
        foreach (Button b in lvlSelectHolder.GetComponentsInChildren<Button>()) b.interactable = true;
        lvlSelectHolder.DORotate(new Vector3(0,0,0), 0.5f).SetEase(Ease.InOutElastic);
    }

    public void ExitClicked() {
        Application.Quit();
    }

    public void LoadLevel(string levelName) {
        Debug.Log($"Should load level: {levelName}");
        SceneManager.LoadScene(levelName);
    }
}
