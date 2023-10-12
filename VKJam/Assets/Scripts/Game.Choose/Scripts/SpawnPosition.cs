using UnityEngine;
public abstract class SpawnPosition : ScriptableObject
{
    [SerializeField] protected bool _isMurderously;
    public bool IsMurderously { get { return _isMurderously; } set { _isMurderously = value; UpdatePositions(); } }
    private void OnValidate() { UpdatePositions(); }
    protected abstract void UpdatePositions();
    public abstract Vector3 ReturnPositions();
}