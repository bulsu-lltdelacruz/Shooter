using System.Collections.Generic;
using UnityEngine;

public class ProjectileAndOwnerManager : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> relation;
    private void Start()
    {
        relation = new Dictionary<GameObject, List<GameObject>>();
    }
    public void RegisterRelation(GameObject parent, GameObject projectile)
    {
        if (relation.ContainsKey(parent))
        {
            relation[parent].Add(projectile);
            return;
        }
        List<GameObject> l = new List<GameObject>();
        l.Add(projectile);
        relation.Add(gameObject, l);
    }
    public bool UnregisterProjectile(GameObject parent, GameObject projectile)
    {
        if (relation.ContainsKey(parent))
        {
            relation[parent].Remove(projectile);
            return true;
        }else
            return false;
    }
    public bool UnregisterParent(GameObject parent)
    {
        if (relation.ContainsKey(parent))
        {
            relation.Remove(parent);
            return true;
        }
        else
            return false;
            
    }
    public void GetProjectileParent(GameObject projectile)
    {
        //string key = relation.Keys(x => x.Value == 2).Key;
    }
}