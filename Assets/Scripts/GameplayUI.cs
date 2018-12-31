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
    public Text scoreTextLeft;
    public Text scoreTextRight;
    public Text falusTextLeft;
    public Text faulsTextRight;
    public Text timeText;
    public Image colorBallImageRed;
    public Image colorBallImageBlue;

    private Vector2Int score;
    private Vector2Int folus;

    private void Init()
    {
        scoreTextLeft.text = "0";
        scoreTextRight.text = "0";
        falusTextLeft.text = "0";
        faulsTextRight.text = "0";
        timeText.text = "0:0";
        colorBallImageRed.fillAmount = 0.5f;
        colorBallImageBlue.fillAmount = 0.5f;
        UniverseManager.ScoreChanged += UpdateScoreText;
        UniverseManager.FoulsChanged += UpdateFaulText;
        UniverseManager.TimeChanged += UpdateTimeText;
        UniverseManager.EndOfTheMatch += OnEndMatch;
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
        falusTextLeft.text = string.Format("{0}", folus.x);
        faulsTextRight.text = string.Format("{0}", folus.y);

        this.folus = folus;
    }

    void OnEndMatch(Vector2Int finalPoints)
    {

    }

    void UpdateTimeText(float time)
    {
        timeText.text = string.Format("{0}:{1}", ((int)time / 60).ToString("D2"), (Mathf.RoundToInt(time % 60)).ToString("D2")  );
    }

}