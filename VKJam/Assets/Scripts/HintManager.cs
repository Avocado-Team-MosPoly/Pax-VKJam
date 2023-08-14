using UnityEngine;

public enum HintType
{
    Notebook,
    Hand
}

public class HintManager : MonoBehaviour
{
    [SerializeField] private Hint notebookHint;
    [SerializeField] private Hint handHint;

    [SerializeField] private NotebookBounds notebookBounds;
    
    private Hint currentHint;

    private string hintText;

    private void Awake()
    {
        notebookBounds.OnEnabled.AddListener( () =>
        {
            if (currentHint != null)
            {
                currentHint.gameObject.SetActive(false);
            }
        });
    }

    private void UpdateHint()
    {
        if (currentHint != null)
            currentHint.SetData(hintText);
    }

    public void SetHintData(string ingredientName)
    {
        hintText = ingredientName;
        UpdateHint();
    }

    public void ActivateHint()
    {
        //if (currentHint != null)
        //    currentHint.gameObject.SetActive(false);


        //if (hintType == HintType.Notebook)
        //{
        //    currentHint = handHint;
        //    notebookBounds.SetHoldHintTexture();
        //}
        //else
        //{
        //    currentHint = notebookHint;
        //    notebookBounds.SetDefaultTexture();
        //}

        //currentHint.gameObject.SetActive(true);
        //UpdateHint();
    }

    public void DeactivateHint()
    {
        if (currentHint == null)
            return;

        currentHint.gameObject.SetActive(false);
        currentHint = null;
    }
}