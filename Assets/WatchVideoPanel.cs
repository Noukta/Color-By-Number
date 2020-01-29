using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchVideoPanel : MonoBehaviour
{
    private LibraryItem item;

    public void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void Open(LibraryItem item)
    {
        this.item = item;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Close()
    {
        this.item = null;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void WatchVideo()
    {
        if (item != null)
        {
            GameState.itemWatchingAd = item;
            //#if UNITY_EDITOR
            //            HandleRewardBasedVideoRewarded();
#if UNITY_ANDROID || UNITY_IOS
            //if (IsAdmobAvailable()) {
            //    AdmobController.instance.ShowRewardBasedVideo();
            //}
            //else{
            UnityAdsController.instance.ShowRewardedVideo(item);
            //}
            item = null;
        }
        transform.GetChild(0).gameObject.SetActive(false);

#endif
    }
}
