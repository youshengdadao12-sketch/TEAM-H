using UnityEngine;

public sealed class EnemyDropMoney : MonoBehaviour
{
    [SerializeField] private MoneyDrop moneyDrop;

    public void Drop()
    {
        moneyDrop?.Drop(transform.position);
    }
}
