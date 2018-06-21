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

    public float axeCoolDown = 1.0f;
    public float pickaxeCoolDown = 1.0f;
    public float meleeCoolDown = 1.0f;

    float lastActionTime;

    public Transform misc;

    bool stoppedMoving = true;

    string state = "";
    GameController gc;

    Animator animator;

    int jumps = 0;

    private void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        misc = GameObject.FindGameObjectWithTag("Misc").transform;
        animator = GetComponent<Animator>();
        lastActionTime = Time.time;
    }

    void Update () {
        if (isLocalPlayer)
        {
            var movex = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;
            //Check if player needs to move
            if (movex >= 0.01f || movex <= -0.01f)
            {
                stoppedMoving = false;
                transform.Translate(movex, 0, 0);

                animator.SetTrigger("Walk");
                if (movex < 0)
                {
                    GetComponent<SpriteRenderer>().flipX = true;
                }
                else if (movex > 0)
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                }
            } else if (!stoppedMoving && -0.001f < GetComponent<Rigidbody2D>().velocity.y && GetComponent<Rigidbody2D>().velocity.y < 0.001f)
            {
                MovementStopped();
            }  

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdFire();
                animator.SetTrigger("Attack");
            }

            if(state != "")
            {
                if (state == "lumber" && lastActionTime + axeCoolDown < Time.time)
                {
                    gc.CmdFarmResource(Mathf.RoundToInt(transform.position.x / 1.28f + forwardDist));
                }else if (state == "miner" && lastActionTime + pickaxeCoolDown < Time.time)
                {
                    gc.CmdFarmResource(Mathf.RoundToInt(transform.position.x / 1.28f + forwardDist));
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

    void MovementStopped()
    {
        stoppedMoving = true;
        //Check for a new state after movement ends
        float forwardDist = 0.3f;
        if (GetComponent<SpriteRenderer>().flipX == true)
            forwardDist *= -1;
        string obj = gc.getObjAtPos(Mathf.RoundToInt(transform.position.x / 1.28f + forwardDist));

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
    }

    [Command]
    void CmdFire()
    {
        int dirMult = GetComponent<SpriteRenderer>().flipX ? -1 : 1;

        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position +  new Vector3(dirMult/5f, 0, 0),
            bulletSpawn.rotation,
            misc);

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
