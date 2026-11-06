using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] private Player player;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PickUp"))
            return;
        // Existing drops (healing / bullets)
        Drops drop = other.gameObject.GetComponent<Drops>();
        if (drop == null)
            return;

        DropType dropType = drop.dropType;
        if (dropType.Equals(DropType.HEALING))
        {
            player.Heal(drop.healingAmount);
            SoundFXManager.instance.PlaySoundFXClip(drop.sound, transform, 1, SoundType.SOUNDFX);
        }
        else if (dropType.Equals(DropType.BULLET))
        {
            player.weaponController.RestoreBullets(.15f);
            SoundFXManager.instance.PlaySoundFXClip(drop.sound, transform, 1, SoundType.SOUNDFX);
        }
        else if (dropType.Equals(DropType.KEY))
        {
            Key key = other.GetComponent<Key>();
            player.keyInventory.AddKey(key);
            SoundFXManager.instance.PlaySoundFXClip(key.sound, transform, 1, SoundType.SOUNDFX);
        }
        Destroy(other.gameObject);
    }
}