using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Text;
using System.IO;

public class Board : MonoBehaviour {

    public Texture2D colorImage;
    public Texture2D grayImage;
    public Material material;

    public Color backColor;

    public Tile tilePrefab;
    public Camera boardCam, fixedCam;
    public Slider slider;
    public BottomButtons bottomButtons;
    public GameObject eventHandler;
    public MainCamera mainCam;
    public TilePooler tilePooler;
    public GameObject completeArt;
    public RectTransform mainCanvasTr, topTr;

    [HideInInspector]
    public float minX, maxX, minY, maxY;
    [HideInInspector]
    public List<Tile> tiles;

    public Dictionary<int, int> numTiles;

    public int size;
    private float boardSize;
    public float markedOrthographicSize = 2.6f, minOrthographicSize = 0.8f, maxOrthographicSize;
    private bool hasTilePainted;
    private string progressPrefKey, itemStatusPrefKey;

    public List<Color> colors;

    private GameObject mainObject, coverObject, highlightObject, errorObject;
    private Mesh mainMesh, coverMesh, highlightMesh, errorMesh;
    private List<Tile> paintedTiles, notPaintedTiles, beingHighlights, errorTiles;
    private List<int> tileStatus;
    private List<string> myWorkItem;

    private bool isGameComplete;
    private bool vibrationEnabled;

    public static Board instance;

    private void Awake()
    {
        instance = this;
    }

    public void OnEnable()
    {
        slider.value = 0;
        zoom = -1;
        hasTilePainted = false;
        isGameComplete = false;

        completeArt.SetActive(false);
        boardCam.transform.position = new Vector3(0, 0, boardCam.transform.position.z);
        mainCam.enabled = true;
        transform.position = Vector3.zero;

        slider.gameObject.SetActive(!Application.isMobilePlatform);
        vibrationEnabled = PlayerPrefs.GetInt("vibration_enabled", 0) == 1 && Application.isMobilePlatform;

        tiles = new List<Tile>();
        numTiles = new Dictionary<int, int>();
        colors = new List<Color>();
        paintedTiles = new List<Tile>();
        notPaintedTiles = new List<Tile>();
        beingHighlights = new List<Tile>();
        errorTiles = new List<Tile>();
        myWorkItem = CUtils.BuildListFromString<string>(PlayerPrefs.GetString("mywork_items"));

        colorImage = GameState.colorImage.texture;
        grayImage = GameState.grayImage.texture;

        size = colorImage.width;

        tileStatus = new List<int>();

        progressPrefKey = "game_progress_" + GameState.libraryItemName;
        itemStatusPrefKey = "item_status_" + GameState.libraryItemName;

        isGameComplete = PlayerPrefs.GetString(itemStatusPrefKey) == "complete";
        eventHandler.SetActive(!isGameComplete);

        if (isGameComplete)
        {
            LoadCompleteGame();
            GameController.instance.OnComplete(0, true);
        }
        else
        {
            LoadUncompleteGame();
        }

        StartCoroutine(ShowBannerAd());
    }

