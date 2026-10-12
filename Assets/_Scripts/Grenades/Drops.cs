using UnityEngine;

public class Drops : MonoBehaviour
{
    [SerializeField] public DropType dropType;
    [SerializeField] public float healingAmount;
}
public enum DropType
{
    HEALING,
    BULLET
}
