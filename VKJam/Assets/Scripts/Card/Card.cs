using UnityEngine;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    [SerializeField] private int frontMaterialIndex = 1;
    [SerializeField] private string CameraBack_Buttons;

    // TODO: replace animator with DOTween
    private Animator animator;
    private MeshRenderer meshRenderer;

    private static Card firstSelectedCard;

    public UnityEvent<Card> OnSelect { get; private set; } = new();
    public UnityEvent<Card> OnChoose { get; private set; } = new();
    public BaseCardSO CardInfo { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void UpdateVisual()
    {
        gameObject.name = CardInfo.Id + " (Instance)";

        if (meshRenderer != null || TryGetComponent(out meshRenderer))
            meshRenderer.materials[frontMaterialIndex].mainTexture = CardInfo.CardTexture;
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

        GameManager.Instance.SoundList.Play("Flip");
    }

    public void SetCardInfo(BaseCardSO cardInfo)
    {
        CardInfo = cardInfo;
        UpdateVisual();
    }
}