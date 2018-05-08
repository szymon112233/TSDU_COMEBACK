using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamerController : MonoBehaviour {

    Transform mainCameraTransform;
    Camera mainCamera;

    float moveSpeed = 5.0f;
    float zoomSpeed = 5.0f;
    float sizeFactor = 160.0f;

    private void Awake()
    {
        mainCameraTransform = Camera.main.gameObject.transform;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector3 mouseVector = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0.0f);
            mouseVector *= moveSpeed;
            mouseVector *= mainCamera.orthographicSize / sizeFactor;
            mainCameraTransform.position += mouseVector;
        }
        if (Input.mouseScrollDelta.y != 0.0f)
        {
            mainCamera.orthographicSize -= zoomSpeed * Input.mouseScrollDelta.y;
        }
	}
}
