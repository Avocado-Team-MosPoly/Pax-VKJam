using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowRecepiesUI : MonoBehaviour
{
    //[SerializeField] private GameObject Recept;
    //[SerializeField] private Animation anim;
    //[SerializeField] private GameObject animGameObject;

    private void Start()
    {
        //anim = animGameObject.GetComponent<Animation>();
    }

    public void SpawnReceptOld()
    {
        if (Cards.activeIngridients.Count>0)
        {
            int rundomNumber = Random.Range(0, Cards.activeIngridients.Count - 1);
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Cards.activeIngridients[rundomNumber];
            Cards.activeIngridients.Remove(Cards.activeIngridients[rundomNumber]);
        }
        else
        {

        }
    }
    public void SetRecepi(string text)
    {
        Debug.Log("" + text);
        gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        //anim.Play("что то");
    }
    public void HideRecepi()
    {
        gameObject.SetActive(false);
    }
}
