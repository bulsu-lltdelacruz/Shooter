using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    Dictionary<GameObject, IDamageable> enemyEntities;
    public static EntityManager Instance;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        if(enemyEntities == null)
            enemyEntities = new Dictionary<GameObject, IDamageable>();
    }
    public void RegisterEnemy(GameObject enemyObj)
    {
        enemyEntities.Add(enemyObj, enemyObj.GetComponent<IDamageable>());
    }
    public void UnRegisterEnemy(GameObject enemyObj)
    {
        enemyEntities.Remove(enemyObj);
    }
    public bool GetDamageableFromEnemy( GameObject enemyObj, out IDamageable entity)
    {
        entity = enemyEntities[enemyObj];
        return entity != null;
    }
    public void UnregisterAll()
    {
        enemyEntities.Clear();
    }
    private void OnDisable()
    {
        
    }
    private void OnDestroy()
    {
        
    }
}
