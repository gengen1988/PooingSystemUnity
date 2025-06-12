using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public readonly struct RefHandle<T> : IEquatable<RefHandle<T>> where T : class
{
    private readonly T _value;
    private readonly PoolHandle _baseHandle;

    public T Value => IsValid() ? _value : null;

    public PoolHandle BaseHandle => _baseHandle;

    public RefHandle(T value, PoolHandle baseHandle)
    {
        _value = value;
        _baseHandle = baseHandle;
    }

    private bool IsValid()
    {
        if (_baseHandle.IsUndefined())
        {
            return IsValidValue();
        }
        else
        {
            return _baseHandle && IsValidValue();
        }
    }

    private bool IsValidValue()
    {
        if (_value is Object unityObject)
        {
            return unityObject;
        }
        else
        {
            return _value != null;
        }
    }

    public static implicit operator bool(RefHandle<T> exists) => exists.IsValid();

    public bool Equals(RefHandle<T> other)
    {
        return EqualityComparer<T>.Default.Equals(_value, other._value) && _baseHandle.Equals(other._baseHandle);
    }

    public override bool Equals(object obj)
    {
        return obj is RefHandle<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, _baseHandle);
    }
}