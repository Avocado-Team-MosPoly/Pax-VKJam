using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary> Все методы должны выполняться только на сервере, с клиента можно вызывать [ CompareAnswer(string guess), CompareMonster(string guess) ] </summary>
public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private CardManager cardManager;
    [SerializeField] private int monstersPerGame = 4;

    // monsterId - текущий монстр, ingredients - его ингредиенты, currentIngredientIndex - индекс текущего ингредиента
    private string monsterId;
    private List<string> ingredients;
    private NetworkVariable<int> currentIngredientIndex = new();

    private NetworkVariable<ushort> painterId = new(ushort.MaxValue);
    private List<ulong> lastPainterIds = new();

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            LogWarning($"Two GameManagers on scene:\n{Instance}, {this}");
        
        if (cardManager == null)
            cardManager = FindObjectOfType<CardManager>();
    }

    //private void Start()
    //{
    //    StartCoroutine(Initialize());
    //}

    //private IEnumerator Initialize()
    //{
    //    for (; ; )
    //    {
    //        if (Cards.activeIngridients != null)
    //        {
    //            InitializeServerRpc();
    //            answer_monsterId = Cards.activeMonsterName;
    //            answer_ingredients = Cards.activeIngridients;

    //            break;
    //        }
    //        yield return new WaitForSeconds(1f);
    //    }
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void InitializeServerRpc()
    //{
    //    if (currentIngredient.Value > 0)
    //        return;

    //    currentIngredient.Value = 0;
    //}

    public override void OnNetworkSpawn()
    {
        painterId.OnValueChanged += PainterId_OnValueChanged;

        Log($"IsServer : {IsServer}");

        if (IsServer)
            painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
        else
            StartCoroutine(ChooseRole(painterId.Value));
    }

    private void PainterId_OnValueChanged(ushort previousValue, ushort newValue)
    {
        Log("PainterId_OnValueChanged");
        lastPainterIds.Add(newValue);
        StartCoroutine(ChooseRole(newValue));
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (Timer.Instance.NetworkTime.Value <= 0f)
            LoseRound();
    }

    private void StartGame()
    {

    }

    private void StartRound()
    {

    }

    private void SetPainter()
    {
        Log("Painter");
        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(true);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(false);
    }
    private void SetGuesser()
    {
        Log("Guesser");
        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(false);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(true);
    }

    // не работает [ NetworkManager.Singleton.LocalClientId ], с IEnumerator почему-то работает
    private IEnumerator ChooseRole(ushort clientId)
    {
        yield return new WaitForSeconds(0f);

        if (clientId == NetworkManager.Singleton.LocalClientId)
            SetPainter();
        else
            SetGuesser();
    }

    private void ChangeRoles()
    {
        foreach (ushort clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!lastPainterIds.Contains(clientId))
            {
                painterId.Value = clientId;
                return;
            }
        }

        ClearLastPaintersClientRpc();
        painterId.Value = (ushort)NetworkManager.ConnectedClientsIds[0];
    }

    [ClientRpc]
    private void ClearLastPaintersClientRpc()
    {
        lastPainterIds.Clear();
    }

    private void NextRound() { }

    private void ResetFields()
    {

        // обновление ингредиента
    }

    /// <summary>
    /// Угадали монстра
    /// </summary>
    private void WinRound()
    {
        Log("WinRound");

        TokensManager.AddTokens(1);
        ResetFields();
        //ChangeRoles();
        
        // сообщить игрокам о смен раунда
    }

    /// <summary>
    /// Не угадали монстра
    /// </summary>
    private void LoseRound()
    {
        Log("LoseRound");
        
        TokensManager.AddTokens(-1);
        ResetFields();
        //ChangeRoles();
        
        // сообщить игрокам о смен раунда
    }

    /// <summary>
    /// Конец игры (монстры закончились)
    /// </summary>
    private void EndGame()
    {
        // Сообщить всем игрокам о выигрыше
    }

    private void CorrectIngredientGuess()
    {
        Log("Correct guess");
        //WinRound();
    }

    private void WrongIngredientGuess()
    {
        Log("Wrong guess");
    }

    [ServerRpc]
    private void CompareIngredientServerRpc(FixedString32Bytes guess)
    {
        if (ingredients[currentIngredientIndex.Value] == guess.ToString().ToLower())
            CorrectIngredientGuess();
        else
            WrongIngredientGuess();
    }

    public void CompareIngredient(string guess)
    {
        FixedString32Bytes fixedGuess = new(guess);
        CompareIngredientServerRpc(fixedGuess);
    }

    [ServerRpc]
    private void CompareMonsterServerRpc(FixedString32Bytes guess)
    {
        if (monsterId == guess.ToString().ToLower())
            WinRound();
        else
            LoseRound();
    }

    public void CompareMonster(string guess)
    {
        FixedString32Bytes fixedGuess = new(guess);
        CompareMonsterServerRpc(fixedGuess);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMonsterServerRpc()
    {
        //ingredientAnswers = card.Ingredients;
    }

    private void SetMonster(Card card)
    {

    }

    private void Log(string message) => Debug.Log("[GameManager] " + message);
    private void LogWarning(string message) => Debug.LogWarning("[GameManager] " + message);
    private void LogError(string message) => Debug.LogError("[GameManager] " + message);
}