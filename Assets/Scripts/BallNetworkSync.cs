


using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class BallNetworkSync : MonoBehaviour, IPunObservable
{

    public int UpdatePosAndRotRate = 8;
    private int updatesLeft = 0;
    private bool shouldupdate = true;

    private Rigidbody2D m_Body;

    private PhotonView m_PhotonView;

    private Vector2 m_NetworkPosition;

    private float m_NetworkRotation;


    public void Awake()
    {
        this.m_Body = GetComponent<Rigidbody2D>();
        this.m_PhotonView = GetComponent<PhotonView>();

        this.m_NetworkPosition = new Vector2();
    }

    public void FixedUpdate()
    {
        if (!this.m_PhotonView.IsMine && shouldupdate)
        {
            //this.m_Body.position = this.m_NetworkPosition;
            this.m_Body.rotation = this.m_NetworkRotation;
            this.m_Body.position = Vector2.Lerp(this.m_Body.position, this.m_NetworkPosition, Time.deltaTime * UpdatePosAndRotRate);
            //this.m_Body.rotation = Mathf.MoveTowards(this.m_Body.rotation, this.m_NetworkRotation, 1.0f);
            shouldupdate = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.m_Body.position);
            stream.SendNext(this.m_Body.rotation);

            stream.SendNext(this.m_Body.velocity);
            stream.SendNext(this.m_Body.angularVelocity);
        }
        else
        {
            if (updatesLeft > 0)
                updatesLeft--;
            else
            {
                shouldupdate = true;
                updatesLeft = UpdatePosAndRotRate;
                this.m_NetworkPosition = (Vector2)stream.ReceiveNext();
                this.m_NetworkRotation = (float)stream.ReceiveNext();

                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));

                this.m_Body.velocity = (Vector2)stream.ReceiveNext();
                this.m_NetworkPosition += this.m_Body.velocity * lag;

                this.m_Body.angularVelocity = (float)stream.ReceiveNext();
                this.m_NetworkRotation += this.m_Body.angularVelocity * lag;
            }
        }

            
    }
}
