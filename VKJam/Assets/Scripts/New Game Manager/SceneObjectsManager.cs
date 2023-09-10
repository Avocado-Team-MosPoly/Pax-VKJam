using UnityEngine;
using Unity.Netcode;

public class SceneObjectsManager : MonoBehaviour
{
    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private GuesserPainting guesserPaint;
    [SerializeField] private Interactable painterBook;
    [SerializeField] private Bestiary bestiary;
    [SerializeField] private MoveCamera moveCamera;

    [SerializeField] private GameObject guessMonsterStageUI;
    [SerializeField] private GameObject mainCards;
    [SerializeField] private GameObject paintUI;
    [SerializeField] private GameObject guesserUI;
    [SerializeField] private GameObject tokensSummary;
    [SerializeField] private GameObject gameSummary;

    private bool isFirstSetted = false;

    private void Awake()
    {
        GameManager.Instance.OnGuessMonsterStageActivated.AddListener(OnGuessMonsterStageActivated);
        GameManager.Instance.OnGameEnded.AddListener(OnGameEnded);

        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(OnPainterSetted);
        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(OnGuesserSetted);

        if (!NetworkManager.Singleton.IsHost)
        {
            GameManager.Instance.Paint.OnNetworkSpawned.AddListener(() =>
            {
                foreach (GameObject obj in painterGameObjects)
                    obj.SetActive(false);
            });
        }
        //Card.OnChoose.AddListener((Card card) => moveCamera.transform.parent.GetComponent<Animator>().Play("CameraAnimBack"));
    }

    private void OnRoleSetted()
    {
        Debug.Log($"[{name}] OnRoleSetted");

        bestiary.gameObject.SetActive(false);
        tokensSummary.SetActive(false);

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
        
        GameManager.Instance.Paint.ClearCanvas();
        GameManager.Instance.Paint.SetActive(true);
    }

    private void OnGuesserSetted()
    {
        OnRoleSetted();

        if (isFirstSetted)
        {
            foreach (GameObject obj in painterGameObjects)
                obj.SetActive(false);
        }

        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(true);

        painterBook.SetInteractable(true);
        guesserPaint.gameObject.SetActive(true);
        
        GameManager.Instance.Paint.SetActive(false);
        GameManager.Instance.CardManager.enabled = false;

        isFirstSetted = true;
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
        GameManager.Instance.SceneMonster.gameObject.SetActive(true);
        moveCamera.SetActivity(false);
    }

    private void OnGameEnded()
    {
        //gameSummary.SetActive(true);
    }
}