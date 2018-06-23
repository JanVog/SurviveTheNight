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

    public Text woodTxt;

    Dictionary<string, Text> uiDict = new Dictionary<string, Text>();

    Dictionary<string, SpawnableObject> prefabDict = new Dictionary<string, SpawnableObject>();

    List<Dictionary<string, int>> resourceListPlayers;
    List<NetworkConnection> connectionList;
    Dictionary<NetworkInstanceId, int> playerIds;

    public override void OnStartServer()
    {
        if (isServer)
        {
            objGrid = new List<GridObject>();
            initPrefabs();
            initMap();
            playerIds = new Dictionary<NetworkInstanceId, int>();
            resourceListPlayers = new List<Dictionary<string, int>>();
            resourceListPlayers.Add(new Dictionary<string, int>() { { "stone", 0 }, { "tree", 0 }, { "coal_stone", 0 }, { "white_tree", 0 } });
            connectionList = new List<NetworkConnection>();
            woodTxt.text = 87.ToString();
        }
    }

    public void addHostPlayer(NetworkInstanceId nid)
    {
        connectionList.Add(null);
        playerIds.Add(nid, 0);
    }

    [Command]
    public void CmdPlayerConnected(NetworkInstanceId nid)
    {
        playerIds.Add(nid, connectionList.Count);
        resourceListPlayers.Add(new Dictionary<string, int>() { { "stone", 0 }, { "tree", 0 }, { "coal_stone", 0 }, {"white_tree", 0} });
        connectionList.Add(NetworkServer.FindLocalObject(nid).gameObject.GetComponent<NetworkIdentity>().connectionToClient);
    }

    void initPrefabs()
    {
        prefabDict.Add("stone", new SpawnableObject(stonePrefab, 15));
        prefabDict.Add("coal_stone", new SpawnableObject(coalStonePrefab, 15));
        prefabDict.Add("tree", new SpawnableObject(treePrefab, 15));
        prefabDict.Add("white_tree", new SpawnableObject(whiteTreePrefab, 15));
        prefabDict.Add("wall_trap", new SpawnableObject(wallTrapPrefab, 15));

        uiDict.Add("tree", woodTxt);
        uiDict.Add("stone", woodTxt);
        uiDict.Add("coal_stone", woodTxt);
        uiDict.Add("white_tree", woodTxt);
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
                    objGrid[objIndex].nid = SpawnResource(objGridOpen[i][grid_index], name);
                    objGridOpen[i].RemoveAt(grid_index);
                }
            }
        }
    }
    
    NetworkInstanceId SpawnResource(int posx, string resName)
    {
        GameObject prefab = getPrefab(resName);

        // Create the resource on the map
        GameObject res = (GameObject) Instantiate(prefab, prefab.transform.position + new Vector3((posx + 0.5f) * 1.28f, 0, -2), prefab.transform.rotation, misc);
        NetworkServer.Spawn(res);
        return res.GetComponent<NetworkIdentity>().netId;
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

    /*public override void OnStartClient()
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
    }*/
    
    public string getObjAtPos(int posx)
    {
        return objGrid[100 + posx].name;
    }

    [Command]
    public void CmdFarmResource(int posx, NetworkInstanceId nid)
    {
        objGrid[100 + posx].hp -= 1;

        //Todo: chance to get coal, etc.
        GameObject prefab = null;
        string objType = getObjAtPos(posx);
        switch (objType)
        {
            case "stone":
                prefab = stonepiecePrefab;
                break;
            case "coal_stone":
                prefab = stonepiecePrefab;
                break;
            case "tree":
                prefab = woodPrefab;
                break;
            case "white_tree":
                prefab = woodPrefab;
                break;
        }

        GameObject player = NetworkServer.FindLocalObject(nid).gameObject;
        if (prefab != null)
        {
            // Create the resource-piece on the map
            GameObject res = (GameObject)Instantiate(prefab, prefab.transform.position + new Vector3((posx + 0.5f) * 1.28f, 0.5f, -2), prefab.transform.rotation, misc);
            Drop ds = res.GetComponent<Drop>();
            ds.target = player.transform;
            ds.landingpos = res.transform.position.x + (Random.value - 0.5f) * 2;
            ds.gc = this;
            ds.objType = objType;
            ds.playerId = playerIds[nid];
            NetworkServer.Spawn(res);
        }

        if (objGrid[100 + posx].hp <= 0)
        {
            objGrid[100 + posx].hp = 0;
            objGrid[100 + posx].name = null;
            NetworkServer.Destroy(NetworkServer.FindLocalObject(objGrid[100 + posx].nid));
            TargetChangeState(player.GetComponent<NetworkIdentity>().connectionToClient);
        }
    }

    [Command]
    public void CmdPickDrop(string objType, int playerId)
    {
        resourceListPlayers[playerId][objType] += 1;
        if (!isServer)
        {
            TargetAddResource(connectionList[playerId], objType, resourceListPlayers[playerId][objType]);
        } else
        {
            addResource(objType, resourceListPlayers[playerId][objType]);
        }
    }

    [TargetRpc]
    public void TargetAddResource(NetworkConnection target, string objType, int value)
    {
        uiDict[objType].text = value.ToString();
    }

    void addResource(string objType, int value)
    {
        uiDict[objType].text = value.ToString();
    }

    [TargetRpc]
    public void TargetChangeState(NetworkConnection target)
    {
        target.playerControllers[0].gameObject.GetComponent<Player>().state = "";
    }
}
