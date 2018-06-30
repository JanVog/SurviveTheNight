using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceGolem : Enemy {
    
    new void Start()
    {
        base.Start();
        health = maxhealth;
    }

    new void Attack()
    {
        base.Attack();
    }
}
