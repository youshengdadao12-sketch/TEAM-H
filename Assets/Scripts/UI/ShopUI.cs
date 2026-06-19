using UnityEngine;
using UnityEngine.UI;

public sealed class ShopUI : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Text messageText;
    [SerializeField] private Text goalText;
    [SerializeField] private Button endingButton;

    private void OnEnable()
    {
        if (shopManager != null)
        {
            shopManager.PurchaseSucceeded += ShowPurchaseSucceeded;
            shopManager.PurchaseFailed += ShowPurchaseFailed;
            shopManager.SaleSucceeded += ShowSaleSucceeded;
            shopManager.SaleFailed += ShowSaleFailed;
        }

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.MoneyChanged += RefreshGoal;
            RefreshGoal(MoneyManager.Instance.CurrentMoney);
        }
    }

    private void OnDisable()
    {
        if (shopManager != null)
        {
            shopManager.PurchaseSucceeded -= ShowPurchaseSucceeded;
            shopManager.PurchaseFailed -= ShowPurchaseFailed;
            shopManager.SaleSucceeded -= ShowSaleSucceeded;
            shopManager.SaleFailed -= ShowSaleFailed;
        }

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.MoneyChanged -= RefreshGoal;
        }
    }

    public void CloseMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    public void TryGoToEnding()
    {
        if (GameManager.Instance == null || !GameManager.Instance.TryCompleteGame())
        {
            ShowMessage("自由を買うには、まだお金が足りません。");
        }
    }

    private void ShowPurchaseSucceeded(PartData part)
    {
        ShowMessage(part == null
            ? "購入しました。"
            : $"{part.DisplayName}を購入・装備しました。性能は下がっても、差額は自由への一歩です。");
    }

    private void ShowPurchaseFailed(PartData part)
    {
        ShowMessage("購入できませんでした。所持金または在庫を確認してください。");
    }

    private void ShowSaleSucceeded(PartData part)
    {
        ShowMessage(part == null
            ? "パーツを売却しました。"
            : $"{part.DisplayName}を {part.SellPrice} G で売却しました。このパーツの能力は使用できなくなります。");
    }

    private void ShowSaleFailed(PartData part)
    {
        ShowMessage("そのパーツは売却できません。");
    }

    private void RefreshGoal(int currentMoney)
    {
        int goal = GameManager.Instance != null ? GameManager.Instance.FreedomPrice : 0;

        if (goalText != null)
        {
            goalText.text = $"自由資金 {currentMoney} / {goal} G";
        }

        if (endingButton != null)
        {
            endingButton.interactable = goal > 0 && currentMoney >= goal;
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }
    }
}
