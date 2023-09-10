using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private Hint notebookHint;
    [SerializeField] private Hint handHint;
    
    private string hintText;

    public void SetHintData(string ingredientName)
    {
        hintText = ingredientName;
        
        notebookHint.SetData(hintText);
        handHint.SetData(hintText);
    }

    private void EnableHandHint()
    {
        handHint.gameObject.SetActive(true);
    }

    public void DisableHandHint()
    {
        handHint.gameObject.SetActive(false);
    }

    public void InteractRecipeHand()
    {
        if (handHint.gameObject.activeInHierarchy)
            DisableHandHint();
        else if (GameManager.Instance.Paint.enabled == false)
            EnableHandHint();
    }
}