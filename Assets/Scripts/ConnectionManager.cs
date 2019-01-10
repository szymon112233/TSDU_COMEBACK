using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

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

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public PrefabsNetworkPool prefabPool;

    public static System.Action RoomJoined;
    public static System.Action RoomCreated;
    public static System.Action<int> PlayerEnteredRoom;
    public static System.Action<string> ConnectionStatusUpdated;

    public static readonly byte SendMatchSetupPhotonEvent = 0;
    public static readonly byte PlayerHitPhotonEvent = 1;
    public static readonly byte RequestBallPickupPhotonEvent = 2;
    public static readonly byte BallPickedUpPhotonEvent = 3;
    public static readonly byte PointScoredPhotonEvent = 4;
    public static readonly byte MatchEndedPhotonEvent = 5;
    public static readonly byte LevelLoadedPhotonEvent = 6;

    #region singleton
    public static ConnectionManager instance = null;

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
        if (scene.buildIndex == 1 && !PhotonNetwork.IsMasterClient)
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
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = false;

        if (randomRoom)
            PhotonNetwork.JoinRandomRoom();
        else
            PhotonNetwork.JoinOrCreateRoom(secretPhase, roomOptions, TypedLobby.Default);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogFormat("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. Error Code: {0}, Message: {1}", returnCode, message);

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        if (RoomJoined != null)
            RoomJoined();
    }

    public override void OnCreatedRoom()
    {
        if (RoomCreated != null)
            RoomCreated();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PlayerEnteredRoom != null)
            PlayerEnteredRoom(PhotonNetwork.CurrentRoom.PlayerCount);
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            PhotonNetwork.LoadLevel(1);
    }
}
