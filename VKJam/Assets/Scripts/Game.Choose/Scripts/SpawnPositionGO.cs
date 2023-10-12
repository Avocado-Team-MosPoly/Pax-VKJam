using UnityEngine;
[CreateAssetMenu(fileName = "New Ingredient Spawn Positions with Gameobjects", menuName = "IngredientSP-G", order = 57)]
public class SpawnPositionGO : SpawnPosition
{
    public Transform[] positions;
    protected override void UpdatePositions()
    {
        positions = new Transform[_isMurderously ? 5 : 4];
    }
    public override Vector3 ReturnPositions()
    {
        return positions[Random.Range(1, _isMurderously ? 5 : 4)].position;
    }
}