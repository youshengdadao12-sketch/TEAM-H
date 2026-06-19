using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJump))]
public sealed class PlayerDash : MonoBehaviour
{
    [SerializeField, Min(0f)] private float baseDashSpeed = 18f;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.18f;
    [SerializeField, Min(0f)] private float cooldown = 0.2f;
    [SerializeField] private InputActionReference dashAction;

    private Rigidbody body;
    private PlayerMovement movement;
    private PlayerJump jump;
    private bool airDashUsed;
    private bool dashing;
    private float availableAt;
    private Coroutine dashRoutine;

    public bool IsDashing => dashing;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
    }

    private void OnEnable()
    {
        if (dashAction != null)
        {
            dashAction.action.Enable();
            dashAction.action.performed += OnDashPerformed;
        }
    }

    private void OnDisable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed -= OnDashPerformed;
            dashAction.action.Disable();
        }

        StopDash();
    }

    private void Update()
    {
        if (jump.IsGrounded)
        {
            airDashUsed = false;
        }

        if (dashAction == null && Keyboard.current?.leftShiftKey.wasPressedThisFrame == true)
        {
            TryDash();
        }
    }

    public bool TryDash()
    {
        if (dashing || Time.time < availableAt)
        {
            return false;
        }

        EquipmentManager equipment = EquipmentManager.Instance;
        bool grounded = jump.IsGrounded;
        bool allowed = equipment == null
            ? true
            : grounded
                ? equipment.CanGroundDash
                : equipment.CanAirDash && !airDashUsed;

        if (!allowed)
        {
            return false;
        }

        if (!grounded)
        {
            airDashUsed = true;
        }

        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
        }

        dashRoutine = StartCoroutine(DashRoutine());
        return true;
    }

    private IEnumerator DashRoutine()
    {
        dashing = true;
        movement.MovementLocked = true;
        body.useGravity = false;

        float multiplier = EquipmentManager.Instance != null
            ? EquipmentManager.Instance.DashSpeedMultiplier
            : 1f;
        Vector3 direction = movement.GetDesiredWorldDirection();
        body.linearVelocity = direction * baseDashSpeed * multiplier;

        yield return new WaitForSeconds(dashDuration);

        StopDash();
        availableAt = Time.time + cooldown;
    }

    private void StopDash()
    {
        dashing = false;

        if (movement != null)
        {
            movement.MovementLocked = false;
        }

        if (body != null)
        {
            body.useGravity = true;
        }

        dashRoutine = null;
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        TryDash();
    }
}
