using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region References
    public GameObject mainWindow;
    public GameObject optionsWindow;
    public GameObject controlsWindow;
    #endregion

    #region Anims
    [SerializeField] RectTransform mainWindowRect, optionsWindowRect, controlsWindowRect;
    [SerializeField] float topPosY, middlePosY;
    [SerializeField] float tweenDur;
    #endregion

    public void PlayGame()
    {
        Debug.Log("Start Game!");
        //Loads the sceane corresponding by number
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;
    }
}

//by Brodie Detterman
