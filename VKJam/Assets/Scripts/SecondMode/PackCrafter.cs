using Unity.Netcode;
using UnityEngine;

public class PackCrafter : NetworkBehaviour
{
    public static PackCrafter Instance { get; private set; }

    private CardPackSO cardPackSO;
    private string previousCardPack;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"[{nameof(PackCrafter)}] Multiple instances on scene");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        GenerateCardPack();

        previousCardPack = PackManager.Instance.Active.PackName;
        PackManager.Instance.SetPack(cardPackSO);
    }

    public override void OnNetworkDespawn()
    {
        PackManager.Instance.SetPack(previousCardPack);
    }

    private void GenerateCardPack()
    {
        cardPackSO = ScriptableObject.CreateInstance<CardPackSO>();

        cardPackSO.PackName = "Second Mode Pack";
        cardPackSO.PackIsInOwn = true;
        cardPackSO.PackDBIndex = -1;
        cardPackSO.CardInPack = new CardSystem[NetworkManager.Singleton.ConnectedClientsIds.Count];
    }

    public void SendCardInfo(string monsterName, string description, string[] ingredients, Texture2D texture)
    {
        SecondModeCardSO cardInfo = new(monsterName, description, ingredients, texture);
        SendCardInfoServerRpc(cardInfo, new ServerRpcParams());
    }

    [ServerRpc]
    private void SendCardInfoServerRpc(SecondModeCardSO cardInfo, ServerRpcParams rpcParams)
    {
        cardInfo.AssignCreatorId(rpcParams.Receive.SenderClientId);
        AddCard(cardInfo);
        SendCardInfoClientRpc(cardInfo);
    }

    [ClientRpc]
    private void SendCardInfoClientRpc(SecondModeCardSO cardInfo)
    {
        if (IsServer)
            return;

        AddCard(cardInfo);
    }

    private void AddCard(SecondModeCardSO cardInfo)
    {
        //cardPackSO.CardInPack[cardInfo.CreatorId].Card = cardInfo;
        cardPackSO.CardInPack[cardInfo.CreatorId].CardIsInOwn = true;
        cardPackSO.CardInPack[cardInfo.CreatorId].CardDBIndex = -1;
    }
}