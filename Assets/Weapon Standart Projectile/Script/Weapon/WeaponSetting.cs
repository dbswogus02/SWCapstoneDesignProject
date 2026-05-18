using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Biostart.CameraEffects; 
using Biostart.Enemy; 

namespace Biostart.Weapons
{
    public class WeaponSetting : MonoBehaviour
    {
        [Header("Projectile Properties")]
        public float speed; // Bullet speed
        [Range(0, 100)]
        [Tooltip("Shooting accuracy (0% to 100%)")]
        public float accuracy; // Shooting accuracy
        public float bulletTime = 1f; // Spell duration
        public float fireRate; // Fire rate
        private float nextFireTime; // Time of next shot
        public float forceValue = 10f; // Force with which the bullet will push an object
        public int damage = 10; // Damage dealt by the bullet

        [Header("Ammunition")]
        public int magazineSize = 30; // Maximum bullets in the magazine
        public int totalAmmo = 90; // Total ammo for the weapon
        private int currentAmmo; // Current bullets in the magazine
        private bool isReloading = false; // Check if reloading
        private bool wasReloadingInterrupted = false; // Flag for checking reloading interruption
        public float reloadTime = 2f; // Reload time
        private float reloadEndTime; // Time when reload should finish

        [Header("UI")]
        public Text magazineAmmoText; // UI element for displaying ammo in the magazine
        public Text totalAmmoText; // UI element for displaying total ammo
        public Text reloadText; // UI element for showing reload progress   
        
        [Header("Prefabs")]
        public GameObject bulletPrefab; // Bullet prefab
        public GameObject flyingBulletPrefab; // Flying bullet prefab
        public List<Transform> firePoints; // List of points from which the bullet will fire
        public GameObject muzzlePrefab; // Muzzle effect prefab
        public GameObject hitPrefab; // Hit effect prefab
        public float hitOffset = 0.1f; // Offset for hit effect position
        
        [Header("AUDIO")]
        public AudioClip shotFX; // Shot sound effect
        public AudioClip reloadFX; // Reload sound effect
        private AudioSource audioSource; // AudioSource component
        
        public bool alternateShoot = false; // Toggle for alternating fire
        private int firePointIndex = 0; // Current fire point index

        [Header("Shooting Parameters")]
        public int bulletsPerShot = 1; // Number of bullets fired per shot

        [Header("Bullet Destruction Parameters")]
        public bool NotBulletDestroy = false; // If true, bullet will not be destroyed on impact

        [Header("Camera")]
        public CameraShake cameraShake; // Reference to CameraShake script
        public bool enableCameraShake = true; // Toggle to enable/disable camera shake
        public float cameraShakeDuration = 0.1f; // Camera shake duration
        public float cameraShakeMagnitude = 0.1f; // Camera shake magnitude

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
            // If reload was active on disable, set interruption flag
            if (isReloading && Time.time < reloadEndTime)
            {
                wasReloadingInterrupted = true;
                isReloading = false; // Stop current reload
            }

            // Disable reload text so it doesn't stay visible
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
                reloadText.enabled = true; // Show reload text
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
                reloadText.enabled = false; // Hide reload text after finishing
            }

