using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{
    List<int>[] objGridOpen;
    List<int>[] objGridClosed;
    [SyncVar]
    int[] objGrid;

    public GameObject stonePrefab;
    public GameObject coalStonePrefab;
    public GameObject whiteTreePrefab;
    public GameObject treePrefab;

    public override void OnStartServer()
    {
        initMap();
    }

    void initMap()
    {
        if (isServer)
        {
            objGrid = new int[100];
            objGridOpen = new List<int>[20];
            for (int i = -10; i < 10; i++)
            {
                objGridOpen[i + 10] = new List<int>(new int[] { i * 10, i * 10 + 1, i * 10 + 2, i * 10 + 3, i * 10 + 4, i * 10 + 5, i * 10 + 6, i * 10 + 7, i * 10 + 8, i * 10 + 9 });
            }
            objGridClosed = new List<int>[20];
            for (int i = 0; i < 20; i++)
            {
                objGridClosed[i] = new List<int>();
            }

            InvokeRepeating("refillWorld", 0.0f, 20.0f);
        }
    }

    void refillWorld()
    {
        for (int i = 0; i < 20; i++)
        {
            while (objGridOpen[i].Count > 5)
            {
                int resNo = Random.Range(1, 100);
                int index = Random.Range(0, objGridOpen[i].Count - 1);
                objGridClosed[i].Add(objGridOpen[i][index]);
                objGridOpen[i].RemoveAt(index);
                objGrid[50 + objGridOpen[i][index]] = resNo;
                RpcSpawnResource(objGridOpen[i][index], resNo);
            }
        }
    }

    [ClientRpc]
    void RpcSpawnResource(int posx, int resNo)
    {
        Debug.Log("Objects Spawning");
        GameObject prefab = getPrefab(resNo);

        // Create the resource on the map
        var res = (GameObject)Instantiate(prefab, prefab.transform.position + new Vector3(posx * 1.28f, 0, 0), prefab.transform.rotation);
    }

    public GameObject getPrefab(int resNo)
    {
        if (resNo <= 5)
        {
            return whiteTreePrefab;
        }
        else if (resNo <= 10)
        {
            return coalStonePrefab;
        }
        else if (resNo <= 55)
        {
            return stonePrefab;
        }
        else
        {
            return treePrefab;
        }
    }

    public override void OnStartClient()
    {
        Debug.Log("Client started");
        for (int i = 0; i < 100; i++)
        {
            if (objGrid[i] != 0)
            {
                GameObject prefab = getPrefab(objGrid[i]);
                var res = (GameObject)Instantiate(prefab, prefab.transform.position + new Vector3((i - 50) * 1.28f, 0, 0), prefab.transform.rotation);
            }
        }
    }
}
