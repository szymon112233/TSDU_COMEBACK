using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDetetor : MonoBehaviour {

    public System.Action balldetected;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (balldetected != null)
            balldetected.Invoke();
        Destroy(collision.gameObject);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
