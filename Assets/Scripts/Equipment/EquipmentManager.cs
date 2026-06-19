using System;
using UnityEngine;

public sealed class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [SerializeField] private InventoryManager inventory;
    [SerializeField] private PartData equippedBuster;
    [SerializeField] private PartData equippedLeg;
    [SerializeField] private PartData equippedBooster;

    public event Action EquipmentChanged;

    public float MovementSpeedMultiplier => equippedLeg != null ? equippedLeg.MovementSpeedMultiplier : 0.65f;
    public float JumpPowerMultiplier => equippedLeg != null ? equippedLeg.JumpPowerMultiplier : 0.65f;
    public int MaxJumps => equippedLeg != null ? equippedLeg.MaxJumps : 1;
    public bool CanGroundDash => equippedBooster != null && equippedBooster.CanGroundDash;
    public bool CanAirDash => equippedBooster != null && equippedBooster.CanAirDash;
    public float DashSpeedMultiplier => equippedBooster != null ? equippedBooster.DashSpeedMultiplier : 1f;
    public bool CanShoot => equippedBuster != null && equippedBuster.CanShoot;
    public int AttackDamage => equippedBuster != null ? equippedBuster.AttackDamage : 0;
    public float FireInterval => equippedBuster != null ? equippedBuster.FireInterval : 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        inventory ??= InventoryManager.Instance;
    }

    public PartData GetEquippedPart(PartType type)
    {
        return type switch
        {
            PartType.Buster => equippedBuster,
            PartType.Leg => equippedLeg,
            PartType.Booster => equippedBooster,
            _ => null
        };
    }

    public bool IsEquipped(PartData part)
    {
        return part != null && GetEquippedPart(part.PartType) == part;
    }

    public bool Equip(PartData part)
    {
        inventory ??= InventoryManager.Instance;
        if (part == null || (inventory != null && !inventory.Owns(part)))
        {
            return false;
        }

        switch (part.PartType)
        {
            case PartType.Buster:
                equippedBuster = part;
                break;
            case PartType.Leg:
                equippedLeg = part;
                break;
            case PartType.Booster:
                equippedBooster = part;
                break;
            default:
                return false;
        }

        EquipmentChanged?.Invoke();
        return true;
    }

    public bool Unequip(PartData part)
    {
        if (!IsEquipped(part))
        {
            return false;
        }

        switch (part.PartType)
        {
            case PartType.Buster:
                equippedBuster = null;
                break;
            case PartType.Leg:
                equippedLeg = null;
                break;
            case PartType.Booster:
                equippedBooster = null;
                break;
        }

        EquipmentChanged?.Invoke();
        return true;
    }
}
