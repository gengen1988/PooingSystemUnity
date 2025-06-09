using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem2 : MonoBehaviour, IPoolingSystem
{
    private readonly Dictionary<int, ObjectPoolForPoolingSystem2> _poolByPrefab = new();

    public PooledObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent)
    {
        // retrieve pool
        var pool = GetOrAddPool(prefab);

        // acquire entry and put it in right place
        var poolingData = pool.Get();
        var trans = poolingData.transform;
        trans.localPosition = localPosition;
        trans.localRotation = localRotation;

        // set active
        trans.SetParent(parent, false);
        if (!poolingData.gameObject.activeSelf)
        {
            poolingData.gameObject.SetActive(true);
            Debug.LogWarning("spawn a non-active object. force activate it", poolingData);
        }

        return poolingData.CurrentHandle;
    }

    public PooledObjectHandle GetPoolingHandleInParent(GameObject from)
    {
        var found = from.GetComponentInParent<PoolingSystem2DataComponent>();
        if (found)
        {
            return found.CurrentHandle;
        }
        else
        {
            return null;
        }
    }

    /// for despawning subject found by physics query
    public PooledObjectHandle GetPoolingHandle(GameObject from)
    {
        var found = from.GetComponent<PoolingSystem2DataComponent>();
        if (found)
        {
            return found.CurrentHandle;
        }
        else
        {
            return null;
        }
    }

    private ObjectPoolForPoolingSystem2 GetOrAddPool(GameObject prefab)
    {
        var poolID = prefab.GetInstanceID();
        var pool = _poolByPrefab.GetValueOrDefault(poolID);
        if (!pool)
        {
            var poolObject = new GameObject($"Pool ({prefab.name})");
            pool = poolObject.AddComponent<ObjectPoolForPoolingSystem2>();
            pool.Init(prefab);
            _poolByPrefab.Add(poolID, pool);
        }

        return pool;
    }

    // public GameObject Spawn(Transform parent)
    // {
    //     var entry = _pool.Get();
    //     var prefabTrans = prefab.transform;
    //     var entryTrans = entry.transform;
    //     // reset position and rotation to prefab's default
    //     entryTrans.position = prefabTrans.position;
    //     entryTrans.rotation = prefabTrans.rotation;
    //     entryTrans.SetParent(parent);
    //
    //     if (!entry.activeSelf)
    //     {
    //         Debug.LogWarning("spawn a non-active object", entry);
    //     }
    //
    //     return entry;
    // }
    //
    // public GameObject Spawn(Vector3 position, Quaternion rotation)
    // {
    //     var entry = _pool.Get();
    //     var trans = entry.transform;
    //     trans.position = position;
    //     trans.rotation = rotation;
    //     trans.SetParent(null);
    //
    //     if (!entry.activeSelf)
    //     {
    //         Debug.LogWarning("spawn a non-active object", entry);
    //     }
    //
    //     return entry;
    // }
}

