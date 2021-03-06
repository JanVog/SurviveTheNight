﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class enemyWeapon : NetworkBehaviour {

    Enemy enemy;

    private void Start()
    {
        enemy = transform.parent.GetComponent<Enemy>();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {

        enemy.CmdWeaponTriggerEnter(other.gameObject.GetComponent<NetworkIdentity>().netId);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        enemy.CmdWeaponTriggerExit(other.gameObject.GetComponent<NetworkIdentity>().netId);
    }
}
