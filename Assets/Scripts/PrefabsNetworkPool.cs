using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PrefabsNetworkPool : MonoBehaviour, IPunPrefabPool
{
    public List<GameObject> networkPrefabs;
    public GameObject currentBall;


    public void Destroy(GameObject gameObject)
    {
        if (gameObject.name != "Ball")
            GameObject.Destroy(gameObject);
        else
        {
            if (currentBall != null)
                currentBall.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                UniverseManager.instance.BallPickedUp = true;
            }
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        int index = -1;
        for (int i = 0; i < networkPrefabs.Count; i++)
        {
            if (networkPrefabs[i].name == prefabId)
            {
                index = i;
                break;
            }  
        }

        if (index == -1)
        {
            Debug.LogErrorFormat("Cannot find Network prefab with name: {0}, returning null :(", prefabId);
            return null;
        }

        switch (prefabId)
        {
            case "Ball":
                if (currentBall == null)
                {
                    Debug.Log("No ball on the scene yet, creating one");
                    currentBall = Instantiate(networkPrefabs[index], position, rotation);
                    currentBall.name = "Ball";
                }
                else
                {
                    currentBall.transform.position = position;
                    currentBall.GetComponent<Rigidbody2D>().velocity = new Vector2();
                    currentBall.GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
                    currentBall.GetComponent<BallCollisionDetector>().PickedUp = false;
                    currentBall.GetComponent<Rigidbody2D>().position = position;
                    currentBall.GetComponent<Rigidbody2D>().rotation = 0.0f;
                }
                if (PhotonNetwork.IsMasterClient)
                {
                    UniverseManager.instance.BallPickedUp = false;
                }
                UniverseManager.instance.currentBall = currentBall;
                return currentBall;
            case "Player":
                Debug.Log("Creating new TSDUPlayer!");
                return Instantiate(networkPrefabs[index], position, rotation);
            default:
                Debug.LogErrorFormat("Cannot find Network prefab with name: {0}, returning null :(", prefabId);
                return null;
        }
        
    }
}
