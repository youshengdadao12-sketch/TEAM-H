using UnityEngine;
using UnityEngine.UI;

public sealed class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text abilityText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    private PartData part;
    private ShopManager shop;

    public void Setup(PartData partData, ShopManager shopManager)
    {
        part = partData;
        shop = shopManager;

        if (icon != null)
        {
            icon.sprite = part?.Icon;
            icon.enabled = part?.Icon != null;
        }

        if (nameText != null)
        {
            nameText.text = part == null ? "未設定" : $"{part.DisplayName} [{part.Grade}]";
        }

        if (priceText != null)
        {
            priceText.text = part == null ? "-" : $"購入 {part.PurchasePrice} G / 売却 {part.SellPrice} G";
        }

        if (abilityText != null)
        {
            abilityText.text = BuildAbilityText(part);
        }

        RefreshButtons();
    }

    public void Buy()
    {
        shop?.Buy(part);
        RefreshButtons();
    }

    public void Sell()
    {
        shop?.Sell(part);
        RefreshButtons();
    }

    public void Equip()
    {
        shop?.EquipOwnedPart(part);
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        bool owned = part != null && InventoryManager.Instance != null && InventoryManager.Instance.Owns(part);

        if (buyButton != null)
        {
            buyButton.interactable = part != null && shop != null && !owned;
        }

        if (sellButton != null)
        {
            sellButton.interactable = part != null && shop != null && owned;
        }
    }

    private static string BuildAbilityText(PartData data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        return data.PartType switch
        {
            PartType.Buster => data.CanShoot
                ? $"攻撃 {data.AttackDamage} / 連射間隔 {data.FireInterval:0.00}秒"
                : "射撃不可",
            PartType.Leg => $"移動 x{data.MovementSpeedMultiplier:0.00} / ジャンプ {data.MaxJumps}回",
            PartType.Booster => $"地上ダッシュ {(data.CanGroundDash ? "可" : "不可")} / 空中ダッシュ {(data.CanAirDash ? "可" : "不可")}",
            _ => string.Empty
        };
    }
}
