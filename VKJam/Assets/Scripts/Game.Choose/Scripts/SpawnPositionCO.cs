using UnityEngine;
[CreateAssetMenu(fileName = "New Ingredient Spawn Positions with Coordinates", menuName = "IngredientSP-C", order = 56)]
public class SpawnPositionCO : SpawnPosition
{
    public Vector3[] positions;
    protected override void UpdatePositions()
    {
        positions = new Vector3[_isDeadly ? 5 : 4];
    }
    public override Vector3 ReturnPositions()
    {
        return positions[Random.Range(1, _isDeadly ? 5 : 4)];
    }
}