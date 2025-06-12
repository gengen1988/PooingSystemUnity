using System;
using UnityEngine;

public class PoolingData3 : MonoBehaviour
{
    public event Action OnDespawn;

    private ISpawnEffect[] _spawnEffects;

    public long CurrentStamp { get; set; }
    public ObjectPool3 SourcePool { get; set; }
    public PoolHandle CurrentHandle => new(this);

    public void Awake()
    {
        _spawnEffects = gameObject.GetComponents<ISpawnEffect>();
    }

    public void OnSpawn()
    {
        foreach (var effect in _spawnEffects)
        {
            effect.OnSpawn(CurrentHandle);
        }
    }

    public void Despawn(long stamp)
    {
        if (stamp == CurrentStamp)
        {
            OnDespawn?.Invoke();
            OnDespawn = null;

            if (SourcePool)
            {
                SourcePool.Release(this);
            }
            else
            {
                // If it's a scene object without a pool, just destroy it.
                Destroy(gameObject);
            }
        }
    }
}