using UnityEngine;

public class Enemy :  Entity
{
    /*public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log("Damaged with ");
    }*/
    /*public void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source)
    {
        Vector3 forceDirection = transform.position - source;
        forceDirection.Normalize();
        forceDirection *= impactAmount;
    }*/
    private void Start()
    {
        EntityManager.Instance.RegisterEnemy(gameObject);
    }
    private void OnDestroy()
    {
        EntityManager.Instance.UnRegisterEnemy(gameObject);
    }
    private void Update()
    {
        base.FrameUpdate();
    }
}
