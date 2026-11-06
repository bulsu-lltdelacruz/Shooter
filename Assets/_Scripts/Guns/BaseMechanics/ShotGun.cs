using UnityEngine;

public class ShotGun : BaseWeaponClass
{
    [SerializeField] AudioClip reload1;
    [SerializeField] AudioClip reload2;
    [SerializeField] AudioClip cock;

    public void PlayReload1()
    {
        SoundFXManager.instance.PlaySoundFXClip(reload2, transform, .5f, SoundType.SOUNDFX);
    }
    public void PlayReload2()
    {
        SoundFXManager.instance.PlaySoundFXClip(reload2, transform, .5f, SoundType.SOUNDFX);
    }
    public void PlayCock()
    {
        SoundFXManager.instance.PlaySoundFXClip(cock, transform, .5f, SoundType.SOUNDFX);
    }
}
