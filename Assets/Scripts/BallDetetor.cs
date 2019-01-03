using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallDetetor : MonoBehaviour {

    public System.Action balldetected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != null && !collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp)
        {
            collision.gameObject.GetComponent<BallCollisionDetector>().PickedUp = true;
            if (balldetected != null)
                balldetected.Invoke();
            PhotonNetwork.Destroy(collision.gameObject);
        }
        
    }
}
