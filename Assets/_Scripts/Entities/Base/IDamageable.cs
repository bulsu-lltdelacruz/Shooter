using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source);
    // void TakeDamage(float amount);
    // void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}
