using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Weapon : MonoBehaviour {

    public Player player;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        player.enemiesInRange.Add(other.gameObject.GetComponent<NetworkIdentity>().netId);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        player.enemiesInRange.Remove(other.gameObject.GetComponent<NetworkIdentity>().netId);
    }
}
