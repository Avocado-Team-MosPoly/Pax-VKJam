using UnityEngine;
using Unity.Netcode;
using System;
using CartoonFX;

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

    [SerializeField] private GameObject guessMonsterStageUI;
    [SerializeField] private GameObject mainCards;
    [SerializeField] private GameObject paintUI;
    [SerializeField] private GameObject guesserUI;
    [SerializeField] private GameObject tokensSummary;
    [SerializeField] private GameObject gameSummary;

    [Header("Fire")]
    [SerializeField] private CFXR_Effect fireParticle;

    private bool isFirstSetted = false;

    private void Awake()
    {
        chat = chatView.GetComponent<Chat>();

        GameManager.Instance.OnGuessMonsterStageActivatedOnClient.AddListener(OnGuessMonsterStageActivated);
        GameManager.Instance.OnGameEnded.AddListener(OnGameEnded);

        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(OnPainterSetted);
        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(OnGuesserSetted);
        GameManager.Instance.OnIngredientSwitchedOnClient.AddListener(OnIngredientSwitched);

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
        //Logger.Instance.Log($"[{nameof(SceneObjectsManager)}] On Role Setted");

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

        chat.Disable();
        GameManager.Instance.Paint.ClearCanvas();
        GameManager.Instance.Paint.SetActive(true);
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

        chat.Enable();
        painterBook.SetInteractable(true);
        guesserPaint.gameObject.SetActive(true);
        
        GameManager.Instance.Paint.SetActive(false);
        GameManager.Instance.CardManager.enabled = false;

        isFirstSetted = true;
    }

    private void OnIngredientSwitched(int ingredientIndex)
    {
        if (GameManager.Instance.Paint.gameObject.activeInHierarchy)
            fireParticle.gameObject.SetActive(true);
    }

    private void OnGuessMonsterStageActivated(bool isPainter)
    {
        if (isPainter)
        {
            guessMonsterStageUI.SetActive(true);
            painterBook.SetInteractable(false);
            paintUI.SetActive(false);
            moveCamera.SetActivity(false);
        }
        else
        {
            guesserPaint.gameObject.SetActive(false);

            //bestiary.gameObject.SetActive(true);
            //guesserUI.SetActive(false);
        }
    }

    public void OnRoundEnded()
    {
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

    private void OnGameEnded()
    {
        //gameSummary.SetActive(true);
    }
}