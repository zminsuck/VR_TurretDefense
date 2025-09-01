// ¿¹: °ñ Æ®¸®°Å¿¡ ºÎÂø
using UnityEngine;

public class BaseGate : MonoBehaviour
{
    public int damagePerEnemy = 1;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameManager.I?.TakeDamage(damagePerEnemy);
            Destroy(other.gameObject);
        }
    }
}
