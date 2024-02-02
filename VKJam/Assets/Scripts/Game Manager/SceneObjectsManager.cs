using UnityEngine;
using Unity.Netcode;
using CartoonFX;
using TMPro;

public class SceneObjectsManager : MonoBehaviour
{
    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private GuesserPainting guesserPaint;
    [SerializeField] private Interactable painterBook;
    [SerializeField] private Bestiary bestiary;
    [SerializeField] private MoveCamera moveCamera;
    [SerializeField] private ChatView chatView;
    private Chat chat;

    [SerializeField] private ToggleBook toggleBook;

    [SerializeField] private GameObject guessMonsterStageUI;
    [SerializeField] private GameObject mainCards;
    [SerializeField] private GameObject paintUI;
    [SerializeField] private GameObject guesserUI;
    [SerializeField] private GameObject tokensSummary;
    [SerializeField] private GameObject gameSummary;
    [SerializeField] private GameObject bookHint;

    [SerializeField] private GameObject guesserPreRoundCanvas;
    [SerializeField] private string chooseCardText;
    private TextMeshProUGUI guesserPreRoundLabel;

    [Header("Fire")]
    [SerializeField] private CFXR_Effect fireParticle;

    private bool isFirstSetted = false;

    private void Awake()
    {
        chat = chatView.GetComponent<Chat>();
        guesserPreRoundLabel = guesserPreRoundCanvas.GetComponentInChildren<TextMeshProUGUI>();

        GameManager.Instance.OnGuessMonsterStageActivatedOnClient.AddListener(OnGuessMonsterStageActivated);
        GameManager.Instance.OnGameEnded.AddListener(OnGameEnded);
        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(OnPainterSetted);
        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(OnGuesserSetted);
        GameManager.Instance.OnIngredientSwitchedOnClient.AddListener(OnIngredientSwitched);
        GameManager.Instance.OnRoundStartedOnClient.AddListener(OnRoundStarted);
        GameManager.Instance.OnCardChoosedOnClient.AddListener(OnCardChoosed);

        if (!NetworkManager.Singleton.IsHost)
        {
            GameManager.Instance.Paint.OnNetworkSpawned.AddListener(() =>
            {
                foreach (GameObject obj in painterGameObjects)
                    obj.SetActive(false);
            });
        }
    }

    private void OnRoleSetted()
    {
        bestiary.gameObject.SetActive(false);
        tokensSummary.SetActive(false);
        moveCamera.SetActivity(true);

        GameManager.Instance.SceneMonster.SetActive(false);
        GameManager.Instance.CardManager.ResetMonsterSprite();
    }

    private void OnPainterSetted()
    {
        OnRoleSetted();

        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(true);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(false);

        mainCards.SetActive(true);

        chatView.ShowPainterObjects();
        chat.Disable();
        //GameManager.Instance.Paint.ClearCanvas();
        GameManager.Instance.Paint.Enable(true);
    }

    private void OnGuesserSetted()
    {
        OnRoleSetted();

        if (isFirstSetted || NetworkManager.Singleton.IsServer)
        {
            foreach (GameObject obj in painterGameObjects)
                obj.SetActive(false);
        }

        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(true);

        chatView.HidePainterObjects();
        chat.Enable();
        painterBook.SetInteractable(true);
        // guesserPaint.gameObject.SetActive(true);
        guesserPreRoundCanvas.SetActive(true);
        guesserPreRoundLabel.text = $"{PlayersDataManager.Instance.PlayerDatas[GameManager.Instance.PainterId].Name} {chooseCardText}";
        GameManager.Instance.Paint.Disable();
        GameManager.Instance.CardManager.enabled = false;

        isFirstSetted = true;
    }

    private void OnCardChoosed(CardSO cardSO)
    {
        guesserPreRoundCanvas.SetActive(false);

        toggleBook.enabled = true;
        bookHint.SetActive(true);
    }

    private void OnIngredientSwitched(int ingredientIndex)
    {
        if (GameManager.Instance.Paint.gameObject.activeInHierarchy)
            fireParticle.gameObject.SetActive(true);

        GameManager.Instance.SoundList.Play("burn");
    }

    private void OnGuessMonsterStageActivated(bool isPainter)
    {
        if (isPainter)
        {
            toggleBook.enabled = false;
            bookHint.SetActive(false);

            guessMonsterStageUI.SetActive(true);
            painterBook.SetInteractable(false);
            paintUI.SetActive(false);
            moveCamera.SetActivity(false);
        }
        else
        {
            // guesserPaint.gameObject.SetActive(false);

            //bestiary.gameObject.SetActive(true);
            //guesserUI.SetActive(false);
        }
    }

    public void OnRoundEnded()
    {
        toggleBook.enabled = false;
        bookHint.SetActive(false);

        guesserUI.SetActive(false);
        guessMonsterStageUI.SetActive(false);

        tokensSummary.SetActive(true);
        
        painterBook.SetInteractable(false);
        paintUI.SetActive(false);

        bestiary.Close();

        chatView.Close();

        //GameManager.Instance.SceneMonster.SetActive(true);

        //moveCamera.SetActivity(false);
    }

    private void OnRoundStarted()
    {
        toggleBook.enabled = false;
        bookHint.SetActive(false);
    }

    private void OnGameEnded()
    {
        //gameSummary.SetActive(true);
    }
}