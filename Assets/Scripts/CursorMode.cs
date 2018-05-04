using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorMode : MonoBehaviour {


    public GameObject[] prefabs;

    private int currentObjectIndex = 0;
    private Quaternion spawnRotation;

    public GameObject currentObject;

    public float rotationValue = 15.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.A))
            PrevObject();
        else if (Input.GetKeyDown(KeyCode.D))
            NextObject();
        else if (Input.GetMouseButtonDown(0))
            Spawn();
        else if (Input.GetKeyDown(KeyCode.E))
            RotateObjectClockwise();
        else if (Input.GetKeyDown(KeyCode.Q))
            RotateObjectCounterclockwise();
    }

    void RotateObjectClockwise()
    {
        currentObject.transform.rotation *= Quaternion.AngleAxis(rotationValue, Vector3.back);
        spawnRotation = currentObject.transform.rotation;
    }
    void RotateObjectCounterclockwise()
    {
        currentObject.transform.rotation *= Quaternion.AngleAxis(-rotationValue, Vector3.back);
        spawnRotation = currentObject.transform.rotation;
    }

    void NextObject()
    {
        DestroyImmediate(currentObject);
        spawnRotation = Quaternion.identity;
        if (currentObjectIndex + 1 != prefabs.Length)
        {
            currentObjectIndex++;
        }
        else
        {
            currentObjectIndex = 0;
        }
        currentObject = Instantiate(prefabs[currentObjectIndex], transform);
        Rigidbody2D rb = currentObject.GetComponent<Rigidbody2D>();
        if (rb != null)
            Destroy(rb);
    }

    void PrevObject()
    {
        DestroyImmediate(currentObject);
        spawnRotation = Quaternion.identity;
        if (currentObjectIndex - 1 != -1)
        {
            currentObjectIndex--;
        }
        else
        {
            currentObjectIndex = prefabs.Length - 1;
        }
        currentObject = Instantiate(prefabs[currentObjectIndex], transform);
        Rigidbody2D rb = currentObject.GetComponent<Rigidbody2D>();
        if (rb != null)
            Destroy(rb);
    }


    void Spawn()
    {
        Instantiate(prefabs[currentObjectIndex], transform.position, spawnRotation);
    }
}
