using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<PartData> ownedParts = new();

    public IReadOnlyList<PartData> OwnedParts => ownedParts;
    public event Action InventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Owns(PartData part)
    {
        return part != null && ownedParts.Contains(part);
    }

    public bool AddPart(PartData part)
    {
        if (part == null || Owns(part))
        {
            return false;
        }

        ownedParts.Add(part);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool RemovePart(PartData part)
    {
        if (!ownedParts.Remove(part))
        {
            return false;
        }

        InventoryChanged?.Invoke();
        return true;
    }
}
