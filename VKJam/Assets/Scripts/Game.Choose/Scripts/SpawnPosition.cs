using UnityEngine;
public abstract class SpawnPosition : ScriptableObject
{
    [SerializeField] protected bool _isDeadly;
    public bool IsDeadly { get { return _isDeadly; } set { _isDeadly = value; UpdatePositions(); } }
    private void OnValidate() { UpdatePositions(); }
    protected abstract void UpdatePositions();
    public abstract Vector3 ReturnPositions();
}