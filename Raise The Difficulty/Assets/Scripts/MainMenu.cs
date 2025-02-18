using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Start Game!");
        //Loads the sceane corresponding by number
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;
    }
}

//by Brodie Detterman
