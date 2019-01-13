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

[System.Serializable]
public struct PlayerColors
{
    public Color Shirt;
    public Color Pants;
    public Color Acessories;
    public Color Skin;
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
    public float CameraFollowRadius = 300.0f;
    public float CameraFollowRadiusBall = 25.0f;
    public TSDUPlayer controlledPlayer;
    public Dictionary<int, TSDUPlayer> allPlayers;
    public GameObject[] spawners;
    public PlayerColors[] presets; 
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
        allPlayers = new Dictionary<int, TSDUPlayer>();
        currentMatchSetup = GameState.Instance.currentMatchSetup;
        presets = GameState.Instance.defaultGameData.presets;
        ballColors = GameState.Instance.defaultGameData.balls;

        if (GameState.Instance.isMultiplayer)
            BallNetworkSync.BallCreated += (GameObject ball) => {
                currentBall = ball;
                currentBall.GetComponent<SpriteRenderer>().sprite = ballColors[currentMatchSetup.BallColorIndex];
            };
        else
            InitGameSingleplayer();
    }

    void InitGameMultiplayer()
    {

        GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, spawners[PhotonNetwork.LocalPlayer.ActorNumber-1].transform.position, Quaternion.identity, 0);
        controlledPlayer = go.GetComponent<TSDUPlayer>();
        controlledPlayer.localInputDeviceNumber = 0;
        controlledPlayer.number = (uint)PhotonNetwork.LocalPlayer.ActorNumber - 1;
        targetGroup.AddMember(go.transform, 1, CameraFollowRadius);

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

    void InitGameSingleplayer()
    {

        for (int i = 0; i < currentMatchSetup.PlayerCount; i++)
        {
            GameObject go = Instantiate(playerPrefab, spawners[i].transform.position, Quaternion.identity);

            TSDUPlayer player = go.GetComponent<TSDUPlayer>();
            player.ballPosition.GetComponent<SpriteRenderer>().sprite = ballColors[currentBallColor];
            player.localInputDeviceNumber = (uint)i;
            targetGroup.AddMember(go.transform, 1, CameraFollowRadius);
        }

        score = new int[currentMatchSetup.PlayerCount];
        fouls = new int[currentMatchSetup.PlayerCount];

        for (int j = 0; j < pointCounters.Length; j++)
        {
            int i = j;
            pointCounters[j].PointScored += () => {
                score[i]++;
                FireScoreChanged();
                currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
                StartCoroutine(DelayedActionCoroutine(1.5f, ResetPositons));
            };
        }

        for (int i = 0; i < outOfFieldDtetectors.Length; i++)
        {
            outOfFieldDtetectors[i].BallOut += OnBallOutOfField;
        }

        ResetState();

        matchTimer = currentMatchSetup.CountDownTime;
    }

    void ResetPositons()
    {
        if (GameState.Instance.isMultiplayer)
        {
            controlledPlayer.ResetState();
            controlledPlayer.rigibdoy.position = spawners[controlledPlayer.number].transform.position;
            controlledPlayer.transform.position = spawners[controlledPlayer.number].transform.position;

            if (PhotonNetwork.IsMasterClient)
                SpawnBall(ballSpawnPoint.position);
        }
        else
        {
            for (int i = 1; i <= allPlayers.Count; i++)
            {
                allPlayers[i].ResetState();
                allPlayers[i].rigibdoy.position = spawners[allPlayers[i].number].transform.position;
                allPlayers[i].transform.position = spawners[allPlayers[i].number].transform.position;
            }

            SpawnBall(ballSpawnPoint.position);
        }

        
    }

    public void RegisterPlayer(int number, TSDUPlayer player)
    {
        if (!allPlayers.ContainsKey(number))
        {
            allPlayers.Add(number, player);
            player.sprite.material.SetColor("_TshirtColor", presets[currentMatchSetup.PlayerSkinsIndexes[number-1]].Shirt);
            player.sprite.material.SetColor("_PantsColor", presets[currentMatchSetup.PlayerSkinsIndexes[number - 1]].Pants);
            player.sprite.material.SetColor("_ShoesColor", presets[currentMatchSetup.PlayerSkinsIndexes[number - 1]].Acessories);
            player.sprite.material.SetColor("_SkinColor", presets[currentMatchSetup.PlayerSkinsIndexes[number - 1]].Skin);
            player.ballPosition.GetComponent<SpriteRenderer>().sprite = ballColors[currentMatchSetup.BallColorIndex];
        }
            
        else
        {
            Debug.LogErrorFormat("HECK! allPlayers already contains player with number: {0}", number);
        }
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

    public void SetBallColor(int color)
    {
        if (color < 0 || color > ballColors.Length - 1)
        {
            Debug.LogError("Invalid color number!");
            return;
        }

        currentBallColor = color;

    }

    private void FireScoreChanged()
    {
        if (ScoreChanged != null)
            ScoreChanged(new Vector2Int(score[0], score[1]));
    }

    public void FireFoulsChanged()
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
            if (GameState.Instance.isMultiplayer)
                ConnectionManager.instance.Disconnect();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }  
    }

    public void SpawnBall(Vector3 position, Vector2 initialForce = new Vector2(), float torque = 0.0f)
    {
        if (GameState.Instance.isMultiplayer)
        {
            currentBall = PhotonNetwork.Instantiate(ballPrefab.name, position, Quaternion.identity, 0);
        }
        else
        {
            if (currentBall != null)
                DestroyImmediate(currentBall);
            currentBall = Instantiate(ballPrefab, position, Quaternion.identity);
        }
        
        currentBall.GetComponent<Rigidbody2D>().AddForce(initialForce, ForceMode2D.Impulse);
        currentBall.GetComponent<Rigidbody2D>().AddTorque(torque, ForceMode2D.Impulse);
        currentBall.GetComponent<SpriteRenderer>().sprite = ballColors[currentMatchSetup.BallColorIndex];
        if (currentState == MatchState.AFTER)
        {
            currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface += OnBallCollision;
            currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithOutOfField += OnBallOutOfFieldEndGame;
        }

        targetGroup.AddMember(currentBall.transform, 1, CameraFollowRadiusBall);
    }

    public void HitPlayer(int playerID, int direction)
    {
        if (GameState.Instance.isMultiplayer)
        {
            FirePlayerHitPhotonEvent(playerID, direction);
        }
        else
        {
            if (allPlayers.ContainsKey(playerID))
            {
                allPlayers[playerID].GetHit(direction);
            }
            else
            {
                Debug.LogErrorFormat("Couldn't find other player with playerID: {0}", playerID);
            }
        }
       
    }

    public void RequestPickupBall(int playerID)
    {
        if (GameState.Instance.isMultiplayer)
        {
            Debug.LogFormat("RequestPickupBall: networkPlayerID = {0}, controlledPlayer.networkNumber +1 = {1}", playerID, controlledPlayer.number + 1);
            if (PhotonNetwork.IsMasterClient && controlledPlayer.number + 1 == playerID)
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
                FireRequestBallPickupPhotonEvent(playerID);
            }
        }
        else
        {
            if (allPlayers.ContainsKey(playerID))
            {
                allPlayers[playerID].HasBall = true;
                currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;
                Destroy(currentBall);
            }
            else
            {
                Debug.LogErrorFormat("Couldn't find other player with playerID: {0}", playerID);
            }
        }
        
    }

    void OnBallCollision()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithSurface -= OnBallCollision;
        GameInput.instance.SetInputEnabled(false);
        currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;

        if (GameState.Instance.isMultiplayer)
            FireMatchEndedPhotonEvent(2.0);

        StartCoroutine(EndMatchCor(2.0f));
    }

    void OnBallOutOfFieldEndGame()
    {
        currentBall.GetComponent<BallCollisionDetector>().OnCollisionWithOutOfField -= OnBallOutOfFieldEndGame;
        GameInput.instance.SetInputEnabled(false);
        currentBall.GetComponent<BallCollisionDetector>().PickedUp = true;

        if (GameState.Instance.isMultiplayer)
            FireMatchEndedPhotonEvent(0.0);

        EndMatch();
    }

    void OnBallOutOfField(Transform throwPoint)
    {
        if (currentState != MatchState.AFTER)
        {
            if (GameState.Instance.isMultiplayer)
            {
                if (currentBall.GetComponent<PhotonView>().OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    int side = throwPoint.position.x > 0 ? -1 : 1;
                    StartCoroutine(DelayedActionCoroutine(1.5f, () => { SpawnBall(throwPoint.position, new Vector2(350 * side, 200), 50.0f * side); }));
                    StartCoroutine(DelayedActionCoroutine(2.0f, () => { throwBack = false; }));
                }
            }
            else
            {
                int side = throwPoint.position.x > 0 ? -1 : 1;
                StartCoroutine(DelayedActionCoroutine(1.5f, () => { SpawnBall(throwPoint.position, new Vector2(350 * side, 200), 50.0f * side); }));
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

    #region Multiplayer only methods

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

    private void FirePointScoredPhotonEvent(int basketID)
    {
        byte evCode = ConnectionManager.PointScoredPhotonEvent;
        object[] content = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, basketID, PhotonNetwork.Time + 1, 5 };
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
        Debug.LogFormat("Sent SendMatchSetup");
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
        InitGameMultiplayer();
    }

    private void FirePlayerHitPhotonEvent(int networkPlayerID, int direction)
    {
        Debug.LogFormat("Sent HitPlayer with values: networkPlayerID = {0}, direction = {1}", networkPlayerID, direction);
        byte evCode = ConnectionManager.PlayerHitPhotonEvent;
        object[] content = new object[] { networkPlayerID, direction };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
    }

    private void FireRequestBallPickupPhotonEvent(int networkPlayerID)
    {
        Debug.LogFormat("Sent RequestPickupBall with values: networkPlayerID = {0}", networkPlayerID);
        byte evCode = ConnectionManager.RequestBallPickupPhotonEvent;
        object[] content = new object[] { networkPlayerID };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        SendOptions sendOptions = new SendOptions { Reliability = true };
        PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
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
        InitGameMultiplayer();
    }

    public void HandlePlayerHitPhotonEvent(EventData photonEvent)
    {
        object[] recievedData = (object[])photonEvent.CustomData;
        int playerNetworkID = (int)recievedData[0];
        if (allPlayers.ContainsKey(playerNetworkID))
        {
            Debug.LogFormat("Recieved HitPlayer with values: networkPlayerID = {0}, direction = {1}", playerNetworkID, (int)recievedData[1]);
            allPlayers[playerNetworkID].GetHit((int)recievedData[1]);
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
        if (allPlayers.ContainsKey(playerNetworkID))
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
        if (controlledPlayer.number + 1 == playerNetworkID)
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

    #endregion
}