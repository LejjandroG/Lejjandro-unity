using UnityEngine;
using UnityEngine.UI;

public class bullet_Script : MonoBehaviour
{
    public int bulletSpeed = 5;

    public int damage = 10;

    private Vector2 moveDirection = Vector2.right;

    private bool isfacingRight;

    private float horizontal;

    [SerializeField] private LayerMask ObstructionLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(moveDirection * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        player_Script playerScript = collision.GetComponent<player_Script>();
        
        if (playerScript != null)
        {
            playerScript.TakeDamage(damage);
        }
        if ((ObstructionLayer & (1 << collision.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;

        if (moveDirection.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        Debug.Log("moveDirection" + moveDirection);
    }


}
