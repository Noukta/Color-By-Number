using UnityEngine;
using System;

[System.Serializable]
public class GameConfig : MonoBehaviour
{
    //public Admob admob;

    public UnityAds unityAds;

    [Header("")]
    public int adPeriod;
    public int rewardedVideoPeriod;
    public int rewardedVideoAmount;
    //public string androidPackageID;
    //public string iosAppID;

    //[Header("Load pixel images from server")]
    //public string jsonUrl;
    //public string jsonBinSecretKey;
    public bool addItemsToTop = true;

    public static GameConfig instance;
    private void Awake()
    {
        instance = this;
    }
}

[System.Serializable]
public class Admob
{
    [Header("Banner")]
    public string androidBanner;
    public string iosBanner;
    [Header("Interstitial")]
    public string androidInterstitial;
    public string iosInterstitial;
    [Header("RewardedVideo")]
    public string androidRewarded;
    public string iosRewarded;
}

[System.Serializable]
public class UnityAds
{
    public string PlayStoreID;
    public string AppStoreID;
}