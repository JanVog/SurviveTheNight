using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGolem : Enemy {
    
    public float speed;
    public int maxhealth;
    public int damage;
    public float attackRate;

    private void Update()
    {
        lastAttack += Time.deltaTime;

        if (target != null && lastAttack > attackRate)
        {
            if (target.tag == "Player")
            {
                target.GetComponent<Player>().CmdTakeDamage(damage);
            }
            else
            {
                target.GetComponent<Building>().CmdTakeDamage(damage);
            }
            lastAttack = 0;
        }
    }

    private void LateUpdate()
    {
        if (isServer)
        {
            transform.Translate(Time.deltaTime * speed / 10 * dir, 0, 0);
        }
    }
}
