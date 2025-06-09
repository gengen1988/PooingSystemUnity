using UnityEngine;
using UnityEngine.Pool;

/// this is a pool that use inactive parent to recycle
public class ObjectPoolForPoolingSystem2 : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private bool collectionCheck;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 10000;

    private ObjectPool<PoolingSystem2DataComponent> _pool; // or LinkedPool for other behavior
    private Transform _stagingTrans;

    private PoolingSystem2DataComponent OnCreateEntry()
    {
        var go = Instantiate(prefab, _stagingTrans);
        if (!go.TryGetComponent(out PoolingSystem2DataComponent poolingData))
        {
            poolingData = go.AddComponent<PoolingSystem2DataComponent>();
            poolingData.hideFlags = HideFlags.HideAndDontSave;
        }

        return poolingData;
    }

    private void OnGetEntry(PoolingSystem2DataComponent entry)
    {
        var handle = new PoolingHandle2(entry, this);
        entry.CurrentHandle = handle;
        // entry.PoolingReset();
    }

    private void OnReleaseEntry(PoolingSystem2DataComponent entry)
    {
        entry.transform.SetParent(_stagingTrans, false); // do not calc transform changes
        entry.CurrentHandle = null;
    }

    private void OnDestroyEntry(PoolingSystem2DataComponent entry)
    {
        if (!entry)
        {
            // exit play mode
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(entry.gameObject);
        }
        else
        {
            Debug.LogWarning("usually not go through this path, destroying pooled object in editor?");
            DestroyImmediate(entry.gameObject);
        }
    }

    public void Init(GameObject newPrefab)
    {
        prefab = newPrefab;
        _stagingTrans = transform;
        _stagingTrans.gameObject.SetActive(false);
        _pool = new ObjectPool<PoolingSystem2DataComponent>(
            OnCreateEntry,
            OnGetEntry,
            OnReleaseEntry,
            OnDestroyEntry,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    public PoolingSystem2DataComponent Get()
    {
        return _pool.Get();
    }

    public void Release(PoolingSystem2DataComponent entry)
    {
        _pool.Release(entry);
    }
}

public class PoolingHandle2 : PooledObjectHandle
{
    private PoolingSystem2DataComponent _entry;
    private ObjectPoolForPoolingSystem2 _pool;

    public PoolingHandle2(PoolingSystem2DataComponent entry, ObjectPoolForPoolingSystem2 pool)
    {
        _entry = entry;
        _pool = pool;
    }

    public override void DespawnImmediate()
    {
        _pool.Release(_entry);
        _entry = null;
        _pool = null;
    }

    public override bool IsValid()
    {
        return _entry && _pool && _entry.CurrentHandle == this;
    }

    public override GameObject GameObject => IsValid() ? _entry.gameObject : null;
}