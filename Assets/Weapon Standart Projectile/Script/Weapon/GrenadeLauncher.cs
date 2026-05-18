using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Biostart.CameraEffects;
using Biostart.Enemy;

namespace Biostart.Weapons
{
    public class GrenadeLauncher : MonoBehaviour
    {
        [Header("Grenade Launcher Properties")]
        public float launchForce = 30f;          // Force applied to the grenade when launched
        public float fireRate = 1f;             // Rate of fire
        private float nextFireTime;             // Time when the next grenade can be fired

        [Header("Ammunition")]
        public int magazineSize = 6;            // Max grenades in the launcher
        public int totalAmmo = 18;              // Total reserve grenades
        private int currentAmmo;                // Current grenades in the launcher
        private bool isReloading = false;       // Reload status
        private float reloadEndTime;            // Time when reload finishes
        private bool wasReloadingInterrupted = false; // Flag for interrupted reload
        public float reloadTime = 3f;           // Time to reload the launcher

        [Header("UI")]
        public Text magazineAmmoText;           // UI for magazine ammo
        public Text totalAmmoText;              // UI for total ammo
        public Text reloadText;                 // UI for reload notification

        [Header("AUDIO")]
        public AudioClip shotFX;                // Sound for shooting
        public AudioClip reloadFX;              // Sound for reloading
        private AudioSource audioSource;

        [Header("Grenade Settings")]
        public GameObject grenadePrefab;        // Prefab for the grenade
        public Transform firePoint;             // Point from where grenades are fired
        public GameObject MuzzlePrefab; // Hit effect prefab

        [Header("Camera Shake")]
        public CameraShake cameraShake;         // Camera shake component
        public bool enableCameraShake = true;   // Enable or disable camera shake
        public float shakeDuration = 0.2f;      // Duration of camera shake
        public float shakeMagnitude = 0.3f;     // Magnitude of camera shake

        void Start()
        {
            currentAmmo = magazineSize;
            UpdateAmmoUI();
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            CheckReloadTextVisibility(); // Check reload text visibility

            if (isReloading)
            {
                if (Time.time >= reloadEndTime)
                {
                    FinishReload();
                }
                else
                {
                    return; // Exit if reload is not yet finished
                }
            }

            if (currentAmmo <= 0 && totalAmmo > 0)
            {
                StartReload();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
            {
                StartReload();
            }

            HandleShooting();
        }

        void OnEnable()
        {
            UpdateAmmoUI(); // Update ammo UI every frame

            // Restart reload if it was interrupted
            if (wasReloadingInterrupted)
            {
                StartReload();
                wasReloadingInterrupted = false; // Reset flag
            }
        }

        void OnDisable()
        {
            if (isReloading && Time.time < reloadEndTime)
            {
                wasReloadingInterrupted = true;
                isReloading = false;
            }

            if (reloadText != null)
            {
                reloadText.enabled = false;
            }
        }

        private void StartReload()
        {
            isReloading = true;
            reloadEndTime = Time.time + reloadTime;

            if (reloadText != null)
            {
                reloadText.enabled = true;
            }

            if (reloadFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(reloadFX);
            }
        }

        private void FinishReload()
        {
            int ammoToReload = Mathf.Min(totalAmmo, magazineSize - currentAmmo);
            totalAmmo -= ammoToReload;
            currentAmmo += ammoToReload;

            isReloading = false;

            if (reloadText != null)
            {
                reloadText.enabled = false;
            }

            UpdateAmmoUI();
        }

        private void CheckReloadTextVisibility()
        {
            if (reloadText != null)
            {
                reloadText.enabled = isReloading;
            }
        }

        private void HandleShooting()
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0 && !isReloading)
            {
                nextFireTime = Time.time + 1f / fireRate;
                FireGrenade();
            }
        }

       void FireGrenade()
{
    if (grenadePrefab != null && firePoint != null)
    {
        // Создание гранаты
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * launchForce;
        }

                // Эффект при выстреле
        if (MuzzlePrefab != null)
                {
            // Позиция и вращение для эффекта
            Vector3 hitPosition = firePoint.position; // Используем firePoint как источник выстрела
            Quaternion hitRotation = firePoint.rotation; // Ориентируем эффект в сторону выстрела

            // Создание эффекта
            GameObject hitEffect = Instantiate(MuzzlePrefab, hitPosition, hitRotation);
            Destroy(hitEffect, 2f); // Уничтожение эффекта после времени (например, 2 сек)
        }

        // Звук выстрела
        if (shotFX != null)
        {
            AudioSource.PlayClipAtPoint(shotFX, firePoint.position);
        }

        // Эффект тряски камеры
        if (enableCameraShake && cameraShake != null)
        {
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
        }

        currentAmmo--;
        UpdateAmmoUI();
    }
    else
    {
        Debug.LogError("GrenadePrefab or FirePoint is missing.");
    }
}


        private void UpdateAmmoUI()
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
    }
}
