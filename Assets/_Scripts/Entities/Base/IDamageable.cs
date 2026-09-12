using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source);
    void TakeDamageWithImpact(Vector3 source);
    void AddForceAndDamageAmount(float damage, float forceAmount);
    void AddForce(Vector3 direction, float forceAmount);
    // void TakeDamage(float amount);
    // void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}
