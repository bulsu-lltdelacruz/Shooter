using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    Dictionary<GameObject, IDamageable> enemyEntities;
    Dictionary<GameObject, IDamageable> projectileEntities;
    PlayerTarget playerEntity;

    // Usage

    public static EntityManager Instance;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        if(enemyEntities == null)
            enemyEntities = new Dictionary<GameObject, IDamageable>();
        if(projectileEntities == null)
            projectileEntities = new Dictionary<GameObject, IDamageable>();
    }
    public void RegisterProjectile(GameObject projyObj)
    {
        projectileEntities.Add(projyObj, projyObj.GetComponent<IDamageable>());
    }
    public void UnRegisterProjectile(GameObject projyObj)
    {
        projectileEntities.Remove(projyObj);
    }
    public bool GetDamageableFromProjectile(GameObject projObj, out IDamageable projectile)
    {
        projectile = projectileEntities[projObj];
        return projectile != null;
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
    public IDamageable GetDamageableFromAll(GameObject obj)
    {
        enemyEntities.TryGetValue(obj, out IDamageable entity);

        IDamageable playerDamageable = playerEntity.GetValue(obj);
        if (playerDamageable != null)
            return playerDamageable;

        return entity;
    }
    public void UnregisterAll()
    {
        enemyEntities.Clear();
    }
    public void RegisterPlayer(GameObject player, IDamageable playerIDamgeable)
    {
        playerEntity = new PlayerTarget(player, playerIDamgeable);
    }
    public IDamageable GetPlayerDamageable()
    {
        return playerEntity.Damageable;
    }
    public GameObject GetPlayerObject()
    {
        return playerEntity.GameObject;
    }
    private void OnDisable()
    {
        
    }
    private void OnDestroy()
    {
        
    }
}
public struct PlayerTarget
{
    public GameObject GameObject { get; }
    public IDamageable Damageable { get; }

    public PlayerTarget(GameObject gameObject, IDamageable damageable)
    {
        GameObject = gameObject;
        Damageable = damageable;
    }
    public IDamageable GetValue(GameObject obj)
    {
        return obj == GameObject?Damageable:null;
    }
}

