using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Biostart.CameraEffects;

namespace Biostart.Weapons
{
    public class GrenadeSetting : MonoBehaviour
    {
        [Header("Grenade Properties")]
        public float speed;
        public float fireRate;
        private float nextFireTime;
        public float forceValue = 10f;
        public float maxThrowDistance = 20f;
        public float minThrowAngle = 15f;
        public float maxThrowAngle = 45f;

        [Header("Ammunition")]
        public int magazineSize = 3000;
        public int totalAmmo = 900;
        private int currentAmmo;
        private bool isReloading = false;
        public float reloadTime = 2f;
        private float reloadEndTime;

        [Header("UI")]
        public Text magazineAmmoText;
        public Text totalAmmoText;
        public Text reloadText;

        [Header("AUDIO")]
        public AudioClip shotFX;
        public AudioClip reloadFX;

        public GameObject grenadePrefab;
        public List<Transform> firePoints;

        private GameObject activeGrenade;

        [Header("Camera Shake")]
        public CameraShake cameraShake;
        public bool enableCameraShake = true;
        public float shakeDuration = 0.2f;
        public float shakeMagnitude = 0.3f;

        private void Start()
        {
            currentAmmo = magazineSize;
            UpdateAmmoUI();
            CreateActiveGrenade();
        }

        private void Update()
        {
            CheckReloadTextVisibility();

            if (isReloading)
            {
                if (Time.time >= reloadEndTime)
                {
                    FinishReload();
                }
                else
                {
                    return;
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

            HandleThrowing();
        }

        private void HandleThrowing()
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0 && !isReloading)
            {
                nextFireTime = Time.time + 1f / fireRate;
                ThrowGrenade();
            }
        }

        private void CreateActiveGrenade()
        {
            if (grenadePrefab != null && firePoints.Count > 0)
            {
                Transform firePoint = firePoints[0];
                activeGrenade = Instantiate(grenadePrefab, firePoint.position, firePoint.rotation, firePoint);

                Rigidbody activeRb = activeGrenade.GetComponent<Rigidbody>();
                if (activeRb != null)
                {
                    activeRb.isKinematic = true;
                }
                else
                {
                    Debug.LogError("Rigidbody component is missing on the grenade prefab.");
                }
            }
            else
            {
                Debug.LogError("No available fire points for the grenade.");
            }
        }

        private void ThrowGrenade()
        {
            if (activeGrenade != null)
            {
                Rigidbody activeRb = activeGrenade.GetComponent<Rigidbody>();
                if (activeRb != null)
                {
                    activeGrenade.GetComponent<Grenade>().enabled = true;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    Vector3 targetPoint;

                    if (Physics.Raycast(ray, out hit))
                    {
                        targetPoint = hit.point;
                    }
                    else
                    {
                        targetPoint = ray.GetPoint(maxThrowDistance);
                    }

                    float distanceToTarget = Vector3.Distance(firePoints[0].position, targetPoint);
                    float angle = Mathf.Lerp(maxThrowAngle, minThrowAngle, distanceToTarget / maxThrowDistance);
                    float angleInRadians = angle * Mathf.Deg2Rad;

                    Vector3 throwDirection = firePoints[0].forward;
                    throwDirection.y += Mathf.Tan(angleInRadians);

                    activeRb.isKinematic = false;
                    activeRb.linearVelocity = throwDirection.normalized * speed;

                    Vector3 randomTorque = new Vector3(
                        Random.Range(-500f, 500f),
                        Random.Range(-500f, 500f),
                        Random.Range(-500f, 500f)
                    );
                    activeRb.AddTorque(randomTorque);

                    activeGrenade.transform.SetParent(null);
                    activeGrenade = null;
                    currentAmmo--;

                    if (enableCameraShake && cameraShake != null)
                    {
                        StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));
                    }

                    if (shotFX != null)
                    {
                        AudioSource.PlayClipAtPoint(shotFX, firePoints[0].position);
                    }
                }
            }

            UpdateAmmoUI();
            if (currentAmmo > 0)
            {
                CreateActiveGrenade();
            }
        }

        private void StartReload()
        {
            if (!isReloading && currentAmmo < magazineSize && totalAmmo > 0)
            {
                isReloading = true;
                reloadEndTime = Time.time + reloadTime;

                if (reloadText != null)
                {
                    reloadText.enabled = true;
                }

                if (reloadFX != null)
                {
                    AudioSource.PlayClipAtPoint(reloadFX, transform.position);
                }
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
            CreateActiveGrenade();
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

        public void CheckReloadTextVisibility()
        {
            if (reloadText != null)
            {
                reloadText.enabled = isReloading;
            }
        }
    }
}
