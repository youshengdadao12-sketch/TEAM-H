using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 respawnPoint;

    public void Respawn()
    {
        transform.position = respawnPoint;
    }
}
