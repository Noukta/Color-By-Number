using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour {
    public int x, y;

    public Color color, gray, backColor, highlight1, highlight2;
    public TextMesh numberText;
    public MeshRenderer textRenderer;
    public int number, index;

    [HideInInspector]
    public Board board;
    [HideInInspector]
    public MainCamera mainCam;
    [HideInInspector]
    public GameObject eventHandler;
    [HideInInspector]
    public bool isPainted, isErrored;
    [HideInInspector]
    public Color errorColor;
    [HideInInspector]
    public int errorNumber;

    public static float size = 0.1f;

    public void ResetVariables()
    {
        isPainted = false;
        numberText.gameObject.SetActive(true);
    }

    private float mouseDownTime;
    public bool isHolding;
    private void OnMouseDown()
    {
        bool touchCheck = Input.touchCount == 1 && Application.isMobilePlatform || !Application.isMobilePlatform;
        bool errorCheck = !isErrored || errorNumber != GameState.selectedNumber;

        if (touchCheck && errorCheck && !IsPointerOverUIObject(eventHandler))
        {
            mouseDownTime = Time.time;
            isHolding = true;
        }
    }

    private void OnMouseOver()
    {
        bool touchCheck = Input.touchCount == 1 && Application.isMobilePlatform || !Application.isMobilePlatform;
        if (touchCheck && !IsPointerOverUIObject(eventHandler))
        {
            if (isHolding)
            {
                float holdTime = Time.time - mouseDownTime;
                if (holdTime > 0.3f && !mainCam.dragging)
                {
                    mainCam.isDragLocked = true;
                    GameState.continuousPaint = true;
                }
            }

            if (GameState.continuousPaint)
            {
                Paint();
            }
        }
    }

    private void Update()
    {
        if (isHolding)
        {
            if (Application.isMobilePlatform && Input.touchCount > 1 || !Application.isMobilePlatform && Input.GetMouseButtonUp(0))
            {
                isHolding = false;
            }
        }
    }

    private void OnMouseUp()
    {
        bool touchCheck = Input.touchCount == 1 && Application.isMobilePlatform || !Application.isMobilePlatform;
        if (isHolding && touchCheck && !IsPointerOverUIObject(eventHandler))
        {
            if (!mainCam.dragging)
            {
                Paint();
            }
        }
    }

    public static bool IsPointerOverUIObject(GameObject exceptObject)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var go in results)
        {
            if (go.gameObject != exceptObject) return true;
        }
        return false;
    }

    public void Paint()
    {
        if (isPainted) return;
        if (isErrored && errorNumber == GameState.selectedNumber) return;

        if (GameState.selectedNumber == number)
        {
            isPainted = true;
            isErrored = false;

            board.PaintTile(this);
            gameObject.SetActive(false);
        }
        else
        {
            var c = GameState.selectedColor;
            errorColor = new Color(c.r, c.g, c.b, 0.5f);
            errorNumber = GameState.selectedNumber;
            isErrored = true;
            board.PaintErrorTile(this);
        }
    }
}
