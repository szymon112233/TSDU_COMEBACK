using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    public System.Action OnTriggerEnterEv;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnTriggerEnterEv != null)
            OnTriggerEnterEv.Invoke();
    }
}