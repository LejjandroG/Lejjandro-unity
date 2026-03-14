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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
        anim.SetBool("isRunning", true);
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVCheck());
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
            Vector2 directionToTarget = (target.position - transform.position).normalized;


            if (Vector2.Angle(transform.right, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer))
                {
                    canSeePlayer = true;
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
        else if (canSeePlayer)
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


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);


        Gizmos.color = Color.white;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, radius);


        Vector3 angle01 = DirectionFromAngle(-transform.eulerAngles.z, -angle / 2);
        Vector3 angle02 = DirectionFromAngle(-transform.eulerAngles.z, angle / 2);


        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + angle01 * radius);
        Gizmos.DrawLine(transform.position, transform.position + angle02 * radius);


        if (canSeePlayer)
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