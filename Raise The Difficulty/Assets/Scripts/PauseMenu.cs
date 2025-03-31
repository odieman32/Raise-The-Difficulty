using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Threading.Tasks;

public class PauseMenu : MonoBehaviour
{
    public bool GameIsPaused = false; //bool to tell if game is paused
    public bool UpgradeActive = false; 

    public GameObject pauseMenuUI; //game object for UI
    [SerializeField] RectTransform pausePanelRect, staminaBar, hitTracker;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float topPosY, middlePosY;
    [SerializeField] float tweenDur;

    private void Update()
    {
        if (UpgradeActive)
            return;

        if (Input.GetKeyDown(KeyCode.Tab)) //Checks if escape is pressed
        {
            if (GameIsPaused) //if game is paused then pressing it will call the resume button
            {
                Resume();
            }
            else //all if game is not paused it will pause the game with the Pause function
            {
                Pause();
            }
        }
    }
    public async void Resume()
    {
        await PauseOutro();
        pauseMenuUI.SetActive(false); //all pause menu UI is set to false
        Time.timeScale = 1f; //resumes time back to normal
        GameIsPaused = false; //Game paused is put back to false
        Cursor.lockState = CursorLockMode.Locked; //cursor goes back to the camera
        Cursor.visible = false; //cursor is not visable

    }

    void Pause()
    {
        PauseIntro();
        pauseMenuUI.SetActive(true);//pause is set at active
        Time.timeScale = 0f; //stops the time of the game to freeze everything
        GameIsPaused = true; //Game is puased is set to true
        Cursor.lockState = CursorLockMode.None; //allows use of cursor to click buttons
        Cursor.visible = true; //cursor is visable
        
    }

    public void Home()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    void PauseIntro()
    {
        canvasGroup.DOFade(1, tweenDur).SetUpdate(true);
        pausePanelRect.DOAnchorPosY(middlePosY, tweenDur).SetUpdate(true);
        staminaBar.DOAnchorPosX(-530, tweenDur).SetUpdate(true);
        hitTracker.DOAnchorPosY(93, tweenDur).SetUpdate(true);
    }

    async Task PauseOutro()
    {
        canvasGroup.DOFade(0, tweenDur).SetUpdate(true);
        await pausePanelRect.DOAnchorPosY(topPosY, tweenDur).SetUpdate(true).AsyncWaitForCompletion();
        staminaBar.DOAnchorPosX(-53, tweenDur).SetUpdate(true);
        hitTracker.DOAnchorPosY(-115, tweenDur).SetUpdate(true);
    }
}
//Rehope Games
