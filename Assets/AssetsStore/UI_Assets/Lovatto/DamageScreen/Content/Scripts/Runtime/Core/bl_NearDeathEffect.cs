using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

public class bl_NearDeathEffect : MonoBehaviour
{
    [SerializeField] private CanvasGroup heartBeatVessel = null;
    [Tooltip("Minimum heartbeat vessel alpha factor while pulsing (0.4 means 40% to 100% of current intensity).")]
    [SerializeField, Range(0f, 1f)] private float heartBeatVesselMinOscillation = 0.4f;
    [Tooltip("Audio source for the heartbeat sound.")]
    public AudioSource heartbeatAudioSource;
    [Tooltip("Heartbeat clip assigned to the AudioSource at runtime if provided.")]
    [SerializeField] private AudioClip heartbeatSound = null;
    [Tooltip("Canvas-based fallback visual for fast mode (useFastScreenEffect).")]
    [SerializeField] private CanvasGroup alphaCanvas = null;
#if UNITY_POST_PROCESSING_STACK_V2
    [Tooltip("Post-process volume used when the legacy Post Processing Stack v2 is enabled.")]
    [SerializeField] PostProcessVolume postProcessVolume;
    private ChromaticAberration chromaticAberration;
#endif
    [Tooltip("URP/HDRP volume used for near-death visual intensity when fast mode is disabled.")]
    [SerializeField] Volume volume;

    private bool isPlaying = false;
    float targetHealthRatio;
    float volumeWeight;
    private float heartbeatTimer = 0f;
    private bool warnedMissingHeartbeatClip;

    void Awake()
    {

#if UNITY_POST_PROCESSING_STACK_V2
        if (postProcessVolume != null)
        {
            postProcessVolume.weight = 0;
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        }
#endif

        if (volume != null)
        {
            volume.weight = 0;
            volume.gameObject.SetActive(!bl_DamageScreenSettings.Instance.useFastScreenEffect);
        }
        if (heartBeatVessel != null) heartBeatVessel.alpha = 0;

        if (alphaCanvas != null)
        {
            alphaCanvas.alpha = 0;
            alphaCanvas.gameObject.SetActive(bl_DamageScreenSettings.Instance.useFastScreenEffect);
        }

        if (heartbeatAudioSource == null)
        {
            heartbeatAudioSource = gameObject.AddComponent<AudioSource>();
            heartbeatAudioSource.spatialBlend = 0f;
        }

        heartbeatAudioSource.playOnAwake = false;
        heartbeatAudioSource.loop = false;

        // Prefer explicit clip assignment, but preserve an already configured AudioSource clip.
        if (heartbeatSound != null)
        {
            heartbeatAudioSource.clip = heartbeatSound;
        }
        else if (heartbeatAudioSource.clip != null)
        {
            heartbeatSound = heartbeatAudioSource.clip;
        }
    }

    /// <summary>
    /// Updates near-death visual/audio intensity from local health.
    /// Call this from your health system whenever local player health changes.
    /// </summary>
    /// <param name="currentHealth">Current local player health after the change.</param>
    /// <param name="maxHealth">Maximum local player health.</param>
    public void OnLocalHealthChanged(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0)
        {
            targetHealthRatio = 0f;
            return;
        }

        targetHealthRatio = Mathf.InverseLerp(bl_DamageScreenSettings.Instance.healthThreshold, bl_DamageScreenSettings.Instance.criticalHealth, currentHealth);

