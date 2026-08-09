using UnityEngine;

public enum FireMode
{
    Single,
    Burst,
    Auto
}
[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Pistol";

    [Header("Fire Mode")]
    public FireMode fireMode = FireMode.Single;
    [Tooltip("Shots per second. Governs cooldown between shots in every fire mode.")]
    public float fireRate = 6f;
    [Tooltip("Only used when Fire Mode = Burst")]
    public int burstCount = 3;
    [Tooltip("Delay between individual shots inside one burst")]
    public float burstShotDelay = 0.08f;

    [Header("Damage & Range")]
    public float damagePerPellet = 20f;
    public float range = 100f;
    public LayerMask hitMask = ~0;

    [Header("Bullet Spread")]
    [Tooltip("Max cone angle (degrees) a bullet can randomly deviate from center. This is the only accuracy model - hipfire only, no ADS.")]
    public float bulletSpread = 3f;

    [Header("Pellets")]
    [Tooltip("1 for normal guns. Set higher for shotguns - each pellet rolls its own spread and damage independently.")]
    [Min(1)] public int pelletsPerShot = 1;

    [Header("Ammo")]
    public int magazineSize = 12;
    public int reserveAmmo = 84;
    public float reloadTime = 1.6f;

    [Header("Projectile Mode (optional)")]
    [Tooltip("Leave empty for instant hitscan. Assign a prefab (with Rigidbody) to fire a physical projectile instead (rockets, grenades launchers, etc).")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 60f;

    [Header("FX")]
    public GameObject muzzleFlashPrefab;
    public GameObject impactEffectPrefab;
    public AudioClip fireSound;
    public AudioClip emptySound;
}