//カメラの範囲制限
using UnityEngine;
public class CameraBounds : MonoBehaviour
{
    [SerializeField] private float minBoundx;
    [SerializeField] private float maxBoundx;
    [SerializeField] private float minBoundy;
    [SerializeField] private float maxBoundy;
    void LateUpdate()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBoundx, maxBoundx);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBoundy, maxBoundy);
        transform.position = clampedPosition;
    }
}