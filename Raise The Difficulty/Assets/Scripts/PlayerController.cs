using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Takes and handles input and movement for a player character
public class PlayerController : MonoBehaviour
{
    #region Public
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;
    #endregion

    #region Stamina
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public float staminaRecoveryRate = 15f; // stamina recovered when not attacking or dashing
    public float dashStaminaCost = 20f;
    public float attackStaminaCost = 15f;
    public Image staminaBar;
    #endregion

    #region References
    Vector2 movementInput;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    Animator animator;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    [SerializeField] SwordAttack swordAttack;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PerformanceWaves performance;
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

    #region Audio
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip dashSound;
    private AudioSource audioSource;
    #endregion

    #region Upgrade
    public int speedUpgradeLevel = 0;
    public int staminaRecoveryUpgradeLevel = 0;
    public int attackUpgradeLevel = 0;
    public Text statsText;
    public Text statsShowText;
    private Color statsDefaultColor = Color.white;
    private bool showStatText;
    #endregion

    #region Afterimage
    [SerializeField] private GameObject afterImage;
    [SerializeField] private float afterImageSpawnRate = 0.05f;
    private Coroutine afterImageRoutine;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();//References to components so they can be used
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;

        currentStamina = maxStamina;
        UpdateStaminaUI();
        UpdateStatsUI();

        showStatText = true;
    }

    private void Update()
    {
        if (isDashing) //Skip imput handling if currently dashing
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash) //Handle dash input
        {
            if (currentStamina >= dashStaminaCost)
            {
                currentStamina -= dashStaminaCost; //Subtract dash stamina cost
                UpdateStaminaUI();
                StartCoroutine(Dash());
                GetComponent<AudioSource>().PlayOneShot(dashSound);
            }
            else
            {
                Debug.Log("Not enough stamina");
            }
        }

        if (pauseMenu.GameIsPaused == true || performance.GameUpgrade == true) //Disable input when paused or in upgrade menu
        {
            playerInput.enabled = false;
        }
        else if (pauseMenu.GameIsPaused == false || performance.GameUpgrade == false)
        {
            playerInput.enabled = true;
        }


        RecoverStamina();
    }

    private void FixedUpdate() 
    {
        if (isDashing) //Skip movement while dashing
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
                    success = TryMove(new Vector2(movementInput.x, 0)); //Horizontal movement
                }

                if(!success) 
                {
                    success = TryMove(new Vector2(0, movementInput.y)); //Vertical movement
                }
                
                animator.SetBool("isMoving", success); //Animation trigger
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
        movementInput = movementValue.Get<Vector2>(); //Get direction input
    }

    void OnFire() 
    {
        if (currentStamina >= attackStaminaCost)
        {
            currentStamina -= attackStaminaCost; //Subtract attack cost
            UpdateStaminaUI();

            animator.SetTrigger("swordAttack");
            GetComponent<AudioSource>().PlayOneShot(attackSound);
        }
        else
        {
            Debug.Log("Not enough stamina");
        }
    }

    public void SwordAttack() 
    {
        LockMovement(); //Prevent move during attack

        if(spriteRenderer.flipX == true)
        {
            swordAttack.AttackLeft(); //Attack left
        } 
        else 
        {
            swordAttack.AttackRight(); //attack right
        }
    }

    public void EndSwordAttack() 
    {
        UnlockMovement(); //Allow movement
        swordAttack.StopAttack(); //Disable sword collider
    }

    public void LockMovement() 
    {
        canMove = false;
    }

    public void UnlockMovement() 
    {
        canMove = true;
    }

    //Coroutine for dashing
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        afterImageRoutine = StartCoroutine(SpawnAfterImage());

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; //Ignore Gravity
        rb.velocity = new Vector2(movementInput.x * dashingPower, movementInput.y * dashingPower); //Dash in input direction
        tr.emitting = true; //Emit trail

        yield return new WaitForSeconds(dashingTime); //Wait for dash duration

        if (afterImageRoutine != null)
        {
            StopCoroutine(afterImageRoutine);
        }

        rb.velocity =Vector2.zero; //Stop dash movement
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown); //wait for another dash
        canDash = true;
    }

    private IEnumerator SpawnAfterImage()
    {
        var sr = GetComponent<SpriteRenderer>(); //component for sprite renderer

        while (isDashing)
        {
            var go = Instantiate(afterImage, transform.position, transform.rotation); //position of the afterimage
            var ghostSr = go.GetComponent<SpriteRenderer>();

            ghostSr.sprite = sr.sprite;
            ghostSr.flipX = sr.flipX; //flip afterimage
            ghostSr.sortingOrder = sr.sortingOrder - 1; //sort number of images

            yield return new WaitForSeconds(afterImageSpawnRate);
        }
    }

    private void RecoverStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); //Prevent overfill
            UpdateStaminaUI();
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = currentStamina / maxStamina; //Fill bar proportionally
        }
    }

    public void UpdateStatsUI()
    {
        if (statsText != null)
        {
            statsText.text = $"Attack: {swordAttack.damage}\nSpeed: {moveSpeed}\nStamina Recovery: {staminaRecoveryRate}";
        }
    }

    //Coroutine to display stats text
    private IEnumerator ShowStatText(bool increased)
    {
        if (!showStatText) yield break; //If stat text display is disabled, exit coroutine

        if (statsShowText != null)
        {
            statsShowText.text = increased ? "Stats Up" : "Stats Down"; //set message and color depending on increased or decreased
            statsShowText.color = increased ? Color.green : Color.red;
            statsShowText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            statsShowText.gameObject.SetActive(false);
        }
    }

    public void UpgradeAttack()
    {
        swordAttack.damage += .5f; //increase attack
        attackUpgradeLevel++; //track upgrade number
        UpdateStatsUI();
        StartCoroutine(ShowStatText(true));
    }
    public void UpgradeSpeed()
    {
        moveSpeed += .2f; //increase move speed
        speedUpgradeLevel++;
        UpdateStatsUI();
        StartCoroutine(ShowStatText(true));
    }
    public void UpgradeStaminaRecovery()
    {
        staminaRecoveryRate += 3f; //increase recovery rate
        staminaRecoveryUpgradeLevel++;
        UpdateStatsUI();
        StartCoroutine(ShowStatText(true));
    }
    public void RevertAttack()
    {
        if (attackUpgradeLevel > 0)
        {
            swordAttack.damage -= .5f; //decrease attack
            attackUpgradeLevel--; //subtract upgrade level
            UpdateStatsUI();
            StartCoroutine(ShowStatText(false));
        }
    }
    public void RevertSpeed()
    {
        if (speedUpgradeLevel > 0)
        {
            moveSpeed -= 0.2f; //decrease speed
            speedUpgradeLevel--;
            UpdateStatsUI();
            StartCoroutine(ShowStatText(false));
        }
    }
    public void RevertStaminaRecovery()
    {
        if (attackUpgradeLevel > 0)
        {
            staminaRecoveryRate -= 3f; //decrease stamina recovery
            staminaRecoveryUpgradeLevel--;
            UpdateStatsUI();
            StartCoroutine(ShowStatText(false));
        }
    }
}
