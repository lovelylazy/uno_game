using UnityEngine;
using UnityEngine.UI;
using TMPro; // 必须加，否则TextMeshProUGUI会报错

public class CardUI : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI cardNumberText;

    public void InitCard(Card card)
    {
        Debug.Log("InitCard called: " + card.color + " " + card.number);
        cardImage.color = GetColorFromEnum(card.color);
        cardNumberText.text = card.number.ToString();
    }

    private Color GetColorFromEnum(CardColor color)
    {
        switch (color)
        {
            case CardColor.Red: return Color.red;
            case CardColor.Blue: return Color.blue;
            case CardColor.Yellow: return Color.yellow;
            case CardColor.Green: return Color.green;
            default: return Color.white;
        }
    }
}