    private IEnumerator ShowBannerAd()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            CUtils.ShowBannerAd();
        }
    }

    public void StopLoadingBanner()
    {
        StopCoroutine(ShowBannerAd());
    }

    private void LoadUncompleteGame()
    {
        if (PlayerPrefs.HasKey(progressPrefKey))
        {
            string progress = PlayerPrefs.GetString(progressPrefKey);
            for (int i = 0; i < progress.Length; i++)
            {
                int value = progress[i].Equals('0') ? 0 : 1;
                tileStatus.Add(value);
            }
        }

        int index = 0;
        bool hasProgress = tileStatus.Count > 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Color color = colorImage.GetPixel(i, j);
                Color gray = grayImage.GetPixel(i, j);

                if (color.a != 0 && color != Color.white)
                {
                    gray = Color.Lerp(gray, Color.white, 0.3f);
                    Tile tile = Instantiate(tilePooler.GetPooledTile());
                    tile.gameObject.SetActive(true);
                    tile.transform.SetParent(transform);
                    tile.transform.localPosition = new Vector3(i + 0.5f, j + 0.5f) * Tile.size;

                    if (hasProgress)
                    {
                        if (tileStatus[index] == 0)
                        {
                            notPaintedTiles.Add(tile);
                        }
                        else
                        {
                            tile.isPainted = true;
                            tile.GetComponent<MeshRenderer>().enabled = false;
                            paintedTiles.Add(tile);
                        }
                    }
                    else
                    {
                        notPaintedTiles.Add(tile);
                        tileStatus.Add(0);
                    }

                    tile.gray = gray;
                    tile.color = color;
                    tile.board = this;
                    tile.mainCam = mainCam;
                    tile.eventHandler = eventHandler;

                    tile.x = i;
                    tile.y = j;
                    tile.index = index;

                    tiles.Add(tile);

                    int colorIndex = colors.IndexOf(color);
                    if (colorIndex == -1)
                    {
                        colors.Add(color);
                        int number = colors.Count;
                        tile.number = number;

                        if (!tile.isPainted)
                            AddUnpaintedTile(tile);
                    }
                    else
                    {
                        tile.number = colorIndex + 1;
                        if (!tile.isPainted)
                            AddUnpaintedTile(tile);
                    }
                    tile.numberText.text = tile.number.ToString();

                    index++;
                }
            }
        }

        mainObject = CreateMesh(1);
        coverObject = CreateMesh(0.95f);
        highlightObject = CreateMeshObject();
        errorObject = CreateMeshObject();

        mainObject.name = "Main Tiles";
        coverObject.name = "Cover Tiles";
        highlightObject.name = "Highlight Tiles";
        errorObject.name = "Error Tiles";

        mainMesh = mainObject.GetComponent<MeshFilter>().mesh;
        coverMesh = coverObject.GetComponent<MeshFilter>().mesh;
        highlightMesh = highlightObject.GetComponent<MeshFilter>().mesh;
        errorMesh = errorObject.GetComponent<MeshFilter>().mesh;

        coverObject.GetComponent<MeshRenderer>().sortingOrder = 1;
        highlightObject.GetComponent<MeshRenderer>().sortingOrder = 2;
        errorObject.GetComponent<MeshRenderer>().sortingOrder = 4;

        Color[] mainColors = new Color[tiles.Count * 4];
        Color[] coverColors = new Color[tiles.Count * 4];

        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            if (tile.isPainted)
            {
                mainColors[i * 4] = tile.color;
                mainColors[i * 4 + 1] = tile.color;
                mainColors[i * 4 + 2] = tile.color;
                mainColors[i * 4 + 3] = tile.color;
            }
            else
            {
                mainColors[i * 4] = backColor;
                mainColors[i * 4 + 1] = backColor;
                mainColors[i * 4 + 2] = backColor;
                mainColors[i * 4 + 3] = backColor;

                coverColors[i * 4] = tile.gray;
                coverColors[i * 4 + 1] = tile.gray;
                coverColors[i * 4 + 2] = tile.gray;
                coverColors[i * 4 + 3] = tile.gray;
            }
        }

        mainMesh.colors = mainColors;
        coverMesh.colors = coverColors;

        mainMesh.RecalculateNormals();
        coverMesh.RecalculateNormals();

        var list = GetCompletePainting();
        int selectedNumber = GetSelectedNumber();
        var bottomInfo = GetBottomInfo();

        bottomButtons.completeNumers = list;
        bottomButtons.selectedNumber = selectedNumber;
        bottomButtons.LoadButtons(colors, bottomInfo);

        CUtils.ShowInterstitialAd();

        float width = bottomInfo.width, cellSize = bottomInfo.cellSize;
        float bottomHeight = cellSize * bottomInfo.row;
        float freeHeight = mainCanvasTr.rect.height - bottomHeight - topTr.rect.height;

        boardSize = size * Tile.size;

        transform.position = - new Vector3(0.5f * boardSize , 0.5f * boardSize, 0);
        minX = -0.5f * boardSize;
        minY = -0.5f * boardSize;
        maxX = 0.5f * boardSize;
        maxY = 0.5f * boardSize;

        float coef = Mathf.Max(1, width / freeHeight);
        maxOrthographicSize = boardSize * coef * 0.5f / boardCam.aspect;

        float gap = Mathf.Max(0, (freeHeight - width) / 2);
        float camCoef0 = Mathf.Max(0, (width - freeHeight) / (width));
        float camCoef1 = 1 - ((bottomHeight + gap) / mainCanvasTr.rect.height) * 2 ;
        float camCoef2 = 1 - ((topTr.rect.height + gap) / mainCanvasTr.rect.height) * 2;

        mainCam.coef0 = camCoef0;
        mainCam.coef1 = camCoef1;
        mainCam.coef2 = camCoef2;

        boardCam.orthographicSize = maxOrthographicSize;

        Highlight(selectedNumber);
    }

    private BottomInfo GetBottomInfo()
    {
        BottomInfo info = new BottomInfo();

        info.width = mainCanvasTr.rect.width;
        info.column = (int)Mathf.Ceil(info.width / 160f);
        info.column = Mathf.Max(5, info.column);

        info.row = Mathf.Min(2, (int)Mathf.Ceil(colors.Count / (float)info.column));

        info.cellSize = info.width / info.column;
        info.numGrids = (int)Mathf.Ceil(colors.Count / (float)(info.column * info.row));
        return info;
    }

    private void LoadCompleteGame()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Color color = colorImage.GetPixel(i, j);

                if (color.a != 0 && color != Color.white)
                {
                    int colorIndex = colors.IndexOf(color);
                    if (colorIndex == -1)
                    {
                        colors.Add(color);
                    }
                }
            }
        }

        var list = GetCompletePainting();
        int selectedNumber = GetSelectedNumber();
        var bottomInfo = GetBottomInfo();

        bottomButtons.completeNumers = list;
        bottomButtons.selectedNumber = selectedNumber;
        bottomButtons.LoadButtons(colors, bottomInfo);

        CUtils.ShowInterstitialAd();

        completeArt.SetActive(true);
        completeArt.GetComponent<SpriteRenderer>().sprite = GameState.colorImage;
        boardSize = size / 100f;

        float width = bottomInfo.width, cellSize = bottomInfo.cellSize;
        float bottomHeight = cellSize * bottomInfo.row;
        float freeHeight = mainCanvasTr.rect.height - bottomHeight - topTr.rect.height;

        float coef = Mathf.Max(1, width / freeHeight);
        maxOrthographicSize = boardSize * coef * 0.5f / boardCam.aspect;

        float gap = Mathf.Max(0, (freeHeight - width) / 2);
        float camCoef = 1 - ((bottomHeight + gap) / mainCanvasTr.rect.height) * 2;

        boardCam.orthographicSize = maxOrthographicSize;
        boardCam.transform.position = new Vector3(0, -0.5f * boardSize + maxOrthographicSize * camCoef, boardCam.transform.position.z);
        mainCam.enabled = false;
    }

    private void AddUnpaintedTile(Tile tile)
    {
        if (numTiles.ContainsKey(tile.number))
            numTiles[tile.number]++;
        else
            numTiles.Add(tile.number, 1);
    }

    private GameObject CreateMeshObject()
    {
        GameObject go = new GameObject();
        go.transform.parent = transform;
        MeshFilter mf = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;
        Renderer rend = go.AddComponent<MeshRenderer>();
        rend.material = material;
        go.AddComponent<MeshCollider>();

        return go;
    }

    private void UpdateErrorMesh()
    {
        int count = errorTiles.Count;
        Vector3[] vertices = new Vector3[count * 4];
        for (int i = 0; i < count; i++)
        {
            var tile = errorTiles[i];

            Vector3 basePosition = new Vector3(tile.x, tile.y) * Tile.size + new Vector3(0.5f, 0.5f) * Tile.size;
            vertices[i * 4] = basePosition + new Vector3(-0.5f, -0.5f) * Tile.size;
            vertices[i * 4 + 1] = basePosition + new Vector3(-0.5f, 0.5f) * Tile.size;
            vertices[i * 4 + 2] = basePosition + new Vector3(0.5f, 0.5f) * Tile.size;
            vertices[i * 4 + 3] = basePosition + new Vector3(0.5f, -0.5f) * Tile.size;
        }

        errorMesh.triangles = null;
        errorMesh.vertices = vertices;

        int[] triangles = new int[errorMesh.vertices.Length / 2 * 3];

        for (int j = 0; j < vertices.Length / 4; j++)
        {
            triangles[j * 6 + 0] = j * 4 + 0;    //     0_ 3        0 ___ 3
            triangles[j * 6 + 1] = j * 4 + 3;    //   | /         |    /|
            triangles[j * 6 + 2] = j * 4 + 1;    //  1|/            1|/__|2

            triangles[j * 6 + 3] = j * 4 + 3;    //       3
            triangles[j * 6 + 4] = j * 4 + 2;    //    /|
            triangles[j * 6 + 5] = j * 4 + 1;    //  1/_|2
        }

        errorMesh.triangles = triangles;

        Color[] eColors = new Color[count * 4];
        for (int i = 0; i < count; i++)
        {
            var tile = errorTiles[i];
            eColors[i * 4] = tile.errorColor;
            eColors[i * 4 + 1] = tile.errorColor;
            eColors[i * 4 + 2] = tile.errorColor;
            eColors[i * 4 + 3] = tile.errorColor;
        }

        errorMesh.colors = eColors;
        errorMesh.RecalculateNormals();
    }

    private void UpdateHighlightMesh()
    {
        int count = beingHighlights.Count;
        Vector3[] vertices = new Vector3[count * 4];
        for (int i = 0; i < count; i++)
        {
            var tile = beingHighlights[i];

            Vector3 basePosition = new Vector3(tile.x, tile.y) * Tile.size + new Vector3(0.5f, 0.5f) * Tile.size;
            vertices[i * 4] = basePosition + new Vector3(-0.5f, -0.5f) * Tile.size * 0.95f;
            vertices[i * 4 + 1] = basePosition + new Vector3(-0.5f, 0.5f) * Tile.size * 0.95f;
            vertices[i * 4 + 2] = basePosition + new Vector3(0.5f, 0.5f) * Tile.size * 0.95f;
            vertices[i * 4 + 3] = basePosition + new Vector3(0.5f, -0.5f) * Tile.size * 0.95f;
        }

        highlightMesh.triangles = null;
        highlightMesh.vertices = vertices;

        int[] triangles = new int[highlightMesh.vertices.Length / 2 * 3];

        for (int j = 0; j < vertices.Length / 4; j++)
        {
            triangles[j * 6 + 0] = j * 4 + 0;    //     0_ 3        0 ___ 3
            triangles[j * 6 + 1] = j * 4 + 3;    //   | /         |    /|
            triangles[j * 6 + 2] = j * 4 + 1;    //  1|/            1|/__|2

            triangles[j * 6 + 3] = j * 4 + 3;    //       3
            triangles[j * 6 + 4] = j * 4 + 2;    //    /|
            triangles[j * 6 + 5] = j * 4 + 1;    //  1/_|2
        }

        highlightMesh.triangles = triangles;

        Color[] hColors = new Color[count * 4];
        for (int i = 0; i < count; i++)
        {
            var tile = beingHighlights[i];
            Color color = Color.Lerp(tile.highlight1, tile.highlight2, (zoom - 0.3f) / 0.7f);
            hColors[i * 4] = color;
            hColors[i * 4 + 1] = color;
            hColors[i * 4 + 2] = color;
            hColors[i * 4 + 3] = color;
        }

        highlightMesh.colors = hColors;
        highlightMesh.RecalculateNormals();
    }

    private GameObject CreateMesh(float tileSizeCoef)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(transform);
        MeshFilter mf = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;
        Renderer rend = go.AddComponent<MeshRenderer>();
        rend.material = material;
        go.AddComponent<MeshCollider>();

        Vector3[] vertices = new Vector3[tiles.Count * 4];
        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            Vector3 basePosition = new Vector3(tile.x, tile.y) * Tile.size + new Vector3(0.5f, 0.5f) * Tile.size;
            vertices[i * 4] = basePosition + new Vector3(-0.5f, -0.5f) * Tile.size * tileSizeCoef;
            vertices[i * 4 + 1] = basePosition + new Vector3(-0.5f, 0.5f) * Tile.size * tileSizeCoef;
            vertices[i * 4 + 2] = basePosition + new Vector3(0.5f, 0.5f) * Tile.size * tileSizeCoef;
            vertices[i * 4 + 3] = basePosition + new Vector3(0.5f, -0.5f) * Tile.size * tileSizeCoef;
        }

        mesh.vertices = vertices;

        int[] triangles = new int[mesh.vertices.Length / 2 * 3];

        for (int j = 0; j < vertices.Length / 4; j++)
        {
            triangles[j * 6 + 0] = j * 4 + 0;    //     0_ 3        0 ___ 3
            triangles[j * 6 + 1] = j * 4 + 3;    //   | /         |    /|
            triangles[j * 6 + 2] = j * 4 + 1;    //  1|/            1|/__|2

            triangles[j * 6 + 3] = j * 4 + 3;    //       3
            triangles[j * 6 + 4] = j * 4 + 2;    //    /|
            triangles[j * 6 + 5] = j * 4 + 1;    //  1/_|2
        }

        mesh.triangles = triangles;
       
        return go;
    }

    private float zoom;
    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.instance.OnMainBackClick();
        }
