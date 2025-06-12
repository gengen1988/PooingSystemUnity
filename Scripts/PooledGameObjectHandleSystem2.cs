using UnityEngine;

public class PooledGameObjectHandleSystem2 : GameObjectHandle
{
    private PoolingDataSystem2 _entry;
    private ObjectPoolSystem2 _pool;

    public PooledGameObjectHandleSystem2(PoolingDataSystem2 entry, ObjectPoolSystem2 pool)
    {
        _entry = entry;
        _pool = pool;
    }

    protected override void DespawnUnsafe()
    {
        if (!IsValid())
        {
            return;
        }

        _pool.Release(_entry);
        _entry = null;
        _pool = null;
    }

    protected override bool IsValid()
    {
        return _entry && _pool && _entry.CurrentHandle == this;
    }

    protected override GameObject GetGameObjectUnsafe()
    {
        return _entry.gameObject;
    }
}