using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

// Custom serializable event class so this UnityEvent's arguments show up properly in the Inspector.
[System.Serializable] public class AmmoChangedEvent : UnityEvent<int, int> { }   // current, reserve

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [SerializeField] WeaponData data;
    [SerializeField] Transform firePoint;      // muzzle - used for projectile spawn / muzzle flash position
    [SerializeField] Camera aimCamera;         // raycast origin/direction comes from here, assign your player camera
    [SerializeField] Transform fireOrigin;
    //[SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject handGunLineRender;

    [Header("Events (wire these to animation, UI, camera recoil, etc.)")]
    public UnityEvent OnFire;
    public UnityEvent OnReloadStart;
    public UnityEvent OnReloadComplete;
    public AmmoChangedEvent OnAmmoChanged;

    int currentAmmo;
    int reserveAmmo;
    float nextFireTime;
    bool isReloading;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = data.magazineSize;
        reserveAmmo = data.reserveAmmo;
        
    }

    void Start()
    {
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }

    /// <summary>Call every frame while the fire button is held down. Only does anything for Auto weapons.</summary>
    public void TryFireHeld()
    {
        if (data.fireMode != FireMode.Auto) return;
        TryFireSingleShot();
    }

    /// <summary>Call once, on the frame the fire button is first pressed.</summary>
    public void TryFirePressed()
    {
        switch (data.fireMode)
        {
            case FireMode.Auto:
                TryFireSingleShot();
                break;
            case FireMode.Burst:
                TryStartBurst();
                break;
            case FireMode.Single:
                TryFireSingleShot();
                break;
            default:
                TryFireSingleShot();
                break;
        }
    }

    void TryFireSingleShot()
    {
        if (isReloading) return;
        if (Time.time < nextFireTime) return;

        /*if (currentAmmo <= 0)
        {
            if (data.emptySound) audioSource.PlayOneShot(data.emptySound);
            return;
        }*/

        nextFireTime = Time.time + 1f / Mathf.Max(data.fireRate, 0.01f);
        //currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);

        FireShot();
    }

    void TryStartBurst()
    {
        if (isReloading || Time.time < nextFireTime) return;
        StartCoroutine(BurstRoutine());
    }

    IEnumerator BurstRoutine()
    {
        nextFireTime = Time.time + 1f / Mathf.Max(data.fireRate, 0.01f);

        for (int i = 0; i < data.burstCount; i++)
        {
            if (currentAmmo <= 0)
            {
                if (data.emptySound) audioSource.PlayOneShot(data.emptySound);
                yield break;
            }

            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
            FireShot();

            yield return new WaitForSeconds(data.burstShotDelay);
        }
    }

    void FireShot()
    {
        OnFire?.Invoke();

        if (data.fireSound) audioSource.PlayOneShot(data.fireSound);
        if (data.muzzleFlashPrefab) Instantiate(data.muzzleFlashPrefab, firePoint.position, firePoint.rotation);

        // Each pellet gets its own independent spread roll - this is what makes shotguns feel right.
        // Set data.pelletsPerShot = 1 for normal weapons and this loop just runs once.
        for (int i = 0; i < data.pelletsPerShot; i++)
        {
            Vector3 direction = ApplySpread(aimCamera.transform.forward, data.bulletSpread);
            Vector3 rawOrigin = ApplyStartSpread(data.bulletStartSpread);
            if (data.projectilePrefab != null)
                FireProjectile(rawOrigin, direction);
            else
                FireHitscan(rawOrigin, direction);
        }
    }

    void FireHitscan(Vector3 rawOrigin, Vector3 direction)
    {
        Vector3 origin = rawOrigin + aimCamera.transform.position;
        Vector3 endPoint = origin + (direction * 99999);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 99999))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Damageable"))
            {
            if (data.impactEffectPrefab)
                Instantiate(data.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

            if (hit.collider.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(data.damagePerPellet, hit.point, hit.normal);
            }
            endPoint = hit.point;
        }
        //lineRenderer.SetPosition(0, fireOrigin.position + rawOrigin);
        //lineRenderer.SetPosition(1, endPoint);
        ObjectPoolManager.SpawnLineRenderer(handGunLineRender, fireOrigin.transform.position,
            fireOrigin.transform.rotation,
             fireOrigin.position + rawOrigin,
             endPoint,
             ObjectPoolManager.PoolType.ParticleSystems);
    }
    

    void FireProjectile(Vector2 origin, Vector3 direction)
    {
        GameObject proj = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        if (proj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * data.projectileSpeed; // Unity < 6: use rb.velocity instead
        }
    }

    /// <summary>Randomizes a direction within a cone around 'forward', up to 'spreadAngle' degrees.</summary>
    Vector3 ApplySpread(Vector3 forward, float spreadAngle)
    {
        if (spreadAngle <= 0f) return forward;

        Vector2 randomPoint = Random.insideUnitCircle * spreadAngle;
        Quaternion spreadRotation = Quaternion.Euler(-randomPoint.y, randomPoint.x, 0f);
        return spreadRotation * forward;
    }
    Vector3 ApplyStartSpread(float spreadAmount)
    {
        if(spreadAmount <= 0f) return Vector3.zero;
        return Random.insideUnitCircle * spreadAmount;
    }
    public void Reload()
    {
        if (isReloading || currentAmmo == data.magazineSize || reserveAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadStart?.Invoke();

        yield return new WaitForSeconds(data.reloadTime);

        int needed = data.magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;

        isReloading = false;
        OnReloadComplete?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
}