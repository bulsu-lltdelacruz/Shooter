using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VolumeComponent;
using static UnityEngine.UI.Image;

// Custom serializable event class so this UnityEvent's arguments show up properly in the Inspector.
[System.Serializable] public class AmmoChangedEvent : UnityEvent<int, int> { }   // current, reserve

[RequireComponent(typeof(AudioSource))]

public class WeaponController : MonoBehaviour
{
    [SerializeField] public WeaponData data;
    [SerializeField] public BaseWeaponClass weaponState;
    [SerializeField] Transform firePoint;      // muzzle - used for projectile spawn / muzzle flash position
    [SerializeField] Camera aimCamera;         // raycast origin/direction comes from here, assign your player camera
    [SerializeField] public Transform fireOrigin;
    //[SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject handGunLineRender;
    [SerializeField] private LayerMask gunMask;
    PlayerInventory inventory;

    [Header("UI Reference")]
    [SerializeField] UIDocument HUDDoc;
    Label bulletCountDocument;

    [Header("Events (wire these to animation, UI, camera recoil, etc.)")]
    public UnityEvent OnFire;
    public UnityEvent OnReloadStart;
    public UnityEvent OnReloadComplete;
    public AmmoChangedEvent OnAmmoChanged;

    int currentAmmo;
    AudioSource audioSource;

    private int damageableLayer;
    private int projectileLayer;

    //ewawawa
    Vector2[] spiralPointsCache;
    int cachedPelletCount = -1;


    List<HitData> damageable;
    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        audioSource = GetComponent<AudioSource>();
        damageable = new List<HitData>();

