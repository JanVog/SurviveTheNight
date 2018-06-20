using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{
    List<int>[] objGridOpen;
    List<int>[] objGridClosed;
    SyncListInt objGrid = new SyncListInt();

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
            for (int i = 0; i < 100; i++)
            {
                objGrid.Add(0);
            }
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
        if (isServer)
        {
            for (int i = 0; i < 20; i++)
            {
                while (objGridOpen[i].Count > 5)
                {
                    int resNo = Random.Range(1, 100);
                    int index = Random.Range(0, objGridOpen[i].Count - 1);
                    Debug.Log(index);
                    int objIndex = 50 + objGridOpen[i][index];
                    objGrid[objIndex] = resNo;
                    objGrid.Dirty(objIndex);
                    objGridClosed[i].Add(objGridOpen[i][index]);
                    RpcSpawnResource(objGridOpen[i][index], resNo);
                    objGridOpen[i].RemoveAt(index);
                }
            }
        }
    }

    [ClientRpc]
    void RpcSpawnResource(int posx, int resNo)
    {
        Debug.Log("Objects Spawning");
        GameObject prefab = getPrefab(resNo);

        // Create the resource on the map
        Instantiate(prefab, prefab.transform.position + new Vector3(posx * 1.28f, 0, 0), prefab.transform.rotation);
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
        for (int i = 0; i < 100; i++)
        {
            if (objGrid[i] != 0)
            {
                GameObject prefab = getPrefab(objGrid[i]);
                Instantiate(prefab, prefab.transform.position + new Vector3((i - 50) * 1.28f, 0, 0), prefab.transform.rotation);
            }
        }
    }
}
