using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    public class GridObject
    {
        public string name;
        public int hp;
        public NetworkInstanceId nid;
    }

    public class SpawnableObject
    {
        public GameObject prefab;
        public int hp;

        public SpawnableObject(GameObject prefab, int hp)
        {
             this.prefab = prefab;
             this.hp = hp;
        }
    }

    List<GridObject> objGrid;
    List<int>[] objGridOpen;
    List<int>[] objGridClosed;

    public GameObject stonePrefab;
    public GameObject coalStonePrefab;
    public GameObject whiteTreePrefab;
    public GameObject treePrefab;
    public GameObject wallTrapPrefab;
    public GameObject stonepiecePrefab;
    public GameObject woodPrefab;

    public Transform misc;
    public NetworkManager nm;

    public Text woodTxt;

    Dictionary<string, SpawnableObject> prefabDict = new Dictionary<string, SpawnableObject>();

    public override void OnStartServer()
    {
        if (isServer)
        {
            objGrid = new List<GridObject>();
            initPrefabs();
            initMap();
            woodTxt.text = 87.ToString();
        }
    }

    void initPrefabs()
    {
        prefabDict.Add("stone", new SpawnableObject(stonePrefab, 15));
        prefabDict.Add("coal_stone", new SpawnableObject(coalStonePrefab, 15));
        prefabDict.Add("tree", new SpawnableObject(treePrefab, 15));
        prefabDict.Add("white_tree", new SpawnableObject(whiteTreePrefab, 15));
        prefabDict.Add("wall_trap", new SpawnableObject(wallTrapPrefab, 15));
    }

    void initMap()
    {
        if (isServer)
        {
            for (int i = 0; i < 200; i++)
            {
                objGrid.Add(new GridObject());
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
                    name = getObjName(resNo);
                    objGrid[objIndex].name = name;
                    objGrid[objIndex].hp = prefabDict[name].hp;
                    objGridClosed[i].Add(objGridOpen[i][grid_index]);
                    SpawnResource(objGridOpen[i][grid_index], name);
                    objGridOpen[i].RemoveAt(grid_index);
                }
            }
        }
    }
    
    void SpawnResource(int posx, string resName)
    {
        GameObject prefab = getPrefab(resName);

        // Create the resource on the map
        GameObject res = (GameObject) Instantiate(prefab, prefab.transform.position + new Vector3(posx * 1.28f, 0, -2), prefab.transform.rotation, misc);
        NetworkServer.Spawn(res);
    }

    public GameObject getPrefab(string res)
    {
        return prefabDict[res].prefab;
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
        foreach (GridObject o in objGrid)
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
                if (objGrid[i].name != "")
                {
                    GameObject prefab = getPrefab(objGrid[i].name);
                    Instantiate(prefab, prefab.transform.position + new Vector3((i - 100) * 1.28f, 0, 0), prefab.transform.rotation, misc);
                }
            }
        }
    }
    
    public string getObjAtPos(int posx)
    {
        return objGrid[100 + posx].name;
    }

    [Command]
    public void CmdFarmResource(int posx, NetworkConnection connection)
    {
        objGrid[100 + posx].hp -= 1;

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

        if (objGrid[100 + posx].hp <= 0)
        {
            objGrid[100 + posx].hp = 0;
            objGrid[100 + posx].name = null;
            NetworkServer.Destroy(NetworkServer.FindLocalObject(objGrid[100 + posx].nid));
            TargetChangeState(connection);

        }
    }

    [TargetRpc]
    public void TargetChangeState(NetworkConnection connecton)
    {
        nm.client.connection.playerControllers[0].gameObject.GetComponent<Player>().state = "";
    }
}
