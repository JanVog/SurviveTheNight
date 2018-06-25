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

    public float axeCoolDown;
    public float pickaxeCoolDown;
    public float meleeCoolDown;
    public int meleeDamage;

    float lastActionTime = 0;
    float lastMeleeTime = 0;

    Transform misc;

    bool stoppedMoving = true;

    public string state = "";
    GameController gc;

    Animator animator;

    int jumps = 0;
    int dir = 1;

    public List<NetworkInstanceId> enemiesInRange;

    private void Start()
    {
        if (isLocalPlayer)
        {
            enemiesInRange = new List<NetworkInstanceId>();
            gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            if (!isServer)
            {
                gc.CmdPlayerConnected(GetComponent<NetworkIdentity>().netId);
            }
            else
            {
                gc.addHostPlayer(GetComponent<NetworkIdentity>().netId);
            }
            misc = GameObject.FindGameObjectWithTag("Misc").transform;
            animator = GetComponent<Animator>();
            gc.CmdAddPlayerLight(transform.GetChild(3).gameObject);
            gc.CmdAddPlayerLight(transform.GetChild(4).gameObject);
        } else
        {
            Destroy(transform.GetComponentInChildren<CircleCollider2D>());
        }
    }

    void Update () {
        if (isLocalPlayer)
        {
            lastActionTime += Time.deltaTime;
            lastMeleeTime += Time.deltaTime;
            var movex = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;
            //Check if player needs to move
            if (movex >= 0.01f || movex <= -0.01f)
            {
                if (stoppedMoving)
                {
                    stoppedMoving = false;
                    animator.SetTrigger("Walk");
                    state = "";
                }

                if (movex < 0)
                {
                    dir = -1;
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else if (movex > 0)
                {
                    dir = 1;
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }

                transform.Translate(Mathf.Abs(movex), 0, 0);
            } else if (!stoppedMoving && -0.001f < GetComponent<Rigidbody2D>().velocity.y && GetComponent<Rigidbody2D>().velocity.y < 0.001f)
            {
                lastActionTime = 0.2f;
                MovementStopped();
            }  

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdFire();
                animator.SetTrigger("Attack");
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (lastMeleeTime > meleeCoolDown)
                {
                    lastMeleeTime = 0;
                    CmdHitEnemies(enemiesInRange.ToArray(), dir);
                }
            }

            if (state != "")
            {
                bool farming = false;
                if (state == "lumber" && lastActionTime > axeCoolDown)
                {
                    farming = true;
                    lastActionTime = 0;
                }
                else if (state == "miner" && lastActionTime > pickaxeCoolDown)
                {
                    farming = true;
                    lastActionTime = 0;
                }
                if (farming)
                {
                    float forwardDist = 0.15f * dir;
                    gc.CmdFarmResource((int) Mathf.Floor(transform.position.x / 1.28f + forwardDist), GetComponent<NetworkIdentity>().netId);
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (jumps < 2)
                {
                    Jump();
                    jumps += 1;
                    animator.SetTrigger("Jump");
                }
            }

            if (GetComponent<Rigidbody2D>().velocity.y < 0) {
                animator.SetTrigger("Falling");
            }
        }
    }

    [Command]
    public void CmdHitEnemies(NetworkInstanceId[] enemiesInRange, int dir)
    {
        foreach(NetworkInstanceId enemyId in enemiesInRange)
        {
            NetworkServer.FindLocalObject(enemyId).gameObject.GetComponent<Enemy>().TakeDamage(meleeDamage, dir);
        }
    }

    void MovementStopped()
    {
        stoppedMoving = true;
        //Check for a new state after movement ends
        float forwardDist = 0.3f * dir;
        string obj = gc.getObjAtPos((int)Mathf.Floor(transform.position.x / 1.28f + forwardDist));

        bool farming = true;
        switch (obj)
        {
            case "tree":
                //change to axe
                state = "lumber";
                break;
            case "white_tree":
                //change to axe
                state = "lumber";
                break;
            case "stone":
                //change to pickaxe
                state = "miner";
                break;
            case "coal_stone":
                //change to pickaxe
                state = "miner";
                break;
            default:
                farming = false;
                state = "idle";
                break;
        }
        if (farming)
        {
            animator.SetTrigger("Attack");
        }
        else
        {
            animator.SetTrigger("Idle");
        }
    }

    void Jump()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 1 + 4 / (jumps+1)), ForceMode2D.Impulse);
        animator.SetTrigger("Jump");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "Ground"){
            jumps = 0;
            animator.SetTrigger("Hit Ground");
        }
    }

    public override void OnStartLocalPlayer()
    {
        GameObject.FindWithTag("MainCamera").GetComponent<CameraController>().player = transform;
        GetComponent<SpriteRenderer>().color = new Color(0.6f, 1, 0.6f, 1);
        Destroy(healthBar.gameObject);
        if (!isServer)
        {
            CmdAddPlayerForEnemies();
        } else
        {
            Enemy.players.Add(transform);
        }
    }

    [Command]
    void CmdAddPlayerForEnemies()
    {
        Enemy.players.Add(transform);
    }

    [Command]
    void CmdFire()
    {

        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position +  new Vector3(5 * dir, 0, 0),
            bulletSpawn.rotation,
            misc);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(8 * dir, 0);

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
