using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamHelper : MonoBehaviour, IDamageable
{
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] ParticleSystem particle;
    [SerializeField] MeshRenderer meshRenderer;
    [Tooltip("Failsafe: beam is always returned to the pool after this many seconds.")]
    [SerializeField] float maxLifetime = 5f;

    public GameObject parent;

    readonly List<IDamageable> entitiesInRange = new List<IDamageable>();
    readonly WaitForSeconds explodeWait = new WaitForSeconds(.5f);

    float speed;
    float maxDistance;
    float traveled;
    float lifeTimer;
    bool isLaunched;
    bool isExploded;
    bool isReturned;

    public EffectController effectController => null;

    private void OnEnable()
    {
        // Reset all state, since this object comes from a pool
        isLaunched = false;
        isExploded = false;
        isReturned = false;
        traveled = 0f;
        lifeTimer = 0f;
        entitiesInRange.Clear();

        _ToggleParticle(false);
        meshRenderer.enabled = true;
        trailRenderer.enabled = true;
        EntityManager.Instance.RegisterProjectile(gameObject);
    }

    private void OnDisable()
    {
        if (EntityManager.Instance != null)
            EntityManager.Instance.UnRegisterProjectile(gameObject);
        entitiesInRange.Clear();
    }

    public void Launch(GameObject owner, float beamSpeed, float beamMaxDistance)
    {
        parent = owner;
        speed = beamSpeed;
        maxDistance = beamMaxDistance;
        traveled = 0f;
        lifeTimer = 0f;
        isLaunched = true;
        trailRenderer.Clear();
    }

    private void Update()
    {
        if (isReturned)
            return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            ReturnToPool();
            return;
        }

        if (!isLaunched || isExploded)
            return;

        float step = speed * Time.deltaTime;
        transform.position += transform.forward * step;
        traveled += step;

        if (traveled >= maxDistance)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isReturned || isExploded)
            return;

        // ignore the shooter (and any of its child colliders)
        if (parent != null && other.transform.IsChildOf(parent.transform))
            return;

        if (!other.CompareTag("Enemy") && !other.CompareTag("Player"))
            return;

        IDamageable damageable = EntityManager.Instance.GetDamageableFromAll(other.gameObject);
        if (damageable == null || entitiesInRange.Contains(damageable))
            return;

        damageable.TakeDamage(10, transform.position, transform.forward);

        entitiesInRange.Add(damageable);
        Explode();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("Player"))
            return;
        IDamageable damageable = EntityManager.Instance.GetDamageableFromAll(other.gameObject);
        if (damageable != null)
            entitiesInRange.Remove(damageable);
    }

    public void Explode()
    {
        if (isExploded || isReturned)
            return;

        isExploded = true;
        meshRenderer.enabled = false;
        trailRenderer.enabled = false;
        _ToggleParticle(true);
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator DestroyAfterTime()
    {
        yield return explodeWait;
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (isReturned)
            return;

        isReturned = true;
        trailRenderer.enabled = false;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void _ToggleParticle(bool isPlay)
    {
        if (particle == null)
            return;
        if (isPlay && !particle.isPlaying)
        {
            particle.Play();
        }
        else if (!isPlay && particle.isPlaying)
        {
            particle.Stop();
        }
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isExploded || isReturned)
            return;

        GameObject impactPrefab = Prefabs.Instance.GetImpactPrefab();
        ObjectPoolManager.SpawnObject(impactPrefab, transform.position, transform.rotation, ObjectPoolManager.PoolType.GameObjects);
        Explode();
    }

    public void TakeDamageWithImpact(float amount, float impactAmount, Vector3 source, DamageType damageType) { }

    public void TakeDamageWithImpact(Vector3 source, DamageType damageType) { }

    public void TakeDamageWithImpact(Vector3 source, DamageType damageType, Vector3 hitPoint, Quaternion hitRotation) { }

    public void AddForceAndDamageAmount(float damage, float forceAmount, float poiseDamage) { }

    public void AddForce(Vector3 direction, float forceAmount) { }

    public GameObject GetParryPrefab()
    {
        throw new System.NotImplementedException();
    }
}