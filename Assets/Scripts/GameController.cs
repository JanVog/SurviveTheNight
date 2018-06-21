﻿using System.Collections;
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
            for (int i = 0; i < 200; i++)
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
                    int grid_index = Random.Range(0, objGridOpen[i].Count - 1);
                    int objIndex = 100 + objGridOpen[i][grid_index];
                    objGrid[objIndex] = resNo;
                    objGrid.Dirty(objIndex);
                    objGridClosed[i].Add(objGridOpen[i][grid_index]);
                    SpawnResource(objGridOpen[i][grid_index], resNo);
                    objGridOpen[i].RemoveAt(grid_index);
                }
            }
        }
    }
    
    void SpawnResource(int posx, int resNo)
    {
        GameObject prefab = getPrefab(resNo);

        // Create the resource on the map
        GameObject res = (GameObject) Instantiate(prefab, prefab.transform.position + new Vector3(posx * 1.28f, 0, 0), prefab.transform.rotation);
        NetworkServer.Spawn(res);
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

    [Command]
    public void CmdPrintGrid()
    {
        foreach (int o in objGrid)
        {
            Debug.Log(o);
        }
    }

    public override void OnStartClient()
    {
        if (!isServer)
        {
            for (int i = 0; i < 200; i++)
            {
                if (objGrid[i] != 0)
                {
                    Debug.Log("Creating resource");
                    GameObject prefab = getPrefab(objGrid[i]);
                    Instantiate(prefab, prefab.transform.position + new Vector3((i - 100) * 1.28f, 0, 0), prefab.transform.rotation);
                }
            }
        }
    }
}
