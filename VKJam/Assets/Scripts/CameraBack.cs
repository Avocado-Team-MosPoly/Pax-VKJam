using System.Collections.Generic;
using UnityEngine;

public class CameraBack : MonoBehaviour
{
    [SerializeField] private List<GameObject> setActiveFalseOnBack;
    [SerializeField] private List<GameObject> setActiveTrueOnBack;
    [SerializeField] private GameObject book;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject animationObject;
    [SerializeField] private string animationName;
    [SerializeField] private List<GameObject> setActiveFalseOnBackWithoutChosenCard;
    [SerializeField] private List<GameObject> setActiveTrueOnBackWithoutChosenCard;

    public void back()
    {
        for (int i = 0; i < setActiveFalseOnBack.Count; i++)
        {
            setActiveFalseOnBack[i].SetActive(false);
        }
        for (int i = 0; i < setActiveTrueOnBack.Count; i++)
        {
            setActiveTrueOnBack[i].SetActive(true);
        }
        book.GetComponent<Interactable>().SetInteractable(true);
        animationObject.GetComponent<AnimateList>().Play(animationName);
        mainCamera.GetComponent<MoveCamera>().SetActivity(true);
    }

    public void backWithoutCard()
    {
        for (int i = 0; i < setActiveFalseOnBackWithoutChosenCard.Count; i++)
        {
            setActiveFalseOnBackWithoutChosenCard[i].SetActive(false);
        }
        for (int i = 0; i < setActiveTrueOnBackWithoutChosenCard.Count; i++)
        {
            setActiveTrueOnBackWithoutChosenCard[i].SetActive(true);
        }
        book.GetComponent<Interactable>().SetInteractable(true);
        mainCamera.GetComponent<MoveCamera>().SetActivity(true);
        animationObject.GetComponent<AnimateList>().Play(animationName);
    }
}
