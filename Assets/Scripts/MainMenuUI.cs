using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour {

    public void StartSinglePlayer()
    {
        GameState.Instance.isMultiplayer = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("COURT_1");
    }

    public void StartMultiPlayer()
    {
        GameState.Instance.isMultiplayer = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MultiplayerJoin");
    }

    public void ShowCredits()
    {
        
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
