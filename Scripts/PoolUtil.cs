using UnityEngine;

public static class PoolUtil
{
    public static GameObjectHandle Spawn(GameObject prefab, Transform parent = null)
    {
        if (!prefab)
        {
            return null;
        }

        var prefabTrans = prefab.transform;
        return PoolManager.Instance.Spawn(prefab.gameObject, prefabTrans.localPosition, prefabTrans.localRotation, parent);
    }

    public static GameObjectHandle Spawn(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent = null)
    {
        if (!prefab)
        {
            return null;
        }

        return PoolManager.Instance.Spawn(prefab.gameObject, localPosition, localRotation, parent);
    }

    public static void Despawn(GameObjectHandle handle)
    {
        PoolManager.Instance.DespawnAtLateUpdate(handle);
    }

    public static GameObjectHandle GetHandle(this GameObject gameObject, bool findInParent = false)
    {
        var dataComponent = findInParent ? gameObject.GetComponentInParent<IPoolingData>() : gameObject.GetComponent<IPoolingData>();
        return dataComponent?.CurrentHandle;
    }
    
    public static RefHandle<T> CreateRef<T>(this GameObjectHandle baseHandle, T toBeRef) where T : class
    {
        return new RefHandle<T>(toBeRef, baseHandle);
    }

    public static T Get<T>(this GameObjectHandle handle) where T : class
    {
        if (!handle)
        {
            return null;
        }

        if (!handle.Value.TryGetComponent(out T component))
        {
            return null;
        }

        return component;
    }
}