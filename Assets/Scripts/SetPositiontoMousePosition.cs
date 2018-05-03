using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositiontoMousePosition : MonoBehaviour {

    Transform trans;

	// Use this for initialization
	void Start () {
        trans = transform;
	}
	
	// Update is called once per frame
	void Update () {
        trans.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
	}
}
