using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


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

public class MultiplayerConnector : MonoBehaviourPunCallbacks
{
    public PrefabsNetworkPool prefabPool;

    public static System.Action RoomJoined;
    public static System.Action RoomCreated;
    public static System.Action<int> PlayerEnteredRoom;

    public static readonly byte SendMatchSetupPhotonEvent = 0;
    public static readonly byte PlayerHitPhotonEvent = 1;
    public static readonly byte RequestBallPickupPhotonEvent = 2;
    public static readonly byte BallPickedUpPhotonEvent = 3;
    public static readonly byte PointScoredPhotonEvent = 4;
    public static readonly byte MatchEndedPhotonEvent = 5;

    #region Private Serializable Fields


    #endregion


    #region Private Fields


    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";


    #endregion


    #region MonoBehaviour CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.PrefabPool = prefabPool;
    }


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        //PhotonNetwork.SendRate = 60;
        //PhotonNetwork.SerializationRate = 30;
        Debug.LogFormat("Send rate: {0}. serializationRate = {1}", PhotonNetwork.SendRate, PhotonNetwork.SerializationRate);
        Debug.Log("Connect");
        Connect();
    }


    #endregion


    #region Public Methods


    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    #endregion

    #region MonoBehaviourPunCallbacks Callbacks


    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
        PhotonNetwork.JoinRandomRoom();
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions());
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
    }


    #endregion
}
