using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public void Move(float horizontal)
    {
        transform.Translate(Vector2.right * horizontal * moveSpeed * Time.deltaTime);
    }
}
