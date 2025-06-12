using UnityEngine;
using UnityEngine.Pool;

/// this is a pool that use inactive parent to recycle
public class ObjectPoolSystem2 : MonoBehaviour
{
    private enum PoolType
    {
        Stack,
        LinkedList
    }

    [SerializeField] private PoolType poolType = PoolType.Stack;
    [SerializeField] private GameObject prefab;
    [SerializeField] private bool collectionCheck;
    [SerializeField] private int defaultCapacity = 10; // only for stack pool
    [SerializeField] private int maxSize = 10000;

    private IObjectPool<PoolingDataSystem2> _pool; // or LinkedPool for other behavior
    private Transform _stagingTrans;

    private PoolingDataSystem2 OnCreateEntry()
    {
        var go = Instantiate(prefab, _stagingTrans);
        if (!go.TryGetComponent(out PoolingDataSystem2 poolingData))
        {
            poolingData = go.AddComponent<PoolingDataSystem2>();
            poolingData.hideFlags = HideFlags.HideAndDontSave;
        }

        return poolingData;
    }

    private void OnGetEntry(PoolingDataSystem2 entry)
    {
        var handle = new PooledGameObjectHandleSystem2(entry, this);
        entry.CurrentHandle = handle;
        // entry.PoolingReset();
    }

    private void OnReleaseEntry(PoolingDataSystem2 entry)
    {
        entry.transform.SetParent(_stagingTrans, false); // do not calc transform changes
        entry.CurrentHandle = null;
    }

    private void OnDestroyEntry(PoolingDataSystem2 entry)
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

        switch (poolType)
        {
            case PoolType.Stack:
                _pool = new ObjectPool<PoolingDataSystem2>(
                    OnCreateEntry,
                    OnGetEntry,
                    OnReleaseEntry,
                    OnDestroyEntry,
                    collectionCheck,
                    defaultCapacity,
                    maxSize
                );
                break;
            case PoolType.LinkedList:
                _pool = new LinkedPool<PoolingDataSystem2>(
                    OnCreateEntry,
                    OnGetEntry,
                    OnReleaseEntry,
                    OnDestroyEntry,
                    collectionCheck,
                    // defaultCapacity,
                    maxSize
                );
                break;
        }
    }

    public PoolingDataSystem2 Get()
    {
        return _pool.Get();
    }

    public void Release(PoolingDataSystem2 entry)
    {
        _pool.Release(entry);
    }
}