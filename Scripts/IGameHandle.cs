using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class PooledObjectHandle
{
    public abstract GameObject GameObject { get; }
    public abstract void DespawnImmediate();
    public abstract bool IsValid();

    public static implicit operator bool(PooledObjectHandle exists)
    {
        return exists != null && exists.IsValid();
    }
}

public readonly struct PooledRef<T> : IEquatable<PooledRef<T>> where T : Object
{
    private readonly T _value;
    private readonly PooledObjectHandle _baseHandle;

    public T Value => IsValid() ? _value : null;
    // public PooledObjectHandle BaseHandle => _baseHandle;  // should not visit, if this happens, build a domain handle instead

    public PooledRef(T value, PooledObjectHandle baseHandle)
    {
        _value = value;
        _baseHandle = baseHandle;
    }

    public bool IsValid()
    {
        if (_baseHandle != null)
        {
            return _baseHandle.IsValid() && _value;
        }
        else
        {
            return _value;
        }
    }

    public static implicit operator bool(PooledRef<T> exists)
    {
        return exists.IsValid();
    }

    public bool Equals(PooledRef<T> other)
    {
        return Equals(_baseHandle, other._baseHandle) && EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object obj)
    {
        return obj is PooledRef<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_baseHandle, _value);
    }
}