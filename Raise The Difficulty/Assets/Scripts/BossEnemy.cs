using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    #region BossEnemyMove
    private Rigidbody2D rb;
    private PlayerController playerController;
    public float moveSpeed;
    private Vector3 directionToPlayer;
    private Vector3 localscale;
    private bool isDefeated = false;
    #endregion

    #region BossEnemyHealth
    public float health;
    Animator animator;
    Collider2D Collider2D;
    #endregion

    #region Audio
    [SerializeField] AudioClip hitSound;
    private AudioSource audioSource;
    #endregion

    #region BossShoot
    public GameObject projectilePrefab;
    public GameObject firePoint;
    public float projectileSpeed = 10f;
    public float shootCooldown = 2f;
    private float shootTimer;
    #endregion



    private PlayerHit hit;
    public float Health
    {
        set
        {
            health = value;

            if (health <= 0 && !isDefeated)
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
        hit = FindAnyObjectByType(typeof(PlayerHit)) as PlayerHit;
        Collider2D = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        shootTimer = shootCooldown;
    }

    private void FixedUpdate()
    {
        if (!isDefeated)
        {
            MoveEnemy();
            ShootAtPlayer();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
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
        rb.velocity = new Vector2(directionToPlayer.x, directionToPlayer.y) * moveSpeed;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sword"))
        {
            
            var sword = collision.GetComponent<SwordAttack>();
            if (sword != null)
            {
                Health -= sword.damage;
            }

            animator.SetTrigger("Hit");
            audioSource.PlayOneShot(hitSound);
            return;
        }
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

    private void ShootAtPlayer()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;
        shootTimer = shootCooldown;

        animator.SetTrigger("Shoot");

        StartCoroutine(ShootAfterDelay(.3f));
    }

    private IEnumerator ShootAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projectilePrefab != null && firePoint != null && playerController != null)
        {
            Vector2 dir = (playerController.transform.position - firePoint.transform.position).normalized;

            GameObject proj = Instantiate(projectilePrefab, firePoint.transform.position, Quaternion.identity);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (proj.TryGetComponent<Rigidbody2D>(out var projRb))
            {
                projRb.velocity = dir * projectileSpeed;
            }
        }
    }
}



