using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class attack_A2_Script : MonoBehaviour
{
    public int damage = 20;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player_Script playerScript = collision.GetComponent<player_Script>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }
    }
}
