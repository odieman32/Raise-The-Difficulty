using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainWindow;
    public GameObject optionsWindow;
    public GameObject controlsWindow;
    [SerializeField] RectTransform mainWindowRect, optionsWindowRect, controlsWindowRect;
    [SerializeField] float topPosY, middlePosY;
    [SerializeField] float tweenDur;

    public void PlayGame()
    {
        Debug.Log("Start Game!");
        //Loads the sceane corresponding by number
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;
    }

    //public void OptionsBack()
    //{
    //    MainIntro();
    //    optionsWindow.SetActive(false);
    //    mainWindow.SetActive(true);
    //}
    //public void Options()
    //{
    //    OptionsIntro();
    //    optionsWindow.SetActive(true);
    //    mainWindow.SetActive(false);
    //}

    //public void Controls()
    //{
    //    ControlsIntro();
    //    controlsWindow.SetActive(true);
    //    mainWindow.SetActive(false);
    //}

    //public void ControlsBack()
    //{
    //    MainIntro();
    //    controlsWindow.SetActive(false);
    //    mainWindow.SetActive(true);
    //}

    //void OptionsIntro()
    //{
    //    optionsWindowRect.DOAnchorPosY(middlePosY, tweenDur).SetUpdate(true);
    //    mainWindowRect.DOAnchorPosY(topPosY, tweenDur).SetUpdate(true);
    //}

    //void MainIntro()
    //{
    //    mainWindowRect.DOAnchorPosY(middlePosY, tweenDur).SetUpdate(true);
    //    optionsWindowRect.DOAnchorPosY(topPosY, tweenDur).SetUpdate(true);
    //    controlsWindowRect.DOAnchorPosY(topPosY, tweenDur).SetUpdate(true);
    //}

    //void ControlsIntro()
    //{
    //    controlsWindowRect.DOAnchorPosY(middlePosY, tweenDur).SetUpdate(true);
    //    mainWindowRect.DOAnchorPosY(topPosY, tweenDur).SetUpdate(true);
    //}


}

//by Brodie Detterman
