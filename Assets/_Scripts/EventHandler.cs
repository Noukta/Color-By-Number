using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler ,IPointerClickHandler, IPointerUpHandler
{
    public MainCamera mainCam;

    public void OnBeginDrag(PointerEventData eventData)
    {
        mainCam.OnBeginDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        mainCam.OnDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mainCam.OnEndDrag();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameState.continuousPaint = false;
        mainCam.isDragLocked = false;
    }
}
