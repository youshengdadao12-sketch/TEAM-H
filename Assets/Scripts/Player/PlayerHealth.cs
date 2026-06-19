using System;
using System.Collections;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 5;
    [SerializeField, Min(0f)] private float respawnDelay = 1f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private Coroutine respawnRoutine;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
        {
            Died?.Invoke();
            respawnRoutine ??= StartCoroutine(RespawnRoutine());
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void RestoreFullHealth()
    {
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        PlayerController player = GetComponent<PlayerController>();
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.Respawn(player);
        }
        else
        {
            SceneLoader.ReloadCurrentScene();
        }

        respawnRoutine = null;
    }
}
