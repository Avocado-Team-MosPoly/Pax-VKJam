using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text[] texts;
    public GameObject underlineAnimationPrefab;

    private GameObject[] underlineAnimations;

    private void Start()
    {
        // Создаем префаб анимации подчеркивания для каждого текстового объекта
        underlineAnimations = new GameObject[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            underlineAnimations[i] = Instantiate(underlineAnimationPrefab, texts[i].transform);
            underlineAnimations[i].SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Находим индекс текстового объекта, на который наведена мышь
        int index = System.Array.IndexOf(texts, eventData.pointerEnter.GetComponent<TMP_Text>());
        if (index >= 0)
        {
            // Активируем анимацию подчеркивания только для соответствующего текстового объекта
            underlineAnimations[index].SetActive(true);
            underlineAnimations[index].GetComponent<Animator>().Play("YourAnimationName");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Деактивируем все анимации подчеркивания при уходе курсора с текста
        for (int i = 0; i < underlineAnimations.Length; i++)
        {
            underlineAnimations[i].SetActive(false);
        }
    }
}