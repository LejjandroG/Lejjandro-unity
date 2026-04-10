using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR;
public class player_Script : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    [Header("Rörelse/Movement")]
    public float movementSpeed = 5f;
    private bool isfacingRight = true;
    private float horizontal;
    [Header("Jump/Hoppa")]
    public float jumpPower = 10f;
    private int extraJump;
    public int extraJumpsValue = 1;
    [Header("Ground check/Markkontroll")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    [Header("Players health/Players hälsa")]
    public int health = 100;
    public Image healthBar;
    [Header("Wallsliding check/Väggglidningskontroll")]
    public float wallslidingSpeed = 2f;
    private  bool isWallSliding;
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;
    [Header("Walljump/Vägg hopp")]
    public float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    public float wallJumpingDuration = 0.2f;
    public Vector3 wallJumpingPower = new Vector3(10f, 10f);
    private bool isWallJumping;
    private float wallJumpDirection;
    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    private bool isDashing;
    private bool canDash = true;
    public float dashCooldown = 1f;
    [Header("Attack")]
    public float timeBtwAttack;
    public float startTimeBtwAttack;
    public Transform attackPos;
    public float attackRange;
    public LayerMask whatIsEnemies;
    public int damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        extraJump = extraJumpsValue;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Healthbar
        //Hälsobar
        healthBar.fillAmount = health / 100f;

        HandleAttack();
        HandleMovement();
        HandleJump();
        flip();
        WallJump();
        Dash();
    }
    
    // Use FixedUpdate for physics
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
        anim.SetFloat("yVelocity",rb.linearVelocityY);
         
        WallSlide();
        IsOnGround();

    }

        private void OnCollisionEnter2D(Collision2D collision)
    {
        //Spikes collision
        if (collision.gameObject.tag == "Spikes")
        {
            health -= 50;
            StartCoroutine(HurtAnimation());

            rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);

            if (health <= 0)
            {
                Die();
            }
        }
    }

    private IEnumerator HurtAnimation()
    {
        anim.SetBool("gotHurt", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("gotHurt", false);
    }

    private bool IsOnGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //Debug.Log("IsGrounded: " + isGrounded);
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private bool isOnWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    //Rörelse
    //Movement
    private void HandleMovement()
    {
        if(!isDashing)
        {
            rb.linearVelocity = new Vector3(horizontal * movementSpeed, rb.linearVelocityY);
        }
        horizontal = Input.GetAxisRaw("Horizontal");
    }

    private void HandleJump()
    {
        //Jump
        //Hoppa
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);

                anim.SetBool("isJumping", true);
            }
            //Double Jump
            //Dubbelhopp
            else if (extraJump > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);
                extraJump --;
            }

        }

        if (isGrounded && rb.linearVelocityY <= 0)
        {
            anim.SetBool("isJumping", false);
        }

        //Double Jump
        //Dubbelhopp
        if (isGrounded)
        {
            extraJump = extraJumpsValue;
        }
    }

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float direction = isfacingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);
        anim.SetBool("isDashing", true);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        anim.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void WallSlide()
    {
        if (isOnWall())
        {
            isWallSliding = true;
            anim.SetBool("isWallSliding", true);

        
            if (rb.linearVelocityY <= 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX,-wallslidingSpeed);
            }
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("isWallSliding", false);
        }

            //Debug.Log("IsWallSliding: " + isWallSliding);
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = true;
            wallJumpDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingDuration;

            CancelInvoke(nameof(stopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump") && (wallJumpingCounter > 0f))
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector3(wallJumpDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if(transform.localScale.x != wallJumpDirection)
            {
                isfacingRight = !isfacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

                Invoke(nameof(stopWallJumping), wallJumpingDuration);
        }
    }

    private void HandleAttack()
    {
        if (timeBtwAttack <= 0)
        {
            if (Input.GetMouseButtonDown(0) && isGrounded)
            {
                Debug.Log("Player Attacked!");
                anim.SetTrigger("attack");
                Collider2D[] enemysToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
                for (int i = 0; i < enemysToDamage.Length; i++)
                {
                    enemysToDamage[i].GetComponent<enemy_1_script>().TakeDamage(damage);
                }
            }
            timeBtwAttack = startTimeBtwAttack;
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
        }
    }

    private void stopWallJumping()
    {
        isWallJumping = false;
    }

    private void flip()
    {
        if (isfacingRight && horizontal < 0f || !isfacingRight && horizontal > 0f)
        {
            isfacingRight = !isfacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void Die()
    {
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }

        void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
    
}
