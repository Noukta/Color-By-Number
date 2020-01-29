using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;

public class CreateManager : MonoBehaviour, IDragHandler {

    public Image image, preview;
    private Color32[] imageColors;
    public Texture2D imageTexture;
    public Button applyButton;
    private float scale = 1f;

    public Texture2D testTexture;
    public Slider slider;
    private int originalWidth;
    private const int frameSize = 200;

    private void OnEnable()
    {
        ResetVariables();
    }

    public void Load()
    {
        LoadImage(testTexture);
    }

    public void LoadImage(Texture2D _imageTexture)
    {
        imageTexture = _imageTexture;

        int width = imageTexture.width;
        int height = imageTexture.height;
        int newWidth, newHeight;
        if (height > width)
        {
            newWidth = frameSize;
            newHeight = Mathf.RoundToInt(height / (float)width * newWidth);
        }
        else
        {
            newHeight = frameSize;
            newWidth = Mathf.RoundToInt(width / (float)height * newHeight);
        }

        TextureScale.Point(imageTexture, newWidth, newHeight);

        applyButton.interactable = true;
        originalWidth = imageTexture.width;
        imageColors = imageTexture.GetPixels32();

        Rect rec = new Rect(0, 0, imageTexture.width, imageTexture.height);
        image.sprite = Sprite.Create(imageTexture, rec, new Vector2(0.5f, 0.5f), 100);
        image.SetNativeSize();

        if (height > width)
        {
            var pos = image.transform.localPosition;
            image.transform.localPosition = new Vector3(0, -(newHeight - frameSize) / 2, 0);
        }
        else
        {
            var pos = image.transform.localPosition;
            image.transform.localPosition = new Vector3(-(newWidth - frameSize) / 2, 0, 0);
        }

        UpdateTexture();
    }

    private void SetPivot(RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        Vector2 size = rectTransform.rect.size * scale;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }

    public void Zoom(bool isZoomIn)
    {
        if (imageTexture == null) return;

        if (scale <= 1 && !isZoomIn || scale >= 3 && isZoomIn) return;

        Vector2 centerBoard = new Vector2(-image.transform.localPosition.x, -image.transform.localPosition.y) + Vector2.one * 150;
        Vector2 pivot = new Vector2(centerBoard.x / (imageTexture.width * scale), centerBoard.y / (imageTexture.height * scale));
        SetPivot(image.GetComponent<RectTransform>(), pivot);

        scale += isZoomIn ? 0.2f : -0.2f;
        image.transform.localScale = new Vector3(scale, scale, 0);
        SetPivot(image.GetComponent<RectTransform>(), Vector2.zero);

        Timer.Schedule(this, 0f, UpdateTexture);
    }


    private Texture2D frameTexture, colorTexture, grayTexture;
    private Sprite graySprite;
    private Color32[] colorTextureColors;

    private void UpdateTexture()
    {
        int size = Mathf.RoundToInt(frameSize / scale);
        frameTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        int index = 0;
        Color[] colors = new Color[size * size];

        int offsetY = Mathf.RoundToInt(-image.transform.localPosition.y / scale);
        int offsetX = Mathf.RoundToInt(-image.transform.localPosition.x / scale);

        for (int i = offsetY; i < size + offsetY; i++)
        {
            for(int j = offsetX; j < size + offsetX; j++)
            {
                Color c = imageColors[originalWidth * i + j];
                colors[index] = c;
                index++;
            }
        }
        frameTexture.SetPixels(colors);

        UpdateColorTexture();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (imageTexture == null) return;
        UpdateTexture();
    }

    public void OnSliderChanged()
    {
        if (imageTexture == null) return;

        UpdateColorTexture();
    }

