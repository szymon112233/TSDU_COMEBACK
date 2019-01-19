using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchSetupUI : MonoBehaviour, IOnEventCallback
{
    public float maxMatchTime = 900;
    public float minMatchTime = 60;

    //References
    public Text timeText;
    public Text infoText;

    public Image redPlayerPreview;
    public Image bluePlayerPreview;

    public Image mapPreview;

    public Image[] ballsPreviews;

    public GameObject firstPlayerReady;
    public GameObject secondPlayerReady;

    public Button redPlayerNext;
    public Button redPlayerPrev;
    public Button bluePlayerNext;
    public Button bluePlayerPrev;
    public Button timeNext;
    public Button timePrev;
    public Button mapNext;
    public Button mapPrev;
    public Button ballNext;
    public Button ballPrev;

    private float currentMatchTime;
    private int currentBallIndex;
    private int currentMapIndex;
    private int currentRedPlayerPresetIndex;
    private int currentBluePlayerPresetIndex;

    private bool isReady = false;
    private bool isOtherReady = false;

    private void Awake()
    {
        ConnectionManager.RoomJoinedLastPlayer += Show;
        ConnectionManager.PlayerLeftRoom += Hide;
        Hide();
    }

    private void OnDestroy()
    {
        ConnectionManager.RoomJoinedLastPlayer -= Show;
        ConnectionManager.PlayerLeftRoom -= Hide;
    }

    void Start () {
        currentMatchTime = GameState.Instance.currentMatchSetup.MatchTime;
        currentBallIndex = GameState.Instance.currentMatchSetup.BallColorIndex;
        currentMapIndex = GameState.Instance.currentMatchSetup.MapIndex;

        // HARDCODE 2 player
        currentRedPlayerPresetIndex = GameState.Instance.currentMatchSetup.PlayerSkinsIndexes[0];
        currentBluePlayerPresetIndex = GameState.Instance.currentMatchSetup.PlayerSkinsIndexes[1];

        firstPlayerReady.SetActive(false);
        secondPlayerReady.SetActive(false);

        ChangeTime(0);

        ChangeRedPlayerPreview(0);
        ChangeBluePlayerPreview(0);

        ChangeMapIndex(0);

        ChangeBallIndex(0);

        if (GameState.Instance.isMultiplayer)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                infoText.text = "Host chooses match setup, choose you player";

                redPlayerNext.interactable = false;
                redPlayerPrev.interactable = false;
                timeNext.interactable = false;
                timePrev.interactable = false;
                mapNext.interactable = false;
                mapPrev.interactable = false;
                ballNext.interactable = false;
                ballPrev.interactable = false;
            }
            else
            {
                infoText.text = "You're a host!\n Choose match setup:";

                bluePlayerNext.interactable = false;
                bluePlayerPrev.interactable = false;
            }
        }
        else
        {
            infoText.enabled = false;
        }

    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void ChangeRedPlayerPreview(int value)
    {
        currentRedPlayerPresetIndex += value;

        if (currentRedPlayerPresetIndex < 0)
            currentRedPlayerPresetIndex = GameState.Instance.defaultGameData.presets.Length - 1;
        else if (currentRedPlayerPresetIndex >= GameState.Instance.defaultGameData.presets.Length)
            currentRedPlayerPresetIndex = 0;

        redPlayerPreview.material.SetColor("_TshirtColor", GameState.Instance.defaultGameData.presets[currentRedPlayerPresetIndex].Shirt);
        redPlayerPreview.material.SetColor("_PantsColor", GameState.Instance.defaultGameData.presets[currentRedPlayerPresetIndex].Pants);
        redPlayerPreview.material.SetColor("_ShoesColor", GameState.Instance.defaultGameData.presets[currentRedPlayerPresetIndex].Acessories);
        redPlayerPreview.material.SetColor("_SkinColor", GameState.Instance.defaultGameData.presets[currentRedPlayerPresetIndex].Skin);

        if (GameState.Instance.isMultiplayer)
        {
            FireMatchSetupPlayerChangedPhotonEvent(0, currentRedPlayerPresetIndex);
        }
    }

    public void ChangeBluePlayerPreview(int value)
    {
        currentBluePlayerPresetIndex += value;

        if (currentBluePlayerPresetIndex < 0)
            currentBluePlayerPresetIndex = GameState.Instance.defaultGameData.presets.Length - 1;
        else if (currentBluePlayerPresetIndex >= GameState.Instance.defaultGameData.presets.Length)
            currentBluePlayerPresetIndex = 0;

        if (currentBluePlayerPresetIndex < GameState.Instance.defaultGameData.presets.Length)
        {
            bluePlayerPreview.material.SetColor("_TshirtColor", GameState.Instance.defaultGameData.presets[currentBluePlayerPresetIndex].Shirt);
            bluePlayerPreview.material.SetColor("_PantsColor", GameState.Instance.defaultGameData.presets[currentBluePlayerPresetIndex].Pants);
            bluePlayerPreview.material.SetColor("_ShoesColor", GameState.Instance.defaultGameData.presets[currentBluePlayerPresetIndex].Acessories);
            bluePlayerPreview.material.SetColor("_SkinColor", GameState.Instance.defaultGameData.presets[currentBluePlayerPresetIndex].Skin);
        }

        if (GameState.Instance.isMultiplayer)
        {
            FireMatchSetupPlayerChangedPhotonEvent(1, currentBluePlayerPresetIndex);
        }
    }

    public void ChangeBallIndex(int value)
    {
        currentBallIndex += value;

        if (currentBallIndex < 0)
            currentBallIndex = GameState.Instance.defaultGameData.balls.Length - 1;
        else if (currentBallIndex >= GameState.Instance.defaultGameData.balls.Length)
            currentBallIndex = 0;

        for (int i = 0; i < ballsPreviews.Length; i++)
        {
            int calculatedBallIndex = currentBallIndex + (i - ballsPreviews.Length / 2);
            if (calculatedBallIndex < 0)
                calculatedBallIndex = GameState.Instance.defaultGameData.balls.Length + calculatedBallIndex;
            else if (calculatedBallIndex >= GameState.Instance.defaultGameData.balls.Length)
                calculatedBallIndex = calculatedBallIndex - GameState.Instance.defaultGameData.balls.Length;

            ballsPreviews[i].sprite = GameState.Instance.defaultGameData.balls[calculatedBallIndex];
        }

        if (GameState.Instance.isMultiplayer && PhotonNetwork.IsMasterClient)
        {
            FireMatchSetupBallChangedPhotonEvent();
        }
    }

    public void ChangeMapIndex(int value)
    {
        currentMapIndex += value;

        if (currentMapIndex < 0)
            currentMapIndex = GameState.Instance.defaultGameData.courts.Length - 1;
        else if (currentMapIndex >= GameState.Instance.defaultGameData.courts.Length)
            currentMapIndex = 0;

        mapPreview.sprite = GameState.Instance.defaultGameData.courts[currentMapIndex].miniPreviewSprite;

        if (GameState.Instance.isMultiplayer && PhotonNetwork.IsMasterClient)
        {
            FireMatchSetupMapChangedPhotonEvent();
        }
    }

    public void ChangeTime(float value)
    {
        currentMatchTime += value;

        if (currentMatchTime < minMatchTime)
            currentMatchTime = maxMatchTime;
        else if (currentMatchTime > maxMatchTime)
            currentMatchTime = minMatchTime;

        timeText.text = string.Format("{0}:{1}", ((int)currentMatchTime / 60).ToString("D2"), (Mathf.RoundToInt(currentMatchTime % 60)).ToString("D2"));

        if (GameState.Instance.isMultiplayer && PhotonNetwork.IsMasterClient)
        {
            FireMatchSetupTimeChangedPhotonEvent();
        }
    }

    public void ReadyClicked()
    {
        

        if (GameState.Instance.isMultiplayer)
        {
            isReady = true;
            if (!PhotonNetwork.IsMasterClient)
            {
                FireMatchSetupPlayerReadyPhotonEvent(isReady);
                secondPlayerReady.SetActive(isReady);
            }
            else
            {
                FireMatchSetupPlayerReadyPhotonEvent(isReady);
                firstPlayerReady.SetActive(isReady);
                TryStartGame();
            }

                
        }
        else
        {
            GameState.Instance.currentMatchSetup.BallColorIndex = currentBallIndex;
            GameState.Instance.currentMatchSetup.MapIndex = currentMapIndex;
            GameState.Instance.currentMatchSetup.MatchTime = currentMatchTime;
            GameState.Instance.currentMatchSetup.PlayerCount = 2;
            GameState.Instance.currentMatchSetup.PlayerSkinsIndexes = new int[] { currentRedPlayerPresetIndex, currentBluePlayerPresetIndex };
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameState.Instance.defaultGameData.courts[GameState.Instance.currentMatchSetup.MapIndex].SceneName);
        }
    }

    public void CancelClicked()
    {
        if (GameState.Instance.isMultiplayer)
        {
            if (isReady)
            {
                isReady = false;
                if (!PhotonNetwork.IsMasterClient)
                {
                    FireMatchSetupPlayerReadyPhotonEvent(isReady);
                    secondPlayerReady.SetActive(isReady);
                }
                else
                {
                    FireMatchSetupPlayerReadyPhotonEvent(isReady);
                    firstPlayerReady.SetActive(isReady);
                }
            }
                
            else
            {
                PhotonNetwork.Disconnect();
                Hide();
            }
                
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


    #region Multiplayer only methods

    public void TryStartGame()
    {
        GameState.Instance.currentMatchSetup.BallColorIndex = currentBallIndex;
        GameState.Instance.currentMatchSetup.MapIndex = currentMapIndex;
        GameState.Instance.currentMatchSetup.MatchTime = currentMatchTime;
        GameState.Instance.currentMatchSetup.PlayerCount = 2;
        GameState.Instance.currentMatchSetup.PlayerSkinsIndexes = new int[] { currentRedPlayerPresetIndex, currentBluePlayerPresetIndex };

        if (isReady && isOtherReady)
            PhotonNetwork.LoadLevel(GameState.Instance.defaultGameData.courts[GameState.Instance.currentMatchSetup.MapIndex].SceneName);
    }

    private void FireMatchSetupTimeChangedPhotonEvent()
    {
        byte evCode = ConnectionManager.MatchSetupTimeChangedPhotonEvent;
        object[] content = new object[] { currentMatchTime };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireMatchSetupBallChangedPhotonEvent()
    {
        byte evCode = ConnectionManager.MatchSetupBallChangedPhotonEvent;
        object[] content = new object[] { currentBallIndex };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireMatchSetupMapChangedPhotonEvent()
    {
        byte evCode = ConnectionManager.MatchSetupMapChangedPhotonEvent;
        object[] content = new object[] { currentMapIndex };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireMatchSetupPlayerChangedPhotonEvent(int player, int index)
    {
        byte evCode = ConnectionManager.MatchSetupPlayerChangedPhotonEvent;
        object[] content = new object[] { player, index };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireMatchSetupPlayerReadyPhotonEvent(bool ready)
    {
        byte evCode = ConnectionManager.MatchSetupPlayerReadyPhotonEvent;
        object[] content = new object[] { ready };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == ConnectionManager.MatchSetupTimeChangedPhotonEvent)
            HandleMatchSetupTimeChangedPhotonEvent(photonEvent);
        else if (eventCode == ConnectionManager.MatchSetupBallChangedPhotonEvent)
            HandleMatchSetupBallChangedPhotonEvent(photonEvent);
        else if (eventCode == ConnectionManager.MatchSetupMapChangedPhotonEvent)
            HandleMatchSetupMapChangedPhotonEvent(photonEvent);
        else if (eventCode == ConnectionManager.MatchSetupPlayerChangedPhotonEvent)
            HandleMatchSetupPlayerChangedPhotonEvent(photonEvent);
        else if (eventCode == ConnectionManager.MatchSetupPlayerReadyPhotonEvent)
            HandleMatchSetupPlayerReadyPhotonEvent(photonEvent);
    }

    private void HandleMatchSetupTimeChangedPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        float time = (float)recievedData[0];

        timeText.text = string.Format("{0}:{1}", ((int)time / 60).ToString("D2"), (Mathf.RoundToInt(time % 60)).ToString("D2"));
    }

    private void HandleMatchSetupBallChangedPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int ballIndex = (int)recievedData[0];

        for (int i = 0; i < ballsPreviews.Length; i++)
        {
            int calculatedBallIndex = ballIndex + (i - ballsPreviews.Length / 2);
            if (calculatedBallIndex < 0)
                calculatedBallIndex = GameState.Instance.defaultGameData.balls.Length + calculatedBallIndex;
            else if (calculatedBallIndex >= GameState.Instance.defaultGameData.balls.Length)
                calculatedBallIndex = calculatedBallIndex - GameState.Instance.defaultGameData.balls.Length;

            ballsPreviews[i].sprite = GameState.Instance.defaultGameData.balls[calculatedBallIndex];
        }
    }

    private void HandleMatchSetupMapChangedPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int mapIndex = (int)recievedData[0];

        mapPreview.sprite = GameState.Instance.defaultGameData.courts[mapIndex].miniPreviewSprite;
    }

    private void HandleMatchSetupPlayerChangedPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerIndex = (int)recievedData[0];
        int presetIndex = (int)recievedData[1];
        if (playerIndex == 0)
        {
            if (presetIndex < GameState.Instance.defaultGameData.presets.Length)
            {
                redPlayerPreview.material.SetColor("_TshirtColor", GameState.Instance.defaultGameData.presets[presetIndex].Shirt);
                redPlayerPreview.material.SetColor("_PantsColor", GameState.Instance.defaultGameData.presets[presetIndex].Pants);
                redPlayerPreview.material.SetColor("_ShoesColor", GameState.Instance.defaultGameData.presets[presetIndex].Acessories);
                redPlayerPreview.material.SetColor("_SkinColor", GameState.Instance.defaultGameData.presets[presetIndex].Skin);
            }
        }
        else if (playerIndex == 1)
        {
            if (presetIndex < GameState.Instance.defaultGameData.presets.Length)
            {
                currentBluePlayerPresetIndex = presetIndex;

                bluePlayerPreview.material.SetColor("_TshirtColor", GameState.Instance.defaultGameData.presets[presetIndex].Shirt);
                bluePlayerPreview.material.SetColor("_PantsColor", GameState.Instance.defaultGameData.presets[presetIndex].Pants);
                bluePlayerPreview.material.SetColor("_ShoesColor", GameState.Instance.defaultGameData.presets[presetIndex].Acessories);
                bluePlayerPreview.material.SetColor("_SkinColor", GameState.Instance.defaultGameData.presets[presetIndex].Skin);
            }
        }
        
    }

    private void HandleMatchSetupPlayerReadyPhotonEvent(EventData photonEvent)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            object[] recievedData = (object[])photonEvent.CustomData;
            isOtherReady = (bool)recievedData[0];
            secondPlayerReady.SetActive(isOtherReady);

            TryStartGame();
        }
        else
        {
            object[] recievedData = (object[])photonEvent.CustomData;
            firstPlayerReady.SetActive((bool)recievedData[0]);
        }
        
    }

    #endregion
}
