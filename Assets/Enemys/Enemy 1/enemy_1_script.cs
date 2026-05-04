using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;


public class enemy_1_script : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int health = 5;
    [Header("Enemy movement")]
    public float speed = 5f;
    public float waitTime = 2f;

    [Header("Enemy Patrol Points")]
    public GameObject pointA;
    public GameObject pointB;
    [Header("Enemy Attack")]
    public GameObject waveAttack;
    public GameObject bulletAttack;
    private bool isAttacking = false;
    [Header("Enemy Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform currentPoint;
    public float radius = 5;
    [Range(1, 360)] public float angle = 45f;
    public LayerMask targetLayer;
    public LayerMask obstructionLayer;
    public GameObject playerRef;
    public bool canSeePlayer { get; private set; }
    private bool isWaiting = false;
    private float dotThreshold;
    private SpriteRenderer sprite;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
        sprite = GetComponent<SpriteRenderer>();
        anim.SetBool("isRunning", true);
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVCheck());
        dotThreshold = Mathf.Cos((angle * 0.5f) * Mathf.Deg2Rad);
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 point = currentPoint.position - transform.position;

        HandleMovement();
        HandleAttacks();
    }


    private IEnumerator FOVCheck()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            FOV();
        }
    }

    private void FOV()
    {
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

        if (rangeCheck.Length > 0)
        {
            Transform target = rangeCheck[0].transform;

            Vector2 directionToTarget = (target.position - transform.position);
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget < 0.5f)
            {
                canSeePlayer = true;
                return;
            }

            directionToTarget.Normalize();

            Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            float dot = Vector2.Dot(facingDirection, directionToTarget);

            if (dot > dotThreshold)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer);
                if (!hit)
                {
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                }
            }
        }
        else
        {
            canSeePlayer = false;
        }
    }

    private void HandleMovement()
    {
        // If the enemy can see the player, move towards them
        //Om fienden kan se spelaren, rör sig mot dem
        if (canSeePlayer == true)
        {
            isWaiting = false;
            
            // Move towards the player
            //Rör sig mot spelaren
            Vector2 directionToPlayer = (playerRef.transform.position - transform.position).normalized;
            rb.linearVelocity = directionToPlayer * speed * 1.5f;

            anim.SetBool("isRunning", true);
            
            // Flip to face the player
            //Vänd för att möta spelaren
            if (playerRef.transform.position.x > transform.position.x && transform.localScale.x < 0)
            {
                Flip();
            }
            else if (playerRef.transform.position.x < transform.position.x && transform.localScale.x > 0)
            {
                Flip();
            }
            return;
        }
        
        // If waiting at a patrol point or attacking, don't move
        //Om väntar vid en patrullpunkt eller attackerar, rör sig inte
        if (isWaiting || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Patrol between points
        //Patrullera mellan punkterna
        MoveTowardsPoint();
        CheckReachedPoint();
    
    }
    
    private void MoveTowardsPoint()
    {

        float directionX;

        if (currentPoint == pointB.transform)
        {
            directionX = pointB.transform.position.x > transform.position.x ? 1f : -1f;
            //directionX = 1f;
            rb.linearVelocity = new Vector2(speed*directionX, 0f);
        }
        else
        {
            directionX = pointA.transform.position.x > transform.position.x ? 1f : -1f;
            //directionX = -1f;
            rb.linearVelocity = new Vector2(speed*directionX, 0f);
        }

        // Flip to match patrol direction
        if (directionX > 0f && transform.localScale.x < 0f)
        {
            Flip();
        }
        else if (directionX < 0f && transform.localScale.x > 0f)
        {
            Flip();
        }
    }


    private void CheckReachedPoint()
    {
        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(WaitAtPoint());
        }
    }


    IEnumerator WaitAtPoint()
    {
        isWaiting = true;


        anim.SetBool("isRunning", false);


        yield return new WaitForSeconds(waitTime);


        if (currentPoint == pointB.transform)
        {
            currentPoint = pointA.transform;
        }
        else
        {
            currentPoint = pointB.transform;
        }

        anim.SetBool("isRunning", true);
        isWaiting = false;
    }


    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

        private Vector2 RotateVector(Vector2 vector, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;

        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private void HandleAttacks()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            anim.SetTrigger("Attack A");
            StartCoroutine(Attack_A());
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            anim.SetTrigger("Attack C");
            StartCoroutine(Attack_C());
        }
    }

    
    //IENumerator Slam()
    //Vänta 0.5 sec
    //Loopa så många ground objekt som ska spawna
    //Instansiera objekt x + 30 pixlar bortom gubben

    private IEnumerator Attack_A()
    {
        isAttacking = true;
        anim.SetBool("isRunning", false);
        
        float attackDistance = 1.1f;
        yield return new WaitForSeconds(0.5f);
        //Spawna 3 ground slam objekt
        for (int i = 0; i < 3; i++)
        {
            Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            Vector2 spawnPosition = (Vector2)transform.position + facingDirection * attackDistance + new Vector2(0, -0.4f);
            //GameObject waveAttack = Resources.Load<GameObject>("Attack A-2.prefab");
            GameObject attackWaveRef = Instantiate(waveAttack, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            attackDistance += 1.1f;
            Destroy(attackWaveRef, 0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        anim.SetBool("isRunning", true);
    }

    private IEnumerator Attack_C()
    {
        isAttacking = true;
        anim.SetBool("isRunning", false);
        yield return new WaitForSeconds(0.3f);
        
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 spawnPosition = (Vector2)transform.position + facingDirection + new Vector2(0, 0);
        
        GameObject bulletAttackRef = Instantiate(bulletAttack, spawnPosition, Quaternion.identity);
        
        bulletAttackRef.GetComponent<bullet_Script>().SetDirection(facingDirection);
        Destroy(bulletAttackRef, 5f);
        
        yield return new WaitForSeconds(0.7f);
        isAttacking = false;
        anim.SetBool("isRunning", true);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        anim.SetTrigger("Hit");
        sprite.color = new Color(250f, 89f, 89f, 255f);
        Debug.Log("Enemy took " + damage + " damage!");
        Invoke("ResetColor", 0.4f);
        if (health <= 0)
        {
            Die();
        }
    }

    private void ResetColor()
    {
        sprite.color = Color.white;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);


        // Vision radius
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Facing direction (matches your movement + flip)
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // FOV boundaries
        Vector2 leftBoundary = RotateVector(facingDirection, -angle / 2);
        Vector2 rightBoundary = RotateVector(facingDirection, angle / 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(leftBoundary * radius));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(rightBoundary * radius));

        // Optional: draw center direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(facingDirection * radius));

        // If player is visible
        if (canSeePlayer && playerRef != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerRef.transform.position);
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }


    private Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}