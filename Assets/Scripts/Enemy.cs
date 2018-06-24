using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    int state = 0;  // 0 = moving, 1 = attacking
    public float speed;
    public int health;
    public static List<Transform> players = new List<Transform>();
    int dir;

    private void Start()
    {
        if (isServer)
        {
            //Todo: repeat rate lower than 1?
            InvokeRepeating("searchNearestPlayer", 0, 0.25f);
        }
    }

    private void LateUpdate()
    {
        if (isServer) {
            transform.Translate(Time.deltaTime * speed / 10 * dir, 0, 0);
        }
    }

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

    public void TakeDamage(int amount, int dir)
    {
        health -= amount;
        GetComponent<Rigidbody2D>().AddForce(new Vector2((amount/4 + 1) * dir, 0), ForceMode2D.Impulse);
        if (health <= 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
