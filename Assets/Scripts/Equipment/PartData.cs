using UnityEngine;

public enum PartGrade
{
    Junk,
    Cheap,
    Premium
}

[CreateAssetMenu(fileName = "NewPart", menuName = "Game/Part Data")]
public sealed class PartData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string displayName;
    [SerializeField] private PartType partType;
    [SerializeField] private PartGrade grade;
    [SerializeField, Min(0)] private int purchasePrice;
    [SerializeField, Min(0)] private int sellPrice;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea] private string description;

    [Header("脚部性能")]
    [SerializeField, Min(0.1f)] private float movementSpeedMultiplier = 1f;
    [SerializeField, Min(0.1f)] private float jumpPowerMultiplier = 1f;
    [SerializeField, Range(1, 2)] private int maxJumps = 1;

    [Header("ブースター性能")]
    [SerializeField] private bool canGroundDash;
    [SerializeField] private bool canAirDash;
    [SerializeField, Min(0.1f)] private float dashSpeedMultiplier = 1f;

    [Header("バスター性能")]
    [SerializeField] private bool canShoot = true;
    [SerializeField, Min(1)] private int attackDamage = 1;
    [SerializeField, Min(0.05f)] private float fireInterval = 0.35f;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public PartType PartType => partType;
    public PartGrade Grade => grade;
    public int PurchasePrice => purchasePrice;
    public int SellPrice => sellPrice;
    public int Price => purchasePrice;
    public Sprite Icon => icon;
    public string Description => description;
    public float MovementSpeedMultiplier => movementSpeedMultiplier;
    public float JumpPowerMultiplier => jumpPowerMultiplier;
    public int MaxJumps => maxJumps;
    public bool CanGroundDash => canGroundDash;
    public bool CanAirDash => canAirDash;
    public float DashSpeedMultiplier => dashSpeedMultiplier;
    public bool CanShoot => canShoot;
    public int AttackDamage => attackDamage;
    public float FireInterval => fireInterval;
}
