﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MatchState
{
    BEFORE = 0,
    MATCH,
    AFTER
}

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

    public static System.Action<Vector2Int> ScoreChanged;
    public static System.Action<Vector2Int> FoulsChanged;
    public static System.Action<float> TimeChanged;
    public static System.Action<Vector2Int> EndOfTheMatch;

    public GameObject ballPrefab;
    public GameObject playerPrefab;
    public Cinemachine.CinemachineTargetGroup targetGroup;
    public Player[] players;
    public PointsCounter[] pointCounters;
    public GameObject[] spawners;
    public Transform ballSpawnPoint;
    public GameObject currentBall;

    public int[] score;
    public int[] fouls;
    public float countDownDuration = 3;
    public float matchDuration = 180;
    public float matchTimer;
    private MatchState currentState = MatchState.BEFORE;

    private uint lastPlayer = 0;

    public MatchState CurrentState
    {
        get
        {
            return currentState;
        }

        set
        {
            currentState = value;
            if (value == MatchState.BEFORE)
            {
                currentBall.GetComponent<Rigidbody2D>().simulated = false;
                GameInput.instance.SetInputEnabled(false);
            }    
            else if (value == MatchState.MATCH)
            {
                currentBall.GetComponent<Rigidbody2D>().simulated = true;
                GameInput.instance.SetInputEnabled(true);
            }
                
        }
    }

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
        fouls = new int[players.Length];
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

        ResetState();
    }

    void ResetPositons()
    {
        for (int i = 0; i < spawners.Length; i++)
        {
            players[i].transform.position = spawners[i].transform.position;
            players[i].ResetState();
        }
        if (currentBall != null)
            DestroyImmediate(currentBall);
        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
    }

    private void ResetState()
    {
        for (int i = 0; i < players.Length; i++)
        {
            score[i] = 0;
            fouls[i] = 0;
        }
        matchTimer = countDownDuration;
        lastPlayer = 0;
        FireScoreChanged();
        FireFoulsChanged();
        ResetPositons();
        // Set AFTER Reset Positions so that a ball already exists
        CurrentState = MatchState.BEFORE;
    }

    private void FireScoreChanged()
    {
        if (ScoreChanged != null)
            ScoreChanged(new Vector2Int(score[0], score[1]));
    }
    private void FireFoulsChanged()
    {
        if (FoulsChanged != null)
            FoulsChanged(new Vector2Int(fouls[0], fouls[1]));
    }

    private void Update()
    {
        if (matchTimer > 0)
            matchTimer -= Time.deltaTime;
        else if (CurrentState == MatchState.BEFORE)
        {
            CurrentState = MatchState.MATCH;
            matchTimer = matchDuration;
        }
        else if (CurrentState == MatchState.MATCH)
        {
            CurrentState = MatchState.AFTER;
        }
        if (TimeChanged != null)
            TimeChanged(matchTimer);

        if (Input.GetKeyDown(KeyCode.R))
            ResetState();
    }

    public void SpawnBall(Vector3 position ,Vector2 initialForce = new Vector2(), float torque = 0.0f)
    {
        if (currentBall != null)
            DestroyImmediate(currentBall);
        currentBall = Instantiate(ballPrefab, position, Quaternion.identity);
        currentBall.GetComponent<Rigidbody2D>().AddForce(initialForce, ForceMode2D.Impulse);
        currentBall.GetComponent<Rigidbody2D>().AddTorque(torque, ForceMode2D.Impulse);
        if (currentState == MatchState.AFTER)
        {
            currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface += OnBallCollision;
        }
            
    }

    void OnBallCollision()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface -= OnBallCollision;
        GameInput.instance.SetInputEnabled(false);
        StartCoroutine(EndMatchCor());
        
    }

    IEnumerator EndMatchCor()
    {
        yield return new WaitForSeconds(2);
        if (EndOfTheMatch != null)
            EndOfTheMatch(new Vector2Int(score[0] - fouls[0], score[1] - fouls[1]));
        GameInput.instance.SetInputEnabled(true);
        Debug.Log("End of the match!");
    }
}
