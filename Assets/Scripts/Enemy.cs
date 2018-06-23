using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour {

    int state = 0;  // 0 = moving, 1 = attacking
    public float speed;
    public int health;

    private void Start()
    {
        if (isServer)
        {
            Debug.Log(health);
        }
    }

    private void LateUpdate()
    {
        if (isServer) {
            transform.Translate(Time.deltaTime * speed/ 10, 0, 0);
        }
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
