using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(1f)] private float range = 80f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private InputActionReference attackAction;

    private float nextAttackTime;

    private void Awake()
    {
        aimCamera ??= Camera.main;
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        attackAction?.action.Disable();
    }

    private void Update()
    {
        bool pressed = attackAction != null
            ? attackAction.action.IsPressed()
            : Mouse.current?.leftButton.isPressed == true;

        if (pressed)
        {
            TryAttack();
        }
    }

    public bool TryAttack()
    {
        EquipmentManager equipment = EquipmentManager.Instance;
        bool canShoot = equipment == null || equipment.CanShoot;
        int damage = equipment != null ? equipment.AttackDamage : 3;
        float fireInterval = equipment != null ? equipment.FireInterval : 0.2f;

        if (!canShoot || damage <= 0 || Time.time < nextAttackTime || aimCamera == null)
        {
            return false;
        }

        nextAttackTime = Time.time + fireInterval;
        muzzleEffect?.Play();

        Ray ray = new(aimCamera.transform.position, aimCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitLayers, QueryTriggerInteraction.Ignore))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            enemy?.TakeDamage(damage);
        }

        return true;
    }
}
