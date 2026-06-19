using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerHealth))]
public sealed class PlayerController : MonoBehaviour
{
    public PlayerMovement Movement { get; private set; }
    public PlayerJump Jump { get; private set; }
    public PlayerDash Dash { get; private set; }
    public PlayerLook Look { get; private set; }
    public PlayerAttack Attack { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerAbility Ability { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Jump = GetComponent<PlayerJump>();
        Dash = GetComponent<PlayerDash>();
        Look = GetComponent<PlayerLook>();
        Attack = GetComponent<PlayerAttack>();
        Health = GetComponent<PlayerHealth>();
        Ability = GetComponent<PlayerAbility>();

        Rigidbody body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }
}
