using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollisionDetector : MonoBehaviour {

    public System.Action OnCollisionWithSurface;
    public bool PickedUp = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (OnCollisionWithSurface != null)
            OnCollisionWithSurface.Invoke();
    }
}
