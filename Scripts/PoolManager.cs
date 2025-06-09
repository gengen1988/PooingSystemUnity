using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // Singleton implementation
    private static PoolManager _instance;
    public static PoolManager Instance => _instance;

    private IPoolingSystem _poolingSystem;
    private readonly List<PooledObjectHandle> _pendingDespawn = new();

    protected virtual void Awake()
    {
        if (_instance)
        {
            Debug.LogAssertion($"{typeof(PoolManager)} has more than one instances", this);
        }

        _instance = this;
        Debug.Log($"[{typeof(PoolManager)}] registered as Singleton.", this);

        TryGetComponent(out _poolingSystem);
    }

    private void LateUpdate()
    {
        for (var i = _pendingDespawn.Count - 1; i >= 0; i--)
        {
            var handle = _pendingDespawn[i];
            if (handle)
            {
                handle.DespawnImmediate();
            }

            // swap and remove
            var lastIndex = _pendingDespawn.Count - 1;
            _pendingDespawn[i] = _pendingDespawn[lastIndex];
            _pendingDespawn.RemoveAt(lastIndex);
        }
    }

    public PooledObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent)
    {
        if (!prefab)
        {
            Debug.LogWarning("PoolManager: Attempting to spawn a null prefab.");
            return null;
        }

        if (_poolingSystem == null)
        {
            Debug.LogError("PoolManager: Pooling system is not initialized.");
            return null;
        }

        return _poolingSystem.Spawn(prefab, localPosition, localRotation, parent);
    }

    public PooledObjectHandle GetPoolingHandle(GameObject instance)
    {
        if (!instance)
        {
            return null;
        }

        if (_poolingSystem == null)
        {
            Debug.LogError("PoolManager: Pooling system is not initialized.");
            return null;
        }

        return _poolingSystem.GetPoolingHandle(instance);
    }

    public PooledObjectHandle GetPoolingHandleInParent(GameObject instance)
    {
        if (!instance)
        {
            return null;
        }

        if (_poolingSystem == null)
        {
            Debug.LogError("PoolManager: Pooling system is not initialized.");
            return null;
        }

        return _poolingSystem.GetPoolingHandleInParent(instance);
    }

    public void DespawnAtLateUpdate(PooledObjectHandle handle)
    {
        if (!handle)
        {
            return;
        }

        _pendingDespawn.Add(handle);
    }
}

public interface IPoolingSystem
{
    public PooledObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent);
    public PooledObjectHandle GetPoolingHandle(GameObject from);
    public PooledObjectHandle GetPoolingHandleInParent(GameObject from);
}