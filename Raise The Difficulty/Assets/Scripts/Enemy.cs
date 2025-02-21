using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region EnemyMove
    private Rigidbody2D rb;
    private PlayerController playerController;
    public float moveSpeed;
    private Vector3 directionToPlayer;
    private Vector3 localscale;
    #endregion

    #region EnemyHealth
    public float health;
    Animator animator;
    #endregion

    private PlayerHit hit;
    public float Health 
    {
        set 
        {
            health = value;

            if(health <= 0) 
            {
                Defeated();
            }
        }
        get 
        {
            return health;
        }
    }

    

    private void Start() 
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerController = FindAnyObjectByType(typeof(PlayerController)) as PlayerController;
        localscale = transform.localScale;
        hit = FindAnyObjectByType(typeof (PlayerHit)) as PlayerHit;
    }

    void Awake()
    {
       
    }


    private void FixedUpdate()
    {
        MoveEnemy();
    }

    private void LateUpdate()
    {
        if (rb.velocity.x > 0)
        {
           transform.localScale = new Vector3(localscale.x, localscale.y, localscale.z);
        }
        else if (rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(-localscale.x, localscale.y, localscale.z);
        }
    }

    private void MoveEnemy()
    {
        directionToPlayer = (playerController.transform.position - transform.position).normalized;
        rb.velocity = new Vector2 (directionToPlayer.x, directionToPlayer.y) * moveSpeed;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sword")
        {
            animator.SetTrigger("Hit");
        }

        if (collision.tag == "Hitbox")
        {
            hit.RegisterHit();
        }
    }

    public void Defeated()
    {
        animator.SetTrigger("Defeated");
    }

    public void RemoveEnemy() 
    {
        Destroy(gameObject);
    }
}

//Chris' Tutorials https://www.youtube.com/watch?v=7iYWpzL9GkM