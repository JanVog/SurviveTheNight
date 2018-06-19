using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    int state = 0;  // 0 = moving, 1 = attacking
    public float speed = 20.0f;
    Vector2 speedvector;

    private void Start()
    {
        speedvector = new Vector2(0, 0);
    }

    private void LateUpdate()
    {
        speedvector.x = Time.deltaTime * speed * 10;
        GetComponent<Rigidbody2D>().velocity = speedvector;
    }
}
