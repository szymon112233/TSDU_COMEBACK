using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct MatchSetup
{
    public float MatchTime;
    public float CountDownTime;
    public int MapIndex;
    public int BallColorIndex;
    public int PlayerCount;
    public int[] PlayerSkinsIndexes;
}

public class GameState : MonoBehaviour {

    #region singleton

    private static GameState instance = null;

    //Awake is always called before any Start functions
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Init();
    }

    public static GameState Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject("GameState");
                instance = gameObject.AddComponent<GameState>();
            }
            return instance;
        }
    }

    #endregion

    public bool isMultiplayer;
    public GameDefaultData defaultGameData;


    void Init()
    {
        isMultiplayer = false;
    }
}
