using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GraterGameManager : MonoBehaviour
{
    public static GraterGameManager instance;
    public AudioSource AudioSource;
    public AudioClip CompleteSound;
    public AudioClip FailSound;
    public AudioClip KnifeCutSound;
    public GameObject levelcomplete, levelfail, ingame, playerRank, aiRank, lfPRank, lfAiRank;

    public Text leveltext, scoretext;

    public Image fill1, ailevelfill, aiFillBar, aiImage, playerImage;
    public Text AiWinpercentage, playerwinper;
    public float score;
    public float aiscore;
    public Text AiscoreText;

    public GameObject particale;
    public Material AiBoard;

    public float speed;
    public GraterColliderScript gmcontrol;

    [Header("Background + Pause (auto-built at runtime, no scene wiring needed)")]
    public bool autoBuildBackgroundAndPause = true;

    private GameObject _pauseButton;
    private GameObject _pausePanel;
    private bool _isPaused;

    void Start()
    {
        RenderSettings.fogStartDistance = 60;
        RenderSettings.fogEndDistance = 100;
        gmcontrol = FindObjectOfType<GraterColliderScript>();
        //RenderSettings.fogColor = Random.ColorHSV(0,360,24,25,89,90,99,100);
        GameObject.FindGameObjectWithTag("AIsetup").transform.position = new Vector3(-10.5f, 0, -6.1f);
        leveltext.text = "LEVEL" + (SceneManager.GetActiveScene().buildIndex).ToString();

        if (MenuScript.mul == 0)
        {
            GameObject.FindGameObjectWithTag("AiObjectSpawnPoint").SetActive(false);
            GameObject.FindGameObjectWithTag("AIsetup").SetActive(false);
            GameObject.FindGameObjectWithTag("AiPlayer").SetActive(false);
        }
        if (MenuScript.mul == 1)
        {
            Camera.main.transform.position = new Vector3(1f, 18.7f, -19.4f);
            Camera.main.fieldOfView = 78;
            aiFillBar.gameObject.SetActive(true);
            AiscoreText.gameObject.SetActive(true);
        }

        if (autoBuildBackgroundAndPause)
        {
            CleanupMissingIcons();
            BuildBackgroundCanvas();
            BuildPauseButtonAndPanel();
            BuildCoinHud();
        }
    }

    int i = 0;
    public platform[] pl;
    public platform[] pl2;

    void Update()
    {
        scoretext.text = score.ToString();
        if (MenuScript.mul == 1)
        {
            AiscoreText.text = aiscore.ToString();
        }

        if (fill1.fillAmount == 1 && i == 0)
        {
            FindObjectOfType<cameramove>().enabled = true;
            gameover();
            Debug.Log("levelcomplete");
            Invoke("LevelComplete", 1.2f);
            pl = FindObjectsOfType<platform>();
            FindObjectOfType<PlatforMove>().tomakecancelinvoke();

            for (int i = 0; i < pl.Length; i++)
            {
                pl[i].stopmovement = false;
            }
            Instantiate(particale, new Vector3(100, 25, 61), Quaternion.Euler(23, 29, -4));
            i = 1;
        }
        if (ailevelfill.fillAmount == 1 && i == 0 && FindObjectOfType<AIScript>())
        {
            LevelFail();
            Debug.Log("levelfail");
            FindObjectOfType<PlatforMove>().tomakecancelinvoke();

            pl2 = FindObjectsOfType<platform>();
            for (int i = 0; i < pl2.Length; i++)
            {
                pl2[i].stopmovement = false;
            }
            i = 1;
        }
    }

    public void LevelComplete()
    {
        AudioSource.PlayOneShot(CompleteSound);
        levelcomplete.SetActive(true);
        ingame.SetActive(false);
        if (_pauseButton != null) _pauseButton.SetActive(false);

        int level = int.Parse(SceneManager.GetActiveScene().name);
        PlayerPrefs.SetString("level", (level + 1).ToString());

        if (MenuScript.mul == 1)
        {
            playerwinper.text = (int)((score / gmcontrol.totalscore) * 100) + "%";
            AiWinpercentage.text = (int)((aiscore / gmcontrol.totalscore) * 100) + "%";
            GameObject.FindGameObjectWithTag("AiPlayer").SetActive(false);
            aiImage.gameObject.SetActive(true);
            playerImage.gameObject.SetActive(true);
        }
        else
        {
            playerRank.SetActive(false);
            aiRank.SetActive(false);
        }
    }
    public void LevelFail()
    {
        AudioSource.PlayOneShot(FailSound);
        levelfail.SetActive(true);
        ingame.SetActive(false);
        if (_pauseButton != null) _pauseButton.SetActive(false);
        if (MenuScript.mul == 1)
        {
            playerwinper.text = (int)((score / gmcontrol.totalscore) * 100) + "%";
            AiWinpercentage.text = (int)((aiscore / gmcontrol.totalscore) * 100) + "%";
            aiImage.gameObject.SetActive(true);
            playerImage.gameObject.SetActive(true);
        }
        else
        {
            lfAiRank.SetActive(false);
            lfPRank.SetActive(false);
        }
    }

    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void gameover()
    {
        FindObjectOfType<GraterInputManager>().enabled = false;
        FindObjectOfType<GraterInputManager>().GetComponent<Animator>().enabled = false;
        FindObjectOfType<GraterColliderScript>().GetComponent<MeshCollider>().enabled = false;
        FindObjectOfType<PlatforMove>().enabled = false;
        FindObjectOfType<platform>().enabled = false;
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // ----------------------------------------------------------------------
    // Background + Pause (built at runtime so every level gets it automatically,
    // without needing to hand-wire a Canvas/Button into all 100 level scenes).
    // ----------------------------------------------------------------------

    /// <summary>Xoá các icon cross-promo còn sót (script bị gỡ khi dọn ads) trên panel Complete/Fail.</summary>
    private void CleanupMissingIcons()
    {
        RemoveIconChildren(levelcomplete);
        RemoveIconChildren(levelfail);
    }

    private void RemoveIconChildren(GameObject panel)
    {
        if (panel == null) return;

        for (int idx = panel.transform.childCount - 1; idx >= 0; idx--)
        {
            var child = panel.transform.GetChild(idx);
            if (child.name == "Icon" || child.name.StartsWith("Icon ("))
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>Màu đại diện lấy từ image 96 (điểm giữa dải sunburst vàng-cam) - dùng làm màu dự phòng nếu thiếu shader.</summary>
    private static readonly Color BackgroundColor96 = new Color(0.992f, 0.969f, 0.424f, 1f);

    // Màu gradient nền - lấy tông từ image 96 (xanh trên -> cam dưới) nhưng đậm/bão hoà hơn cho đẹp.
    private static readonly Color BackgroundGradientTop = new Color(0.05f, 0.20f, 0.75f, 1f);
    private static readonly Color BackgroundGradientBottom = new Color(0.85f, 0.30f, 0.03f, 1f);

    private static Texture2D s_backgroundGradientTex;

    /// <summary>
    /// Nền gradient cho level, tông màu lấy từ image 96 (xanh trên -> cam dưới), đậm hơn bản gốc cho đẹp.
    /// Tự sinh 1 texture gradient nhỏ rồi vẽ bằng 1 quad gắn làm con của Main Camera (scale theo FOV/aspect
    /// nên luôn che kín khung hình ở mọi level, không lệ thuộc vị trí/camera từng level).
    /// </summary>
    private void BuildBackgroundCanvas()
    {
        var oldBgQuad = GameObject.Find("Managers/PresetManager/Background");
        if (oldBgQuad != null) oldBgQuad.SetActive(false);

        var cam = Camera.main;
        if (cam == null) return;

        // Màu phẳng làm nền dự phòng (hiện ngay cả khi vì lý do gì đó quad gradient chưa kịp vẽ).
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BackgroundColor96;

        float distance = 80f;
        float height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * cam.aspect;

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "BackgroundGradient";
        Destroy(quad.GetComponent<Collider>());
        quad.transform.SetParent(cam.transform, false);
        quad.transform.localPosition = new Vector3(0f, 0f, distance);
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(width, height, 1f);

        if (s_backgroundGradientTex == null)
            s_backgroundGradientTex = BuildGradientTexture(BackgroundGradientTop, BackgroundGradientBottom, 128);

        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = s_backgroundGradientTex;
        quad.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static Texture2D BuildGradientTexture(Color top, Color bottom, int height)
    {
        var tex = new Texture2D(1, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < height; y++)
        {
            float t = y / (float)(height - 1);
            tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
        }
        tex.Apply();
        return tex;
    }

    /// <summary>Coin HUD góc trên phải — dùng icon tiền (Resources/iv_money), điểm = CoinWallet.</summary>
    private void BuildCoinHud()
    {
        if (ingame == null) return;
        Transform guiCanvas = ingame.transform.parent;
        if (guiCanvas == null) return;

        if (guiCanvas.Find("Coin HUD") != null) return;

        var hudGO = new GameObject("Coin HUD", typeof(RectTransform));
        hudGO.transform.SetParent(guiCanvas, false);
        var hudRT = (RectTransform)hudGO.transform;
        hudRT.anchorMin = new Vector2(1f, 1f);
        hudRT.anchorMax = new Vector2(1f, 1f);
        hudRT.pivot = new Vector2(1f, 1f);
        hudRT.anchoredPosition = new Vector2(-24f, -24f);
        hudRT.sizeDelta = new Vector2(280f, 80f);

        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGO.transform.SetParent(hudGO.transform, false);
        var iconRT = (RectTransform)iconGO.transform;
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot = new Vector2(0f, 0.5f);
        iconRT.anchoredPosition = new Vector2(8f, 0f);
        iconRT.sizeDelta = new Vector2(64f, 64f);
        var iconImg = iconGO.GetComponent<Image>();
        iconImg.sprite = Resources.Load<Sprite>("iv_money");
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        var textGO = new GameObject("Coins Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(hudGO.transform, false);
        var textRT = (RectTransform)textGO.transform;
        textRT.anchorMin = new Vector2(0f, 0.5f);
        textRT.anchorMax = new Vector2(0f, 0.5f);
        textRT.pivot = new Vector2(0f, 0.5f);
        textRT.anchoredPosition = new Vector2(84f, 0f);
        textRT.sizeDelta = new Vector2(180f, 64f);
        var label = textGO.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "{0}";
        label.fontSize = 42;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(1f, 0.85f, 0.32f, 1f);
        label.raycastTarget = false;

        var coinHud = hudGO.AddComponent<CoinHUD>();
        coinHud.BindCoinsText(label);
    }

    /// <summary>Nút Pause góc trái + panel Resume/Restart/Home, tái dùng sprite sẵn có (UiRing, Retry, Home, Next).</summary>
    private void BuildPauseButtonAndPanel()
    {
        if (ingame == null) return;
        Transform guiCanvas = ingame.transform.parent;
        if (guiCanvas == null) return;

        var fillBar = ingame.transform.Find("FillBar");
        Sprite ringSprite = fillBar != null ? fillBar.GetComponent<Image>().sprite : null;

        var btnGO = new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(guiCanvas, false);
        var btnRT = (RectTransform)btnGO.transform;
        btnRT.anchorMin = new Vector2(0f, 1f);
        btnRT.anchorMax = new Vector2(0f, 1f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(120f, -120f);
        btnRT.sizeDelta = new Vector2(150f, 150f);

        var btnImg = btnGO.GetComponent<Image>();
        btnImg.sprite = ringSprite;
        btnImg.color = new Color(0f, 0f, 0f, 0.55f);

        CreatePauseBar(btnGO.transform, new Vector2(-18f, 0f));
        CreatePauseBar(btnGO.transform, new Vector2(18f, 0f));

        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(TogglePause);
        _pauseButton = btnGO;

        BuildPausePanel(guiCanvas);
    }

    private void CreatePauseBar(Transform parent, Vector2 pos)
    {
        var barGO = new GameObject("Bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barGO.transform.SetParent(parent, false);
        var rt = (RectTransform)barGO.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(16f, 55f);
        barGO.GetComponent<Image>().color = Color.white;
    }

    private void BuildPausePanel(Transform guiCanvas)
    {
        if (levelfail == null || levelcomplete == null) return;

        var retrySrc = levelfail.transform.Find("RetryButton");
        var homeSrc = levelfail.transform.Find("HomeButton");
        var nextSrc = levelcomplete.transform.Find("NextButton");
        if (retrySrc == null || homeSrc == null || nextSrc == null) return;

        var panelGO = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGO.transform.SetParent(guiCanvas, false);
        var panelRT = (RectTransform)panelGO.transform;
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        // Khoảng cách giữa tâm các nút - đủ rộng để không đè lên nhau kể cả khi nút gốc to.
        float spacing = 260f;

        var resumeBtn = CloneMenuButton(nextSrc, panelGO.transform, new Vector2(0f, spacing));
        resumeBtn.onClick.AddListener(ResumeGame);

        var restartBtn = CloneMenuButton(retrySrc, panelGO.transform, new Vector2(0f, 0f));
        restartBtn.onClick.AddListener(RestartButton);

        var homeBtn = CloneMenuButton(homeSrc, panelGO.transform, new Vector2(0f, -spacing));
        homeBtn.onClick.AddListener(Home);

        panelGO.SetActive(false);
        _pausePanel = panelGO;
    }

    private Button CloneMenuButton(Transform source, Transform newParent, Vector2 anchoredPos)
    {
        var clone = Instantiate(source.gameObject, newParent);
        clone.name = source.name + "_Pause";
        var rt = (RectTransform)clone.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        var btn = clone.GetComponent<Button>();
        if (btn != null) btn.onClick.RemoveAllListeners();

        clone.SetActive(true);
        return btn;
    }

    public void TogglePause()
    {
        if (_pausePanel == null) return;
        if (_isPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        if (_pausePanel == null) return;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void ResumeGame()
    {
        if (_pausePanel == null) return;
        _pausePanel.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;
    }
}
