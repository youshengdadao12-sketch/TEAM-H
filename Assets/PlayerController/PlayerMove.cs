using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpPower = 7f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) moveDirection += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) moveDirection += Vector3.back;
        if (Keyboard.current.aKey.isPressed) moveDirection += Vector3.left;
        if (Keyboard.current.dKey.isPressed) moveDirection += Vector3.right;

        moveDirection = moveDirection.normalized;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            Debug.Log("Jump!");
        }
    }

    void FixedUpdate()
    {
        if (moveDirection != Vector3.zero)
        {
            Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }
    }
}