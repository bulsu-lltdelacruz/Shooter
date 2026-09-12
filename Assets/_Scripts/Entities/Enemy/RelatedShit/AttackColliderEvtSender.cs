using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackColliderEvtSender : MonoBehaviour
{
    [SerializeField]UnityEvent<IDamageable> attackCollisionEvt;
    List<IDamageable> entitiesInRange;
    private void Start()
    {
        entitiesInRange = new List<IDamageable>();
    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = EntityManager.Instance.GetPlayerDamageable();
        if(entitiesInRange.Contains(damageable))
            return;
        attackCollisionEvt?.Invoke(damageable);
        entitiesInRange.Add(damageable);
    }
    private void OnTriggerExit(Collider other)
    {
        IDamageable damageable = EntityManager.Instance.GetPlayerDamageable();
        entitiesInRange.Remove(damageable);
    }
    private void OnDisable()
    {
        entitiesInRange.Clear();
        //Debug.Log("deleted entties");
    }
    private void OnDestroy()
    {
        entitiesInRange.Clear();
    }
}
