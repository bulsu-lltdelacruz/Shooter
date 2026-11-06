using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField] public AudioClip footsteps_default;
    [SerializeField] public AudioClip footsteps_water;
    [SerializeField] public AudioClip handgun_fire;
    [SerializeField] public AudioClip shotgun_fire;
    [SerializeField] public AudioClip shotgun_reload1;
    [SerializeField] public AudioClip shotgun_cock;
    [SerializeField] public AudioClip shotgun_reload2;
    [SerializeField] public AudioClip hit_sound;
    [SerializeField] public AudioClip dash_sound;
    [SerializeField] public AudioClip menu_hover;
    [SerializeField] public AudioClip menu_click;
    [SerializeField] public AudioClip punch;
    public static PlayerSounds Instance;
    private void Start()
    {
        if(Instance == null)
            Instance = this;
    }

}
public enum FootStepType
{
    DEFAULT,
    WATER
}
