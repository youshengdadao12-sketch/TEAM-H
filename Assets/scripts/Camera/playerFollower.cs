//カメラ追従
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    [SerializeField] private Transform PlayerTransform;
    private Vector3 offset;
    void Start()
    {
        offset = transform.position - PlayerTransform.position;
    }
    void LateUpdate()
    {
        if (PlayerTransform != null)
        {
            transform.position = PlayerTransform.position + offset;
        }
    }
}