using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Enemy : NetworkBehaviour {
    
    public static List<Transform> players = new List<Transform>();
    
    List<GameObject> objectsInRange;
    
    protected int health = 0;
    protected GameObject target;
    protected int dir;
    public int speed;
    public int maxhealth;
    public int damage;
    public float attackRate;

    private void LateUpdate()
    {
        if (isServer)
        {
            transform.Translate(Time.deltaTime * speed / 10.0f * dir, 0, 0);
        }
    }

    public void Start()
    {
        if (isServer)
        {
            objectsInRange = new List<GameObject>();
            InvokeRepeating("searchNearestPlayer", 0, 0.25f);
        } else
        {
            //ToDo: check if it really happens
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }
    }

    protected void Attack()
    {
        if (target != null)
        {
            if (target.tag == "Player")
            {
                target.GetComponent<Player>().TakeDamage(damage);
            }
            else
            {
                target.GetComponent<Building>().TakeDamage(damage);
            }
            Invoke("Attack", attackRate);
        }
    }

    void searchNearestPlayer()
    {
        if (target == null)
        {
            int closest = 0;
            float diff = Mathf.Abs(transform.position.x - players[0].position.x);
            for (int i = 1; i < players.Count; i++)
            {
                if (Mathf.Abs(transform.position.x - players[i].position.x) < diff)
                {
                    diff = Mathf.Abs(transform.position.x - players[i].position.x);
                    closest = i;
                }
            }
            dir = players[closest].position.x > transform.position.x ? 1 : -1;
        }
    }

    [Command]
    public void CmdTakeDamage(int amount, int dir)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(transform.GetChild(0).gameObject);
            NetworkServer.Destroy(gameObject);
        }
        GetComponent<Rigidbody2D>().AddForce(new Vector2((amount / 4.0f + 1) * dir * 1.3f, 0), ForceMode2D.Impulse);
    }

    [Command]
    public void CmdWeaponTriggerEnter(NetworkInstanceId nid)
    {   
        objectsInRange.Add(NetworkServer.FindLocalObject(nid));
        objectsInRange.Sort((o1, o2) => o1.transform.position.x < o2.transform.position.x ? 1 : -1);
        if (target == null)
        {
            dir = 0;
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
            target = objectsInRange[0];
            Invoke("Attack", 0);
        } else
        {
            target = objectsInRange[0];
        }
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
                searchNearestPlayer();
            } else
            {
                target = objectsInRange[0];
            }
        }
    }
}
