using System;
using UnityEngine;

public readonly struct PoolHandle : IEquatable<PoolHandle>
{
    private readonly long _stamp;
    private readonly PoolingData3 _poolingData;

    public GameObject Value => IsValid() ? _poolingData.gameObject : null;

    public PoolHandle(PoolingData3 poolingData)
    {
        _stamp = poolingData.CurrentStamp;
        _poolingData = poolingData;
    }

    public void DespawnImmediate()
    {
        _poolingData?.Despawn(_stamp);
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

    public bool IsUndefined() => _poolingData is null;

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