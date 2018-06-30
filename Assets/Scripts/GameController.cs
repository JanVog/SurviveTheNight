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
    public GameObject coalPrefab;

    public GameObject enemyPrefab;

    public Transform misc;
    public Light sun;
    public Light sunBack;
    public List<Light> playerLights;

    public Text woodTxt;
    public Text stoneTxt;
    public Text coalTxt;

    Dictionary<string, Text> uiDict = new Dictionary<string, Text>();

    Dictionary<string, SpawnableObject> prefabDict = new Dictionary<string, SpawnableObject>();
    int enemyCount = 0;

    List<Dictionary<string, int>> resourceListPlayers;
    List<NetworkConnection> connectionList;
    Dictionary<NetworkInstanceId, int> playerIds;
    List<GameObject> playerGos;

    //Todo check if value changes although theres a hook
    [SyncVar(hook="changeSceneToDay")]
    int day = 0;
    int dayLength;

    private void Start()
    {
        playerLights = new List<Light>();
    }

    public override void OnStartServer()
    {
        if (isServer)
        {
            objGrid = new List<GridObject>();
            playerGos = new List<GameObject>();
            initPrefabs();
            initMap();
            playerIds = new Dictionary<NetworkInstanceId, int>();
            resourceListPlayers = new List<Dictionary<string, int>>();
            resourceListPlayers.Add(new Dictionary<string, int>() { { "stone", 0 }, { "tree", 0 }, { "coal_stone", 0 }, { "white_tree", 0 } });
            connectionList = new List<NetworkConnection>();

            dayLength = Random.Range(25, 35);
            //Invoke("changeToNight", dayLength);
            Invoke("changeToNight", 2);
        }
    }

    public void addHostPlayer(NetworkInstanceId nid)
    {
        connectionList.Add(null);
        playerIds.Add(nid, 0);
        playerGos.Add(NetworkServer.FindLocalObject(nid));
    }

    [Command]
    public void CmdPlayerConnected(NetworkInstanceId nid)
    {
        GameObject tmpplayer = NetworkServer.FindLocalObject(nid);
        playerIds.Add(nid, connectionList.Count);
        resourceListPlayers.Add(new Dictionary<string, int>() { { "stone", 0 }, { "tree", 0 }, { "coal_stone", 0 }, {"white_tree", 0} });
        connectionList.Add(tmpplayer.GetComponent<NetworkIdentity>().connectionToClient);
        playerGos.Add(tmpplayer);
    }

    void initPrefabs()
    {
        prefabDict.Add("stone", new SpawnableObject(stonePrefab, 15));
        prefabDict.Add("coal_stone", new SpawnableObject(coalStonePrefab, 15));
        prefabDict.Add("tree", new SpawnableObject(treePrefab, 15));
        prefabDict.Add("white_tree", new SpawnableObject(whiteTreePrefab, 15));
        prefabDict.Add("wall_trap", new SpawnableObject(wallTrapPrefab, 15));
        prefabDict.Add("enemy", new SpawnableObject(enemyPrefab, 100));

        uiDict.Add("tree", woodTxt);
        uiDict.Add("stone", stoneTxt);
        uiDict.Add("coal_stone", coalTxt);
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
                    string objName = getObjName(resNo);
                    objGrid[objIndex].name = objName;
                    objGrid[objIndex].hp = prefabDict[objName].hp;
                    objGridClosed[i].Add(objGridOpen[i][grid_index]);
                    objGrid[objIndex].nid = SpawnResource(objGridOpen[i][grid_index], objName);
                    objGridOpen[i].RemoveAt(grid_index);
                }
            }
        }
    }
    
    NetworkInstanceId SpawnResource(int posx, string resName)
    {   
        GameObject prefab = getPrefab(resName);

        // Create the resource on the map
        GameObject res = (GameObject) Instantiate(prefab, prefab.transform.position + new Vector3((posx + 0.5f) * 1.28f, 0, -2 - (posx % 2) / 2.0f), prefab.transform.rotation, misc);
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
        
        GameObject prefab = null;
        float rnd = Random.value;
        string objType = getObjAtPos(posx);
        switch (objType)
        {
            case "stone":
                if (rnd <= 0.05f) {
                    prefab = coalPrefab;
                    objType = "coal_stone";
                } else
                {
                    prefab = stonepiecePrefab;
                }
                break;
            case "coal_stone":
                if (rnd <= 0.3f)
                {
                    prefab = coalPrefab;
                    objType = "coal_stone";
                }
                else
                {
                    prefab = stonepiecePrefab;
                }
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
            Vector3 pos = prefab.transform.position + new Vector3((posx + 0.5f) * 1.28f, 0.5f, -7);
            GameObject res = (GameObject)Instantiate(prefab, pos, prefab.transform.rotation, misc);
            Drop ds = res.GetComponent<Drop>();
            ds.target = player.transform;
            float rndx = (Random.value - 0.5f) * 2;
            ds.landingpos = res.transform.position.x + rndx + (rndx >= 0 ? 0.5f : -0.5f);
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

    void SpawnEnemy()
    {
        enemyCount++;
        GameObject prefab = prefabDict["enemy"].prefab;
        float rnd = Random.value;
        GameObject enemy;
        if (rnd < 0.5f)
        {
            float minx = 0;
            foreach(GameObject p in playerGos)
            {
                if (p.transform.position.x < minx)
                    minx = p.transform.position.x;
            }
            enemy = (GameObject)Instantiate(prefab, prefab.transform.position - new Vector3(-minx + 25, 0, 0.01f * enemyCount), prefab.transform.rotation);
        } else
        {
            float maxx = 0;
            foreach (GameObject p in playerGos)
            {
                if (p.transform.position.x > maxx)
                    maxx = p.transform.position.x;
            }
            enemy = (GameObject)Instantiate(prefab, prefab.transform.position - new Vector3(-maxx - 25, 0, 0.01f * enemyCount), prefab.transform.rotation);
        }
        NetworkServer.Spawn(enemy);
    }

    void changeToDay()
    {
        CancelInvoke("SpawnEnemy");
        day += 1;
        dayLength = Random.Range(25, 35);
        Invoke("changeToNight", dayLength);
    }

    void changeSceneToDay(int day)
    {
        sun.intensity = 0.7f;
        sunBack.intensity = 0.7f;
        foreach (Light light in playerLights)
        {
            light.intensity = 0;
        }
    }

    void changeToNight()
    {
        InvokeRepeating("SpawnEnemy", 0, 5 - 4 * day);
        RpcChangeToNight();
        Invoke("changeToDay", 60 - dayLength);
    }

    [ClientRpc]
    void RpcChangeToNight()
    {
        sun.intensity = 0;
        sunBack.intensity = 0;
        foreach(Light light in playerLights)
        {
            light.intensity = 15;
        }
    }

    [Command]
    public void CmdAddPlayerLights(NetworkInstanceId nid)
    {
        RpcAddPlayerLight(nid);
    }

    [ClientRpc]
    void RpcAddPlayerLight(NetworkInstanceId nid)
    {
        Transform player = ClientScene.FindLocalObject(nid).transform;
        playerLights.Add(player.GetChild(player.childCount - 1).GetComponent<Light>());
        playerLights.Add(player.GetChild(player.childCount - 2).GetComponent<Light>());
    }

}
