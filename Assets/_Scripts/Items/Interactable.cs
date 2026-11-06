using UnityEngine;
public enum ItemType
{
    Weapon,
    Door,
    Terminal,
    Guide,
    None
}
public class Interactable : MonoBehaviour
{
    public ItemType interactableType;

}
