using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[System.Serializable]
public class ScaleItem
{
    public bool enabled = false;
    public float scaleNormal = 1;
    public float scaleTo = 1.2f;
    public float time = 0.3f;
}

public class SnapScrollRect : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public int screens;
    float[] points;
    public int speed = 10;
    float stepSize;

    ScrollRect scroll;
    bool lerp;
    float target;
    [HideInInspector]
    public int index = 0;

    public GameObject[] indicators;
    public Text tabName;
    public string tabNamePrefix;

    public ScaleItem scaleItem;

    public Action<int> onPageChanged;

    void Awake()
    {
        scroll = gameObject.GetComponent<ScrollRect>();
    }

    public void InitPoints(int _screens)
    {
        screens = _screens;
        points = new float[screens];
        if (screens > 1)
        {
            stepSize = 1 / (float)(screens - 1);

            for (int i = 0; i < screens; i++)
            {
                points[i] = i * stepSize;
            }
        }
        else
        {
            points[0] = 0;
        }
    }

    void Update()
    {
        if (lerp == false) return;
        if (scroll.horizontal)
        {
            scroll.horizontalNormalizedPosition = Mathf.Lerp(scroll.horizontalNormalizedPosition, target, speed * scroll.elasticity * Time.deltaTime);
            if (Mathf.Approximately(scroll.horizontalNormalizedPosition, target)) lerp = false;
        }
        else
        {
            scroll.verticalNormalizedPosition = Mathf.Lerp(scroll.verticalNormalizedPosition, target, speed * scroll.elasticity * Time.deltaTime);
            if (Mathf.Approximately(scroll.verticalNormalizedPosition, target)) lerp = false;
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (Mathf.Abs(scroll.velocity.x) > 100)
        {
            int delta = scroll.velocity.x > 0 ? -1 : 1;
            index += delta;
        }
        else
        {
            index = scroll.horizontal ? FindNearest(scroll.horizontalNormalizedPosition, points) :
                     scroll.vertical ? FindNearest(scroll.verticalNormalizedPosition, points) : index;
        }
        MoveToPage(index);
    }

    public void NextPage()
    {
        MoveToPage(index + 1);
        Sound.instance.PlayButton();
    }

    public void PreviousPage()
    {
        MoveToPage(index - 1);
        Sound.instance.PlayButton();
    }

    public void MoveToPage(int pageIndex)
    {
        index = Mathf.Clamp(pageIndex, 0, screens - 1);
        target = points[index];
        lerp = true;
        UpdateIndicator();

        if (onPageChanged != null) onPageChanged(index);
        if (scaleItem.enabled) ScaleEffect(index);
    }

    public void SetPage(int pageIndex)
    {
        index = Mathf.Clamp(pageIndex, 0, screens - 1);
        target = points[index];
        if (scroll.horizontal) scroll.horizontalNormalizedPosition = target;
        else scroll.verticalNormalizedPosition = target;

        UpdateIndicator();
        if (onPageChanged != null) onPageChanged(index);
        if (scaleItem.enabled) ScaleEffect(index, true);
    }

    public void UpdateIndicator()
    {
        for (int i = 0; i < indicators.Length; i++)
        {
            indicators[i].SetActive(i == index);
        }
        if (tabName != null)
        {
            tabName.text = tabNamePrefix + (index + 1);
        }
    }

    public void OnDrag(PointerEventData data)
    {
        lerp = false;
    }

    int FindNearest(float f, float[] array)
    {
        float distance = Mathf.Infinity;
        int output = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if (Mathf.Abs(array[index] - f) < distance)
            {
                distance = Mathf.Abs(array[index] - f);
                output = index;
            }
        }
        return output;
    }

    private void ScaleEffect(int index, bool immediate = false)
    {
        int i = 0;
        foreach (Transform item in scroll.content.transform)
        {
            if (immediate)
            {
                item.localScale = Vector3.one * (index == i ? scaleItem.scaleTo : scaleItem.scaleNormal);
            }
            else
            {
                iTween.ScaleTo(item.gameObject, Vector3.one * (index == i ? scaleItem.scaleTo : scaleItem.scaleNormal), scaleItem.time);
            }
            i++;
        }
    }
}