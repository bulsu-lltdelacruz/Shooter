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
            enemiesHit.Add(other.gameObject);
            other.TryGetComponent<IDamageable>(out IDamageable damageable);
            //damageable.TakeDamage
            SphereCollider collider = GetComponent<SphereCollider>();
            damageable.TakeDamageWithImpact(damage, impactAmount, transform.position);
        }
    }

}
