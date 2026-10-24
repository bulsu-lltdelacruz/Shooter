using UnityEngine;
using UnityEngine.Audio;

public class HitLag : MonoBehaviour
{
    public static HitLag Instance;

    [Header("Audio")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] string pitchParam = "GamePitch";
    [SerializeField] float minPitch = 0.05f; // mixer pitch can't be exactly 0

    float freezeTimer;
    float slowTimer;
    float slowScale = 1f;
    float prevScale = 1f;

    bool Active => freezeTimer > 0f || slowTimer > 0f;

    void Awake() => Instance = this;

    public void Freeze(float duration)
    {
        if (!Active) prevScale = Time.timeScale;

        freezeTimer = Mathf.Max(freezeTimer, duration);
        ApplyScale();
    }

    public void SlowMo(float duration, float scale = 0.3f)
    {
        if (!Active) prevScale = Time.timeScale;

        scale = Mathf.Clamp01(scale);
        slowScale = slowTimer > 0f ? Mathf.Min(slowScale, scale) : scale;
        slowTimer = Mathf.Max(slowTimer, duration);
        ApplyScale();
    }

    void Update()
    {
        if (!Active) return;

        float dt = Time.unscaledDeltaTime;
        freezeTimer -= dt;
        slowTimer -= dt;

        ApplyScale();
    }

    void ApplyScale()
    {
        if (freezeTimer > 0f) Time.timeScale = 0f;
        else if (slowTimer > 0f) Time.timeScale = slowScale;
        else Time.timeScale = prevScale;

        //SetAudioPitch(Time.timeScale);
    }

    void SetAudioPitch(float pitch)
    {
        if (mixer != null)
            mixer.SetFloat(pitchParam, Mathf.Max(minPitch, pitch));
    }

    void OnDisable()
    {
        if (Active) Time.timeScale = prevScale;
        freezeTimer = slowTimer = 0f;
        //SetAudioPitch(1f);
    }
}