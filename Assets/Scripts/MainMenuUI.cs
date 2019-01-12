using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour {

    public void StartSinglePlayer()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void StartMultiPlayer()
    {

    }

    public void ShowCredits()
    {

    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
