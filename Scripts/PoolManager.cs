using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private static PoolManager _instance;
    public static PoolManager Instance => _instance;

    [SerializeField] private bool registerSceneObjects = true;

    private IPoolingSystem _poolingSystem;
    private readonly List<GameObjectHandle> _pendingDespawn = new();

    protected virtual void Awake()
    {
        TryGetComponent(out _poolingSystem);
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

    public GameObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent)
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

    public void DespawnAtLateUpdate(GameObjectHandle handle)
    {
        if (!handle)
        {
            return;
        }

        _pendingDespawn.Add(handle);
    }

    private void RegisterSceneObjects()
    {
        foreach (var found in FindComponentsWithInterface<IPoolingData>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            _poolingSystem.RegisterSceneObject(found);
        }
    }

    private static IEnumerable<T> FindComponentsWithInterface<T>(FindObjectsInactive findObjectsInactive, FindObjectsSortMode sortMode)
        where T : class
    {
        var interfaceType = typeof(T);
        var components = FindObjectsByType<MonoBehaviour>(findObjectsInactive, sortMode);
        foreach (var component in components)
        {
            var componentType = component.GetType();
            if (interfaceType.IsAssignableFrom(componentType))
            {
                yield return component as T;
            }
        }
    }
}

public interface IPoolingSystem
{
    public GameObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent);
    public void RegisterSceneObject(IPoolingData dataComponent);
}

public interface IPoolingData
{
    public GameObjectHandle CurrentHandle { get; }
}