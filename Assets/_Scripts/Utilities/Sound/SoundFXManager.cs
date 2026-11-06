using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private AudioSource menuFXObject;
    private bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }
    public void PlayForSeconds(AudioClip clip, Vector3 position, float seconds, float volume = 1f)
    {
        GameObject go = new GameObject("TempSound");
        go.transform.position = position;

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.spatialBlend = 1f;
        src.Play();

        Object.Destroy(go, seconds);
    }
    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume, SoundType source)
    {
        if (isQuitting)
            return;
        AudioSource audioSource = null;
        if (source == SoundType.SOUNDFX)
        {
            audioSource = ObjectPoolManager.SpawnObject(soundFXObject, spawnTransform.position, Quaternion.identity);
        }
        else if (source == SoundType.MENUFX)
        {
            audioSource = ObjectPoolManager.SpawnObject(menuFXObject, spawnTransform.position, Quaternion.identity);
        }
        else if (source == SoundType.MENUFX)
        {
            audioSource = ObjectPoolManager.SpawnObject(soundFXObject, spawnTransform.position, Quaternion.identity);
        }


        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }
}
public enum SoundType
{
    SOUNDFX,
    MENUFX,
    MUSICFX
}