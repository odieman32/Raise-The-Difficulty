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
    private bool isDefeated = false;
    #endregion

    #region EnemyHealth
    public float health;
    Animator animator;
    Collider2D Collider2D;
    #endregion

    #region Audio
    [SerializeField] AudioClip hitSound;
    private AudioSource audioSource;
    #endregion

    #region color change
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color flashColor = Color.red;
    public float flashDuration = .1f;
    public int flashCount = 2;
    private bool isFlashing = false;
    #endregion

    #region Knockback
    public float knockbackForce = 5f;
    public float knockbackDuration = .2f;
    private bool isKnockedBack = false;
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
        Collider2D = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void FixedUpdate()
    {
        if (isDefeated)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        
        if (isKnockedBack)
        {
            return;
        }

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
            GetComponent<AudioSource>().PlayOneShot(hitSound);

            if (!isFlashing)
            {
                StartCoroutine(FlashDamage());
            }

            if (!isKnockedBack)
            {
                StartCoroutine(ApplyKnockback(collision.transform));
            }
        }

        if (collision.tag == "Hitbox")
        {
            hit.RegisterHit();
        }

        if (collision.tag == "Player")
        {
            if (!isKnockedBack)
            {
                StartCoroutine(ApplyKnockback(collision.transform));
            }
        }
    }

    private IEnumerator FlashDamage()
    {
        isFlashing = true;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        isFlashing = false;
    }

    private IEnumerator ApplyKnockback(Transform source)
    {
        isKnockedBack = true;

        Vector2 knockDir = (transform.position - source.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }

    public void Defeated()
    {
        isDefeated = true;
        animator.SetTrigger("Defeated");
        Collider2D.enabled = false;
    }

    public void RemoveEnemy() 
    {
        Destroy(gameObject);
    }
}

//Chris' Tutorials https://www.youtube.com/watch?v=7iYWpzL9GkM