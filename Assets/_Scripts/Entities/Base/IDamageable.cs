using UnityEngine;

public interface IDamageable
{
    public EffectController effectController { get; }
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source, DamageType damageType);
    void TakeDamageWithImpact(Vector3 source, DamageType damageType);
    void TakeDamageWithImpact(Vector3 source, DamageType damageType, Vector3 hitPoint, Quaternion hitRotation);
    void AddForceAndDamageAmount(float damage, float forceAmount, float poiseDamage);
    void AddForce(Vector3 direction, float forceAmount);
    GameObject GetParryPrefab();
    // void TakeDamage(float amount);
    // void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}
public enum DamageType
{
    MELEE,
    HANDGUN,
    SHOTGUN,
    CROSSGUN,
    FALL
}