        if (targetHealthRatio > 0f)
        {
            if (!isPlaying)
            {
                if (heartbeatAudioSource == null || heartbeatAudioSource.clip == null)
                {
                    if (!warnedMissingHeartbeatClip)
                    {
                        Debug.LogWarning("NearDeathEffect heartbeat audio cannot play because no AudioClip is assigned. Assign 'heartbeatSound' or set a clip in 'heartbeatAudioSource'.", this);
                        warnedMissingHeartbeatClip = true;
                    }
                    return;
                }

                warnedMissingHeartbeatClip = false;
                heartbeatAudioSource.loop = true;
                heartbeatAudioSource.volume = 0;
                heartbeatTimer = 0f;
                heartbeatAudioSource.Play();
                isPlaying = true;
            }
        }
    }

    private void Update()
    {
        if (isPlaying || volumeWeight > 0f || targetHealthRatio > 0f)
        {
            volumeWeight = Mathf.MoveTowards(volumeWeight, targetHealthRatio, Time.deltaTime);

            if (isPlaying)
            {
                heartbeatAudioSource.volume = Mathf.Lerp(bl_DamageScreenSettings.Instance.heartbeatMinVolume, bl_DamageScreenSettings.Instance.heartbeatMaxVolume, volumeWeight);
            }

            float pulse = 0f;
            if (isPlaying)
            {
                heartbeatTimer += Time.deltaTime * bl_DamageScreenSettings.Instance.pulseSpeed;
                pulse = (Mathf.Sin(heartbeatTimer * Mathf.PI * 2f) + 1f) * 0.5f; // 0 to 1 oscillation
            }

            if (bl_DamageScreenSettings.Instance.useFastScreenEffect)
            {
                // Fast mode skips post-processing and drives a lightweight canvas alpha instead.
                if (alphaCanvas != null) alphaCanvas.alpha = volumeWeight;
            }
            else
            {
#if UNITY_POST_PROCESSING_STACK_V2
                if (postProcessVolume != null)
                {
                    postProcessVolume.weight = volumeWeight;
                    if (chromaticAberration != null)
                    {
                        // Heartbeat pulse effect (sinusoidal pulsing effect)
                        float chromaValue = Mathf.Lerp(0f, bl_DamageScreenSettings.Instance.maxChromaticAberration, volumeWeight * pulse);
                        chromaticAberration.intensity.value = chromaValue;
                    }
                }
#endif
                if (volume != null)
                {
                    float weight = volumeWeight;

#if UNITY_POST_PROCESSING_STACK_V2
                    bool shouldPulseWeight = postProcessVolume == null;
#else
                    bool shouldPulseWeight = true;
#endif

                    if (shouldPulseWeight && isPlaying)
                    {
                        float pulseAmount = Mathf.Clamp01(bl_DamageScreenSettings.Instance.volumePulseAmount);
                        float maxPulsedWeight = weight + pulseAmount;

                        if (maxPulsedWeight >= 1f)
                        {
                            float minPulseWeight = Mathf.Clamp01(1f - pulseAmount);
                            weight = Mathf.Lerp(minPulseWeight, 1f, pulse);
                        }
                        else
                        {
                            weight += pulse * pulseAmount;
                        }

                        weight = Mathf.Clamp01(weight);
                    }

                    volume.weight = weight;
                    if (heartBeatVessel != null)
                    {
                        float vesselAlpha = weight;
                        if (isPlaying)
                        {
                            float minOscillation = Mathf.Clamp01(heartBeatVesselMinOscillation);
                            float maxAlpha = Mathf.Clamp01(weight);
                            float minAlpha = maxAlpha * minOscillation;
                            vesselAlpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
                        }
                        heartBeatVessel.alpha = Mathf.Clamp01(vesselAlpha);
                    }
                }
            }

            if (isPlaying && targetHealthRatio <= 0f && volumeWeight <= 0.001f)
            {
                heartbeatAudioSource.Stop();
                heartbeatAudioSource.volume = 0f;
                isPlaying = false;
            }
        }
    }

    /// <summary>
    /// Fully resets near-death visuals and heartbeat audio.
    /// Call this on respawn, scene reset, or when player state is reinitialized.
    /// </summary>
    public void ResetEffect()
    {
        targetHealthRatio = 0f;
        volumeWeight = 0f;
        heartbeatTimer = 0f;
        if (heartBeatVessel != null) heartBeatVessel.alpha = 0;
        if (heartbeatAudioSource != null)
        {
            heartbeatAudioSource.Stop();
            heartbeatAudioSource.volume = 0f;
        }
    }
}