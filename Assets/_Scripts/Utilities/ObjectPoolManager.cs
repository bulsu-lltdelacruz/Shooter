using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private bool _addToDontDestroyOnLoad = false;

    private GameObject _emptyHolder;

    private static GameObject _particleSystemsEmpty;
    private static GameObject _gameObjectsEmpty;
    private static GameObject _soundFXEmpty;

    private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
    private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

    public enum PoolType
    {
        ParticleSystems,
        GameObjects,
        SoundFX
    }
    private void Awake()
    {
        _objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetupEmpties();
    }
    private void SetupEmpties()
    {
        _emptyHolder = new GameObject("Object Pools");

        _particleSystemsEmpty = new GameObject("Particle Effects");
        _particleSystemsEmpty.transform.SetParent(_emptyHolder.transform);

        _gameObjectsEmpty = new GameObject("GameObjects");
        _gameObjectsEmpty.transform.SetParent(_emptyHolder.transform);

        _soundFXEmpty = new GameObject("Sound FX");
        _soundFXEmpty.transform.SetParent(_emptyHolder.transform);

        if (_addToDontDestroyOnLoad)
            DontDestroyOnLoad(_particleSystemsEmpty.transform.root);

    }

    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>
        (
            createFunc: () => CreateObject(prefab, pos, rot, pooltype),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject
        );
        _objectPools.Add(prefab, pool);
    }
    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, pos, rot);

        prefab.SetActive(true);

        GameObject parentObject = SetParent(pooltype);

        obj.transform.SetParent(parentObject.transform);

        return obj;

    }
    private static void OnGetObject(GameObject obj)
    {

    }
    private static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }
    private static void OnDestroyObject(GameObject obj)
    {
        if (_cloneToPrefabMap.ContainsKey(obj))
        {
            _cloneToPrefabMap.Remove(obj);
        }
    }

    private static GameObject SetParent(PoolType poolType)
    {
        switch(poolType)
        {
            case PoolType.ParticleSystems:
                return _particleSystemsEmpty;
            case PoolType.GameObjects:
                return _gameObjectsEmpty;
            case PoolType.SoundFX:
                return _soundFXEmpty;
            default:
                return null;
        }
    }

    private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)where T : Object
    {
        //if pool doesnt have a pool for this prefab, create new
        if(!_objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
        }
        //the logic that auto gets, or create if doesnt exist
        //this creates the summoned thing
        GameObject obj = _objectPools[objectToSpawn].Get();

        if(obj != null)
        {
            if(!_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Add(obj, objectToSpawn);//makes a key of the created gameobject, to a non instaniated prfab  as value (in think)
                //we set this because the value of the clone to prefab is the same is the key for the object pool. so its same memory
            }
            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRotation;
            obj.SetActive(true);

            if(typeof(T) == typeof(GameObject))
            {
                return obj as T;//if gameobject, then just getit
            }
            //if di nagana, it means component so just do it
            T component = obj.GetComponent<T>();
            if ((component == null))
            {
                Debug.LogError($"Object {objectToSpawn.name} doesn't have a component type of {typeof(T)}");
                return null;
            }
            return component;
        }
        return null;
    }
    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    }
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    }
    public static GameObject SpawnBulletTrail(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, Vector3 origin, Vector3 endPoint, PoolType poolType = PoolType.GameObjects)
    {
        //spawn the pooled object first, then get the LineRenderer from the instance
        BulletTrailHelper obj = SpawnObject<BulletTrailHelper>(objectToSpawn, spawnPos, spawnRotation, poolType);
        obj.SetFinalPos(endPoint);
        if (obj == null) return null;
        //LineRenderer line = obj.GetComponent<LineRenderer>();
        /*if (line == null)
        {
            Debug.LogError("LineRenderer Component not found on pooled instance");
            return obj;
        }*/
        /*line.startWidth = 0.08f;
        line.endWidth = 0.08f;
        line.positionCount = 2;
        line.SetPosition(0, origin);
        line.SetPosition(1, endPoint);
        line.enabled = true;*/

        return obj.gameObject;
    }
    public static GameObject SpawnLineRenderer(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation, Vector3 origin, Vector3 endPoint, PoolType poolType = PoolType.GameObjects)
    {
        //spawn the pooled object first, then get the LineRenderer from the instance

        GameObject obj = SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
        LineRenderer line = obj.GetComponent<LineRenderer>();
        if (line == null)
        {
            Debug.LogError("LineRenderer Component not found on pooled instance");
            return obj;
        }
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
        line.positionCount = 2;
        line.SetPosition(0, origin);
        line.SetPosition(1, endPoint);
        line.enabled = true;

        return obj;
    }
    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if(_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParent(poolType);
            if(obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform); 
            }
            if(_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
        }
    }
}
