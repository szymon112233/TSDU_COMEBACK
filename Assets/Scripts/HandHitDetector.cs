using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHitDetector : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        TSDUPlayer player = collision.transform.parent.parent.gameObject.GetComponent<TSDUPlayer>();
        if (player != null)
        {
            UniverseManager.instance.HitPlayer((int)player.number + 1, Mathf.RoundToInt(gameObject.transform.parent.localScale.x));
        }
    }
}
