using System;
using UnityEngine;

public sealed class ShopManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private EquipmentManager equipment;

    public event Action<PartData> PurchaseSucceeded;
    public event Action<PartData> PurchaseFailed;
    public event Action<PartData> SaleSucceeded;
    public event Action<PartData> SaleFailed;

    private void Awake()
    {
        inventory ??= InventoryManager.Instance;
        equipment ??= EquipmentManager.Instance;
    }

    public bool Buy(PartData part)
    {
        inventory ??= InventoryManager.Instance;
        equipment ??= EquipmentManager.Instance;

        if (part == null || inventory == null || inventory.Owns(part) || MoneyManager.Instance == null)
        {
            PurchaseFailed?.Invoke(part);
            return false;
        }

        if (!MoneyManager.Instance.TrySpend(part.PurchasePrice))
        {
            PurchaseFailed?.Invoke(part);
            return false;
        }

        inventory.AddPart(part);
        equipment?.Equip(part);
        PurchaseSucceeded?.Invoke(part);
        return true;
    }

    public bool Sell(PartData part)
    {
        inventory ??= InventoryManager.Instance;
        equipment ??= EquipmentManager.Instance;

        if (part == null || inventory == null || !inventory.Owns(part) || MoneyManager.Instance == null)
        {
            SaleFailed?.Invoke(part);
            return false;
        }

        equipment?.Unequip(part);
        inventory.RemovePart(part);
        MoneyManager.Instance.AddMoney(part.SellPrice);
        SaleSucceeded?.Invoke(part);
        return true;
    }

    public bool EquipOwnedPart(PartData part)
    {
        inventory ??= InventoryManager.Instance;
        equipment ??= EquipmentManager.Instance;
        return part != null && inventory != null && inventory.Owns(part) && equipment != null && equipment.Equip(part);
    }
}
