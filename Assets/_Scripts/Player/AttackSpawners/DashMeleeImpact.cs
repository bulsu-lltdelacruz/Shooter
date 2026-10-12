using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DashMeleeImpact : MonoBehaviour
{
    [SerializeField] private LayerMask impactMask;
    [SerializeField] private float impactAmount;
    [SerializeField] private float damage;
    [SerializeField] private float duration;
    private List<GameObject> enemiesHit;
    private float deltaTime;
    void OnEnable()
    {
        
    }
    private void Start()
    {
        enemiesHit = new List<GameObject>();
    }
    private void Update()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime >= duration)
        {
            deltaTime = 0;
            enemiesHit.Clear();
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if((impactMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if(enemiesHit.Contains(other.gameObject))
                return;
            //other.TryGetComponent<IDamageable>(out IDamageable damageable);
            try
            {
                EntityManager.Instance.GetDamageableFromEnemy(other.gameObject, out IDamageable damageable);
                //damageable.TakeDamage
                SphereCollider collider = GetComponent<SphereCollider>();
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                float damageMultiplier = (enemy.isStunned)?1f:.06f;
                damageable.AddForceAndDamageAmount(damage* damageMultiplier, 7, 0);
                damageable.TakeDamageWithImpact(transform.position, DamageType.MELEE);
            }
            catch(KeyNotFoundException e)
            {
                Debug.LogError($"{other.gameObject.name} was not found inside the dictionary");
            }
            enemiesHit.Add(other.gameObject);

        }
    }

}
