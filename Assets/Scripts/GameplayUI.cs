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

    private void Init()
    {
        UniverseManager.scoreChanged += UpdateScoreText;
    }

    public Text scoreText;

    // Use this for initialization
    void Start () {
        
	}

    void UpdateScoreText(Vector2Int scores)
    {
        scoreText.text = string.Format("{0}:{1}", scores.x, scores.y);
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}