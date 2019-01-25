using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfFieldDetector : MonoBehaviour {

    public Transform throwBackPoint;
    public System.Action<Transform> BallOut;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (BallOut != null && throwBackPoint != null && !UniverseManager.instance.throwBack)
        {
            if (GameState.Instance.isMultiplayer)
            {
                if (collision.gameObject.GetComponent<Photon.Pun.PhotonView>().IsMine)
                {
                    UniverseManager.instance.throwBack = true;
                    UniverseManager.instance.targetGroup.RemoveMember(collision.gameObject.transform);
                    BallOut(throwBackPoint);
                }
            }
            else
            {
                UniverseManager.instance.throwBack = true;
                UniverseManager.instance.targetGroup.RemoveMember(collision.gameObject.transform);
                BallOut(throwBackPoint);
            }
            
        }
    }

}
