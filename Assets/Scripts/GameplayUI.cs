using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {

    #region singleton
    public static GameplayUI instance = null;

    //Awake is always called before any Start functions
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        Init();
    }

    #endregion

    public Font font;

    private void Init()
    {
        UniverseManager.scoreChanged += UpdateScoreText;
        font.material.mainTexture.filterMode = FilterMode.Point;
    }

    public Text scoreTextLeft;
    public Text scoreTextRight;

    // Use this for initialization
    void Start () {
        
	}

    void UpdateScoreText(Vector2Int scores)
    {
        scoreTextLeft.text = string.Format("{0}", scores.x);
        scoreTextRight.text = string.Format("{0}", scores.y);
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}