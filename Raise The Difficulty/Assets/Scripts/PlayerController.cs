using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

// Takes and handles input and movement for a player character
public class PlayerController : MonoBehaviour
{
    #region Public
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;
    public SwordAttack swordAttack;
    #endregion

    #region References
    Vector2 movementInput;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    Animator animator;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    #endregion

    #region Dash
    bool canMove = true;
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 16f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer tr;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (isDashing)
        {
           return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartCoroutine(Dash());
        }
        else
        {
            StopCoroutine(Dash());
        }
    }

    private void FixedUpdate() 
    {
        if (isDashing)
        {
            return;
        }

       if(canMove) 
        {
            // If movement input is not 0, try to move
            if(movementInput != Vector2.zero){
                
                bool success = TryMove(movementInput);

                if(!success) 
                {
                    success = TryMove(new Vector2(movementInput.x, 0));
                }

                if(!success) 
                {
                    success = TryMove(new Vector2(0, movementInput.y));
                }
                
                animator.SetBool("isMoving", success);
            } 
            else 
            {
                animator.SetBool("isMoving", false);
            }

            // Set direction of sprite to movement direction
            if(movementInput.x < 0) {
                spriteRenderer.flipX = true;
            } else if (movementInput.x > 0) {
                spriteRenderer.flipX = false;
            }
        }
    }

    private bool TryMove(Vector2 direction) {
        if(direction != Vector2.zero) 
        {
            // Check for potential collisions
            int count = rb.Cast(
                direction, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
                movementFilter, // The settings that determine where a collision can occur on such as layers to collide with
                castCollisions, // List of collisions to store the found collisions into after the Cast is finished
                moveSpeed * Time.fixedDeltaTime + collisionOffset); // The amount to cast equal to the movement plus an offset

            if(count == 0)
            {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
                return true;
            } 
            else 
            {
                return false;
            }
        } 
        else 
        {
            // Can't move if there's no direction to move in
            return false;
        }
        
    }

    void OnMove(InputValue movementValue) {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnFire() 
    {
        animator.SetTrigger("swordAttack");
    }

    public void SwordAttack() 
    {
        LockMovement();

        if(spriteRenderer.flipX == true)
        {
            swordAttack.AttackLeft();
        } 
        else 
        {
            swordAttack.AttackRight();
        }
    }

    public void EndSwordAttack() 
    {
        UnlockMovement();
        swordAttack.StopAttack();
    }

    public void LockMovement() 
    {
        canMove = false;
    }

    public void UnlockMovement() 
    {
        canMove = true;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(movementInput.x * dashingPower, movementInput.y * dashingPower);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        rb.velocity = new Vector2(movementInput.x = 0, movementInput.y = 0);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
