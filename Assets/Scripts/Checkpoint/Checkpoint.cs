using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    private bool activated;

    public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        activated = true;
        CheckpointManager.Instance?.Activate(this);
    }
}
