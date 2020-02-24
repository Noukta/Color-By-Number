using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdsController : MonoBehaviour
{

    public static UnityAdsController instance;

    private LibraryItem libraryItem = null;

    void Awake()
    {
        instance = this;;
    }

    void Start()
    {
        if (Advertisement.isSupported)
            Advertisement.Initialize(GameConfig.instance.unityAds.PlayStoreID);
    }

    public bool IsReady()
    {
        if (!Advertisement.IsReady())
        {
            return false;
        }
        return true;
    }

    public void ShowInterstitial()
    {
        Debug.Log("Show Interstitial");
        Advertisement.Show();
    }

    public void ShowRewardedVideo(LibraryItem libraryItem = null)
    {
        this.libraryItem = libraryItem;
        ShowOptions options = new ShowOptions();
        options.resultCallback = HandleShowResult;

        Advertisement.Show("rewardedVideo", options);
    }

    void HandleShowResult(ShowResult result)
    {
        if (result == ShowResult.Finished)
        {
            Debug.Log("Video completed - Offer a reward to the player");
            if(libraryItem != null)
                libraryItem.HandleRewardBasedVideoRewarded();

        }
        else if (result == ShowResult.Skipped)
        {
            Debug.LogWarning("Video was skipped - Do NOT reward the player");

        }
        else if (result == ShowResult.Failed)
        {
            Debug.LogError("Video failed to show");
        }
    }
}
