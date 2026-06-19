using UnityEngine;
using UnityEngine.UI;

public sealed class HUDManager : MonoBehaviour
{
    [SerializeField] private MoneyUI moneyUI;
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Text actionStatusText;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        moneyUI?.Bind(MoneyManager.Instance);
        healthUI?.Bind(playerHealth);

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.EquipmentChanged += RefreshActionStatus;
        }

        RefreshActionStatus();
    }

    private void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.EquipmentChanged -= RefreshActionStatus;
        }
    }

    private void RefreshActionStatus()
    {
        if (actionStatusText == null)
        {
            return;
        }

        EquipmentManager equipment = EquipmentManager.Instance;
        if (equipment == null)
        {
            actionStatusText.text = "二段ジャンプ / 地上ダッシュ / 空中ダッシュ / バスター";
            return;
        }

        string jump = equipment.MaxJumps >= 2 ? "二段ジャンプ" : "通常ジャンプ";
        string groundDash = equipment.CanGroundDash ? "地上DASH" : "地上DASH不可";
        string airDash = equipment.CanAirDash ? "空中DASH" : "空中DASH不可";
        string shot = equipment.CanShoot ? $"バスター威力 {equipment.AttackDamage}" : "射撃不可";
        actionStatusText.text = $"{jump}  |  {groundDash}  |  {airDash}  |  {shot}";
    }
}
