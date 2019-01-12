using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHitDetector : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TSDUPlayer player = collision.gameObject.GetComponent<TSDUPlayer>();
        if (player != null)
        {
            //player.GetHit(Mathf.RoundToInt(gameObject.transform.localScale.x));
            UniverseManager.instance.HitPlayer((int)player.networkNumber + 1, Mathf.RoundToInt(gameObject.transform.parent.localScale.x));
        }
    }
}
