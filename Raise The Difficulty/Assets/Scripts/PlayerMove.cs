using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    #region Enums
    private enum Directions { UP, DOWN, LEFT, RIGHT };
    #endregion

    #region Editor Data
    [SerializeField] float movespeed = 50f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    #endregion

    #region Internal Data
    private Vector2 moveDir = Vector2.zero;
    private Directions facingDirections = Directions.RIGHT;

    private bool isAttacking = false;

    private readonly int animMoveRight = Animator.StringToHash("Movement Blend Tree");
    private readonly int animIdleRight = Animator.StringToHash("Idle Blend Tree");
    private readonly int animAttack = Animator.StringToHash("Attack Blend Tree");
    #endregion


    #region Tick
    private void Update()
    {
        GatherInput();
        CalculatingFacingDirection();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Movement();
    }
    #endregion


    #region Input Logic
    private void GatherInput()
    {
        if (!isAttacking)
        {
            moveDir.x = Input.GetAxisRaw("Horizontal");
            moveDir.y = Input.GetAxisRaw("Vertical");
        }
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartAttack();
        }
    }
    #endregion

    #region Movement Logic
    private void Movement()
    {
        if (!isAttacking)
        {
            rb.velocity = moveDir * movespeed * Time.fixedDeltaTime;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        
    }
    #endregion

    #region Animation Logic
    private void CalculatingFacingDirection()
    {
        if (moveDir.x != 0)
        {
            if (moveDir.x > 0) // Moving Right
            {
                facingDirections = Directions.RIGHT;
            }
            else if (moveDir.x < 0) //Moving Left
            {
                facingDirections = Directions.LEFT;
            }
        }
    }

    private void UpdateAnimation()
    {
        if (isAttacking) return;

        if (facingDirections == Directions.LEFT)
        {
            spriteRenderer.flipX = true;
        }
        else if (facingDirections == Directions.RIGHT)
        {
            spriteRenderer.flipX = false;
        }

        if (moveDir.SqrMagnitude() > 0) //Moving
        {
            animator.CrossFade(animMoveRight, 0);
        }
        else
        {
            animator.CrossFade(animIdleRight, 0);
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        animator.SetTrigger(animAttack);

        StartCoroutine(ResetAttack(0.5f));
    }

    private IEnumerator ResetAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }
    #endregion
}


//Digital Oddities https://www.youtube.com/watch?v=l-Bbwvk3RXY&list=PLvdaHZw6zzmt94YFtnfzTLtgkyZlNh9go&index=5