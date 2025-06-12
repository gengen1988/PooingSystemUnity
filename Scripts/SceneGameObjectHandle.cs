using UnityEngine;

public class SceneGameObjectHandle : GameObjectHandle
{
    private GameObject _entry;

    public SceneGameObjectHandle(GameObject entry)
    {
        _entry = entry;
    }

    protected override bool IsValid()
    {
        return _entry;
    }

    protected override void DespawnUnsafe()
    {
        Object.Destroy(_entry);
        _entry = null;
    }

    protected override GameObject GetGameObjectUnsafe()
    {
        return _entry;
    }
}