using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardedVideoButton : MonoBehaviour
{
    private const string ACTION_NAME = "rewarded_video";
    
    public void OnClick()
    {
        if (IsAvailableToShow())
        {
            //if (IsAdmobAvailable())
            //    AdmobController.instance.ShowRewardBasedVideo();
            //else
                UnityAdsController.instance.ShowRewardedVideo();
        }
        else if (!IsActionAvailable())
        {
            int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime(ACTION_NAME));
            Toast.instance.ShowMessage("Please wait " + remainTime + " seconds for the next ad");
        }
        else
        {
            Toast.instance.ShowMessage("Ad is not available now, please wait..");
        }

        Sound.instance.PlayButton();
    }
    

    public bool IsAvailableToShow()
    {
        return IsActionAvailable() && IsAdAvailable();
    }

    private bool IsActionAvailable()
    {
        return CUtils.IsActionAvailable(ACTION_NAME, GameConfig.instance.rewardedVideoPeriod);
    }

    private bool IsAdAvailable()
    {
        return /*IsAdmobAvailable() ||*/ IsUnityAdsAvailable();
    }

    private bool IsUnityAdsAvailable()
    {
        return UnityAdsController.instance.IsReady();
    }
}
