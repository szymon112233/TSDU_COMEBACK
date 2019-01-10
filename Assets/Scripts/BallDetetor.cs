using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallDetetor : MonoBehaviour {

    public System.Action balldetected;
    public TSDUPlayer myPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogFormat("BallDetetor, OnTriggerEnter2D: {0} ", collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp && myPlayer.networkNumber + 1 == PhotonNetwork.LocalPlayer.ActorNumber);
        if (collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp && myPlayer.networkNumber + 1 == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            
            collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp = true;
            UniverseManager.instance.RequestPickupBall((int)myPlayer.networkNumber + 1);       
        }
        
    }
}