            UpdateAmmoUI();
        }

        private void HandleShooting()
        {
            // Check if mouse button is pressed and can shoot
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                nextFireTime = Time.time + 1f / fireRate;
                Fire();
                
                // Play shot sound effect
                if (shotFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(shotFX);
                }
            }
        }

        private void Fire()
        {
            currentAmmo--; // Decrease ammo in magazine

            Transform selectedFirePoint = alternateShoot ? GetFirePoint() : null;

            // Create muzzle effect and bullets for the selected or all fire points
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    if (alternateShoot && selectedFirePoint != firePoint) continue; // Create only for selected fire point in alternate shoot mode

                    CreateMuzzleEffect(firePoint); // Create muzzle effect

                    // Create multiple bullets for one shot without spread
                    for (int i = 0; i < bulletsPerShot; i++)
                    {
                        Vector3 shootDirection = firePoint.forward; // Straight shooting direction

                        CreateBullet(firePoint, shootDirection); // Create bullet with direction
                    }
                }
            }

            // Enable camera shake if activated
            if (enableCameraShake && cameraShake != null)
            {
                cameraShake.ShakeCamera(cameraShakeDuration, cameraShakeMagnitude);
            }

            UpdateAmmoUI(); // Update UI after shooting
        }

        private Transform GetFirePoint()
        {
            if (firePoints.Count == 0)
            {
                Debug.LogError("No fire points.");
                return null;
            }

            Transform selectedFirePoint = firePoints[firePointIndex];
            firePointIndex = (firePointIndex + 1) % firePoints.Count;

            return selectedFirePoint;
        }

        private void CreateBullet(Transform firePoint, Vector3 shootDirection)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

                if (bulletRb != null)
                {
                    bulletRb.mass = forceValue * 0.1f; // Set bullet mass

                    // Use accuracy to control spread
                    float spread = (100f - accuracy) / 100f * 5f; // Spread angle
                    float randomAngleX = Random.Range(-spread, spread);
                    float randomAngleY = Random.Range(-spread, spread);

                    // Rotate direction with spread
                    Quaternion spreadRotation = Quaternion.Euler(randomAngleX, randomAngleY, 0);
                    Vector3 spreadDirection = spreadRotation * shootDirection;

                    bulletRb.linearVelocity = spreadDirection.normalized * speed; // Direction with spread
                    bullet.transform.forward = spreadDirection.normalized; // Set direction
                    Destroy(bullet, bulletTime);
                }
                else
                {
                    Debug.LogError("No Rigidbody component on the bullet prefab.");
                }

                // Add collider for handling collisions
                BulletCollision bulletCollision = bullet.AddComponent<BulletCollision>();
                bulletCollision.weaponSetting = this; // Pass reference to WeaponSetting

                // Create flying bullet effect if specified
                if (flyingBulletPrefab != null)
                {
                    GameObject flyingBulletEffect = Instantiate(flyingBulletPrefab, firePoint.position, firePoint.rotation);
                }
            }
            else
            {
                Debug.LogError("Bullet prefab or fire point not assigned.");
            }
        }

        private void CreateMuzzleEffect(Transform firePoint)
        {
            if (muzzlePrefab != null && firePoint != null)
            {
                GameObject muzzleVFX = Instantiate(muzzlePrefab, firePoint.position, firePoint.rotation, firePoint);
                muzzleVFX.transform.localPosition = Vector3.zero;
                muzzleVFX.transform.forward = firePoint.forward;

                ParticleSystem ps = muzzleVFX.GetComponent<ParticleSystem>();
            }
        }

        public void CheckReloadTextVisibility()
        {
            if (reloadText != null)
            {
                // Enable or disable text based on reload state
                reloadText.enabled = isReloading;
            }
        }

        private void UpdateAmmoUI()
        {
            // Update on-screen text
            if (magazineAmmoText != null)
            {
                magazineAmmoText.text = currentAmmo.ToString();
            }

            if (totalAmmoText != null)
            {
                totalAmmoText.text = totalAmmo.ToString();
            }
        }
    

 private class BulletCollision : MonoBehaviour
    {
        public WeaponSetting weaponSetting;
        private bool hasCollided = false; 

        private void OnCollisionEnter(Collision collision)
        {
            if (hasCollided) return; 
            hasCollided = true; 


            if (weaponSetting.hitPrefab != null)
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 hitPosition = contact.point + contact.normal * weaponSetting.hitOffset; //
                Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

                
                GameObject hitEffect = Instantiate(weaponSetting.hitPrefab, hitPosition, hitRotation);
            }


            Rigidbody bulletRb = GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                Destroy(bulletRb); 
            }

       
            Vector3 penetrationDepth = collision.contacts[0].normal * 0.1f; 
            transform.position = collision.contacts[0].point + penetrationDepth; 


            if (weaponSetting.NotBulletDestroy)
            {
                Vector3 incomingDirection = -collision.relativeVelocity.normalized; 
                transform.rotation = Quaternion.LookRotation(incomingDirection);
                transform.SetParent(collision.transform); 
            }
            else
            {
                
                transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal) * Quaternion.Euler(0, 180, 0);
                Destroy(gameObject); 
            }

 
            if (Random.Range(0f, 100f) <= weaponSetting.accuracy)
            {
                Health enemyHealth = collision.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(weaponSetting.damage);
                }

               
                Rigidbody enemyRb = collision.collider.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 forceDirection = -collision.contacts[0].normal; 
                    enemyRb.AddForce(forceDirection * weaponSetting.forceValue, ForceMode.Impulse);
                }
            }
        }
    }
}
}

