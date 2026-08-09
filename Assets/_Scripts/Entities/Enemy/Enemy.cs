using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log("Damaged with "+amount);
    }
}
