using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private TMP_Text[] texts;
    [SerializeField] private GameObject underlineAnimationPrefab;

    [SerializeField] private GameObject[] underlineAnimations;

    private int clicked=new();
    [SerializeField] private bool isLock;

    private void Awake()
    {
        if (texts.Length > 0)
            Initialize();

    }
    private void Start()
    {
        GameManager.Instance.OnRoundStartedOnClient.AddListener(() =>
        {
            Clear();
        });
    }

    private void Initialize()
    {
        foreach (GameObject go in underlineAnimations)
            Destroy(go);

        underlineAnimations = new GameObject[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            underlineAnimations[i] = Instantiate(underlineAnimationPrefab, texts[i].transform);
            underlineAnimations[i].SetActive(false);
        }
    }

    public void SetTexts(List<TMP_Text> listTexts)
    {
        texts = listTexts.ToArray();
        Initialize();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        int index = System.Array.IndexOf(texts, eventData.pointerEnter.GetComponent<TMP_Text>());

        if (index >= 0)
        {
            underlineAnimations[index].SetActive(true);
            Animator animator = underlineAnimations[index].GetComponent<Animator>();

            foreach (AnimationClip animationClip in animator.runtimeAnimatorController.animationClips)
            {
                if (animationClip.name == "OnMouseEnter")
                {
                    animator.Play("OnMouseEnter");
                    break;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        for (int i = 0; i < underlineAnimations.Length; i++)
        {
            if (clicked!=i)
            {
                underlineAnimations[i].SetActive(false);
            }
        }
    }
    public void Clear()
    {
    
        underlineAnimations[clicked].SetActive(false);
        clicked=-1;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        underlineAnimations[clicked].SetActive(false);

        int index = System.Array.IndexOf(texts, eventData.pointerEnter.GetComponent<TMP_Text>());

        clicked=index;
    }
}