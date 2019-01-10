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
            UniverseManager.instance.throwBack = true;
            BallOut(throwBackPoint);
        }
    }

}
