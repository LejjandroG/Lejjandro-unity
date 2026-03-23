using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
        //Rörelse
        //Movement
        horizontal = Input.GetAxisRaw("Horizontal");

        //Double Jump
        //Dubbelhopp
        if (isGrounded)
        {
            extraJump = extraJumpsValue;
        }
        
        //Jump
        //Hoppa
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);
            }
        //Double Jump
        //Dubbelhopp
            else if (extraJump > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);
                extraJump --;
            }

        }
        //Healthbar
        //Hälsobar
        healthBar.fillAmount = health / 100f;

        flip();
        WallJump();
        Dash();
    }
    // Use FixedUpdate for physics
    private void FixedUpdate()
    {
         if (isDashing)
        {
            anim.SetBool("isDashing", true);
            return;
        }
            
        rb.linearVelocity = new Vector3(horizontal * movementSpeed, rb.linearVelocityY);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
         
        WallSlide();
        IsOnGround();
    }
    private bool IsOnGround()
    {
        Debug.Log("IsGrounded: " + isGrounded);
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private bool isOnWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
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

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void WallSlide()
    {
        if (isOnWall())
        {
        isWallSliding = true;

        
            if (rb.linearVelocityY <= 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX,-wallslidingSpeed);
            }
        }
        else
        {
            isWallSliding = false;
        }

            Debug.Log("IsWallSliding: " + isWallSliding);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Spikes collision
        if (collision.gameObject.tag == "Spikes")
        {
            health -= 50;
            rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);
            if (health <= 0)
            {
                Die();
            }
        }
    }
    private void Die()
    {
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
