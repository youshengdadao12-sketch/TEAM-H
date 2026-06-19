using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.12f;
    [SerializeField, Min(1f)] private float gamepadSensitivity = 140f;
    [SerializeField, Range(30f, 89f)] private float verticalLimit = 85f;
    [SerializeField] private InputActionReference lookAction;

    private float pitch;

    private void Awake()
    {
        if (cameraPivot == null && Camera.main != null)
        {
            cameraPivot = Camera.main.transform;
        }
    }

    private void Start()
    {
        SetCursorLocked(true);
    }

    private void OnEnable()
    {
        lookAction?.action.Enable();
    }

    private void OnDisable()
    {
        lookAction?.action.Disable();
    }

    private void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
        }

        if (Cursor.lockState != CursorLockMode.Locked || cameraPivot == null)
        {
            return;
        }

        Vector2 look = lookAction != null
            ? lookAction.action.ReadValue<Vector2>()
            : Mouse.current?.delta.ReadValue() ?? Vector2.zero;

        bool usingMouse = Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0f;
        float scale = usingMouse ? mouseSensitivity : gamepadSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up, look.x * scale, Space.World);
        pitch = Mathf.Clamp(pitch - look.y * scale, -verticalLimit, verticalLimit);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
