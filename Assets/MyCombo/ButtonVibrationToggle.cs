using UnityEngine;
using System.Collections;

public class ButtonVibrationToggle : TButton {

    protected override void Start()
    {
        base.Start();
        IsOn = PlayerPrefs.GetInt("vibration_enabled", 0) == 1;
    }

    public override void OnButtonClick()
    {
        base.OnButtonClick();
        PlayerPrefs.SetInt("vibration_enabled", isOn ? 1 : 0);
    }
}
