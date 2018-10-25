using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniverseManager : MonoBehaviour {

    #region singleton
    public static UniverseManager instance = null;

    //Awake is always called before any Start functions
    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        InitGame();
    }

    #endregion

    public GameObject ballPrefab;

    //Initializes the game for each level.
    void InitGame()
    {

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SpawnBall(Vector3 position ,Vector2 initialForce = new Vector2())
    {
        GameObject go = Instantiate(ballPrefab, position, Quaternion.identity);
        go.GetComponent<Rigidbody2D>().AddForce(initialForce, ForceMode2D.Impulse);
    }
}
