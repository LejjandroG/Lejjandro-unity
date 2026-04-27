using UnityEngine;

public class dashAttack : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log(gameObject.name + " hit!");
            collision.gameObject.GetComponent<enemy_1_script>().TakeDamage(1);
        }
    }
}
