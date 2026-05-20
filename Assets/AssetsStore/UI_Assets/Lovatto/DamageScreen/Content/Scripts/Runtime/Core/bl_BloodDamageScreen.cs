using UnityEngine;

public class bl_BloodDamageScreen : MonoBehaviour
{
    [Tooltip("Ordered blood overlays used by the screen effect. In OnePerHit mode the script cycles one overlay per hit.")]
    [SerializeField] private CanvasGroup[] screenAlphas = null;
    [Tooltip("Main red overlay alpha controller that represents the global blood intensity.")]
    [SerializeField] private CanvasGroup redOverlayAlpha = null;
    [Tooltip("Optional animator that plays an extra flash animation when damage is received.")]
    [SerializeField] private Animator flashScreenAnimator = null;

    private float normalizedPercentage;
    private float[] targetAlphas;
    private bool isVisible = false;
    private bool hasHealthValue = false;
    private int lastHealth;
    private bool isFlashing;
    private Coroutine flashCoroutine;
    private float overlayTargetAlpha;
    private float autoFadeDelayTimer;
    private bool autoFadeActive;
    private int nextPerHitScreenIndex;
    private int activePerHitAnimations;
    private Coroutine[] perHitScreenCoroutines;

    private void Awake()
    {
        for (int i = 0; i < screenAlphas.Length; i++)
        {
            screenAlphas[i].alpha = 0;
        }
        redOverlayAlpha.alpha = 0;
        overlayTargetAlpha = 0f;
        targetAlphas = new float[screenAlphas.Length];
        perHitScreenCoroutines = new Coroutine[screenAlphas.Length];
    }

    /// <summary>
    /// Updates blood screen state when local health changes.
    /// Call this from your health system every time local player health is modified.
    /// </summary>
    /// <param name="currentHealth">Current local player health after the change.</param>
    /// <param name="maxHealth">Maximum local player health.</param>
    public void OnLocalHealthChanged(int currentHealth, int maxHealth)
    {
        bool tookDamage = hasHealthValue && currentHealth < lastHealth;

        lastHealth = currentHealth;
        hasHealthValue = true;

        float threshold = bl_DamageScreenSettings.Instance.bloodHealthThreshold;
        float percentage = Mathf.Clamp01((1 - ((float)currentHealth / (float)maxHealth) - (1 - threshold)) / threshold);

        // Convert current health into a normalized blood intensity in the 0..1 range.
        normalizedPercentage = Mathf.Clamp01(percentage / bl_DamageScreenSettings.Instance.bloodHealthThreshold);
        overlayTargetAlpha = normalizedPercentage;

        if (bl_DamageScreenSettings.Instance.screenAlphaDisplayMode == ScreenAlphaDisplayMode.GradualByHealth)
        {
            SetGradientTargetsFromNormalized(normalizedPercentage);
        }
        else
        {
            ClearGradientTargets();
            if (tookDamage)
            {
                TriggerPerHitScreen();
            }
        }

        // Auto-fade mode starts a timer after each health update and fades out once delay has elapsed.
        if (bl_DamageScreenSettings.Instance.bloodScreenFadeMode == BloodScreenFadeMode.FadeOutAfterDelay)
        {
            autoFadeDelayTimer = Mathf.Max(0f, bl_DamageScreenSettings.Instance.bloodFadeOutDelay);
            autoFadeActive = true;
        }
        else
        {
            autoFadeActive = false;
        }

        if (bl_DamageScreenSettings.Instance.flashOnDamage && tookDamage)
        {
            TriggerDamageFlash();
        }

        isVisible = true;
    }

    private void Update()
    {
        UpdateAutoFadeTargets();
        UpdateAlphas();
    }

    private void UpdateAutoFadeTargets()
    {
        if (bl_DamageScreenSettings.Instance.bloodScreenFadeMode != BloodScreenFadeMode.FadeOutAfterDelay || !autoFadeActive)
        {
            return;
        }

        if (autoFadeDelayTimer > 0f)
        {
            autoFadeDelayTimer -= Time.deltaTime;
            return;
        }

        float fadeDuration = Mathf.Max(0.01f, bl_DamageScreenSettings.Instance.bloodFadeOutDuration);
        float fadeStep = Time.deltaTime / fadeDuration;

        overlayTargetAlpha = Mathf.MoveTowards(overlayTargetAlpha, 0f, fadeStep);

        if (bl_DamageScreenSettings.Instance.screenAlphaDisplayMode == ScreenAlphaDisplayMode.GradualByHealth)
        {
            for (int i = 0; i < targetAlphas.Length; i++)
            {
                targetAlphas[i] = Mathf.MoveTowards(targetAlphas[i], 0f, fadeStep);
            }
        }

        if (overlayTargetAlpha <= 0.001f && AreGradientTargetsZero())
        {
            autoFadeActive = false;
        }
    }

    private void UpdateAlphas()
    {
        if (bl_DamageScreenSettings.Instance.screenAlphaDisplayMode == ScreenAlphaDisplayMode.GradualByHealth)
        {
            for (int i = 0; i < screenAlphas.Length; i++)
            {
                if (isFlashing && i == 0)
                {
                    continue;
                }

                screenAlphas[i].alpha = Mathf.Lerp(screenAlphas[i].alpha, targetAlphas[i], Time.deltaTime * 8);
            }
        }

        if (!isFlashing)
        {
            redOverlayAlpha.alpha = Mathf.MoveTowards(redOverlayAlpha.alpha, overlayTargetAlpha, Time.deltaTime * 40);
        }

        if (isVisible && redOverlayAlpha.alpha <= 0.001f && overlayTargetAlpha <= 0.001f && !HasVisibleScreen())
        {
            isVisible = false;
            redOverlayAlpha.alpha = 0;
        }
    }

