using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackColliderEvtSender : MonoBehaviour
{
    [SerializeField] UnityEvent<IDamageable> attackCollisionEvt;
    private readonly List<IDamageable> damagedAlready = new List<IDamageable>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        IDamageable damageable = EntityManager.Instance.GetPlayerDamageable();
        if (damageable == null || damagedAlready.Contains(damageable))
            return;

        damagedAlready.Add(damageable);
        attackCollisionEvt.Invoke(damageable);
    }

    private void OnDisable()
    {
        damagedAlready.Clear();
    }
}