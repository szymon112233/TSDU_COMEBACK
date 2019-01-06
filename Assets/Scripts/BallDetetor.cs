using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallDetetor : MonoBehaviour {

    public System.Action balldetected;
    public TSDUPlayer myPLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp && myPLayer.networkNumber + 1 == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp = true;
            if (balldetected != null)
                balldetected.Invoke();
            if (collision.gameObject.GetComponent<PhotonView>().IsMine)
                PhotonNetwork.Destroy(collision.gameObject);
            else
            {
                collision.gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                PhotonNetwork.Destroy(collision.gameObject);
            }
        }
        
    }
}
