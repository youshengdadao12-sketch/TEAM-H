using UnityEngine;

public sealed class MoneyDrop : MonoBehaviour
{
    [SerializeField] private MoneyPickup pickupPrefab;
    [SerializeField, Min(1)] private int amount = 1;

    public void Drop(Vector3 position)
    {
        if (pickupPrefab == null)
        {
            MoneyManager.Instance?.AddMoney(amount);
            return;
        }

        MoneyPickup pickup = Instantiate(pickupPrefab, position, Quaternion.identity);
        pickup.Configure(amount);
    }
}
