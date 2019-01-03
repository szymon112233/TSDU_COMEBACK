using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawnerMenu : MonoBehaviour {

    public float MinX;
    public float MaxX;
    public float spawnTime = 0.5f;
    private float spawnTimer = 0.0f;
    private int currentBall = 0;

    public GameObject ballPrefab;
    public GameObject[] ballsPool = new GameObject[500];

	// Use this for initialization
	void Start () {
        MinX =  - Screen.width / 2;
        MaxX = Screen.width / 2;

        for (int i = 0; i < ballsPool.Length; i++)
        {
            ballsPool[i] = Instantiate(ballPrefab);
            ballsPool[i].transform.position = new Vector3(10000, 10000, 10000);
            ballsPool[i].SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            spawnTime = 0.0f;
            ballsPool[currentBall].transform.position = new Vector3(Random.Range(MinX, MaxX), Screen.height, 100);
            ballsPool[currentBall].GetComponent<Rigidbody2D>().velocity = new Vector2();
            ballsPool[currentBall].SetActive(true);
            currentBall++;
            if (currentBall > ballsPool.Length - 1)
                currentBall = 0;

        }
	}
}
