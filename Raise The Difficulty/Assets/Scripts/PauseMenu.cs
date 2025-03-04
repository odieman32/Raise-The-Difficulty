using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool GameIsPaused = false; //bool to tell if game is paused

    public GameObject pauseMenuUI; //game object for UI

    private void Update()
    {
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
    public void Resume()
    {
        pauseMenuUI.SetActive(false); //all pause menu UI is set to false
        Time.timeScale = 1f; //resumes time back to normal
        GameIsPaused = false; //Game paused is put back to false
        Cursor.lockState = CursorLockMode.Locked; //cursor goes back to the camera
        Cursor.visible = false; //cursor is not visable

    }

    void Pause()
    {
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
}

