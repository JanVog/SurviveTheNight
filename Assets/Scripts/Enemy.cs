﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Enemy : NetworkBehaviour {
    
    public static List<Transform> players = new List<Transform>();
    
    List<GameObject> objectsInRange;
    
    protected int health = 0;
    protected GameObject target;
    protected int dir;
        
    private void Start()
    {
        if (isServer)
        {
            objectsInRange = new List<GameObject>();
            InvokeRepeating("searchNearestPlayer", 0, 0.25f);
        } else
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }
    }

    protected abstract void Attack();

    void searchNearestPlayer()
    {
        int closest = 0;
        float diff = Mathf.Abs(transform.position.x - players[0].position.x);
        for(int i = 1; i < players.Count; i++)
        {
            if (Mathf.Abs(transform.position.x -players[i].position.x) < diff)
            {
                diff = Mathf.Abs(transform.position.x - players[i].position.x);
                closest = i;
            }
        }
        dir = players[closest].position.x > transform.position.x ? 1 : -1;
    }

    [Command]
    public void CmdTakeDamage(int amount, int dir)
    {
        health -= amount;
        GetComponent<Rigidbody2D>().AddForce(new Vector2((amount/4 + 1) * dir, 0), ForceMode2D.Impulse);
        if (health <= 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }

    [Command]
    public void CmdWeaponTriggerEnter(NetworkInstanceId nid)
    {   
        objectsInRange.Add(NetworkServer.FindLocalObject(nid));
        objectsInRange.Sort((o1, o2) => o1.transform.position.x < o2.transform.position.x ? 1 : -1);
        if (target == null)
        {
            target = objectsInRange[0];
            Invoke("Attack", 0);
        } else
        {
            target = objectsInRange[0];
        }
        Debug.Log(NetworkServer.FindLocalObject(nid).name + "   " +  objectsInRange.Count);
    }

    [Command]
    public void CmdWeaponTriggerExit(NetworkInstanceId nid)
    {
        GameObject nexttarget = NetworkServer.FindLocalObject(nid);
        objectsInRange.Remove(nexttarget);
        if (nexttarget == target)
        {
            if (objectsInRange.Count == 0)
            {
                CancelInvoke("Attack");
                target = null;
            } else
            {
                target = objectsInRange[0];
            }
        }
    }
}
