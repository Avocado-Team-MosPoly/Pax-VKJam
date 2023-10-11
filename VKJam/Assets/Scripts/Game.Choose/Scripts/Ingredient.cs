using UnityEngine;
[CreateAssetMenu(fileName = "New Ingredient", menuName = "Ingredient", order = 55)]
public class Ingredient : ScriptableObject
{
    public string Name { get { return _name; } private set { _name = value; } }
    [SerializeField] private string _name;
    public string Description { get { return _description; } private set { _description = value; } }
    [SerializeField] private string _description;
    public GameObject Model { get { return _model; } private set { _model = value; } }
    [SerializeField] private GameObject _model;
    public CardSO[] Monster { get { return _monster; } private set { _monster = value; } }
    [SerializeField] private CardSO[] _monster;

    [SerializeField] SpawnPosition Positions;
    public void SummonIngredient()
    {
        Instantiate(_model, Positions.ReturnPositions(), Quaternion.identity);
    }
    public GameObject SummonIngredient_Returnable()
    {
        return Instantiate(_model, Positions.ReturnPositions(), Quaternion.identity);
    }
}
