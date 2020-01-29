using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FooterButton : MonoBehaviour {

    public Image icon;
    public Text text;
    public Sprite iconOn, iconOff;
    public Color colorOn, colorOff;

    public void SetActive(bool active)
    {
        icon.sprite = active ? iconOn : iconOff;
        text.color = active ? colorOn : colorOff;
    }

    public void OnClick()
    {
        GameController.instance.OnTabClick(transform.GetSiblingIndex());
    }
}
