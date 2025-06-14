using System;
using UnityEngine;

public readonly struct PoolHandle : IEquatable<PoolHandle>
{
    private readonly long _stamp;
    private readonly PoolingData3 _poolingData;
    public static PoolHandle Undefined = default;

    public bool IsUndefined() => _poolingData is null;
    public GameObject Root => IsValid() ? _poolingData.gameObject : null;

    public PoolHandle(PoolingData3 poolingData, long stamp)
    {
        _poolingData = poolingData;
        _stamp = stamp;
    }

    public void DespawnImmediate()
    {
        if (IsValid())
        {
            _poolingData.Despawn();
        }
    }

    public void RegisterOnDespawn(Action callback)
    {
        if (IsValid())
        {
            _poolingData.OnDespawn += callback;
        }
    }

    private bool IsValid()
    {
        return _poolingData && _stamp == _poolingData.CurrentStamp;
    }

    public static implicit operator bool(PoolHandle exists) => exists.IsValid();

    public bool Equals(PoolHandle other)
    {
        return _stamp == other._stamp && Equals(_poolingData, other._poolingData);
    }

    public override bool Equals(object obj)
    {
        return obj is PoolHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_stamp, _poolingData);
    }
}