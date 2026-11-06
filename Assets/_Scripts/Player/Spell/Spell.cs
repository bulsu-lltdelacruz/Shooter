using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    [SerializeField] SpellType spellType;
    EffectBase effect;
    List<IDamageable> enemiesInRange;
    private void Start()
    {
        enemiesInRange = new();
        switch (spellType)
        {
            case SpellType.Decay:
                effect = new Decay();
                break;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;
        EntityManager.Instance.GetDamageableFromEnemy(other.gameObject, out IDamageable enemyDamageable);
        if (enemiesInRange.Contains(enemyDamageable))
            return;
        Debug.Log("beenm hitters");
        other.GetComponent<Enemy>().ApplyDebuffForTime(2, effect);
        enemiesInRange.Add(enemyDamageable);
    }
    private void OnTriggerExit(Collider other)
    {

    }
    private void OnEnable()
    {

    }
    private void OnDisable()
    {
        enemiesInRange.Clear();
    }
}
enum SpellType
{
    Decay
}
