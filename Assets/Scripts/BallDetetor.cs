using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallDetetor : MonoBehaviour {

    public TSDUPlayer myPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameState.Instance.isMultiplayer)
        {
            Debug.LogFormat("BallDetetor, OnTriggerEnter2D: {0} ", collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp && myPlayer.number + 1 == PhotonNetwork.LocalPlayer.ActorNumber);
            if (collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp && myPlayer.number + 1 == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp = true;
                UniverseManager.instance.RequestPickupBall((int)myPlayer.number + 1);
            }
        }
        else
        {
            if (collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp)
            {
                collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp = true;
                UniverseManager.instance.RequestPickupBall((int)myPlayer.number + 1);
            }
        }
    }
}
