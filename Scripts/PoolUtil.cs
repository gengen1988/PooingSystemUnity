using UnityEngine;

public static class PoolUtil
{
    public static PooledObjectHandle Spawn(GameObject prefab, Transform parent = null)
    {
        if (!prefab)
        {
            return null;
        }

        var prefabTrans = prefab.transform;
        return PoolManager.Instance.Spawn(prefab.gameObject, prefabTrans.localPosition, prefabTrans.localRotation, parent);
    }

    public static PooledObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent = null)
    {
        if (!prefab)
        {
            return null;
        }

        return PoolManager.Instance.Spawn(prefab.gameObject, localPosition, localRotation, parent);
    }

    public static void Despawn(PooledObjectHandle handle)
    {
        PoolManager.Instance.DespawnAtLateUpdate(handle);
    }

    public static void Despawn(GameObject instance, bool findInParent = false)
    {
        PooledObjectHandle handle;
        if (findInParent)
        {
            handle = PoolManager.Instance.GetPoolingHandleInParent(instance);
        }
        else
        {
            handle = PoolManager.Instance.GetPoolingHandle(instance);
        }

        if (!handle)
        {
            Debug.LogWarning($"not found pooling handle in {instance} (parent:{findInParent})", instance);
        }

        PoolManager.Instance.DespawnAtLateUpdate(handle);
    }

    public static PooledRef<T> CreateHandle<T>(this T component, bool baseInParent = false) where T : Component
    {
        PooledObjectHandle baseHandle;
        if (baseInParent)
        {
            baseHandle = PoolManager.Instance.GetPoolingHandleInParent(component.gameObject);
        }
        else
        {
            baseHandle = PoolManager.Instance.GetPoolingHandle(component.gameObject);
        }

        return new PooledRef<T>(component, baseHandle);
    }

    public static T Get<T>(this PooledObjectHandle handle) where T : Component
    {
        if (!handle)
        {
            return null;
        }

        if (!handle.GameObject.TryGetComponent(out T component))
        {
            return null;
        }

        return component;
    }
}