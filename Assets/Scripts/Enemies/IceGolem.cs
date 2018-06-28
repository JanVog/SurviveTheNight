using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGolem : Enemy {
    
    public float speed;
    public int maxhealth;
    public int damage;
    public float attackRate;

    private void Start()
    {
        health = maxhealth;
    }

    private void LateUpdate()
    {
        if (isServer)
        {
            transform.Translate(Time.deltaTime * speed / 10 * dir, 0, 0);
        }
    }

    override protected void Attack()
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
}
