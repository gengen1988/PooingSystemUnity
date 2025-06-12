using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem2 : MonoBehaviour, IPoolingSystem
{
    private readonly Dictionary<int, ObjectPoolSystem2> _poolByPrefab = new();

    public GameObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent)
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

        // notify gameplay (such as Explode)
        poolingData.OnSpawn();

        return poolingData.CurrentHandle;
    }

    public void RegisterSceneObject(IPoolingData dataComponent)
    {
        if (dataComponent is PoolingDataSystem2 poolingSystem2Data)
        {
            Debug.Log($"register {poolingSystem2Data} as SceneObjectHandle", poolingSystem2Data);
            var handle = new SceneGameObjectHandle(poolingSystem2Data.gameObject);
            poolingSystem2Data.CurrentHandle = handle;
            poolingSystem2Data.OnSpawn();
        }
        else
        {
            Debug.LogWarning("invalid data component type");
        }
    }

    private ObjectPoolSystem2 GetOrAddPool(GameObject prefab)
    {
        var poolID = prefab.GetInstanceID();
        var pool = _poolByPrefab.GetValueOrDefault(poolID);
        if (!pool)
        {
            var poolObject = new GameObject($"Pool ({prefab.name})");
            pool = poolObject.AddComponent<ObjectPoolSystem2>();
            pool.Init(prefab);
            _poolByPrefab.Add(poolID, pool);
        }

        return pool;
    }
}