using System;
using UnityEngine;

[CreateAssetMenu()]
public class GameConfigSO : ScriptableObject
{
    [Header("Timer (time in seconds)")]
    public int TimeForIngredientGuess = 45;
    public int TimeForMonsterGuess = 120;

    [Header("Team Mode - TM, CompetitiveMode - CM" +
            "\nDangerous Monster - DM, Murderous Monster - MM" +
            "\nGuesser - G, Plainter - P")]

    [Header("Team Mode")]

    public SetOf<int> BonusIngredientGuessed_TM = new(2, 1, 1);
    
    public SetOf<int> BonusIfAllIngredientsGuessed_TM_DM = new(1, 1, 1);
    public SetOf<int> BonusIfAllIngredientsGuessed_TM_MM = new(2, 2, 2);

    public SetOf<int> BonusIfMonsterGuessed_TM_DM = new(4, 6, 8);
    public SetOf<int> BonusIfMonsterGuessed_TM_MM = new(6, 9, 12);

    public SetOf<int> BonusIfMonsterGuessedMoreThanOnePlayer_TM = new(0, 3, 4);

    [Space(10)]

    public float TokensMultiplyerIfRoundLosed_TM = 0.4f;

    [Header("Competitive Mode")]
    [Header("Guesser")]

    public SetOf<int> BonusForIngredient_CM_G = new(1, 1, 1);
    
    public SetOf<int> BonusIfAllIngredientsGuessed_CM_DM_G = new(1, 1, 1);
    public SetOf<int> BonusIfAllIngredientsGuessed_CM_MM_G = new(2, 2, 2);

    public SetOf<int> BonusIfMonsterGuessed_CM_DM_G = new(4, 4, 4);
    public SetOf<int> BonusIfMonsterGuessed_CM_MM_G = new(6, 6, 6);

    public SetOf<int> PenaltyIfMonsterIsNotGuessed_CM_G = new(4, 4, 4);

    [Header("Painter")]

    public SetOf<int> BonusForIngredient_CM_P = new(1, 1, 1);

    public SetOf<int> BonusIfAllIngredientsGuessed_CM_DM_P = new(1, 1, 1);
    public SetOf<int> BonusIfAllIngredientsGuessed_CM_MM_P = new(2, 2, 2);

    public SetOf<int> BonusIfMonsterGuessed_CM_DM_P = new(4, 4, 4);
    public SetOf<int> BonusIfMonsterGuessed_CM_MM_P = new(6, 6, 6);

    public SetOf<int> BonusIfMonsterGuessedMoreThanOnePlayer_CM_P = new(0, 2, 3);

    public SetOf<int> PenaltyIfMonsterIsNotGuessed_CM_P = new(5, 3, 2);

    [Serializable]
    public struct SetOf<T>
    {
        public T Players_2 => players_2;
        public T Players_3 => players_3;
        public T Players_4 => players_4;

        [SerializeField] private T players_2;
        [SerializeField] private T players_3;
        [SerializeField] private T players_4;

        public SetOf(T p2, T p3, T p4)
        {
            players_2 = p2;
            players_3 = p3;
            players_4 = p4;
        }

        public T GetValue(int playersCount)
        {
            switch (playersCount)
            {
                case 2:
                    return Players_2;
                case 3:
                    return Players_3;
                case 4:
                    return Players_4;
            }

            throw new ArgumentOutOfRangeException("PlayersCount count be less than 2 and more than 4.");
        }
    }
}