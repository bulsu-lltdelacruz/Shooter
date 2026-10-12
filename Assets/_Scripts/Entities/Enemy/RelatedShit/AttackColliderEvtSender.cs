using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackColliderEvtSender : MonoBehaviour
{
    [SerializeField]UnityEvent<IDamageable> attackCollisionEvt;
  //  List<IDamageable> entitiesInRange;
    List<IDamageable> damagedAlready;
    private void Start()
    {
     //   entitiesInRange = new List<IDamageable>();
        damagedAlready = new List<IDamageable>();
    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = EntityManager.Instance.GetPlayerDamageable();
        if(other.CompareTag("Player"))
            Debug.Log(other.tag + "  :  " + damagedAlready.Contains(damageable));
        if (damagedAlready.Contains(damageable))
            return;
        attackCollisionEvt.Invoke(damageable);
        damagedAlready.Add(damageable);
    }
    private void OnTriggerExit(Collider other)
    {
       // IDamageable damageable = EntityManager.Instance.GetPlayerDamageable();
        //damagedAlready.Remove(damageable);
    }
    private void OnDisable()
    {
      //  entitiesInRange.Clear();
        damagedAlready.Clear();
      //  Debug.Log("RESET");
    }
    private void OnDestroy()
    {
       // entitiesInRange.Clear();
        damagedAlready.Clear();
    }
}
