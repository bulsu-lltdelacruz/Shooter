using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] private Player player;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("PickUp"))
            return;
        Drops drop = other.gameObject.GetComponent<Drops>();
        DropType dropType = drop.dropType;
        if(dropType.Equals(DropType.HEALING))
        {
            player.Heal(drop.healingAmount);
        }
        else if (dropType.Equals(DropType.BULLET))
        {
            player.weaponController.RestoreBullets(.1f);
        }
        Destroy(other.gameObject);
    }
}
