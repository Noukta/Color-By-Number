using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Rate : MonoBehaviour
{
    public string email = "";
    public int minVisitedTimes = 5;
    public int maxVisitedTimes = 50;

    public GameObject dialogOverlay;
    public Transform InTr, OutTr;

    public GameObject askForRate, rateButton, sendButton;
    public Slider rateSlider;
    public InputField feedback;
    
    private int visitedTimes = 1;
    private string touchPhase;

    void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (PlayerPrefs.HasKey("visited_times"))
        {
            visitedTimes = PlayerPrefs.GetInt("visited_times");
            if (visitedTimes == minVisitedTimes)
                Open();
            else if (visitedTimes == maxVisitedTimes)
                visitedTimes = 0;

            PlayerPrefs.SetInt("visited_times", 1 + visitedTimes);
        }
        else
            PlayerPrefs.SetInt("visited_times", 1);
    }

    void Open()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        askForRate.SetActive(true);
        rateSlider.gameObject.SetActive(true);
        rateButton.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(false);
        feedback.gameObject.SetActive(false);
        dialogOverlay.SetActive(true);

        transform.GetChild(0).transform.position = OutTr.position;
        iTween.MoveTo(transform.GetChild(0).gameObject, InTr.position, 0.3f);
    }

    public void Close()
    {
        PlayerPrefs.SetInt("visited_times", 1);
        dialogOverlay.SetActive(false);
        iTween.MoveTo(transform.GetChild(0).gameObject, OutTr.position, 0.3f);

        Timer.Schedule(this, 0.3f, () =>
        {
            transform.GetChild(0).gameObject.SetActive(false);
        });
        Sound.instance.PlayButton();
    }

    public void RateChanged()
    {
        if (rateSlider.value>3)
        {
            rateSlider.gameObject.SetActive(false);
            rateButton.SetActive(true);
        }
        else
        {
            rateSlider.gameObject.SetActive(false);
            feedback.gameObject.SetActive(true);
            sendButton.SetActive(true);
        }
    }

    public void RateConfirmed()
    {
        Application.OpenURL("market://details?id=" + Application.identifier);
        transform.GetChild(0).gameObject.SetActive(false);
        dialogOverlay.SetActive(false);
    }
    
    public void SendFeedback()
    {
        string subject = Application.productName + " Feedback";
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + feedback.text);
        transform.GetChild(0).gameObject.SetActive(false);
        dialogOverlay.SetActive(false);
    }

    string MyEscapeURL(string URL)
    {
        return UnityWebRequest.EscapeURL(URL).Replace("+", "%20");
    }
}
