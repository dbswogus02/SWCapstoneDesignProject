using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Biostart.CameraEffects;
using Biostart.Enemy; 

namespace Biostart.Weapons
{
public class LaserWeapon : MonoBehaviour
{
    [Header("Боеприпасы")]
    public int magazineSize = 3000;
    public int totalAmmo = 900;
    private int currentAmmo;
    private bool isReloading = false;
    public float reloadTime = 2f;
    private float reloadEndTime;
    private bool wasReloadingInterrupted = false;

    [Header("Урон")]
    public float damage = 25f;

    [Header("UI")]
    public Text magazineAmmoText;
    public Text totalAmmoText;
    public Text reloadText;

    [Header("Префабы")]
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;
    public float hitEffectOffset = 0.5f;
    public List<Transform> firePoints;
    
    [Header("AUDIO")]
    public AudioClip shotFX;
    public AudioClip reloadFX;
    public LineRenderer laserLine;

    [Header("Сила толчка")]
    public float pushForce = 10f;

    [Header("Camera Shake")]
    public bool enableCameraShake = true;
    public CameraShake cameraShake;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;
    
    [Header("Настройки проекторов")]
    [SerializeField] private GameObject projectorWall;
    [SerializeField] private GameObject projectorEnemy;
    [SerializeField] private float minProjectorLifetime = 1f;
    [SerializeField] private float maxProjectorLifetime = 5f;
    [SerializeField] private float projectorScale = 1f;

    private AudioSource shotAudioSource;
    private GameObject activeMuzzleEffect;
    private GameObject activeHitEffect;

    private void Start()
    {
        currentAmmo = magazineSize;
        UpdateUI();
        
        laserLine.enabled = false;

        shotAudioSource = gameObject.AddComponent<AudioSource>();
        shotAudioSource.clip = shotFX;
        shotAudioSource.loop = true;

        activeHitEffect = Instantiate(hitPrefab);
        activeHitEffect.SetActive(false);
    }

    private void Update()
    {
        CheckReloadTextVisibility();
        
        if (Input.GetButton("Fire1") && !isReloading && currentAmmo > 0)
        {
            if (!shotAudioSource.isPlaying) shotAudioSource.Play();

            if (activeMuzzleEffect == null)
            {
                activeMuzzleEffect = Instantiate(muzzlePrefab, firePoints[0].position, firePoints[0].rotation, firePoints[0]);
                activeMuzzleEffect.SetActive(true);
            }

            activeMuzzleEffect.transform.position = firePoints[0].position;
            activeMuzzleEffect.transform.rotation = firePoints[0].rotation;

            Shoot();
            if (enableCameraShake && cameraShake != null) StartCoroutine(ShakeCameraWhileFiring());
        }
        else
        {
            StopFiringEffects();
        }

        // Проверка перезарядки
        if ((Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0) && !isReloading && totalAmmo > 0)
        {
            StartReload();
        }
        
        if (isReloading && Time.time >= reloadEndTime)
        {
            FinishReload();
        }
    }

    private void Shoot()
    {
        laserLine.SetPosition(0, firePoints[0].position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Length > 0)
        {
            RaycastHit closestHit = hits[0];
            foreach (var hit in hits)
                if (hit.distance < closestHit.distance) closestHit = hit;

            Vector3 hitPosition = closestHit.point + closestHit.normal * hitEffectOffset;
            laserLine.enabled = true;
            laserLine.SetPosition(1, hitPosition);

            if (activeHitEffect == null) activeHitEffect = Instantiate(hitPrefab);
            activeHitEffect.transform.position = hitPosition;
            activeHitEffect.transform.rotation = Quaternion.LookRotation(closestHit.normal);
            activeHitEffect.SetActive(true);

            GameObject projector = null;
            switch (closestHit.transform.gameObject.layer)
            {
                case 8:
                    projector = projectorWall;
                    break;
                case 9:
                    projector = projectorEnemy;
                    break;
            }

            if (projector == null)
            {
                Debug.Log("No projector assigned for this layer!");
                return;
            }

            Quaternion projectorRotation = Quaternion.FromToRotation(-Vector3.forward, closestHit.normal);
            GameObject obj = Instantiate(projector, hitPosition + closestHit.normal * 0.25f, projectorRotation);

            Projector proj = obj.GetComponent<Projector>();
            if (proj != null)
            {
                proj.orthographicSize *= projectorScale;
            }

            obj.transform.parent = closestHit.transform;
            Quaternion randomRotZ = Quaternion.Euler(obj.transform.eulerAngles.x, obj.transform.eulerAngles.y, Random.Range(0, 360));
            obj.transform.rotation = randomRotZ;

            float projectorLifetime = Random.Range(minProjectorLifetime, maxProjectorLifetime);
            Destroy(obj, projectorLifetime);

            currentAmmo--;
            UpdateUI();

            if (currentAmmo <= 0) StopFiringEffects();

            if (closestHit.collider.TryGetComponent<Rigidbody>(out var hitRigidbody))
            {
                Vector3 pushDirection = (closestHit.point - firePoints[0].position).normalized;
                hitRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }

            if (closestHit.collider.TryGetComponent<Health>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
        }
        else
        {
            StopFiringEffects();
        }
    }

    private IEnumerator ShakeCameraWhileFiring()
    {
        while (Input.GetButton("Fire1"))
        {
            cameraShake?.ShakeCamera(shakeDuration, shakeMagnitude);
            yield return new WaitForSeconds(shakeDuration);
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

        AudioSource.PlayClipAtPoint(reloadFX, transform.position);
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

        UpdateUI();
    }

    private void StopFiringEffects()
    {
        laserLine.enabled = false;
        shotAudioSource.Stop();

        if (activeMuzzleEffect != null)
        {
            Destroy(activeMuzzleEffect);
            activeMuzzleEffect = null;
        }

        if (activeHitEffect != null) activeHitEffect.SetActive(false);
    }

    private void UpdateUI()
    {
        magazineAmmoText.text = currentAmmo.ToString();
        totalAmmoText.text = totalAmmo.ToString();
    }

    private void OnEnable()
    {
        UpdateUI();
        if (wasReloadingInterrupted)
        {
            StartReload();
            wasReloadingInterrupted = false;
        }
    }

    private void OnDisable()
    {
        if (activeMuzzleEffect != null) Destroy(activeMuzzleEffect);
        if (activeHitEffect != null) Destroy(activeHitEffect);

        if (isReloading)
        {
            wasReloadingInterrupted = true;
            isReloading = false;
        }

        if (reloadText != null) reloadText.enabled = false;
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