using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private static PoolManager _instance;
    public static PoolManager Instance => _instance;

    [SerializeField] private bool registerSceneObjects = true;

    private readonly List<PoolHandle> _pendingDespawn = new();
    private readonly Dictionary<int, ObjectPool3> _poolByPrefab = new();

    protected virtual void Awake()
    {
        if (registerSceneObjects)
        {
            RegisterSceneObjects();
        }

        _instance = this;
        Debug.Log($"[{typeof(PoolManager)}] registered as Singleton.", this);
    }

    private void LateUpdate()
    {
        for (var i = _pendingDespawn.Count - 1; i >= 0; i--)
        {
            var handle = _pendingDespawn[i];
            handle.DespawnImmediate();

            // swap and remove
            var lastIndex = _pendingDespawn.Count - 1;
            _pendingDespawn[i] = _pendingDespawn[lastIndex];
            _pendingDespawn.RemoveAt(lastIndex);
        }
    }

    public void DespawnAtLateUpdate(PoolHandle handle)
    {
        _pendingDespawn.Add(handle);
    }

    public PoolHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent)
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

        // distribute new stamp and notify gameplay (such as Explode)
        poolingData.Spawn();
        return poolingData.CurrentHandle;
    }

    private void RegisterSceneObjects()
    {
        foreach (var found in FindObjectsByType<PoolingData3>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            found.Spawn();
        }
    }

    private ObjectPool3 GetOrAddPool(GameObject prefab)
    {
        var poolID = prefab.GetInstanceID();
        var pool = _poolByPrefab.GetValueOrDefault(poolID);
        if (!pool)
        {
            var poolObject = new GameObject($"Pool ({prefab.name})");
            pool = poolObject.AddComponent<ObjectPool3>();
            pool.Init(prefab);
            _poolByPrefab.Add(poolID, pool);
        }

        return pool;
    }
}