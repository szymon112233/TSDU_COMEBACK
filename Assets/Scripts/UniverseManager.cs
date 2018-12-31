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

    public static System.Action<Vector2Int> scoreChanged;
    public static System.Action<Vector2Int> foulsChanged;
    public static System.Action<float> timeChanged;


    public GameObject ballPrefab;
    public GameObject playerPrefab;
    public Cinemachine.CinemachineTargetGroup targetGroup;
    public Player[] players;
    public PointsCounter[] pointCounters;
    public GameObject[] spawners;

    public int[] score;
    public int[] fauls;
    public float matchTimerStartValue = 180;
    public float matchTimer;

    private uint lastPlayer = 0;
    
    void InitGame()
    {
        spawners = GameObject.FindGameObjectsWithTag("SpawnPoint");
        players = new Player[spawners.Length];

        for (int i = 0; i < spawners.Length; i++)
        {
            GameObject go = Instantiate(playerPrefab, spawners[i].transform.position, Quaternion.identity);
            players[i] = go.GetComponent<Player>();
            players[i].number = (uint)i;
            targetGroup.AddMember(go.transform, 1, 50.0f);
        }

        score = new int[players.Length];
        fauls = new int[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            int j = i;
            players[i].BallThrown += () => { lastPlayer = players[j].number; };
        }
        for (int j = 0; j < pointCounters.Length; j++)
        {
            int i = j;
            pointCounters[j].PointScored += () => { score[i]++; FireScoreChanged(); };
        }

        matchTimer = matchTimerStartValue;
    }

    private void FireScoreChanged()
    {
        if (scoreChanged != null)
            scoreChanged(new Vector2Int(score[0], score[1]));
    }

    private void Update()
    {
        if (matchTimer > 0)
            matchTimer -= Time.deltaTime;
        if (timeChanged != null)
            timeChanged(matchTimer);
    }

    public void SpawnBall(Vector3 position ,Vector2 initialForce = new Vector2(), float torque = 0.0f)
    {
        GameObject go = Instantiate(ballPrefab, position, Quaternion.identity);
        go.GetComponent<Rigidbody2D>().AddForce(initialForce, ForceMode2D.Impulse);
        go.GetComponent<Rigidbody2D>().AddTorque(torque, ForceMode2D.Impulse);
    }
}
