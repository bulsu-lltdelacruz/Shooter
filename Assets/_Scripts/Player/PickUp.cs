using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] private Player player;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PickUp"))
        {
            Drops drop = other.gameObject.GetComponent<Drops>();
            DropType dropType = drop.dropType;
            if(dropType.Equals(DropType.HEALING))
            {
                player.Heal(drop.healingAmount);
            }
            Destroy(other.gameObject);
        }
    }
}
