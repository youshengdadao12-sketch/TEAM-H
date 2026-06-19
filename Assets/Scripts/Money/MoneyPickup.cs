using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class MoneyPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int amount = 1;
    [SerializeField, Min(0f)] private float rotationSpeed = 120f;

    public void Configure(int value)
    {
        amount = Mathf.Max(1, value);
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        MoneyManager.Instance?.AddMoney(amount);
        Destroy(gameObject);
    }
}
