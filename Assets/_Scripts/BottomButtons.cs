using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomButtons : MonoBehaviour {

    public ScrollRect scrollRect;
    public RectTransform mainCanvasTr;
    public GameObject gridPrefab, numberButtonPrefab;
    public RectTransform selectionTr;
    public Color selectionBlackColor, selectionWhiteColor;

    [HideInInspector]
    public List<int> completeNumers;
    [HideInInspector]
    public int selectedNumber;

    private List<Color> colors;

    public List<NumberButton> numButtons;

    private const float maxButtonWidth = 160;

    private void OnEnable()
    {
        selectionTr.gameObject.SetActive(false);
    }

    public void LoadButtons(List<Color> colors, BottomInfo info)
    {
        this.colors = colors;
        numButtons = new List<NumberButton>();

        scrollRect.GetComponent<RectTransform>().sizeDelta = new Vector2(info.width, info.cellSize * info.row);
        selectionTr.sizeDelta = new Vector2(info.cellSize, info.cellSize);

        scrollRect.content.GetComponent<RectTransform>().sizeDelta = new Vector2(info.numGrids * info.width, info.cellSize * info.row);

        var snap = scrollRect.GetComponent<SnapScrollRect>();
        snap.InitPoints(info.numGrids);
        snap.SetPage(0);

        int index = 0;
        for(int i = 0; i < info.numGrids; i++)
        {
            GameObject grid = Instantiate(gridPrefab);
            grid.transform.SetParent(scrollRect.content);
            grid.transform.localScale = Vector3.one;
            grid.transform.localPosition = new Vector2(info.width * i, grid.transform.localPosition.y);
            grid.GetComponent<RectTransform>().sizeDelta = new Vector2(info.width, info.cellSize * info.row);

            var gridLayout = grid.GetComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(info.cellSize, info.cellSize);
            gridLayout.constraintCount = info.column;

            for (int j = 0; j < info.column * info.row; j++)
            {
                if (index < colors.Count)
                {
                    GameObject numberButton = Instantiate(numberButtonPrefab);
                    numberButton.transform.SetParent(grid.transform);
                    numberButton.transform.localScale = Vector3.one;

                    var script = numberButton.GetComponent<NumberButton>();
                    script.SetColor(colors[index]);
                    script.number = index + 1;
                    script.bottomButtons = this;
                    if (completeNumers.Contains(script.number)) script.isComplete = true;
                    numButtons.Add(script);
                }
                index++;
            }
        }

        if (selectedNumber != -1)
        {
            SetSelectionPosition(selectedNumber - 1);
            selectionTr.gameObject.SetActive(true);
        }
    }

    public void SetSelectionPosition(int buttonIndex)
    {
        selectionTr.position = numButtons[buttonIndex].transform.position;
        selectionTr.SetParent(numButtons[buttonIndex].transform);
        selectionTr.GetComponent<Image>().color = numButtons[buttonIndex].color.grayscale < 0.5f ? selectionWhiteColor : selectionBlackColor;
        GameState.selectedNumber = buttonIndex + 1;
        GameState.selectedColor = colors[buttonIndex];
    }

    public void ResetVariables()
    {
        selectionTr.SetParent(transform);

        foreach(Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }
    }
}
