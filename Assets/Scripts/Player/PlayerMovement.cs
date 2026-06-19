using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float baseMoveSpeed = 8f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.65f;
    [SerializeField, Min(0f)] private float acceleration = 45f;
    [SerializeField] private InputActionReference moveAction;

    private Rigidbody body;
    private Vector2 moveInput;

    public Vector2 MoveInput => moveInput;
    public bool MovementLocked { get; set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
    }

    private void Update()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            horizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            vertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        }

        moveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
    }

    private void FixedUpdate()
    {
        if (MovementLocked)
        {
            return;
        }

        Vector3 inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        float equipmentMultiplier = EquipmentManager.Instance != null
            ? EquipmentManager.Instance.MovementSpeedMultiplier
            : 1f;
        float targetSpeed = baseMoveSpeed * equipmentMultiplier;
        Vector3 targetVelocity = inputDirection * targetSpeed;

        Vector3 currentHorizontal = new(body.linearVelocity.x, 0f, body.linearVelocity.z);
        float control = IsGrounded() ? 1f : airControl;
        Vector3 nextHorizontal = Vector3.MoveTowards(
            currentHorizontal,
            targetVelocity,
            acceleration * control * Time.fixedDeltaTime);

        body.linearVelocity = new Vector3(nextHorizontal.x, body.linearVelocity.y, nextHorizontal.z);
    }

    public Vector3 GetDesiredWorldDirection()
    {
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        return direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
    }

    private bool IsGrounded()
    {
        PlayerJump jump = GetComponent<PlayerJump>();
        return jump != null && jump.IsGrounded;
    }
}
