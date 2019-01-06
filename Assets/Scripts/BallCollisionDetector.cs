using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollisionDetector : MonoBehaviour {

    public System.Action OnCollisionWithSurface;
    public System.Action OnCollisionWithOutOfField;
    public bool PickedUp = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (OnCollisionWithSurface != null)
            OnCollisionWithSurface.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("OutOfField"))
            if (OnCollisionWithOutOfField != null)
                OnCollisionWithOutOfField.Invoke();
    }

    private void OnDestroy()
    {
        UniverseManager.instance.targetGroup.RemoveMember(gameObject.transform);
    }
}
