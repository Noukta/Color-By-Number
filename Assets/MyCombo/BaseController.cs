using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BaseController : MonoBehaviour {
    public GameObject gameMaster;
    public string sceneName;

    private bool firstTime = true;

    protected virtual void Awake()
    {
        if (GameMaster.instance == null && gameMaster != null)
            Instantiate(gameMaster);
    }

    protected virtual void Start()
    {
        JobWorker.instance.onEnterScene?.Invoke(sceneName);
    }

    public virtual void OnApplicationPause(bool pause)
    {
        if (pause == false)
        {
            if(firstTime)
            {
                PlayerPrefs.DeleteKey("show_ads_time");
                CUtils.SetActionTime("show_ads_time");
                firstTime = false;
            }
            else
                Timer.Schedule(this, 0.1f, () =>
                {
                    CUtils.ShowInterstitialAd();
                });
        }
    }

    private IEnumerator SavePrefs()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            PlayerPrefs.Save();
        }
    }
}
