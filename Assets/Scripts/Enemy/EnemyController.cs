using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class EnemyController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField, Min(0f)] private float detectionRange = 15f;
    [SerializeField, Min(0f)] private float attackRange = 1.8f;
    [SerializeField, Min(1)] private int contactDamage = 1;
    [SerializeField, Min(0.05f)] private float attackInterval = 1f;

    private Rigidbody body;
    private PlayerController target;
    private float nextAttackTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
    }

    private void Start()
    {
        target = FindFirstObjectByType<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<PlayerController>();
            return;
        }

        Vector3 offset = target.transform.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (distance > detectionRange)
        {
            StopHorizontalMovement();
            return;
        }

        if (distance <= attackRange)
        {
            StopHorizontalMovement();
            TryAttack();
            return;
        }

        Vector3 direction = offset.normalized;
        body.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            body.linearVelocity.y,
            direction.z * moveSpeed);

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || target?.Health == null)
        {
            return;
        }

        nextAttackTime = Time.time + attackInterval;
        target.Health.TakeDamage(contactDamage);
    }

    private void StopHorizontalMovement()
    {
        body.linearVelocity = new Vector3(0f, body.linearVelocity.y, 0f);
    }
}
