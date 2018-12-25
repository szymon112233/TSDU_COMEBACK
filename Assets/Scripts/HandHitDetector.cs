using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHitDetector : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            player.GetHit(Mathf.RoundToInt(gameObject.transform.localScale.x));
        }
    }
}
