using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BeamHelper : MonoBehaviour, IDamageable
{
   // [SerializeField] UnityEvent<IDamageable> damagePlayerEvt;
    [SerializeField] TrailRenderer trailRenderer;
    List<IDamageable> entitiesInRange;
    public GameObject parent;
    bool isReturned;
   // IDamageable playerDamageable;
    private void Start()
    {
        entitiesInRange = new List<IDamageable>();
        //playerDamageable = EntityManager.Instance.GetPlayerDamageable(); ;
    }
    private void OnEnable()
    {
        EntityManager.Instance.RegisterProjectile(gameObject);
        isReturned = false;
        trailRenderer.enabled = true;
    }
    private void OnDisable()
    {
        EntityManager.Instance.UnRegisterProjectile(gameObject);
        entitiesInRange.Clear();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(isReturned || other.gameObject == parent)
            return;
        IDamageable damageable = EntityManager.Instance.GetDamageableFromAll(other.gameObject);

        if (entitiesInRange.Contains(damageable) || (!other.CompareTag("Enemy") && !other.CompareTag("Player")))
            return;

       // damagePlayerEvt?.Invoke(damageable);
        damageable.TakeDamage(2, transform.position, transform.forward);

        entitiesInRange.Add(damageable);
        trailRenderer.enabled = false;

        ObjectPoolManager.ReturnObjectToPool(gameObject);
        isReturned = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy") || !other.CompareTag("Player"))
            return;
        IDamageable damageable = EntityManager.Instance.GetDamageableFromAll(other.gameObject);
        entitiesInRange.Remove(damageable);
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
        ObjectPoolManager.SpawnObject(impactPrefab, transform.position, transform.rotation, ObjectPoolManager.PoolType.GameObjects);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    public void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source)
    {

    }

    public void TakeDamageWithImpact(Vector3 source)
    {

    }

    public void AddForceAndDamageAmount(float damage, float forceAmount)
    {

    }

    public void AddForce(Vector3 direction, float forceAmount)
    {

    }
}
