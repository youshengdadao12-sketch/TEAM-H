using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Part Data")]
public class PartData : ScriptableObject
{
    public PartType partType;
    public string partName;
    public int power;
}
