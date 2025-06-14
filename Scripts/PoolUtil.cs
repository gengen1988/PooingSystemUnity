using UnityEngine;

public static class PoolUtil
{
    public static PoolHandle Spawn(GameObject prefab, Transform parent = null)
    {
        if (!prefab)
        {
            return default;
        }

        var prefabTrans = prefab.transform;
        return PoolManager.Instance.Spawn(prefab.gameObject, prefabTrans.localPosition, prefabTrans.localRotation, parent);
    }

    public static PoolHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent = null)
    {
        if (!prefab)
        {
            return default;
        }

        return PoolManager.Instance.Spawn(prefab.gameObject, localPosition, localRotation, parent);
    }

    public static void Despawn(PoolHandle handle)
    {
        if (!handle)
        {
            return;
        }

        PoolManager.Instance.DespawnAtLateUpdate(handle);
    }

    public static PoolHandle GetHandle(this GameObject gameObject, bool findInParent = false)
    {
        if (!gameObject)
        {
            return PoolHandle.Undefined;
        }

        var dataComponent = findInParent ? gameObject.GetComponentInParent<PoolingData3>() : gameObject.GetComponent<PoolingData3>();
        if (!dataComponent)
        {
            return PoolHandle.Undefined;
        }

        return dataComponent.CurrentHandle;
    }

    public static RefHandle<T> CreateRef<T>(this PoolHandle baseHandle, T toBeRef) where T : class
    {
        return new RefHandle<T>(toBeRef, baseHandle);
    }

    public static T Get<T>(this PoolHandle handle) where T : class
    {
        if (!handle)
        {
            return null;
        }

        if (!handle.Root.TryGetComponent(out T component))
        {
            return null;
        }

        return component;
    }
}