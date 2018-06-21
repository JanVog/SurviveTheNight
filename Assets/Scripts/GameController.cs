using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    List<int>[] objGridOpen;
    List<int>[] objGridClosed;
    SyncListInt objGrid = new SyncListInt();
    SyncListInt objGridHp = new SyncListInt();

    public GameObject stonePrefab;
    public GameObject coalStonePrefab;
    public GameObject whiteTreePrefab;
    public GameObject treePrefab;
    public GameObject wallTrapPrefab;
    public GameObject stonepiecePrefab;
    public GameObject woodPrefab;

    public Transform misc;

    public Text woodTxt;

    Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    public override void OnStartServer()
    {
        initPrefabs();
        initMap();
        woodTxt.text = 87.ToString();
    }

    void initPrefabs()
    {
        prefabDict.Add("stone", stonePrefab);
        prefabDict.Add("coal_stone", coalStonePrefab);
        prefabDict.Add("tree", treePrefab);
        prefabDict.Add("white_tree", whiteTreePrefab);
        prefabDict.Add("wall_trap", wallTrapPrefab);
    }

    void initMap()
    {
        if (isServer)
        {
            for (int i = 0; i < 200; i++)
            {
                objGrid.Add(0);
                objGridHp.Add(0);
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
                    objGridHp[objIndex] = 15;
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
        GameObject res = (GameObject) Instantiate(prefab, prefab.transform.position + new Vector3(posx * 1.28f, 0, -2), prefab.transform.rotation, misc);
        NetworkServer.Spawn(res);
    }

    public GameObject getPrefab(int resNo)
    {
        return prefabDict[getObjName(resNo)];   
    }

    public string getObjName(int resNo)
    {
        if (resNo == 0)
        {
            return "";
        }
        else if (resNo <= 5)
        {
            return "white_tree";
        }
        else if (resNo <= 10)
        {
            return "coal_stone";
        }
        else if (resNo <= 55)
        {
            return "stone";
        }
        else if (resNo <= 100)
        {
            return "tree";
        }
        else if (resNo == 101)
        {
            return "wall_trap";
        }
        else
        {
            return "";
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
        // Todo: Check if needed
        if (!isServer && false)
        {
            for (int i = 0; i < 200; i++)
            {
                if (objGrid[i] != 0)
                {
                    GameObject prefab = getPrefab(objGrid[i]);
                    Instantiate(prefab, prefab.transform.position + new Vector3((i - 100) * 1.28f, 0, 0), prefab.transform.rotation, misc);
                }
            }
        }
    }
    
    public string getObjAtPos(int posx)
    {
        return getObjName(objGrid[100 + posx]);
    }

    [Command]
    public void CmdFarmResource(int posx)
    {
        objGridHp[100 + posx] -= 1;

        //Todo: chance to get coal, etc.
        GameObject prefab = null;
        switch (getObjAtPos(posx))
        {
            case "stone":
                prefab = stonepiecePrefab;
                break;
            case "coal_stone":
                prefab = coalStonePrefab;
                break;
            case "tree":
                prefab = woodPrefab;
                break;
            case "white_tree":
                prefab = woodPrefab;
                break;
        }
        Debug.Log(getObjAtPos(posx));

        // Create the resource-piece on the map
        GameObject res = (GameObject)Instantiate(prefab, prefab.transform.position + new Vector3((posx + Random.value * 2 - 1) * 1.28f, 0, -2), prefab.transform.rotation, misc);
        NetworkServer.Spawn(res);

        if (objGridHp[100 + posx] <= 0)
        {
            objGridHp[100 + posx] = 0;
            objGrid[100 + posx] = 0;
            objGrid.Dirty(100 + posx);
        }
        objGridHp.Dirty(100 + posx);
        //Todo: Change snylists to struct, add networkinstanceid to find gameobjects and remove them
    }
}
