using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Drop : NetworkBehaviour {

    public Transform target;
    public float landingpos;
    float startpos;
    float timeAlive = 0;
    float kx;
    public GameController gc;
    public string objType;
    public int playerId;

    void Start()
    {
        startpos = transform.position.x;
        kx = (landingpos - transform.position.x);
    }

    void Update ()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive < 1)
        {
            transform.position = new Vector3(startpos + kx * timeAlive, Mathf.Sin(Mathf.PI * (1.0f / 6 + timeAlive * 5.0f / 6)), -2);
        } else
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, 0.04f * timeAlive);
        }
        if (isServer && Mathf.Abs(transform.position.x - target.transform.position.x) < 0.05f)
        {
            NetworkServer.Destroy(gameObject);
            gc.CmdPickDrop(this.objType, this.playerId);
        }
	}
}
