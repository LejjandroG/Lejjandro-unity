using UnityEngine;
using System.Collections;


public class enemy_1_script : MonoBehaviour
{
    [Header("Enemy movement")]
    public float speed = 5f;
    public float waitTime = 2f;
    [Header("Enemy Patrol Points")]
    public GameObject pointA;
    public GameObject pointB;
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
        anim.SetBool("isRunning", true);
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVCheck());
        dotThreshold = Mathf.Cos((angle * 0.5f) * Mathf.Deg2Rad);
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 point = currentPoint.position - transform.position;


        if (!isWaiting)
        {
            MoveTowardsPoint();
            CheckReachedPoint();
        }
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
                canSeePlayer = !hit;
            }
            else
            {
                canSeePlayer = false;
            }
        }
        else
        {
            canSeePlayer = false;
        }
    }


    private void MoveTowardsPoint()
    {
        if(currentPoint == pointB.transform)
        {
            rb.linearVelocity = new Vector2(speed, 0 );
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed,0 );
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


        Flip();


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


    private Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}