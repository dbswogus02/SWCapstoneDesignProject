using UnityEngine;

public class bl_DamageScreen : MonoBehaviour
{
    public bl_BloodDamageScreen bloodDamageScreen;
    public bl_NearDeathEffect nearDeathEffect;

    /// <summary>
    /// Call this method to update the damage screen and near-death effect based on the player's current health.
    /// </summary>
    /// <param name="currentHealth">The player's current health value.</param>
    /// <param name="maxHealth">The player's maximum health value.</param>
    public static void UpdateHealth(int currentHealth, int maxHealth)
    {
        var instance = Instance;
        if (instance == null) return;

        instance.bloodDamageScreen.OnLocalHealthChanged(currentHealth, maxHealth);
        if (bl_DamageScreenSettings.Instance.enableNearDeathEffect)
        {
            instance.nearDeathEffect.OnLocalHealthChanged(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Call this method to instantly reset the damage screen and near-death effect to their default states.
    /// </summary>
    public static void ResetScreen()
    {
        var instance = Instance;
        if (instance == null) return;

        instance.bloodDamageScreen.ResetScreen();
        if (bl_DamageScreenSettings.Instance.enableNearDeathEffect) instance.nearDeathEffect.ResetEffect();
    }

    private static bl_DamageScreen _instance;
    public static bl_DamageScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<bl_DamageScreen>();
            }
            return _instance;
        }
    }
}