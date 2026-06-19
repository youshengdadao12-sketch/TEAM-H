using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerJump : MonoBehaviour
{
    [SerializeField, Min(0f)] private float baseJumpPower = 8f;
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private InputActionReference jumpAction;

    private Rigidbody body;
    private int jumpsUsed;
    private bool wasGrounded;

    public bool IsGrounded
    {
        get
        {
            Vector3 center = groundCheck != null
                ? groundCheck.position
                : transform.position + Vector3.down * 0.9f;
            Collider[] hits = Physics.OverlapSphere(
                center,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore);

            foreach (Collider hit in hits)
            {
                if (hit != null && !hit.transform.IsChildOf(transform))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private int MaxJumps => EquipmentManager.Instance != null
        ? EquipmentManager.Instance.MaxJumps
        : 2;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJumpPerformed;
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.Disable();
        }
    }

    private void Update()
    {
        bool grounded = IsGrounded;
        if (grounded && !wasGrounded)
        {
            jumpsUsed = 0;
        }

        wasGrounded = grounded;

        if (jumpAction == null && Keyboard.current?.spaceKey.wasPressedThisFrame == true)
        {
            TryJump();
        }
    }

    public bool TryJump()
    {
        if (IsGrounded)
        {
            jumpsUsed = 0;
        }

        if (jumpsUsed >= MaxJumps)
        {
            return false;
        }

        float equipmentMultiplier = EquipmentManager.Instance != null
            ? EquipmentManager.Instance.JumpPowerMultiplier
            : 1f;

        body.linearVelocity = new Vector3(
            body.linearVelocity.x,
            baseJumpPower * equipmentMultiplier,
            body.linearVelocity.z);
        jumpsUsed++;
        return true;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        TryJump();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = groundCheck != null
            ? groundCheck.position
            : transform.position + Vector3.down * 0.9f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}
