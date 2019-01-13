using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MatchSetupUI : MonoBehaviour {

    public float maxMatchTime = 900;
    public float minMatchTime = 60;

    //References
    public Text timeText;
    public Text infoText;

    public Image redPlayerPreview;
    public Image bluePlayerPreview;

    public Image mapPreview;

    public Image[] ballsPreviews;

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

    void Start () {
        currentMatchTime = GameState.Instance.defaultGameData.defaultSetup.MatchTime;
        currentBallIndex = GameState.Instance.defaultGameData.defaultSetup.BallColorIndex;
        currentMapIndex = GameState.Instance.defaultGameData.defaultSetup.MapIndex;

        // HARDCODE 2 player
        currentRedPlayerPresetIndex = GameState.Instance.defaultGameData.defaultSetup.PlayerSkinsIndexes[0];
        currentBluePlayerPresetIndex = GameState.Instance.defaultGameData.defaultSetup.PlayerSkinsIndexes[1];

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
                infoText.text = "Host chooses match setup";

                redPlayerNext.enabled = false;
                redPlayerPrev.enabled = false;
                timeNext.enabled = false;
                timePrev.enabled = false;
                mapNext.enabled = false;
                mapPrev.enabled = false;
                ballNext.enabled = false;
                ballPrev.enabled = false;
            }
            else
            {
                infoText.text = "You're a host!\n Choose match setup: ";

                bluePlayerNext.enabled = false;
                bluePlayerPrev.enabled = false;
            }
        }
        else
        {
            infoText.enabled = false;
        }
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
    }

    public void ChangeMapIndex(int value)
    {
        currentMapIndex += value;

        if (currentMapIndex < 0)
            currentMapIndex = GameState.Instance.defaultGameData.courts.Length - 1;
        else if (currentMapIndex >= GameState.Instance.defaultGameData.courts.Length)
            currentMapIndex = 0;

        mapPreview.sprite = GameState.Instance.defaultGameData.courts[currentMapIndex].miniPreviewSprite;
    }

    public void ChangeTime(float value)
    {
        currentMatchTime += value;

        if (currentMatchTime < minMatchTime)
            currentMatchTime = maxMatchTime;
        else if (currentMatchTime > maxMatchTime)
            currentMatchTime = minMatchTime;

        timeText.text = string.Format("{0}:{1}", ((int)currentMatchTime / 60).ToString("D2"), (Mathf.RoundToInt(currentMatchTime % 60)).ToString("D2"));
    }

    public void ReadyClicked()
    {
        GameState.Instance.currentMatchSetup.BallColorIndex = currentBallIndex;
        GameState.Instance.currentMatchSetup.MapIndex = currentMapIndex;
        GameState.Instance.currentMatchSetup.MatchTime = currentMatchTime;
        GameState.Instance.currentMatchSetup.PlayerCount = 2;
        GameState.Instance.currentMatchSetup.PlayerSkinsIndexes = new int[] {currentRedPlayerPresetIndex, currentBluePlayerPresetIndex };

        if (GameState.Instance.isMultiplayer)
        {

        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameState.Instance.defaultGameData.courts[GameState.Instance.currentMatchSetup.MapIndex].SceneName);
        }
    }

    public void CancelClicked()
    {
        if (GameState.Instance.isMultiplayer)
        {
            //Dooo stuff
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

}
