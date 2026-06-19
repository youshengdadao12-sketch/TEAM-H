using System;
using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField] private EnemyDropMoney moneyDrop;

    public int CurrentHealth { get; private set; }
    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth == 0)
        {
            moneyDrop?.Drop();
            Died?.Invoke();
            Destroy(gameObject);
        }
    }
}
