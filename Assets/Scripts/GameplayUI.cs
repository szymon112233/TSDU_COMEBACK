using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {

    void Awake()
    {
        Init();
    }

    public Font font;
    [Header("Scoreboard")]
    public Text scoreTextLeft;
    public Text scoreTextRight;
    public Text folusTextLeft;
    public Text foulsTextRight;
    public Text timeText;
    public Image colorBallImageRed;
    public Image colorBallImageBlue;
    [Header("Summary")]
    public GameObject afterMatchSummary;
    public Text scoreTextLeftSummary;
    public Text scoreTextRightSummary;
    public Text foulsTextLeftSummary;
    public Text foulsTextRightSummary;
    public GameObject p1winsImage;
    public GameObject p2winsImage;
    public GameObject tieImage;
    [Header("Mutiplayer")]
    public GameObject multiplayerInfoPanel;
    public Text waitingforPlayersText;


    private Vector2Int score;
    private Vector2Int folus;

    private void Init()
    {
        scoreTextLeft.text = "0";
        scoreTextRight.text = "0";
        folusTextLeft.text = "0";
        foulsTextRight.text = "0";
        timeText.text = "0:0";
        colorBallImageRed.fillAmount = 0.5f;
        colorBallImageBlue.fillAmount = 0.5f;
        UniverseManager.ScoreChanged += UpdateScoreText;
        UniverseManager.FoulsChanged += UpdateFaulText;
        UniverseManager.TimeChanged += UpdateTimeText;
        UniverseManager.EndOfTheMatch += OnEndMatch;
        UniverseManager.MatchStarted += HideSummaryPanel;
        UniverseManager.MatchStarted += HideMutiplayerInfo;
        font.material.mainTexture.filterMode = FilterMode.Point;
    }

    void UpdateScoreText(Vector2Int scores)
    {
        scoreTextLeft.text = string.Format("{0}", scores.x);
        scoreTextRight.text = string.Format("{0}", scores.y);
        int max = scores.x + scores.y;
        if (max != 0)
        {
            colorBallImageRed.fillAmount = (float)scores.x / max;
            colorBallImageBlue.fillAmount = (float)scores.y / max;
        }
        else
        {
            colorBallImageRed.fillAmount = 0.5f;
            colorBallImageBlue.fillAmount = 0.5f;
        }
        score = scores;
        
    }

    void UpdateFaulText(Vector2Int folus)
    {
        folusTextLeft.text = string.Format("{0}", folus.x);
        foulsTextRight.text = string.Format("{0}", folus.y);

        this.folus = folus;
    }

    void OnEndMatch(Vector2Int finalPoints)
    {
        foulsTextLeftSummary.text = string.Format("{0}", folus.x);
        foulsTextRightSummary.text = string.Format("{0}", folus.y);

        scoreTextLeftSummary.text = string.Format("{0}", finalPoints.x);
        scoreTextRightSummary.text = string.Format("{0}", finalPoints.y);
        if (finalPoints.x > finalPoints.y)
            p1winsImage.SetActive(true);
        else if (finalPoints.x < finalPoints.y)
            p2winsImage.SetActive(true);
        else
            tieImage.SetActive(true);

        afterMatchSummary.SetActive(true);
    }

    void UpdateTimeText(float time)
    {
        timeText.text = string.Format("{0}:{1}", ((int)time / 60).ToString("D2"), (Mathf.RoundToInt(time % 60)).ToString("D2")  );
    }

    public void HideSummaryPanel()
    {
        p1winsImage.SetActive(false);
        p2winsImage.SetActive(false);
        tieImage.SetActive(false);
        afterMatchSummary.SetActive(false);
    }

    public void HideMutiplayerInfo()
    {
        multiplayerInfoPanel.SetActive(false);
    }

}