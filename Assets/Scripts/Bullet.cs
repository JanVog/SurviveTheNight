using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    public int dir;
    public int damage;

    void OnCollisionEnter(Collision collision)
    {
        if (isServer)
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.CmdTakeDamage(damage, dir);
            }
            NetworkServer.Destroy(gameObject);
        }
    }
}
