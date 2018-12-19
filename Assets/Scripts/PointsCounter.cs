using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PointsCounter : MonoBehaviour {


    public System.Action PointScored;
    bool scorable = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (scorable)
        {
            scorable = false;
            if (PointScored != null)
                PointScored();
            Debug.Log("SCOREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        scorable = true;
    }
}
