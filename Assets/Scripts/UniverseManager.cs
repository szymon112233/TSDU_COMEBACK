using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public enum MatchState
{
    BEFORE = 0,
    MATCH,
    AFTER
}

public class UniverseManager : MonoBehaviour, IOnEventCallback
{

    #region fake singleton
    public static UniverseManager instance = null;

    //Awake is always called before any Start functions
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        Init();
    }

    #endregion

    public static System.Action<Vector2Int> ScoreChanged;
    public static System.Action<Vector2Int> FoulsChanged;
    public static System.Action<float> TimeChanged;
    public static System.Action<Vector2Int> EndOfTheMatch;
    public static System.Action MatchStarted;

    [Header("Ball")]
    public GameObject ballPrefab;
    
    public int currentBallColor;
    public Transform ballSpawnPoint;
    public Sprite[] ballColors;
    public PointsCounter[] pointCounters;
    public OutOfFieldDetector[] outOfFieldDtetectors;

    [Header("Players")]

    public GameObject playerPrefab;
    public Cinemachine.CinemachineTargetGroup targetGroup;
    public TSDUPlayer controlledPlayer;
    public Dictionary<int, TSDUPlayer> allNetworkPlayers;
    public GameObject[] spawners;
    public bool BallPickedUp;

    [Header("Gameplay")]

    public int[] score;
    public int[] fouls;
    public float countDownDuration = 3;
    public float matchDuration = 180;
    public float matchTimer = 60;
    public MatchSetup currentMatchSetup;
    private float timeToStartMatch;
    public bool throwBack = false;

    public GameObject currentBall;
    private MatchState currentState = MatchState.BEFORE;

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

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Init()
    {
        allNetworkPlayers = new Dictionary<int, TSDUPlayer>();
        currentMatchSetup = new MatchSetup() {
            BallColorIndex = currentBallColor,
            CountDownTime = countDownDuration,
            MatchTime = matchDuration,
            MapIndex = 0,
            PlayerCount = 2,
            PlayerSkinsIndexes = new int[2]};
        BallNetworkSync.BallCreated += (GameObject ball) => { currentBall = ball; };
    }

    void InitGame()
    {
        spawners = GameObject.FindGameObjectsWithTag("SpawnPoint");

        GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawners[PhotonNetwork.LocalPlayer.ActorNumber-1].transform.position, Quaternion.identity, 0);
        controlledPlayer = go.GetComponent<TSDUPlayer>();
        controlledPlayer.ballPosition.GetComponent<SpriteRenderer>().sprite = ballColors[currentMatchSetup.BallColorIndex];
        targetGroup.AddMember(go.transform, 1, 50.0f);

        score = new int[currentMatchSetup.PlayerCount];
        fouls = new int[currentMatchSetup.PlayerCount];

        for (int j = 0; j < pointCounters.Length; j++)
        {
            int i = j;
            pointCounters[j].PointScored += () => {
                if (currentBall.GetComponent<PhotonView>().OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    score[i]++;
                    FireScoreChanged();
                    FirePointScoredPhotonEvent(i);
                    currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
                    StartCoroutine(DelayedActionCoroutine(1.5f, ResetPositons));
                }
            };
        }

        for (int i = 0; i < outOfFieldDtetectors.Length; i++)
        {
            outOfFieldDtetectors[i].BallOut += OnBallOutOfField;
        }

        ResetState();

        if(!PhotonNetwork.IsMasterClient)
            matchTimer = timeToStartMatch;
    }

    void ResetPositons()
    {
        controlledPlayer.ResetState();
        controlledPlayer.rigibdoy.position = spawners[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.position;
        controlledPlayer.transform.position = spawners[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.position;


        if (PhotonNetwork.IsMasterClient)
            SpawnBall(ballSpawnPoint.position);
    }

    private void ResetState()
    {
        CurrentState = MatchState.BEFORE;
        for (int i = 0; i < score.Length; i++)
        {
            score[i] = 0;
            fouls[i] = 0;
        }
        matchTimer = currentMatchSetup.CountDownTime;
        FireScoreChanged();
        FireFoulsChanged();
        ResetPositons();
        GameInput.instance.SetInputEnabled(false);
        if (MatchStarted != null)
            MatchStarted();
    }

    private void FireScoreChanged()
    {
        if (ScoreChanged != null)
            ScoreChanged(new Vector2Int(score[0], score[1]));
    }

    private void FirePointScoredPhotonEvent(int basketID)
    {
        byte evCode = ConnectionManager.PointScoredPhotonEvent;
        object[] content = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, basketID, PhotonNetwork.Time + 1,5 };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireMatchEndedPhotonEvent(double timeToEnd)
    {
        byte evCode = ConnectionManager.MatchEndedPhotonEvent;
        object[] content = new object[] { PhotonNetwork.Time + timeToEnd };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireSendMatchSetupPhotonEvent()
    {
        Debug.LogError("Sending Event");
        byte evCode = ConnectionManager.SendMatchSetupPhotonEvent;
        object[] content = new object[] {
                currentMatchSetup.BallColorIndex,
                currentMatchSetup.CountDownTime,
                currentMatchSetup.MatchTime,
                currentMatchSetup.MapIndex,
                currentMatchSetup.PlayerCount,
                currentMatchSetup.PlayerSkinsIndexes,
                PhotonNetwork.Time + currentMatchSetup.CountDownTime};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        InitGame();
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
            matchTimer = currentMatchSetup.MatchTime;
        }
        else if (CurrentState == MatchState.MATCH)
        {
            CurrentState = MatchState.AFTER;
        }
        if (TimeChanged != null)
            TimeChanged(matchTimer);

        if (Input.GetKeyDown(KeyCode.R))
        {
            ConnectionManager.instance.Disconnect();
        }  
    }

    public void SpawnBall(Vector3 position, Vector2 initialForce = new Vector2(), float torque = 0.0f)
    {
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

    public void HitPlayer(int networkPlayerID, int direction)
    {
        Debug.LogFormat("Sent HitPlayer with values: networkPlayerID = {0}, direction = {1}", networkPlayerID, direction);
        byte evCode = ConnectionManager.PlayerHitPhotonEvent; 
        object[] content = new object[] { networkPlayerID, direction };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    public void RequestPickupBall(int networkPlayerID)
    {
        Debug.LogFormat("RequestPickupBall: networkPlayerID = {0}, controlledPlayer.networkNumber +1 = {1}", networkPlayerID, controlledPlayer.networkNumber + 1);
        if (PhotonNetwork.IsMasterClient && controlledPlayer.networkNumber +1 == networkPlayerID)
        {
            if (!BallPickedUp)
            {
                controlledPlayer.HasBall = true;
                if (currentBall.gameObject.GetComponent<PhotonView>().IsMine)
                    PhotonNetwork.Destroy(currentBall);
                else
                {
                    currentBall.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    PhotonNetwork.Destroy(currentBall);
                }
            }
        }
        else
        {
            Debug.LogFormat("Sent RequestPickupBall with values: networkPlayerID = {0}", networkPlayerID);
            byte evCode = ConnectionManager.RequestBallPickupPhotonEvent;
            object[] content = new object[] { networkPlayerID };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
        }
    }

    void OnBallCollision()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface -= OnBallCollision;
        GameInput.instance.SetInputEnabled(false);
        currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
        FireMatchEndedPhotonEvent(2.0);
        StartCoroutine(EndMatchCor(2.0f));
    }

    void OnBallOutOfFieldEndGame()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithOutOfField -= OnBallOutOfFieldEndGame;
        GameInput.instance.SetInputEnabled(false);
        FireMatchEndedPhotonEvent(0.0);
        EndMatch();
    }

    void OnBallOutOfField(Transform throwPoint)
    {
        if (currentState != MatchState.AFTER)
        {
            if (currentBall.GetComponent<PhotonView>().OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                int side = throwPoint.position.x > 0 ? -1 : 1;
                StartCoroutine(DelayedActionCoroutine(1.5f, ()=> { SpawnBall(throwPoint.position, new Vector2(350 * side, 200), 50.0f * side); }));
                StartCoroutine(DelayedActionCoroutine(2.0f, () => { throwBack = false; }));
            }
        }
    }

    IEnumerator EndMatchCor(float timeToEnd)
    {
        yield return new WaitForSeconds(timeToEnd);
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

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == ConnectionManager.SendMatchSetupPhotonEvent)
        {
            HandleSendMatchSetupPhotonEvent(photonEvent);
        }

        else if(eventCode == ConnectionManager.PlayerHitPhotonEvent)
        {
            HandlePlayerHitPhotonEvent(photonEvent);
        }
        else if (eventCode == ConnectionManager.RequestBallPickupPhotonEvent)
        {
            HandleRequestBallPickupPhotonEvent(photonEvent);
        }

        else if (eventCode == ConnectionManager.BallPickedUpPhotonEvent)
        {
            HandleBallPickedUpPhotonEvent(photonEvent);
        }
        else if (eventCode == ConnectionManager.PointScoredPhotonEvent)
        {
            HandlePointScoredPhotonEvent(photonEvent);
        }
        else if (eventCode == ConnectionManager.MatchEndedPhotonEvent)
        {
            HandleMatchEndedPhotonEvent(photonEvent);
        }
        else if (eventCode == ConnectionManager.LevelLoadedPhotonEvent)
        {
            HandleLevelLoadedPhotonEvent(photonEvent);
        }
    }

    public void HandleSendMatchSetupPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;

        currentMatchSetup = new MatchSetup()
        {
            BallColorIndex = (int)recievedData[0],
            CountDownTime = (float)recievedData[1],
            MatchTime = (float)recievedData[2],
            MapIndex = (int)recievedData[3],
            PlayerCount = (int)recievedData[4],
            PlayerSkinsIndexes = (int[])recievedData[5]
        };
        timeToStartMatch = (float)((double)recievedData[6] - PhotonNetwork.Time);
        Debug.LogErrorFormat("[Multiplayer] match setup recieved form host:\n PlayerCount = {0}\n MapIndex = {1}\n CountDownTime = {2}\n MatchTime = {3}\n BallColorIndex = {4}\n PlayerSkinsIndexes = {5}\n timeToStartMatch = {6}",
            currentMatchSetup.PlayerCount,
            currentMatchSetup.MapIndex,
            currentMatchSetup.CountDownTime,
            currentMatchSetup.MatchTime,
            currentMatchSetup.BallColorIndex,
            currentMatchSetup.PlayerSkinsIndexes,
            timeToStartMatch);
        InitGame();
    }

    public void HandlePlayerHitPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerNetworkID = (int)recievedData[0];
        if (allNetworkPlayers.ContainsKey(playerNetworkID))
        {
            Debug.LogFormat("Recieved HitPlayer with values: networkPlayerID = {0}, direction = {1}", playerNetworkID, (int)recievedData[1]);
            allNetworkPlayers[playerNetworkID].GetHit((int)recievedData[1]);
        }
        else
        {
            Debug.LogErrorFormat("Couldn't find other player with networkID: {0}", playerNetworkID);
        }
    }

    public void HandleRequestBallPickupPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerNetworkID = (int)recievedData[0];
        Debug.LogFormat("Recieved RequestBallPickup with values: networkPlayerID = {0}", playerNetworkID);
        if (allNetworkPlayers.ContainsKey(playerNetworkID))
        {
            if (!BallPickedUp)
            {
                Debug.LogFormat("Sent RequestBallPickup with values: networkPlayerID = {0}", playerNetworkID);
                byte evCode = ConnectionManager.BallPickedUpPhotonEvent;
                object[] content = new object[] { playerNetworkID };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
                if (currentBall.gameObject.GetComponent<PhotonView>().IsMine)
                    PhotonNetwork.Destroy(currentBall);
                else
                {
                    currentBall.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    PhotonNetwork.Destroy(currentBall);
                }
            }
        }
        else
        {
            Debug.LogErrorFormat("Couldn't find other player with networkID: {0}", playerNetworkID);
        }
    }

    public void HandleBallPickedUpPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerNetworkID = (int)recievedData[0];
        Debug.LogFormat("Recieved BallPickedUp with values: networkPlayerID = {0}", playerNetworkID);
        if (controlledPlayer.networkNumber + 1 == playerNetworkID)
        {
            controlledPlayer.HasBall = true;
        }
    }

    public void HandlePointScoredPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerNetworkID = (int)recievedData[0];
        int basketballID = (int)recievedData[1];
        double timeToReset = (double)recievedData[2] - PhotonNetwork.Time;

        Debug.LogFormat("Recieved PointScored with values: networkPlayerID = {0}, basketballID=  {1}, timeToReset = {2}", playerNetworkID, basketballID, timeToReset);

        score[basketballID]++;
        FireScoreChanged();
        currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
        StartCoroutine(DelayedActionCoroutine((float)timeToReset, ResetPositons));
    }

    public void HandleMatchEndedPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        double timeToEnd = (double)recievedData[0] - PhotonNetwork.Time;

        GameInput.instance.SetInputEnabled(false);
        currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;

        if (timeToEnd < 0)
        {
            EndMatch(); 
        }
        else
        {
            StartCoroutine(EndMatchCor((float)timeToEnd));
        }
    }

    public void HandleLevelLoadedPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerNetworkID = (int)recievedData[0];

        if (playerNetworkID != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            FireSendMatchSetupPhotonEvent();
        }
    }
}