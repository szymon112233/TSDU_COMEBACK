using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


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

        Init();
    }

    #endregion

    public static System.Action<Vector2Int> ScoreChanged;
    public static System.Action<Vector2Int> FoulsChanged;
    public static System.Action<float> TimeChanged;
    public static System.Action<Vector2Int> EndOfTheMatch;
    public static System.Action MatchRestarted;

    [Header("Ball")]
    public GameObject ballPrefab;
    
    public int currentBallColor;
    public Transform ballSpawnPoint;
    public Sprite[] ballColors;
    public PointsCounter[] pointCounters;
    public BallDetetor[] outOfFieldDtetectors;

    [Header("Player")]

    public GameObject playerPrefab;
    public Cinemachine.CinemachineTargetGroup targetGroup;
    public TSDUPlayer[] players;
    public GameObject[] spawners;

    [Header("Gameplay")]

    public int[] score;
    public int[] fouls;
    public float countDownDuration = 3;
    public float matchDuration = 180;
    public float matchTimer = 60;

    private GameObject currentBall;
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
            if (value == MatchState.MATCH)
            {

                //currentBall.GetComponent<Rigidbody2D>().simulated = true;
                GameInput.instance.SetInputEnabled(true);
            }
                
        }
    }

    void Init()
    {
        Com.szymon112233.TSDU.MultiplayerConnector.RoomJoined += InitGame;
    }

    void InitGame()
    {
        spawners = GameObject.FindGameObjectsWithTag("SpawnPoint");
        players = new TSDUPlayer[spawners.Length];

        for (int i = 0; i < spawners.Length; i++)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawners[i].transform.position, Quaternion.identity, 0);
            players[i] = go.GetComponent<TSDUPlayer>();
            players[i].ballPosition.GetComponent<SpriteRenderer>().sprite = ballColors[currentBallColor];
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
            pointCounters[j].PointScored += () => {
                score[i]++;
                currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
                StartCoroutine(DelayedActionCoroutine(1.5f, ResetPositons));
                FireScoreChanged();
            };
        }
        for (int i = 0; i < outOfFieldDtetectors.Length; i++)
        {
            outOfFieldDtetectors[i].balldetected += OnBallOutOfField;
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
        SpawnBall(ballSpawnPoint.position);

    }

    private void ResetState()
    {
        CurrentState = MatchState.BEFORE;
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
        //currentBall.GetComponent<Rigidbody2D>().simulated = false;
        GameInput.instance.SetInputEnabled(false);
        if (MatchRestarted != null)
            MatchRestarted();
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

    public void SetBallColor(int color)
    {
        if (color < 0 || color > ballColors.Length - 1 )
        {
            Debug.LogError("Invalid color number!");
            return;
        }
            
        currentBallColor = color;
        
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

    public void SpawnBall(Vector3 position, Vector2 initialForce = new Vector2(), float torque = 0.0f)
    {
        if (currentBall != null)
            PhotonNetwork.Destroy(currentBall);
        currentBall = PhotonNetwork.Instantiate(ballPrefab.name, position, Quaternion.identity, 0);
        currentBall.GetComponent<Rigidbody2D>().AddForce(initialForce, ForceMode2D.Impulse);
        currentBall.GetComponent<Rigidbody2D>().AddTorque(torque, ForceMode2D.Impulse);
        currentBall.GetComponent<SpriteRenderer>().sprite = ballColors[currentBallColor];
        if (currentState == MatchState.AFTER)
        {
            currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface += OnBallCollision;
            currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithOutOfField += OnBallOutOfFieldEndGame;
        }
            
    }

    void OnBallCollision()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface -= OnBallCollision;
        GameInput.instance.SetInputEnabled(false);
        currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
        StartCoroutine(EndMatchCor());
    }

    void OnBallOutOfFieldEndGame()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithOutOfField -= OnBallOutOfFieldEndGame;
        GameInput.instance.SetInputEnabled(false);
        EndMatch();
    }

    void OnBallOutOfField()
    {
        if (currentState != MatchState.AFTER)
            ResetPositons();
    }

    IEnumerator EndMatchCor()
    {
        yield return new WaitForSeconds(2);
        EndMatch();
    }

    IEnumerator DelayedActionCoroutine(float timeDelay, System.Action action)
    {
        yield return new WaitForSeconds(timeDelay);
        if (action != null)
            action();
    }

    void EndMatch()
    {
        if (EndOfTheMatch != null)
            EndOfTheMatch(new Vector2Int(score[0] - fouls[0], score[1] - fouls[1]));
        Debug.Log("End of the match!");
    }
}
