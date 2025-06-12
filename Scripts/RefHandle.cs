using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// baseHandle 是 null 是正常的，即找到的对象不是从池中分配的
/// 此时 InValid 变成了对 Unity 指针的检查，用于处理 interface 引用 Component 的情况
public readonly struct RefHandle<T> : IEquatable<RefHandle<T>> where T : class
{
    private readonly T _value;
    private readonly GameObjectHandle _baseHandle;

    public T Value => IsValid() ? _value : null;
    public GameObjectHandle BaseHandle => _baseHandle;

    public RefHandle(T value, GameObjectHandle baseHandle)
    {
        _value = value;
        _baseHandle = baseHandle;
    }

    private bool IsValid()
    {
        if (_baseHandle is not null)
        {
            return _baseHandle && IsValueValid();
        }
        else
        {
            return IsValueValid();
        }
    }

    private bool IsValueValid()
    {
        if (_value is Object unityObject)
        {
            return unityObject; // 悬垂指针检查，即测试指针指向的物体是否被外部 destroy
        }
        else
        {
            return _value is not null;
        }
    }

    public static implicit operator bool(RefHandle<T> exists)
    {
        return exists.IsValid();
    }

    public bool Equals(RefHandle<T> other)
    {
        return EqualityComparer<T>.Default.Equals(_value, other._value) && Equals(_baseHandle, other._baseHandle);
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