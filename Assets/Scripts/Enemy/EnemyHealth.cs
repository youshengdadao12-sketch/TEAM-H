using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
}
