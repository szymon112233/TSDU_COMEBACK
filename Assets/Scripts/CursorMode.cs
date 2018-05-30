using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorMode : MonoBehaviour {


    public GameObject[] prefabs;
    public GameObject currentObject;
    public GameObject line;
    public float rotationValue = 15.0f;

    private int currentObjectIndex = 0;
    private Quaternion spawnRotation;
    bool throwMode = false;
    LineRenderer lineRenderer;
    GameObject thrownObject;

    

    private void Awake()
    {
        lineRenderer = line.GetComponent<LineRenderer>();
        line.SetActive(false);
    }

    void Update () {
        if (!throwMode)
        {
            if (Input.GetKeyDown(KeyCode.A))
                PrevObject();
            else if (Input.GetKeyDown(KeyCode.D))
                NextObject();
            else if (Input.GetKeyDown(KeyCode.E))
                RotateObjectClockwise();
            else if (Input.GetKeyDown(KeyCode.Q))
                RotateObjectCounterclockwise();
            else if (Input.GetMouseButtonDown(0))
                LeftClick();
            else if (Input.GetKeyDown(KeyCode.Mouse1))
                StartThrowing();
        }
        else
        {
            lineRenderer.SetPosition(1, transform.position);
            if (Input.GetKeyUp(KeyCode.Mouse1))
                EndThrowing();
        }
        
        
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

    void LeftClick()
    {
        if (currentObject.name == "Deleter(Clone)")
            Delete();
        else
            Spawn();

    }

    void Delete()
    {
        Collider2D[] colliders = new Collider2D[10];
        if (currentObject.GetComponentInChildren<CircleCollider2D>().OverlapCollider(new ContactFilter2D(), colliders) > 0)
        {
            
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    Destroy(colliders[i].gameObject);
            }
        }
    }

    void Spawn()
    {
        Instantiate(prefabs[currentObjectIndex], transform.position, spawnRotation);
    }


    void StartThrowing()
    {
        if (prefabs[currentObjectIndex].GetComponent<Rigidbody2D>() == null)
            return;
        throwMode = true;
        thrownObject = Instantiate(prefabs[currentObjectIndex], transform.position, Quaternion.identity);
        thrownObject.GetComponent<Rigidbody2D>().simulated = false;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        currentObject.SetActive(false);
        line.SetActive(true);
        
    }

    void EndThrowing()
    {
        Vector2 throwForce = new Vector2((lineRenderer.GetPosition(0) - lineRenderer.GetPosition(1)).x, (lineRenderer.GetPosition(0) - lineRenderer.GetPosition(1)).y);
        thrownObject.GetComponent<Rigidbody2D>().simulated = true;
        thrownObject.GetComponent<Rigidbody2D>().AddForce(throwForce, ForceMode2D.Impulse);
        thrownObject = null;
        throwMode = false;
        currentObject.SetActive(true);
        line.SetActive(false);
    }
}
