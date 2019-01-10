using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallDetetor : MonoBehaviour {

    public System.Action balldetected;
    public TSDUPlayer myPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp && myPlayer.networkNumber + 1 == PhotonNetwork.LocalPlayer.ActorNumber)
        {

            UniverseManager.instance.RequestPickupBall((int)myPlayer.networkNumber + 1);       
        }
        
    }
}
