using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class attack_A2_Script : MonoBehaviour
{
    public int damage = 20;

    public float destroyTime = 0.8f; // Tid i sekunder innan attacken förstörs

    /*void Start()
    {
        // Förstör attacken efter en kort tid för att undvika att den stannar kvar i scenen
        Destroy(gameObject, destroyTime);
    }*/

    void OnAnimationEnd()
    {
        // Denna metod kan anropas från en animation event i attackens animationsklipp
        // För att säkerställa att attacken förstörs när animationen är klar
        Destroy(gameObject);
    }

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