    private void UpdateColorTexture()
    {
        colorTexture = Instantiate(frameTexture);
        float scaleValue = (slider.minValue + slider.maxValue) - slider.value;

        if (scaleValue != 1)
        {
            int newSize = (int)(frameTexture.width / scaleValue);
            TextureScale.Point(colorTexture, newSize, newSize);
        }

        colorTextureColors = colorTexture.GetPixels32();
        Color32[] grayTextureColors = new Color32[colorTexture.width * colorTexture.height];
        grayTexture = new Texture2D(colorTexture.width, colorTexture.height, TextureFormat.RGBA32, false);

        int i = 0;
        foreach(var color32 in colorTextureColors)
        {
            Color color = color32;
            if (color != Color.clear)
            {
                byte grayScale = (byte)Mathf.RoundToInt(color.grayscale * 255);
                grayTextureColors[i] = new Color32(grayScale, grayScale, grayScale, 255);
            }
            i++;
        }
        grayTexture.SetPixels32(grayTextureColors);
        grayTexture.Apply();

        grayTexture.filterMode = FilterMode.Point;

        Rect rec = new Rect(0, 0, grayTexture.width, grayTexture.height);
        graySprite = Sprite.Create(grayTexture, rec, new Vector2(0.5f, 0.5f), 100);
        preview.sprite = graySprite;
    }

    public void Apply()
    {
        int size = colorTexture.width;
        int length = colorTextureColors.Length;
        for (int i = 0; i < length; i++)
        {
            var aColor = colorTextureColors[i];
            if (aColor == Color.clear || aColor == Color.white) continue;

            int[] positions = { i - 1, i - size};
            float min = float.MaxValue;
            Color32 matchColor = Color.clear;
            foreach (var pos in positions)
            {
                if (pos >= 0)
                {
                    var color = colorTextureColors[pos];
                    float distance = Mathf.Abs(aColor.r - color.r) + Mathf.Abs(aColor.g - color.g) + Mathf.Abs(aColor.b - color.b);
                    if (distance < min)
                    {
                        min = distance;
                        matchColor = color;
                    }
                }
            }

            if (min <= 60f)
            {
                colorTextureColors[i] = matchColor;
            }
            else
            {
                for(int j = i - 2; j >= 0; j--)
                {
                    if (IsColorSimilar(colorTextureColors[i], colorTextureColors[j]))
                    {
                        colorTextureColors[i] = colorTextureColors[j];
                        break;
                    }
                }
            }
        }

        colorTexture.SetPixels32(colorTextureColors);
        colorTexture.filterMode = FilterMode.Point;

        int lastCreateItem = PlayerPrefs.GetInt("last_create_item");
        PlayerPrefs.SetInt("last_create_item", lastCreateItem + 1);
        var myCreateItems = CUtils.BuildListFromString<string>(PlayerPrefs.GetString("mycreate_items"));

        string itemName = "c" + (lastCreateItem + 1);
        myCreateItems.Add(itemName);
        string data = CUtils.BuildStringFromCollection(myCreateItems);
        PlayerPrefs.SetString("mycreate_items", data);

        Rect rec = new Rect(0, 0, colorTexture.width, colorTexture.height);
        var colorSprite = Sprite.Create(colorTexture, rec, new Vector2(0.5f, 0.5f), 100);
        GameState.colorImage = colorSprite;
        GameState.grayImage = graySprite;
        GameState.libraryItemName = itemName;

        byte[] bytes = colorTexture.EncodeToPNG();
        string colorPath = Path.Combine(Application.persistentDataPath, "colorImage_" + itemName + ".png");
        File.WriteAllBytes(colorPath, bytes);

        bytes = grayTexture.EncodeToPNG();
        string grayPath = Path.Combine(Application.persistentDataPath, "grayImage_" + itemName + ".png");
        File.WriteAllBytes(grayPath, bytes);

        GameController.instance.GotoMainScreen();
    }

    private bool IsColorSimilar(Color32 color1, Color32 color2)
    {
        return Mathf.Abs(color1.r - color2.r) + Mathf.Abs(color1.g - color2.g) + Mathf.Abs(color1.b - color2.b) <= 60;
    }

    private void ResetVariables()
    {
        applyButton.interactable = false;
        imageTexture = null;
        image.sprite = null;
        preview.sprite = null;

        scale = 1f;
        image.transform.localScale = new Vector3(scale, scale, 0);
        slider.value = (slider.minValue + slider.maxValue) / 2f;
        image.sprite = null;
    }
}
