using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchVideoPanel : MonoBehaviour
{
    private LibraryItem item;
    public GameObject dialogOverlay;
    public Transform InTr, OutTr;

    public void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void Open(LibraryItem item)
    {
        this.item = item;
        transform.GetChild(0).gameObject.SetActive(true);
        dialogOverlay.SetActive(true);

        transform.GetChild(0).transform.position = OutTr.position;
        iTween.MoveTo(transform.GetChild(0).gameObject, InTr.position, 0.3f);
    }

    public void Close()
    {
        this.item = null;
        dialogOverlay.SetActive(false);
        iTween.MoveTo(transform.GetChild(0).gameObject, OutTr.position, 0.3f);

        Timer.Schedule(this, 0.3f, () =>
        {
            transform.GetChild(0).gameObject.SetActive(false);
        });
        Sound.instance.PlayButton();
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
        dialogOverlay.SetActive(false);

#endif
    }
}
