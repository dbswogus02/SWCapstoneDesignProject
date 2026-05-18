using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Biostart.CameraEffects;
using Biostart.Enemy;

namespace Biostart.Weapons
{
    public class Flamethrower : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public float damage = 10f;               // Damage dealt per second
        public float range = 5f;                 // Flamethrower range
        public float fireRate = 0.1f;            // Rate at which the flamethrower deals damage
        public float coneAngle = 45f;            // Cone angle

        [Header("Ammo")]
        public int magazineSize = 3000;          // Maximum ammo in the magazine
        public int totalAmmo = 900;              // Total ammo for the weapon
        private int currentAmmo;                 // Current ammo in the magazine

        [Header("Reload Settings")]
        public float reloadTime = 2f;            // Reload duration
        private float reloadEndTime;

        [Header("UI")]
        public Text magazineAmmoText;            // UI element for magazine ammo display
        public Text totalAmmoText;               // UI element for total ammo display
        public Text reloadText;                  // UI element for reload text display

        [Header("FX")]
        public AudioClip shotFX;                 // Shooting sound effect
        public AudioClip reloadFX;               // Reload sound effect
        private AudioSource audioSource;         // Audio source for playing sounds
        public GameObject FirePrefab;            // Fire prefab emitted by the weapon
        public Transform firePoint;              // Point from which the fire appears

        [Header("Camera Shake")]
        public CameraShake cameraShake;          // Reference to the camera shake component
        public bool enableCameraShake = true;    // Toggle for enabling/disabling camera shake
        public float shakeDuration = 0.1f;       // Camera shake duration
        public float shakeMagnitude = 0.2f;      // Camera shake magnitude

        private bool isFiring = false;           // Tracks whether the weapon is firing
        private float fireCooldown = 0f;         // Time until the next damage tick

        private bool isReloading = false;        // Tracks whether the weapon is reloading
        private bool wasReloadingInterrupted = false; // Flag to check if reload was interrupted

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            currentAmmo = magazineSize;
            UpdateAmmoUI();

            // Check if reloading is active
            if (!isReloading && reloadText != null)
            {
                reloadText.enabled = false; // Disable reload text if not reloading
            }
        }

        void Update()
        {
            CheckReloadTextVisibility(); // Update reload text visibility

            // Start firing
            if (Input.GetButtonDown("Fire1") && currentAmmo > 0 && !isReloading)
            {
                StartFiring();
            }

            // Stop firing
            if (Input.GetButtonUp("Fire1"))
            {
                StopFiring();
            }

            // Check reload status
            if (isReloading)
            {
                if (Time.time >= reloadEndTime)
                {
                    StartCoroutine(Reload()); // Finish reload
                }
                return; // Skip the rest of Update during reload
            }

            // Apply damage and spawn fire particles at intervals
            if (isFiring && fireCooldown <= 0f)
            {
                ApplyDamage();
                CreateFireEffect();
                fireCooldown = fireRate;

                // Trigger camera shake
                if (enableCameraShake && cameraShake != null)
                {
                    cameraShake.ShakeCamera(shakeDuration, shakeMagnitude);
                }
            }

            // Start reload if ammo is depleted
            if (currentAmmo <= 0 && totalAmmo > 0)
            {
                StartReload();
            }

            // Update fire cooldown
            if (fireCooldown > 0f)
            {
                fireCooldown -= Time.deltaTime;
            }
        }

        void OnEnable()
        {
            UpdateAmmoUI(); // Refresh UI

            if (wasReloadingInterrupted)
            {
                StartReload();
                wasReloadingInterrupted = false; // Reset the flag
            }
        }

        void OnDisable()
        {
            if (isReloading && Time.time < reloadEndTime)
            {
                wasReloadingInterrupted = true;
                isReloading = false; // Stop ongoing reload
            }

            if (reloadText != null)
            {
                reloadText.enabled = false; // Hide reload text on disable
            }
        }

        void StartFiring()
        {
            isFiring = true;
            PlayShotSound();
        }

        void StopFiring()
        {
            isFiring = false;
            StopShotSound();
        }

        void ApplyDamage()
        {
            if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
                StopFiring();
                return;
            }

            // Apply damage to all objects in the cone
            Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, range);

            foreach (Collider hitCollider in hitColliders)
            {
                Vector3 directionToTarget = (hitCollider.transform.position - firePoint.position).normalized;

                if (Vector3.Angle(firePoint.forward, directionToTarget) <= coneAngle / 2)
                {
                    Health health = hitCollider.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(damage * fireRate); // Apply damage based on fire rate
                    }
                }
            }

            currentAmmo--;
            UpdateAmmoUI();
        }

        void CreateFireEffect()
        {
            if (FirePrefab != null && firePoint != null)
            {
                GameObject fireEffect = Instantiate(FirePrefab, firePoint.position, firePoint.rotation, firePoint);
                fireEffect.transform.localPosition = Vector3.zero;
            }
        }

        private void StartReload()
        {
            if (isReloading) return;

            isReloading = true;
            reloadEndTime = Time.time + reloadTime;

            if (reloadText != null)
            {
                reloadText.enabled = true;
            }

            PlayReloadSound();
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSeconds(reloadTime);

            int ammoToReload = Mathf.Min(totalAmmo, magazineSize - currentAmmo);
            currentAmmo += ammoToReload;
            totalAmmo -= ammoToReload;

            UpdateAmmoUI();
            FinishReload();
        }

        private void FinishReload()
        {
            isReloading = false;

            if (reloadText != null)
            {
                reloadText.enabled = false;
            }

            UpdateAmmoUI();
        }

        void PlayShotSound()
        {
            if (shotFX != null && audioSource != null)
            {
                audioSource.clip = shotFX;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        void PlayReloadSound()
        {
            if (reloadFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(reloadFX);
            }
        }

        void StopShotSound()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        void UpdateAmmoUI()
        {
            if (magazineAmmoText != null)
            {
                magazineAmmoText.text = currentAmmo.ToString();
            }

            if (totalAmmoText != null)
            {
                totalAmmoText.text = totalAmmo.ToString();
            }
        }

        void ToggleCameraShake()
        {
            enableCameraShake = !enableCameraShake;
            Debug.Log("Camera shake: " + (enableCameraShake ? "Enabled" : "Disabled"));
        }

        public void CheckReloadTextVisibility()
        {
            if (reloadText != null)
            {
                reloadText.enabled = isReloading;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, range);

            Vector3 coneDirection = firePoint.forward * range;
            Gizmos.DrawLine(firePoint.position, firePoint.position + coneDirection);
            Gizmos.DrawLine(firePoint.position, firePoint.position + Quaternion.Euler(0, coneAngle / 2, 0) * coneDirection);
            Gizmos.DrawLine(firePoint.position, firePoint.position + Quaternion.Euler(0, -coneAngle / 2, 0) * coneDirection);
        }
    }
}
