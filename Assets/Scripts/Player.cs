using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{

    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    public const int maxHealth = 100;
    [SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = maxHealth;
    public RectTransform healthBar;


    void Update () {
        if (isLocalPlayer)
        {
            var movex = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;
            transform.Translate(movex, 0, 0);
            if (movex < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else if(movex > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdFire();
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().color = new Color(0.6f, 1, 0.6f, 1);
        Destroy(healthBar.gameObject);
    }

    [Command]
    void CmdFire()
    {
        int dirMult = GetComponent<SpriteRenderer>().flipX ? -1 : 1;

        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position +  new Vector3(dirMult/5f, 0, 0),
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(dirMult*8, 0);

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 3.0f);
    }

    public void TakeDamage(int amount)
    {
        if (isServer)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Debug.Log("Dead!");
            }
        }
    }

    void OnChangeHealth(int health)
    {
        if (!isLocalPlayer)
        {
            healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
        }
    }
}
