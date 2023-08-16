using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private Hint notebookHint;
    [SerializeField] private Hint handHint;
    
    private string hintText;

    public bool IsActiveHandHint => handHint.gameObject.activeInHierarchy;

    public void SetHintData(string ingredientName)
    {
        hintText = ingredientName;
        
        notebookHint.SetData(hintText);
        handHint.SetData(hintText);
    }

    public void EnableHandHint()
    {
        handHint.gameObject.SetActive(true);
    }

    public void DisableHandHint()
    {
        handHint.gameObject.SetActive(false);
    }
}