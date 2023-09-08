using UnityEngine;

public class SceneObjectsManager : MonoBehaviour
{
    [SerializeField] private GameObject[] painterGameObjects;
    [SerializeField] private GameObject[] guesserGameObjects;

    [SerializeField] private GuesserPainting guesserPaint;
    [SerializeField] private Interactable painterBook;
    [SerializeField] private Bestiary bestiary;

    [SerializeField] private GameObject guessMonsterStageUI;
    [SerializeField] private GameObject mainCards;
    [SerializeField] private GameObject guesserUI;
    [SerializeField] private GameObject tokensSummary;
    [SerializeField] private GameObject gameSummary;

    private void Start()
    {
        GameManager.Instance.OnGuessMonsterStageActivated.AddListener(OnGuessMonsterStageActivated);
        GameManager.Instance.RoleManager.OnPainterSetted.AddListener(OnPainterSetted);
        GameManager.Instance.RoleManager.OnGuesserSetted.AddListener(OnGuesserSetted);
    }

    private void OnRoleSetted()
    {
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

        foreach (GameObject obj in painterGameObjects)
            obj.SetActive(false);
        foreach (GameObject obj in guesserGameObjects)
            obj.SetActive(true);

        painterBook.SetInteractable(true);
        guesserPaint.gameObject.SetActive(true);
        
        GameManager.Instance.Paint.SetActive(false);
        GameManager.Instance.CardManager.enabled = false;
    }

    private void OnGuessMonsterStageActivated(bool isPainter)
    {
        if (isPainter)
        {
            guessMonsterStageUI.SetActive(true);
            painterBook.SetInteractable(false);
            // выводить догадки с разделением по игрокам
        }
        else
        {
            guesserPaint.gameObject.SetActive(false);

            bestiary.gameObject.SetActive(true);
            guesserUI.SetActive(false);
        }
    }
}