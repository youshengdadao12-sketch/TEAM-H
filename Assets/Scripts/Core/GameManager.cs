using System;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Min(1)] private int freedomPrice = 10000;

    public int CurrentStage { get; private set; } = 1;
    public int FreedomPrice => freedomPrice;
    public bool CanBuyFreedom => MoneyManager.Instance != null && MoneyManager.Instance.CurrentMoney >= freedomPrice;

    public event Action<bool> EndingConditionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.MoneyChanged += OnMoneyChanged;
        }
    }

    private void OnDestroy()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.MoneyChanged -= OnMoneyChanged;
        }
    }

    public void SetCurrentStage(int stageNumber)
    {
        CurrentStage = Mathf.Max(1, stageNumber);
    }

    public void StartNewGame()
    {
        CurrentStage = 1;
        MoneyManager.Instance?.ResetMoney();
        SceneLoader.LoadScene("ShopScene");
    }

    public bool TryCompleteGame()
    {
        if (!CanBuyFreedom)
        {
            return false;
        }

        SceneLoader.LoadScene("EndingScene");
        return true;
    }

    private void OnMoneyChanged(int amount)
    {
        EndingConditionChanged?.Invoke(amount >= freedomPrice);
    }
}