        damageableLayer = LayerMask.NameToLayer("Damageable");
        projectileLayer = LayerMask.NameToLayer("ProjectileDamageable");
        bulletCountDocument = HUDDoc.rootVisualElement.Q<Label>("bullet-count");
    }

    void Start()
    {
        //currentAmmo = inventory.activeSlot.activeItem.currentBulletCount;
    }

    /// <summary>Call every frame while the fire button is held down. Only does anything for Auto weapons.</summary>
    public void TryFireHeld()
    {
        if (data.fireMode != FireMode.Auto) return;
        TryFireSingleShot();
    }

    /// <summary>Call once, on the frame the fire button is first pressed.</summary>
    public bool TryFirePressed()
    {
        switch (data.fireMode)
        {
            case FireMode.Single:
                return TryFireSingleShot();
            case FireMode.Auto:
                return TryFireSingleShot();
            case FireMode.Burst:
                TryStartBurst();
                return false;
            default:
                TryFireSingleShot();
                return false;
        }
    }

    bool TryFireSingleShot()
    {

        /*if (currentAmmo <= 0)
        {
            if (data.emptySound) audioSource.PlayOneShot(data.emptySound);
            return;
        }*/

        //currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, inventory.activeSlot.activeItem.currentBulletCount);
        if(inventory.activeSlot.activeItem.currentBulletCount <= 0)
        {
            //play no ammo
            Debug.Log("No Ammo Left");
            return false;
        }
        FireShot();
        return true;
    }

    void TryStartBurst()
    {
        StartCoroutine(BurstRoutine());
    }

    IEnumerator BurstRoutine()
    {

        for (int i = 0; i < data.burstCount; i++)
        {
            if (currentAmmo <= 0)
            {
                if (data.emptySound) audioSource.PlayOneShot(data.emptySound);
                yield break;
            }

            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, inventory.activeSlot.activeItem.currentBulletCount);
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
            /*int barelCount = fireOrigin.Length;
            for (int i = 0; i < barelCount; i++)
            {*/
                ObjectPoolManager.SpawnBulletTrail(handGunLineRender, rawOrigin,
                fireOrigin.rotation,
                rawOrigin,
                target.transform.position,
                ObjectPoolManager.PoolType.ParticleSystems);
                if (EntityManager.Instance.GetDamageableFromEnemy(target, out IDamageable damageable))
                    damageable.TakeDamage(data.damagePerPellet, target.transform.position, target.transform.position);
           // }
            
        }
    }
    void FireShot()
    {
        OnFire?.Invoke();
        DecreaseBulletCount(1);
        if (data.fireSound)
            SoundFXManager.instance.PlaySoundFXClip(data.fireSound, transform, 1, SoundType.SOUNDFX);
        //audioSource.PlayOneShot(data.fireSound);
        if (data.muzzleFlashPrefab)
        {
            ObjectPoolManager.SpawnObject(data.muzzleFlashPrefab, fireOrigin.position, fireOrigin.rotation);
        }
            

        //int barelCount = fireOrigin.Length;
        int pelletsPerShot = data.pelletsPerShot;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = ApplySpread(aimCamera.transform.forward, data.bulletSpread, i, pelletsPerShot);
            Vector3 rawOrigin = ApplyStartSpread(data.bulletStartSpread, i, pelletsPerShot);
            if (data.projectilePrefab != null)
                FireProjectile(rawOrigin, direction, false);
            else
            {
                HitData hitData = FireHitscan(rawOrigin, direction, false, i);
                if (hitData != null)
                {
                    damageable.Add(hitData);
                }
                //damageable = (damageableTemp != null) ? damageableTemp : damageable;
            }
                
        }
        if(damageable.Count > 0)
        {
            foreach (var item in damageable)
            {
                Quaternion spawnRotation = Quaternion.LookRotation(-transform.forward);
                item.damageable.TakeDamageWithImpact(transform.position, data.damageType, item.hitPoint, spawnRotation);

            }
            damageable.Clear();
        }
        else
        {
            
        }
            

    }
    public void RestoreBullets(float percent)
    {
        List<Slot> allSlots = inventory.GetInventorySlots();

        foreach (Slot slot in allSlots)
        {
            Slot.Item item = slot.activeItem;
            if (item == null) continue;

            int maxBullets = item.data.maxBulletCount;
            if (item.currentBulletCount >= maxBullets)
            {
                item.bulletProgress = 0f;
                continue;
            }

            float multiplier = (item.data.weaponType == WeaponType.CrossGun) ? 0.5f : 1f;
            multiplier = (item.data.weaponType == WeaponType.Shotgun) ? 0.75f : 1f;

            item.bulletProgress += maxBullets * percent * multiplier;
            int whole = Mathf.FloorToInt(item.bulletProgress + 0.0001f);

            if (whole > 0)
            {
                item.bulletProgress = Mathf.Max(0f, item.bulletProgress - whole);
                item.currentBulletCount = Mathf.Min(maxBullets, item.currentBulletCount + whole);

                if (item.currentBulletCount >= maxBullets)
                    item.bulletProgress = 0f;
            }
        }

        if (inventory.activeSlot != null && inventory.activeSlot.activeItem != null)
            bulletCountDocument.text = inventory.activeSlot.activeItem.currentBulletCount.ToString();
    }
    public void DecreaseBulletCount(int amount)
    {
        inventory.activeSlot.activeItem.currentBulletCount -= amount;
        bulletCountDocument.text = inventory.activeSlot.activeItem.currentBulletCount.ToString();
    }
    HitData FireHitscan(Vector3 rawOrigin, Vector3 direction, bool isFromOtherSource, int pelletIndex)
    {
        HitData hitData = null;
        // Vector3 origin = rawOrigin + aimCamera.transform.position;
        Vector3 origin = aimCamera.transform.position;
        if (isFromOtherSource)
            origin = rawOrigin;
        //int barelCount = fireOrigin.Length;
       /* for (int i = 0; i < barelCount; i++)
        {*/
            Vector3 endPoint = origin + (direction * 999);
            Debug.DrawRay(origin, direction*10f, Color.blue, 1f);
            if (Physics.Raycast(origin, direction, out RaycastHit hit, 999,gunMask))
            {
                if (hit.transform.gameObject.CompareTag("Enemy"))
                {
                    if (data.impactEffectPrefab)
                        Instantiate(data.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));

                    if (EntityManager.Instance.GetDamageableFromEnemy(hit.collider.gameObject, out IDamageable damageable))
                    {
                        damageable.AddForceAndDamageAmount(data.damagePerPellet, data.pushForce, data.poiseDamagePerPellet);
                        hitData = new HitData(damageable, hit.point);
                    }
                    
                }
                else if (hit.transform.gameObject.CompareTag("Projectile"))
                {
                
                    if (EntityManager.Instance.GetDamageableFromProjectile(hit.collider.gameObject, out IDamageable projDamageable))
                    {
                        projDamageable.TakeDamage(0, transform.position, transform.position);
                        hitData = new HitData(projDamageable, hit.point);
                    }
                }

                endPoint = hit.point;
            }

            Vector3 firOriginfrfr = fireOrigin.position+rawOrigin;
            if (isFromOtherSource)
                firOriginfrfr = rawOrigin;
            ObjectPoolManager.SpawnBulletTrail(handGunLineRender, firOriginfrfr,
                fireOrigin.rotation,
                 fireOrigin.position,
                 endPoint,
                 ObjectPoolManager.PoolType.ParticleSystems);
        // }
        return hitData;
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
            float radiusT = totalPellets > 1 ? Mathf.Sqrt((float)i / (totalPellets - 1)) : 0f;
            float thetaRad = i * goldenAngle * Mathf.Deg2Rad;
            spiralPointsCache[i] = new Vector2(Mathf.Cos(thetaRad), Mathf.Sin(thetaRad)) * radiusT;
        }

        // Raw spiral centroid isn't (0,0) for most pellet counts -> visible bias to one side.
        Vector2 centroid = Vector2.zero;
        for (int i = 0; i < totalPellets; i++)
            centroid += spiralPointsCache[i];
        centroid /= totalPellets;

        for (int i = 0; i < totalPellets; i++)
            spiralPointsCache[i] -= centroid;

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
    /*public void Reload()
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
    }*/
}
class HitData
{
    public IDamageable damageable;
    public Vector3 hitPoint;
    public HitData(IDamageable damageable, Vector3 hitPoint)
    {
        this.damageable = damageable;
        this.hitPoint = hitPoint;
    }
}