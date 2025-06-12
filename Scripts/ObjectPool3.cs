using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool3 : MonoBehaviour
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

    private IObjectPool<PoolingData3> _pool; // or LinkedPool for other behavior
    private Transform _stagingTrans;

    private PoolingData3 OnCreateEntry()
    {
        var go = Instantiate(prefab, _stagingTrans);
        if (!go.TryGetComponent(out PoolingData3 poolingData))
        {
            poolingData = go.AddComponent<PoolingData3>();
            poolingData.SourcePool = this;
            poolingData.hideFlags = HideFlags.HideAndDontSave;
        }

        return poolingData;
    }

    private void OnGetEntry(PoolingData3 entry)
    {
        // do nothing
    }

    private void OnReleaseEntry(PoolingData3 entry)
    {
        entry.transform.SetParent(_stagingTrans, false); // do not calc transform changes
    }

    private void OnDestroyEntry(PoolingData3 entry)
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
                _pool = new ObjectPool<PoolingData3>(
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
                _pool = new LinkedPool<PoolingData3>(
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

    public PoolingData3 Get()
    {
        return _pool.Get();
    }

    public void Release(PoolingData3 entry)
    {
        _pool.Release(entry);
    }
}