#endif

        if (isGameComplete) return;

        if (!Application.isMobilePlatform)
        {
            boardCam.orthographicSize = slider.value * (minOrthographicSize - maxOrthographicSize) + maxOrthographicSize;
            fixedCam.orthographicSize = boardCam.orthographicSize;
        }

        if (!Application.isMobilePlatform)
        {
            if (slider.value != zoom)
            {
                zoom = slider.value;
                ZoomUpdate();
            }
        }
        else
        {
            float newZoom = (boardCam.orthographicSize - maxOrthographicSize) / (minOrthographicSize - maxOrthographicSize);
            if (newZoom != zoom)
            {
                zoom = newZoom;
                ZoomUpdate();
            }
        }
    }

    private void ZoomUpdate()
    {
        bool enableText = zoom > 0.15f && boardCam.orthographicSize < markedOrthographicSize;
        foreach (var tile in notPaintedTiles)
        {
            tile.textRenderer.enabled = enableText;
        }

        UpdateNotPaintedTiles();
        UpdateHighlightMesh();
    }

    public void Highlight(int number)
    {
        beingHighlights = notPaintedTiles.FindAll(x => x.number == number && !x.isPainted);
        UpdateHighlightMesh();
    }

    public void UnHighlight(Tile tile)
    {
        beingHighlights.Remove(tile);
        UpdateHighlightMesh();
    }

    public void UpdateNotPaintedTiles()
    {
        Color[] cColor = coverMesh.colors;
        foreach (var tile in notPaintedTiles)
        {
            Color color = Color.Lerp(tile.gray, Color.white, zoom);
            int index = tile.index;
            cColor[index * 4] = color;
            cColor[index * 4 + 1] = color;
            cColor[index * 4 + 2] = color;
            cColor[index * 4 + 3] = color;
        }

        coverMesh.colors = cColor;
        coverMesh.RecalculateNormals();
    }

    public void PaintTile(Tile tile)
    {
        hasTilePainted = true;
        tileStatus[tile.index] = 1;

        numTiles[tile.number]--;
        if (numTiles[tile.number] == 0)
        {
            CompletePainting(tile.number - 1);
        }

        notPaintedTiles.Remove(tile);
        paintedTiles.Add(tile);

        if (errorTiles.Remove(tile))
        {
            UpdateErrorMesh();
        }

        DoPaintTile(mainMesh, tile.index, tile.color);
        DoPaintTile(coverMesh, tile.index, Color.clear);

        UnHighlight(tile);

        if (GetSelectedNumber() == -1)
        {
            isGameComplete = true;
            eventHandler.SetActive(false);

            PlayerPrefs.SetString(itemStatusPrefKey, "complete");
            UpdateMywork();
            GameController.instance.OnComplete(0.3f, false);
            iTween.ValueTo(gameObject, iTween.Hash("from", boardCam.orthographicSize, "to", maxOrthographicSize, "time", 0.3f, "onupdate", "OnUpdateValue"));

            Timer.Schedule(this, 0.15f, () =>
            {
                Sound.instance.Play(Sound.Others.Win);
            });
        }
    }

    public void PaintErrorTile(Tile tile)
    {
        if (!errorTiles.Contains(tile))
        {
            errorTiles.Add(tile);
        }

        UpdateErrorMesh();

        if (vibrationEnabled)
        {
            VibrateOnError();
        }
    }

    private float lastVibrateTime = int.MinValue;
    private void VibrateOnError()
    {
        if (Time.time - lastVibrateTime > 0.2f)
        {
            Handheld.Vibrate();
            lastVibrateTime = Time.time;
        }
    }

    private void DoPaintTile(Mesh mesh, int index, Color color)
    {
        Color[] temp = mesh.colors;
        temp[index * 4] = color;
        temp[index * 4 + 1] = color;
        temp[index * 4 + 2] = color;
        temp[index * 4 + 3] = color;

        mesh.colors = temp;
        mesh.RecalculateNormals();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveProgress();
        }
    }

    public void DeleteProgress()
    {
        PlayerPrefs.DeleteKey(itemStatusPrefKey);
        PlayerPrefs.DeleteKey(progressPrefKey);
        PlayerPrefs.Save();
    }

    public void SaveProgress()
    {
        if (!hasTilePainted || isGameComplete) return;

        PlayerPrefs.SetString(itemStatusPrefKey, "painting");
        UpdateMywork();

        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < tileStatus.Count; i++)
        {
            sb.Append(tileStatus[i]);
        }

        PlayerPrefs.SetString(progressPrefKey, sb.ToString());
        PlayerPrefs.Save();

        SavePaintingImage();
    }

    private void UpdateMywork()
    {
        var itemName = GameState.libraryItemName;

        if (!myWorkItem.Contains(itemName))
        {
            myWorkItem.Add(itemName);
            PlayerPrefs.SetString("mywork_items", CUtils.BuildStringFromCollection(myWorkItem));
        }
    }

    private void SavePaintingImage()
    {
        Texture2D tt = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color[] colors = new Color[size * size];
        foreach (var tile in tiles)
        {
            int index = tile.x + tile.y * size;
            colors[index] = tile.isPainted ? tile.color : tile.gray;
        }

        tt.SetPixels(colors);

        // Encode texture into PNG
        byte[] bytes = tt.EncodeToPNG();

        // Save file to disk
        string filepath = Path.Combine(Application.persistentDataPath, GameState.libraryItemName + ".png");
        File.WriteAllBytes(filepath, bytes);
    }

    public List<int> GetCompletePainting()
    {
        List<int> result = new List<int>();
        for(int i = 1; i <= colors.Count; i++)
        {
            if (!numTiles.ContainsKey(i)) result.Add(i);
        }
        return result;
    }

    public int GetSelectedNumber()
    {
        for (int i = 1; i <= colors.Count; i++)
        {
            if (numTiles.ContainsKey(i) && numTiles[i] != 0) return i;
        }
        return -1;
    }

    public void CompletePainting(int index)
    {
        bottomButtons.numButtons[index].CompletePainting();
        var selectedNumber = GetSelectedNumber();
        if (selectedNumber != -1)
        {
            bottomButtons.SetSelectionPosition(selectedNumber - 1);
            Highlight(selectedNumber);
        }
        else bottomButtons.selectionTr.gameObject.SetActive(false);
    }

    private void OnUpdateValue(float value)
    {
        boardCam.orthographicSize = value;
    }

    public void ResetVariables()
    {
        foreach(var tile in tiles)
        {
            tile.ResetVariables();
            tilePooler.FreeTile(tile);
        }
        bottomButtons.ResetVariables();
        Destroy(mainObject);
        Destroy(coverObject);
        Destroy(highlightObject);
        Destroy(errorObject);
    }
}
