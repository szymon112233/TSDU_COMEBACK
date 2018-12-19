using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniverseManager : MonoBehaviour {

    #region singleton
    public static UniverseManager instance = null;

    //Awake is always called before any Start functions
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        InitGame();
    }

    #endregion

    public GameObject ballPrefab;
    public Player[] players;
    public PointsCounter[] pointCounters;

    public int[] score;

    private uint lastPlayer = 0;
    

    //Initializes the game for each level.
    void InitGame()
    {
        score = new int[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            int j = i;
            players[i].BallThrown += () => { lastPlayer = players[j].number; };
        }
        for (int j = 0; j < pointCounters.Length; j++)
        {
            int i = j;
            pointCounters[j].PointScored += () => { Debug.Log(i); score[i]++; };
        }
    }

    public void SpawnBall(Vector3 position ,Vector2 initialForce = new Vector2(), float torque = 0.0f)
    {
        GameObject go = Instantiate(ballPrefab, position, Quaternion.identity);
        go.GetComponent<Rigidbody2D>().AddForce(initialForce, ForceMode2D.Impulse);
        go.GetComponent<Rigidbody2D>().AddTorque(torque, ForceMode2D.Impulse);
    }
}
