using System;
using UnityEngine;

public sealed class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [SerializeField] private Transform defaultRespawnPoint;

    public Checkpoint ActiveCheckpoint { get; private set; }
    public Vector3 RespawnPosition =>
        ActiveCheckpoint != null
            ? ActiveCheckpoint.RespawnPosition
            : defaultRespawnPoint != null
                ? defaultRespawnPoint.position
                : Vector3.zero;

    public event Action<Checkpoint> CheckpointActivated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Activate(Checkpoint checkpoint)
    {
        if (checkpoint == null || checkpoint == ActiveCheckpoint)
        {
            return;
        }

        ActiveCheckpoint = checkpoint;
        CheckpointActivated?.Invoke(checkpoint);
    }

    public void Respawn(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        Rigidbody body = player.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = RespawnPosition;
        }
        else
        {
            player.transform.position = RespawnPosition;
        }

        player.Health?.RestoreFullHealth();
    }
}
