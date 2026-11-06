using UnityEngine;

public class Key : MonoBehaviour
{
    public KeyType keyType;
    public DropType dropType;
    [SerializeField] public AudioClip sound;
}
public enum KeyType
{
    blue,
    red,
    yellow
}
