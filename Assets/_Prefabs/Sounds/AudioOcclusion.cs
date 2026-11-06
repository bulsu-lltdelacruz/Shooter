using UnityEngine;

/// <summary>
/// Muffles and quiets an AudioSource when a wall is between it and the listener.
/// Add this (and an AudioLowPassFilter) to your SFX AudioSource prefab.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class AudioOcclusion : MonoBehaviour
{
    [Tooltip("Layers that block sound, e.g. Ground and Wall")]
    [SerializeField] private LayerMask occluders;
    [SerializeField] private float openCutoff = 22000f;
    [Tooltip("Lower = more muffled")]
    [SerializeField] private float occludedCutoff = 800f;
    [Range(0f, 1f)]
    [SerializeField] private float occludedVolumeMultiplier = 0.5f;
    [SerializeField] private float checkInterval = 0.1f;
    [SerializeField] private float smoothing = 10f;

    private AudioSource source;
    private AudioLowPassFilter lowPass;
    private Transform listener;

    private float baseVolume;
    private float volumeMultiplier = 1f;
    private float targetCutoff;
    private float targetMultiplier = 1f;
    private float timer;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
    }

    private void Start()
    {
        // Start runs after whoever spawned this has set the volume
        baseVolume = source.volume;

        if (Camera.main != null)
            listener = Camera.main.transform;

        // Snap immediately so short sounds (footsteps) are muffled from the first frame
        Evaluate();
        volumeMultiplier = targetMultiplier;
        lowPass.cutoffFrequency = targetCutoff;
        source.volume = baseVolume * volumeMultiplier;
    }

    private void Update()
    {
        if (listener == null)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = checkInterval;
            Evaluate();
        }

        float t = smoothing * Time.deltaTime;
        lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, targetCutoff, t);
        volumeMultiplier = Mathf.Lerp(volumeMultiplier, targetMultiplier, t);
        source.volume = baseVolume * volumeMultiplier;
    }

    private void Evaluate()
    {
        bool blocked = listener != null &&
                       Physics.Linecast(listener.position, transform.position, occluders, QueryTriggerInteraction.Ignore);

        targetCutoff = blocked ? occludedCutoff : openCutoff;
        targetMultiplier = blocked ? occludedVolumeMultiplier : 1f;
    }
}