    private void SetGradientTargetsFromNormalized(float normalized)
    {
        int totalGroups = screenAlphas.Length;
        if (totalGroups <= 0)
        {
            return;
        }

        float step = 1f / totalGroups;
        for (int i = 0; i < totalGroups; i++)
        {
            float fadeThreshold = step * (i + 1);
            float alphaValue = Mathf.Clamp01(normalized / fadeThreshold);
            targetAlphas[i] = alphaValue;
        }
    }

    private void ClearGradientTargets()
    {
        for (int i = 0; i < targetAlphas.Length; i++)
        {
            targetAlphas[i] = 0f;
        }
    }

    private bool AreGradientTargetsZero()
    {
        for (int i = 0; i < targetAlphas.Length; i++)
        {
            if (targetAlphas[i] > 0.001f)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasVisibleScreen()
    {
        for (int i = 0; i < screenAlphas.Length; i++)
        {
            if (screenAlphas[i].alpha > 0.001f)
            {
                return true;
            }
        }

        return activePerHitAnimations > 0;
    }

    private void TriggerPerHitScreen()
    {
        if (screenAlphas == null || screenAlphas.Length == 0)
        {
            return;
        }

        // Cycle through the overlays to avoid every hit reusing the exact same image region.
        int screenIndex = nextPerHitScreenIndex % screenAlphas.Length;
        nextPerHitScreenIndex = (nextPerHitScreenIndex + 1) % screenAlphas.Length;

        if (perHitScreenCoroutines[screenIndex] != null)
        {
            StopCoroutine(perHitScreenCoroutines[screenIndex]);
            perHitScreenCoroutines[screenIndex] = null;
            activePerHitAnimations = Mathf.Max(0, activePerHitAnimations - 1);
        }

        perHitScreenCoroutines[screenIndex] = StartCoroutine(PerHitScreenRoutine(screenIndex));
    }

    private System.Collections.IEnumerator PerHitScreenRoutine(int index)
    {
        activePerHitAnimations++;

        float targetAlpha = Mathf.Clamp01(bl_DamageScreenSettings.Instance.perHitScreenAlpha);
        float fadeInDuration = Mathf.Max(0.01f, bl_DamageScreenSettings.Instance.perHitFadeInDuration);
        float holdDuration = Mathf.Max(0f, bl_DamageScreenSettings.Instance.perHitHoldDuration);
        float fadeOutDuration = Mathf.Max(0.01f, bl_DamageScreenSettings.Instance.perHitFadeOutDuration);

        float startAlpha = screenAlphas[index].alpha;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            screenAlphas[index].alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        screenAlphas[index].alpha = targetAlpha;

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        elapsed = 0f;
        float fromAlpha = screenAlphas[index].alpha;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            screenAlphas[index].alpha = Mathf.Lerp(fromAlpha, 0f, t);
            yield return null;
        }

        screenAlphas[index].alpha = 0f;
        activePerHitAnimations = Mathf.Max(0, activePerHitAnimations - 1);
        perHitScreenCoroutines[index] = null;
    }

    private void TriggerDamageFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(DamageFlashRoutine());
        if (flashScreenAnimator != null)
        {
            flashScreenAnimator.Play("Flash", 0, 0);
        }
    }

    private System.Collections.IEnumerator DamageFlashRoutine()
    {
        isFlashing = true;

        float duration = Mathf.Max(0.01f, bl_DamageScreenSettings.Instance.flashDuration);

        float targetOverlayAlpha = normalizedPercentage;
        float targetFirstScreenAlpha = targetAlphas.Length > 0 ? targetAlphas[0] : 0f;

        bool hasFirstScreen = screenAlphas.Length > 0;
        float overlayFlashAlpha = Mathf.Clamp01(redOverlayAlpha.alpha + bl_DamageScreenSettings.Instance.flashIntensity);
        float firstScreenFlashAlpha = hasFirstScreen ? Mathf.Clamp01(screenAlphas[0].alpha + bl_DamageScreenSettings.Instance.flashIntensity) : 0f;

        redOverlayAlpha.alpha = overlayFlashAlpha;

        if (hasFirstScreen)
        {
            screenAlphas[0].alpha = firstScreenFlashAlpha;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            redOverlayAlpha.alpha = Mathf.Lerp(overlayFlashAlpha, targetOverlayAlpha, t);
            if (hasFirstScreen)
            {
                screenAlphas[0].alpha = Mathf.Lerp(firstScreenFlashAlpha, targetFirstScreenAlpha, t);
            }

            yield return null;
        }

        redOverlayAlpha.alpha = targetOverlayAlpha;
        if (hasFirstScreen)
        {
            screenAlphas[0].alpha = targetFirstScreenAlpha;
        }

        isFlashing = false;
        flashCoroutine = null;
    }

    /// <summary>
    /// Clears all blood overlays and stops any running flash/per-hit animation.
    /// Call this when respawning, restarting level, or resetting UI state.
    /// </summary>
    public void ResetScreen()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (perHitScreenCoroutines != null)
        {
            for (int i = 0; i < perHitScreenCoroutines.Length; i++)
            {
                if (perHitScreenCoroutines[i] != null)
                {
                    StopCoroutine(perHitScreenCoroutines[i]);
                    perHitScreenCoroutines[i] = null;
                }
            }
        }

        isFlashing = false;
        activePerHitAnimations = 0;
        overlayTargetAlpha = 0f;
        autoFadeDelayTimer = 0f;
        autoFadeActive = false;

        foreach (CanvasGroup screenAlpha in screenAlphas)
        {
            screenAlpha.alpha = 0;
        }
        redOverlayAlpha.alpha = 0;
        isVisible = false;
    }
}