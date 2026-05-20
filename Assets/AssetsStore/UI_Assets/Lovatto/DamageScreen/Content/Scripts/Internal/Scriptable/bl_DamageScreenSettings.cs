using UnityEngine;

public enum BloodScreenFadeMode
{
    PersistentByHealth = 0,
    FadeOutAfterDelay = 1,
}

public enum ScreenAlphaDisplayMode
{
    GradualByHealth = 0,
    OnePerHit = 1,
}

[CreateAssetMenu(fileName = "DamageScreenSettings", menuName = "Lovatto/DamageScreen/Settings")]
public class bl_DamageScreenSettings : ScriptableObject
{
    [Header("Blood Screen Settings")]
    [Tooltip("Normalized health percentage at which the blood screen starts to appear. For example, 0.75 means the blood screen will start appearing when health drops below 75%.")]
    public float bloodHealthThreshold = 0.75f;
    [Tooltip("Controls if blood visuals stay synced to health or fade out after a short delay.")]
    public BloodScreenFadeMode bloodScreenFadeMode = BloodScreenFadeMode.PersistentByHealth;
    [Tooltip("Delay before blood visuals start fading out when using FadeOutAfterDelay mode.")]
    public float bloodFadeOutDelay = 0.2f;
    [Tooltip("Duration used to fade blood visuals out to zero when using FadeOutAfterDelay mode.")]
    public float bloodFadeOutDuration = 0.5f;

    [Header("Screen Alpha Display")]
    [Tooltip("GradualByHealth blends all overlays by health amount. OnePerHit shows one overlay per hit (fade in > hold > fade out).")]
    public ScreenAlphaDisplayMode screenAlphaDisplayMode = ScreenAlphaDisplayMode.GradualByHealth;
    [Tooltip("Peak alpha used by each hit pulse when Screen Alpha Display is set to OnePerHit.")]
    [Range(0f, 1f)] public float perHitScreenAlpha = 1f;
    [Tooltip("Fade-in time (seconds) for each OnePerHit overlay pulse.")]
    public float perHitFadeInDuration = 0.06f;
    [Tooltip("Time (seconds) to keep the OnePerHit overlay fully visible before fading out.")]
    public float perHitHoldDuration = 0.12f;
    [Tooltip("Fade-out time (seconds) for each OnePerHit overlay pulse.")]
    public float perHitFadeOutDuration = 0.35f;

    [Tooltip("If true, the blood screen will flash briefly on taking damage, regardless of the health percentage.")]
    public bool flashOnDamage = true;
    [Tooltip("How long the flash-on-damage pulse lasts in seconds.")]
    public float flashDuration = 0.2f;
    [Tooltip("Extra alpha added during the flash-on-damage pulse.")]
    public float flashIntensity = 1f;

    [Header("Near Death Settings")]
    public bool enableNearDeathEffect = true;
    [Tooltip("Health threshold below which the heartbeat starts playing.")]
    public float healthThreshold = 30f;
    [Tooltip("Critical health value used as the maximum near-death intensity point.")]
    public float criticalHealth = 10f;
    [Tooltip("Use a lightweight canvas overlay instead of post-processing volume effects.")]
    public bool useFastScreenEffect = false;
    [Tooltip("Max Chromatic Aberration at critical health.")]
    [Range(0f, 1f)] public float maxChromaticAberration = 0.7f;

    [Tooltip("Speed of heartbeat pulse effect.")]
    public float pulseSpeed = 1.5f;
    [Tooltip("Extra pulse amount added to Volume weight during heartbeat.")]
    [Range(0f, 1f)] public float volumePulseAmount = 0.1f;

    [Header("Audio Settings")]
    [Tooltip("Maximum volume of the heartbeat sound at lowest health.")]
    [Range(0f, 1f)] public float heartbeatMaxVolume = 1f;

    [Tooltip("Minimum volume when the heartbeat starts.")]
    [Range(0f, 1f)] public float heartbeatMinVolume = 0.2f;

    // Cached singleton loaded from a Resources asset named "DamageScreenSettings".
    public static bl_DamageScreenSettings _instance;

    /// <summary>
    /// Gets the active damage screen settings instance.
    /// Place a <c>DamageScreenSettings</c> asset inside a <c>Resources</c> folder so it can be loaded automatically.
    /// </summary>
    public static bl_DamageScreenSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<bl_DamageScreenSettings>("DamageScreenSettings") as bl_DamageScreenSettings;
            }
            return _instance;
        }
    }
}