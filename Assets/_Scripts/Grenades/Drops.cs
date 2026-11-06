using UnityEngine;

public class Drops : MonoBehaviour
{
    [SerializeField] public DropType dropType;
    [SerializeField] public float healingAmount;
    [SerializeField] public AudioClip sound;
}
public enum DropType
{
    HEALING,
    BULLET,
    KEY
}
