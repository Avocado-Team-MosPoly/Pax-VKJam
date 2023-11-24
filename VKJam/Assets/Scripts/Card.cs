using UnityEngine;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    [SerializeField] private int frontMaterialIndex = 1;

    public CardSO CardSO { get; private set; }

    public static UnityEvent<Card> OnSelect = new();
    public static UnityEvent<Card> OnChoose = new();

    private Animator animator;
    private MeshRenderer meshRenderer;
    [SerializeField] private string CameraBack_Buttons;

    private static Card firstSelectedCard;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void UpdateCardSO()
    {
        gameObject.name = CardSO.Id + " (Instance)";

        if (meshRenderer != null || TryGetComponent(out meshRenderer))
            meshRenderer.materials[frontMaterialIndex].mainTexture = CardSO.CardTexture;
    }

    public void Choose()
    {
        if (firstSelectedCard == null)
        {
            animator.Play("card-rotate");
            firstSelectedCard = this;
            OnSelect.Invoke(this);
        }
        else
        {
            GameObject.Find(CameraBack_Buttons).GetComponent<CameraBack>().back();
            OnChoose.Invoke(this);
        }
    }

    public void SetCardSO(CardSO cardSO)
    {
        CardSO = cardSO;
        UpdateCardSO();
    }
}