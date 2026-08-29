using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using static UnityEngine.UI.Image;

// Custom serializable event class so this UnityEvent's arguments show up properly in the Inspector.
[System.Serializable] public class AmmoChangedEvent : UnityEvent<int, int> { }   // current, reserve

[RequireComponent(typeof(AudioSource))]

public class WeaponController : MonoBehaviour
{
    [SerializeField] public WeaponData data;
    [SerializeField] Transform firePoint;      // muzzle - used for projectile spawn / muzzle flash position
    [SerializeField] Camera aimCamera;         // raycast origin/direction comes from here, assign your player camera
    [SerializeField] public Transform[] fireOrigin;
    //[SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject handGunLineRender;
    [SerializeField] private LayerMask gunMask;

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

    private int damageableLayer;

    //ewawawa
    Vector2[] spiralPointsCache;
    int cachedPelletCount = -1; 
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = data.magazineSize;
        reserveAmmo = data.reserveAmmo;
        damageableLayer = LayerMask.NameToLayer("Damageable");
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
    public void FireShotAutoTarget(GameObject target, Vector3 rawOrigin)
    {
        Vector3 direction = target.transform.position - rawOrigin;
        if (data.projectilePrefab != null)
            FireProjectile(rawOrigin, direction, true);
        else

        {
            int barelCount = fireOrigin.Length;
            for (int i = 0; i < barelCount; i++)
            {
                ObjectPoolManager.SpawnBulletTrail(handGunLineRender, rawOrigin,
                fireOrigin[i].rotation,
                rawOrigin,
                target.transform.position,
                ObjectPoolManager.PoolType.ParticleSystems);
                if (EntityManager.Instance.GetDamageableFromEnemy(target, out IDamageable damageable))
                    damageable.TakeDamage(data.damagePerPellet, target.transform.position, target.transform.position);
            }
            
        }
    }

    void FireShot()
    {
        OnFire?.Invoke();

        if (data.fireSound)
            audioSource.PlayOneShot(data.fireSound);
        if (data.muzzleFlashPrefab)
            Instantiate(data.muzzleFlashPrefab, firePoint.position, firePoint.rotation);

        int barelCount = fireOrigin.Length;
        int totalPellets = data.pelletsPerShot / barelCount;

        for (int i = 0; i < totalPellets; i++)
        {
            Vector3 direction = ApplySpread(aimCamera.transform.forward, data.bulletSpread, i, totalPellets);
            Vector3 rawOrigin = ApplyStartSpread(data.bulletStartSpread, i, totalPellets);
            if (data.projectilePrefab != null)
                FireProjectile(rawOrigin, direction, false);
            else
                FireHitscan(rawOrigin, direction, false);
        }
    }

    void FireHitscan(Vector3 rawOrigin, Vector3 direction, bool isFromOtherSource)
    {
        Vector3 origin = rawOrigin + aimCamera.transform.position;
        if (isFromOtherSource)
            origin = rawOrigin;
        int barelCount = fireOrigin.Length;
        for (int i = 0; i < barelCount; i++)
        {
            Vector3 endPoint = origin + (direction * 999);
            if (Physics.Raycast(origin, direction, out RaycastHit hit, 999,gunMask))
            {
                if (hit.transform.gameObject.layer == damageableLayer)
                {
                    if (data.impactEffectPrefab)
                        Instantiate(data.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

                    if (EntityManager.Instance.GetDamageableFromEnemy(hit.collider.gameObject, out IDamageable damageable))
                        damageable.TakeDamageWithImpact(data.damagePerPellet, data.pushForce, transform.position);
                }
                endPoint = hit.point;
            }
            Vector3 firOriginfrfr = fireOrigin[i].position+rawOrigin;
            if (isFromOtherSource)
                firOriginfrfr = rawOrigin;
            ObjectPoolManager.SpawnBulletTrail(handGunLineRender, firOriginfrfr,
                fireOrigin[i].rotation,
                 fireOrigin[i].position,
                 endPoint,
                 ObjectPoolManager.PoolType.ParticleSystems);
        }
    }
    

    void FireProjectile(Vector2 origin, Vector3 direction, bool isFromOtherSource)
    {
        GameObject proj = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        if (proj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * data.projectileSpeed; // Unity < 6: use rb.velocity instead
        }
    }

    /// <summary>Randomizes a direction within a cone around 'forward', up to 'spreadAngle' degrees.</summary>
    /// <summary>Distributes pellets in a fixed, repeatable Vogel/golden-angle spiral pattern within a cone.</summary>
    void EnsureSpiralCache(int totalPellets)
    {
        if (spiralPointsCache != null && cachedPelletCount == totalPellets) return;

        spiralPointsCache = new Vector2[totalPellets];
        const float goldenAngle = 137.5077f;
        for (int i = 0; i < totalPellets; i++)
        {
            float radiusT = totalPellets > 1 ? Mathf.Sqrt((float)i / (totalPellets - 1)) : 0f; // i=0 -> center
            float thetaRad = i * goldenAngle * Mathf.Deg2Rad;
            spiralPointsCache[i] = new Vector2(Mathf.Cos(thetaRad), Mathf.Sin(thetaRad)) * radiusT;
        }
        cachedPelletCount = totalPellets;
    }

    Vector3 ApplySpread(Vector3 forward, float spreadAngle, int pelletIndex, int totalPellets)
    {
        if (spreadAngle <= 0f) return forward;
        EnsureSpiralCache(totalPellets);

        Vector2 point = spiralPointsCache[pelletIndex] * spreadAngle;
        // rotate around the camera's own axes so the pattern follows the aim direction, not world space
        Quaternion spreadRotation = Quaternion.AngleAxis(point.x, aimCamera.transform.up)
                                   * Quaternion.AngleAxis(-point.y, aimCamera.transform.right);
        return spreadRotation * forward;
    }

    Vector3 ApplyStartSpread(float spreadAmount, int pelletIndex, int totalPellets)
    {
        if (spreadAmount <= 0f) return Vector3.zero;
        EnsureSpiralCache(totalPellets);

        Vector2 point = spiralPointsCache[pelletIndex] * spreadAmount;
        // build offset from camera's right/up so it rotates with the camera instead of staying world-aligned
        return aimCamera.transform.right * point.x + aimCamera.transform.up * point.y;
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