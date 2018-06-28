using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Building : NetworkBehaviour {

    public int maxhealth;
    [SyncVar(hook = "OnChangeHealth")]
    int health;
    public Image healthbar;

    private void Start()
    {
        health = maxhealth;
    }

	public void TakeDamage(int amount)
    {
        health -= amount;
        
        if (health <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    void OnChangeHealth(int amount)
    {
        health -= amount;
        if (health > 0)
        {
            healthbar.fillAmount = maxhealth / health;
        }
    }
}
