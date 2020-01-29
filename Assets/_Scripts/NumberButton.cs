using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberButton : MonoBehaviour
{
    public Text numberText;
    public Image checkedIcon;
    public int number;

    [HideInInspector]
    public BottomButtons bottomButtons;
    [HideInInspector]
    public bool isComplete;
    [HideInInspector]
    public Color color;

    private void Start()
    {
        numberText.text = number.ToString();
        if (isComplete) CompletePainting();
    }

    public void SetColor(Color color)
    {
        this.color = color;
        GetComponent<Image>().color = color;
        numberText.color = color.grayscale < 0.5f ? Color.white : Color.black;
    }

    public void OnClick()
    {
        bottomButtons.SetSelectionPosition(number - 1);
        Board.instance.Highlight(number);
        Sound.instance.PlayButton();
    }

    public void CompletePainting()
    {
        GetComponent<Button>().interactable = false;
        numberText.gameObject.SetActive(false);
        checkedIcon.gameObject.SetActive(true);

        var rt = checkedIcon.GetComponent<RectTransform>();
        float buttonWidth = GetComponent<RectTransform>().rect.width;
        float checkWidth = buttonWidth * 0.38f;
        float checkHeight = checkWidth * rt.rect.height / rt.rect.width;

        rt.sizeDelta = new Vector2(checkWidth, checkHeight);
        checkedIcon.color = color.grayscale < 0.5f ? Color.white : Color.black;
        isComplete = true;
    }
}
