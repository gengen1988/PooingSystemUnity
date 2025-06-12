using System;
using UnityEngine;

public abstract class GameObjectHandle
{
    public event Action OnDespawn;

    private bool _isDespawned;

    public GameObject Value => IsValid() ? GetGameObjectUnsafe() : null;

    public void DespawnImmediate()
    {
        if (_isDespawned)
        {
            throw new InvalidOperationException("Cannot despawn more than once.");
        }

        OnDespawn?.Invoke();
        OnDespawn = null;

        if (IsValid())
        {
            DespawnUnsafe();
        }
        else
        {
            Debug.LogWarning("despawn a invalid handle");
        }

        _isDespawned = true;
    }

    protected abstract bool IsValid();
    protected abstract void DespawnUnsafe();
    protected abstract GameObject GetGameObjectUnsafe();

    public static implicit operator bool(GameObjectHandle exists)
    {
        return exists != null && exists.IsValid();
    }
}