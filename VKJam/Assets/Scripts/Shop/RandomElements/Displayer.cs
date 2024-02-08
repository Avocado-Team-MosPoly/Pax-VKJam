using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Displayer : MonoBehaviour
{
    [SerializeField] private Image Picture;
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI Data;
    [SerializeField] private RandomType Type;

    private void OnEnable()
    {
        CurrencyCatcher.Executor.Refresh();
    }

    public void SetData(Sprite Pic,string Naming, string inputData)
    {
        if (Type == RandomType.Token)
        {
            Data.text = inputData;
        }
        else if (Type == RandomType.Custom)
        {
            Picture.sprite = Pic;
            Name.text = Naming;
        }
        else if (Type == RandomType.Card)
        {
            Picture.sprite = Pic;
            Name.text = Naming;
            Data.text = inputData;
        }
        else Debug.LogError("Uncorrect Type of Displayer - " + Type);
    }
    public void SetData(int inputData)
    {
        if (Type == RandomType.Token)
        {
            Data.text = inputData.ToString();
        }
        else Debug.LogError("Uncorrect Type of Displayer - " + Type + " or incorrect input Data");
    }
    public void SetData(CardSystem inputData)
    {
        if (Type == RandomType.Card)
        {
            Picture.sprite = Sprite.Create(inputData.Card.cardTexture, new Rect(0.0f, 0.0f, inputData.Card.cardTexture.width, inputData.Card.cardTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            Name.text = inputData.Card.Id;
            Data.text = inputData.CardIsInOwn ? "Повторка" : "Новая";
            inputData.CardIsInOwn = true;
        }
        else Debug.LogError("Uncorrect Type of Displayer - " + Type + " or incorrect input Data");
    }
    public void SetData(WareData inputData)
    {
        if (Type == RandomType.Custom)
        {
            Picture.sprite = inputData.icon;
            Name.text = inputData.Data.productName;
            inputData.Data.InOwn = true;
        }
        else Debug.LogError("Uncorrect Type of Displayer - " + Type + " or incorrect input Data");
    }
    public void SetData(Sprite Pic, string Naming)
    {
        if (Type == RandomType.Custom)
        {
            Picture.sprite = Pic;
            Name.text = Naming;
        }
        else Debug.LogError("Uncorrect Type of Displayer - " + Type + " or incorrect input Data");
    }
}
