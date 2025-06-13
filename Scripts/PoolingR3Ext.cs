using System;

public static class PoolingR3Ext
{
    public static void AddTo(this IDisposable disposable, PoolHandle objectHandle)
    {
        objectHandle.RegisterOnDespawn(disposable.Dispose);
    }
}