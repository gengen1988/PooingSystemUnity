using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolingData3 : MonoBehaviour
{
    private const long INVALID_STAMP = 0;
    private static long _nextStamp;

    public event Action OnDespawn;

    private long _currentStamp = INVALID_STAMP; // for inspector debug
    private bool _isQuitting;
    private ISpawnEffect[] _spawnEffects;

    public long CurrentStamp => _currentStamp;
    public PoolHandle CurrentHandle => new(this, _currentStamp);
    public ObjectPool3 SourcePool { get; set; }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void Awake()
    {
        _spawnEffects = gameObject.GetComponents<ISpawnEffect>();
    }

    private void OnEnable()
    {
        Spawn();
    }

    /// 当这个GameObject被Unity销毁时自动调用 (应对情况1)
    private void OnDestroy()
    {
        if (_currentStamp != INVALID_STAMP && !_isQuitting)
        {
            Debug.LogWarning($"Pooled object '{name}' was destroyed while active (Stamp: {_currentStamp}). " +
                             $"This will cause a pool leak. Was it parented to a non-pooled object that got destroyed? {StackTraceUtility.ExtractStackTrace()}",
                this);
        }
    }

    private void Spawn()
    {
        _currentStamp = ++_nextStamp;
        foreach (var effect in _spawnEffects)
        {
            effect.OnSpawn(CurrentHandle);
        }
    }

    public void Despawn()
    {
        // 严格检查：如果 handle 本身就是无效的，说明它从未被 Spawn 过。
        // 在这种情况下，“Despawn”是一个没有意义的操作。
        if (_currentStamp == INVALID_STAMP)
        {
            // 这是一个逻辑错误。我们不应该“猜测”开发者的意图是销毁它。
            // 我们应该通知开发者，他们的代码正在尝试 Despawn 一个无效的对象。
            // 使用 Debug.LogWarning 而不是 Assert，因为它在所有版本中都有效。
            Debug.LogWarning($"Attempted to Despawn an object ('{name}') with an invalid handle. " +
                             "The object was likely never spawned or already despawned. No action taken.", this);
            return;
        }

        // invalidate stamp immediately
        _currentStamp = INVALID_STAMP;

        // for registered callback (such as IDisposable.AddTo)
        OnDespawn?.Invoke();
        OnDespawn = null;

        // despawn children cascade
        foreach (var found in FindTopChildren(transform))
        {
            found.Despawn();
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

    private IEnumerable<PoolingData3> FindTopChildren(Transform root)
    {
        if (root != transform && root.TryGetComponent(out PoolingData3 comp))
        {
            yield return comp;
        }
        else
        {
            foreach (Transform child in root)
            {
                foreach (var found in FindTopChildren(child))
                {
                    yield return found;
                }
            }
        }
    }
}