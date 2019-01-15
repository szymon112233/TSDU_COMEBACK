using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerGUI : MonoBehaviour {

    public GameObject mainPanel;

    public GameObject passwordPanel;
    public InputField phraseInput;

    public GameObject loadingPanel;
    public Text infoText;

    private string sPhrase;
    private int mode = 0; // 0 - random, 1 - friends


    private void Awake()
    {
        ShowMainPanel();
        ConnectionManager.ConnectionStatusUpdated += OnConnectionStatusUpdated;
        ConnectionManager.RoomJoinedLastPlayer += OnRoomJoinedLastPlayer;
        ConnectionManager.PlayerLeftRoom += OnPlayerLeftRoom;
        ConnectionManager.Disconnected += OnDisconnect;
    }

    public void OnBackClicked()
    {
        if (loadingPanel.activeSelf)
        {
            ConnectionManager.instance.Disconnect();
            if (mode == 0)
                ShowMainPanel();
            else
                ShowPasswordPanel();
        }
        else if (passwordPanel.activeSelf)
        {
            ShowMainPanel();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public void ChooseRandom()
    {
        ShowLoadingPanel();
        mode = 0;
        ConnectionManager.instance.ConnectRandomRoom();
    }

    public void ChooseFriends()
    {
        ShowPasswordPanel();
        mode = 1;
    }

    public void OnSPhraseEndEdit(string value)
    {
        sPhrase = value;
    }

    public void OnPassOkClicked()
    {
        ShowLoadingPanel();
        ConnectionManager.instance.ConnectFriendsRoom(sPhrase);
    }

    private void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        loadingPanel.SetActive(false);
        passwordPanel.SetActive(false);
    }

    private void ShowPasswordPanel()
    {
        mainPanel.SetActive(false);
        loadingPanel.SetActive(false);
        passwordPanel.SetActive(true);
        phraseInput.Select();
    }

    private void ShowLoadingPanel()
    {
        mainPanel.SetActive(false);
        loadingPanel.SetActive(true);
        passwordPanel.SetActive(false);
    }

    private void OnConnectionStatusUpdated(string value)
    {
        infoText.text = value;
    }

    private void OnRoomJoinedLastPlayer()
    {
        loadingPanel.SetActive(false);
    }

    private void OnPlayerLeftRoom()
    {
        ConnectionManager.instance.Disconnect();
        if (mode == 0)
            ShowMainPanel();
        else
            ShowPasswordPanel();
    }

    private void OnDisconnect(string cause)
    {
        ConnectionManager.instance.Disconnect();
        if (mode == 0)
            ShowMainPanel();
        else
            ShowPasswordPanel();
    }

    private void OnDestroy()
    {
        ConnectionManager.ConnectionStatusUpdated -= OnConnectionStatusUpdated;
        ConnectionManager.RoomJoinedLastPlayer -= OnRoomJoinedLastPlayer;
        ConnectionManager.PlayerLeftRoom -= OnPlayerLeftRoom;
        ConnectionManager.Disconnected -= OnDisconnect;
    }
}
