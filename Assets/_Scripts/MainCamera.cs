using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainCamera : MonoBehaviour {

    private Vector3 dragDelta;
    private Vector3 beginTouchPosition;
    private Vector3 beginCamPosition;

    public Camera fixedCamera;
    public Camera mainCamera;
    public Board board;

    [HideInInspector]
    public float coef1, coef2, coef0;

    public bool dragging, isDragLocked;

    private float cameraWidth;

    public void OnBeginDrag()
    {
        if (isDragLocked) return;

        bool touchCheck = Input.touchCount == 1 && Application.isMobilePlatform || !Application.isMobilePlatform;
        if (touchCheck)
        {
            beginTouchPosition = fixedCamera.ScreenToWorldPoint(Input.mousePosition);
            beginCamPosition = transform.position;
            dragging = true;
        }
    }

    public void OnDrag()
    {
        if (isDragLocked || !dragging) return;

        dragDelta = fixedCamera.ScreenToWorldPoint(Input.mousePosition) - beginTouchPosition;
        dragDelta.z = 0;
        transform.position = beginCamPosition - dragDelta;
    }

    public void OnEndDrag()
    {
        dragging = false;
    }

    private void LateUpdate()
    {
        Vector3 temp = transform.position;
        cameraWidth = mainCamera.orthographicSize * mainCamera.aspect;
        temp.x = Mathf.Clamp(temp.x, board.minX + cameraWidth * (1 - coef0), board.maxX - cameraWidth * ( 1 - coef0));
        temp.y = Mathf.Clamp(temp.y, board.minY + mainCamera.orthographicSize * coef1, board.maxY - mainCamera.orthographicSize * coef2);
        transform.position = temp;
    }

    public float orthoZoomSpeed = 0.35f;
    void Update()
    {
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            dragging = false;
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // ... change the orthographic size based on the change in distance between the touches.
            mainCamera.orthographicSize += deltaMagnitudeDiff * (mainCamera.orthographicSize / 8f) * Time.deltaTime;

            // Make sure the orthographic size never drops below zero.
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, board.minOrthographicSize, board.maxOrthographicSize);
            fixedCamera.orthographicSize = mainCamera.orthographicSize;
        }
    }
}
