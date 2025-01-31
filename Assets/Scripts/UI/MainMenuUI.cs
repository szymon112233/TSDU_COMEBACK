﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour {

    public MatchSetupUI matchSetup;
    public GameObject creditsPanel;

    public void StartSinglePlayer()
    {
        GameState.Instance.isMultiplayer = false;
        matchSetup.Show();
    }

    public void StartMultiPlayer()
    {
        GameState.Instance.isMultiplayer = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MultiplayerJoin");
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
