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

    private void Init()
    {
        scoreTextLeft.text = "0";
        scoreTextRight.text = "0";
        falusTextLeft.text = "0";
        faulsTextRight.text = "0";
        timeText.text = "0:0";
        colorBallImageRed.fillAmount = 0.5f;
        colorBallImageBlue.fillAmount = 0.5f;
        UniverseManager.scoreChanged += UpdateScoreText;
        UniverseManager.foulsChanged += UpdateFaulText;
        UniverseManager.timeChanged += UpdateTimeText;
        font.material.mainTexture.filterMode = FilterMode.Point;
    }

    // Use this for initialization
    void Start () {
        
	}

    void UpdateScoreText(Vector2Int scores)
    {
        scoreTextLeft.text = string.Format("{0}", scores.x);
        scoreTextRight.text = string.Format("{0}", scores.y);
        int max = scores.x + scores.y;
        colorBallImageRed.fillAmount = (float)scores.x / max;
        colorBallImageBlue.fillAmount = (float)scores.y / max;
    }

    void UpdateFaulText(Vector2Int scores)
    {
        falusTextLeft.text = string.Format("{0}", scores.x);
        faulsTextRight.text = string.Format("{0}", scores.y);
    }

    void UpdateTimeText(float time)
    {
        timeText.text = string.Format("{0}:{1}", ((int)time / 60).ToString("D2"), ((int)time % 60).ToString("D2")  );
    }


    // Update is called once per frame
    void Update () {
	    
	}
}