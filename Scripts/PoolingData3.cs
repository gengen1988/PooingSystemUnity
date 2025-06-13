using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolingData3 : MonoBehaviour
{
    private const long INVALID_STAMP = 0;
    private static long _nextStamp;

    public event Action OnDespawn;

    private ISpawnEffect[] _spawnEffects;
    private bool _isQuitting;

    public long CurrentStamp { get; private set; } = INVALID_STAMP; // default is invalid
    public PoolHandle CurrentHandle => new(this, CurrentStamp);
    public ObjectPool3 SourcePool { get; set; }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void Awake()
    {
        _spawnEffects = gameObject.GetComponents<ISpawnEffect>();
    }

    // 当这个GameObject被Unity销毁时自动调用 (应对情况1)
    private void OnDestroy()
    {
        if (CurrentStamp != INVALID_STAMP && !_isQuitting)
        {
            Debug.LogWarning($"Pooled object '{name}' was destroyed while active (Stamp: {CurrentStamp}). " +
                             "This will cause a pool leak. Was it parented to a non-pooled object that got destroyed?", this);
        }
    }

    public void Spawn()
    {
        CurrentStamp = ++_nextStamp;
        foreach (var effect in _spawnEffects)
        {
            effect.OnSpawn(CurrentHandle);
        }
    }

    public void Despawn(long stamp)
    {
        // guard invalid handle
        if (stamp != CurrentStamp)
        {
            return;
        }

        // 严格检查：如果 handle 本身就是无效的，说明它从未被 Spawn 过。
        // 在这种情况下，“Despawn”是一个没有意义的操作。
        if (CurrentStamp == INVALID_STAMP)
        {
            // 这是一个逻辑错误。我们不应该“猜测”开发者的意图是销毁它。
            // 我们应该通知开发者，他们的代码正在尝试 Despawn 一个无效的对象。
            // 使用 Debug.LogWarning 而不是 Assert，因为它在所有版本中都有效。
            Debug.LogWarning($"Attempted to Despawn an object ('{name}') with an invalid handle. " +
                             "The object was likely never spawned or already despawned. No action taken.", this);
            return;
        }

        // invalidate stamp immediately
        CurrentStamp = INVALID_STAMP;

        // for registered callback (such as IDisposable.AddTo)
        OnDespawn?.Invoke();
        OnDespawn = null;

        // despawn children cascade
        foreach (Transform child in transform)
        {
            foreach (var found in FindTopComponents<PoolingData3>(child))
            {
                found.CurrentHandle.DespawnImmediate();
            }
        }

        // actual despawn
        if (SourcePool)
        {
            SourcePool.Release(this);
        }
        else
        {
            // If it's a registered scene object, just destroy it.
            Destroy(gameObject);
        }
    }

    private static IEnumerable<T> FindTopComponents<T>(Transform root) where T : class
    {
        if (root.TryGetComponent(out T comp))
        {
            yield return comp;
        }
        else
        {
            foreach (Transform child in root)
            {
                foreach (var found in FindTopComponents<T>(child))
                {
                    yield return found;
                }
            }
        }
    }
}