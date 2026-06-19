using UnityEngine;
using UnityEngine.UI;

public sealed class MoneyUI : MonoBehaviour
{
    [SerializeField] private Text moneyText;

    private MoneyManager manager;

    private void Start()
    {
        Bind(MoneyManager.Instance);
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.MoneyChanged -= UpdateDisplay;
        }
    }

    public void Bind(MoneyManager moneyManager)
    {
        if (manager != null)
        {
            manager.MoneyChanged -= UpdateDisplay;
        }

        manager = moneyManager;
        if (manager != null)
        {
            manager.MoneyChanged += UpdateDisplay;
            UpdateDisplay(manager.CurrentMoney);
        }
    }

    private void UpdateDisplay(int amount)
    {
        if (moneyText == null)
        {
            return;
        }

        int goal = GameManager.Instance != null ? GameManager.Instance.FreedomPrice : 0;
        moneyText.text = goal > 0 ? $"{amount} / {goal} G" : $"{amount} G";
    }
}
