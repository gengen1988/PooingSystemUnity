using UnityEngine;

public class PoolingDataSystem2 : MonoBehaviour, IPoolingData
{
    public GameObjectHandle CurrentHandle { get; set; }

    private ISpawnEffect[] _spawnEffects;

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

    // such as scene changes caused destroying
    // intended do not return to pool
    // private void OnDestroy()
    // {
    //     if (CurrentHandle)
    //     {
    //         CurrentHandle.DespawnImmediate();
    //     }
    // }
}

public interface ISpawnEffect
{
    /// should after OnEnable
    public void OnSpawn(GameObjectHandle self);
}