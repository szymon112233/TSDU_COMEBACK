using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public PrefabsNetworkPool prefabPool;

    public static System.Action RoomJoined;
    public static System.Action RoomJoinedLastPlayer;
    public static System.Action RoomCreated;
    public static System.Action<int> PlayerEnteredRoom;
    public static System.Action PlayerLeftRoom;
    public static System.Action<string> ConnectionStatusUpdated;
    public static System.Action<string> Disconnected;

    //Game Sate changes
    public static readonly byte SendMatchSetupPhotonEvent = 0;
    public static readonly byte MatchEndedPhotonEvent = 5;
    public static readonly byte LevelLoadedPhotonEvent = 6;
    //Gameplay logic events
    public static readonly byte PlayerHitPhotonEvent = 1;
    public static readonly byte RequestBallPickupPhotonEvent = 2;
    public static readonly byte BallPickedUpPhotonEvent = 3;
    public static readonly byte PointScoredPhotonEvent = 4;
    //Match setup
    public static readonly byte MatchSetupTimeChangedPhotonEvent = 7;
    public static readonly byte MatchSetupBallChangedPhotonEvent = 8;
    public static readonly byte MatchSetupMapChangedPhotonEvent = 9;
    public static readonly byte MatchSetupPlayerChangedPhotonEvent = 10;
    public static readonly byte MatchSetupPlayerReadyPhotonEvent = 11;
    public static readonly byte MatchSetupStartMatchPhotonEvent = 12;

    #region singleton
    public static ConnectionManager instance = null;

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

    #endregion

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";
    bool randomRoom = true;
    string secretPhase = string.Empty;
    RoomOptions roomOptions = new RoomOptions();


    private void Init()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.PrefabPool = prefabPool;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (scene.name.Contains("COURT") && !PhotonNetwork.IsMasterClient)
        {
            int networkPlayerID = PhotonNetwork.LocalPlayer.ActorNumber;
            Debug.LogFormat("Sent LevelLoaded with values: networkPlayerID = {0}", networkPlayerID);
            byte evCode = LevelLoadedPhotonEvent;
            object[] content = new object[] { networkPlayerID };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(evCode, content, raiseEventOptions, sendOptions);
        }
        
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        //PhotonNetwork.SendRate = 60;
        //PhotonNetwork.SerializationRate = 30;
        Debug.LogFormat("Send rate: {0}. serializationRate = {1}", PhotonNetwork.SendRate, PhotonNetwork.SerializationRate); 
    }

    public void ConnectRandomRoom()
    {
        randomRoom = true;
        Connect();
    }

    public void ConnectFriendsRoom(string secret)
    {
        secretPhase = secret;
        randomRoom = false;
        Connect();
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// Start the connection process.
    /// Connect this application instance to Photon Cloud Network
    /// </summary>
    private void Connect()
    {
        Debug.Log("Connect");
        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        if (ConnectionStatusUpdated != null)
            ConnectionStatusUpdated("Connecting to master server...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = false;

        if (randomRoom)
        {
            PhotonNetwork.JoinRandomRoom();
            if (ConnectionStatusUpdated != null)
                ConnectionStatusUpdated("Connected to master server! Looking for a room to join...");
        }
            
        else
        {
            PhotonNetwork.JoinOrCreateRoom(secretPhase, roomOptions, TypedLobby.Default);
            if (ConnectionStatusUpdated != null)
                ConnectionStatusUpdated("Connected to master server! Looking for a room to join...");
        }
            
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        if (ConnectionStatusUpdated != null)
            ConnectionStatusUpdated("Disconnected! Reason: " + cause);
        if (Disconnected != null)
            Disconnected(cause.ToString());
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("COURT"))
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogFormat("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. Error Code: {0}, Message: {1}", returnCode, message);
        if (ConnectionStatusUpdated != null)
            ConnectionStatusUpdated("Can't find any room, creating a new one...");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        //if (ConnectionStatusUpdated != null)
        //    ConnectionStatusUpdated("Room found! Waiting for other player...");
        if (RoomJoined != null)
            RoomJoined();

        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            if (RoomJoinedLastPlayer != null)
                RoomJoinedLastPlayer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerDisconnected");
        if (ConnectionStatusUpdated != null)
            ConnectionStatusUpdated("Player " + otherPlayer.NickName+ " disconnected, sorry :(");

        if (PlayerLeftRoom != null)
            PlayerLeftRoom();
    }

    public override void OnCreatedRoom()
    {
        if (ConnectionStatusUpdated != null)
            ConnectionStatusUpdated("Room created! Waiting for other player...");
        if (RoomCreated != null)
            RoomCreated();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (ConnectionStatusUpdated != null)
            ConnectionStatusUpdated("Player " + newPlayer.NickName + " joined, starting match setup!");
        if (PlayerEnteredRoom != null)
            PlayerEnteredRoom(PhotonNetwork.CurrentRoom.PlayerCount);
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            if (RoomJoinedLastPlayer != null)
                RoomJoinedLastPlayer();
    }
}
