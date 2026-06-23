//カメラの範囲制限
using UnityEngine;
public class CameraBounds : MonoBehaviour
{
    [serializeField] private Vector2 minBoundx;
    [serializeField] private Vector2 maxBoundx;
    [serializeField] private Vector2 minBoundy;
    [serializeField] private Vector2 maxBoundy;
    void LateUpdate()
    {
        Vector3 clampedPosition = transform.position;
        clampedPositon.x = Mathf.Clamp(clampedPosition.x, minBoundx.x, maxBoundx.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBoundy.y, maxBoundy.y);
        transform.position = clampedPosition;
    }
}