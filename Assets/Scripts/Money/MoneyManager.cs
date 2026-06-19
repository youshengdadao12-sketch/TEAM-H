using System;
using UnityEngine;

public sealed class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [SerializeField, Min(0)] private int startingMoney;

    public int CurrentMoney { get; private set; }
    public event Action<int> MoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentMoney = startingMoney;
    }

    private void Start()
    {
        MoneyChanged?.Invoke(CurrentMoney);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentMoney += amount;
        MoneyChanged?.Invoke(CurrentMoney);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || CurrentMoney < amount)
        {
            return false;
        }

        CurrentMoney -= amount;
        MoneyChanged?.Invoke(CurrentMoney);
        return true;
    }

    public void ResetMoney()
    {
        CurrentMoney = startingMoney;
        MoneyChanged?.Invoke(CurrentMoney);
    }
}
