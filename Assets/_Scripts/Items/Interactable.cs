using UnityEngine;
public enum ItemType
{
    Weapon,
    Door,
    None
}
public class Interactable : MonoBehaviour
{
    public ItemType interactableType;

}
