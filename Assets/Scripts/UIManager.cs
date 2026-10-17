using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameObject hudRoot;
    private GameObject gameOverRoot;
    private GameObject victoryRoot;
    private GameObject pauseRoot;
    private GameObject mainMenuRoot;
    private GameObject mainMenuContent;
    private GameObject settingsMenuContent;
    private GameObject creditsMenuContent;
    private Button startGameButton;
    private Button continueButton;
    private Button gameOverRestartButton;
    private Button fullscreenButton;

    // Subtitle & Dialogue Box
    private GameObject subtitleBox;
    private Text speakerText;
    private Text dialogueText;

    // HUD Elements
    private Image healthBarFill;
    private Image bloodBarFill;
    private Text bloodLevelText;
    private Text serumStatusText;
    private Text timeOfDayText;
    private Text abilityHelpText;
    private Text bitePromptText;
    private Text alertText;

    // Boss HUD
    private GameObject bossBarRoot;
    private Image bossBarFill;

    // Game Over & Victory
    private Text gameOverTitleText;
    private Text gameOverReasonText;
    private Text gameOverActionText;
    private Text masterVolumeValueText;

    private Font gameFont;
    private Sprite solidSprite;
    private Sprite glowSprite;
    private Sprite moonSprite;
    private float masterVolume = 1f;
    private bool fullscreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gameFont = Font.CreateDynamicFontFromOSFont(new[] { "Bahnschrift", "Segoe UI", "Arial" }, 18);
        if (gameFont == null) gameFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (gameFont == null) gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        masterVolume = PlayerPrefs.GetFloat("NightboundMasterVolume", AudioListener.volume);
        fullscreen = PlayerPrefs.GetInt("NightboundFullscreen", Screen.fullScreen ? 1 : 0) == 1;
        AudioListener.volume = masterVolume;
        Screen.fullScreen = fullscreen;

        solidSprite = CreateSolidSprite();
        glowSprite = CreateGlowSprite();
        moonSprite = CreateMoonSprite();
        BuildAllUI();
    }

    private void Update()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && hudRoot != null && hudRoot.activeSelf)
        {
            if (healthBarFill != null) healthBarFill.fillAmount = player.maxHealth > 0f ? Mathf.Clamp01(player.currentHealth / player.maxHealth) : 0f;
            if (bloodBarFill != null) bloodBarFill.fillAmount = player.maxBlood > 0f ? Mathf.Clamp01(player.currentBlood / player.maxBlood) : 0f;
            if (bloodLevelText != null) bloodLevelText.text = $"BLOOD  {Mathf.CeilToInt(player.currentBlood)}/{Mathf.CeilToInt(player.maxBlood)}";

            // Serum Status
            if (serumStatusText != null)
            {
                if (player.isVampire)
                {
                    int mins = Mathf.FloorToInt(player.serumTimer / 60f);
                    int secs = Mathf.FloorToInt(player.serumTimer % 60f);
                    serumStatusText.text = $"VAMPIRE FORM - SERUM: {mins:D2}:{secs:D2}";
                    serumStatusText.color = new Color(0.9f, 0.15f, 0.15f);
                }
                else
                {
                    if (player.serumCooldownTimer > 0f)
                    {
                        serumStatusText.text = $"HUMAN FORM (WEAK) - SERUM COOLDOWN: {Mathf.CeilToInt(player.serumCooldownTimer)}s";
                        serumStatusText.color = new Color(1f, 0.7f, 0.2f);
                    }
                    else
                    {
                        serumStatusText.text = "[V] INJECT VAMPIRE SERUM (READY!)";
                        serumStatusText.color = new Color(0.2f, 1f, 0.4f);
                    }
                }
            }

            // Day / Night Status
            if (timeOfDayText != null && DayNightCycle.Instance != null)
            {
                timeOfDayText.text = DayNightCycle.Instance.GetTimeOfDayStatus();
                timeOfDayText.color = DayNightCycle.Instance.isDay ? new Color(1f, 0.5f, 0.2f) : new Color(0.3f, 0.85f, 1f);
            }

            // Bite prompt
            if (bitePromptText != null)
            {
                bitePromptText.gameObject.SetActive(player.canBiteTarget);
            }

            // Boss Health Bar
            if (BossController.Instance != null && BossController.Instance.isAlive)
            {
                if (bossBarRoot != null && !bossBarRoot.activeSelf) bossBarRoot.SetActive(true);
                if (bossBarFill != null) bossBarFill.fillAmount = BossController.Instance.currentHealth / BossController.Instance.maxHealth;
            }
            else if (bossBarRoot != null && bossBarRoot.activeSelf)
            {
                bossBarRoot.SetActive(false);
            }
        }
    }

    public void ShowHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(true);
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (victoryRoot != null) victoryRoot.SetActive(false);
        HideMainMenu();
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            if (healthBarFill != null) healthBarFill.fillAmount = player.maxHealth > 0f ? Mathf.Clamp01(player.currentHealth / player.maxHealth) : 0f;
            if (bloodBarFill != null) bloodBarFill.fillAmount = player.maxBlood > 0f ? Mathf.Clamp01(player.currentBlood / player.maxBlood) : 0f;
            if (bloodLevelText != null) bloodLevelText.text = $"BLOOD  {Mathf.CeilToInt(player.currentBlood)}/{Mathf.CeilToInt(player.maxBlood)}";
        }
    }

    public void HideMainMenu()
    {
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
    }

    public void HideMenusForCutscene()
    {
        HideMainMenu();
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (victoryRoot != null) victoryRoot.SetActive(false);
        if (pauseRoot != null) pauseRoot.SetActive(false);
    }

    public void HideHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(false);
    }

    public void ShowPause()
    {
        if (pauseRoot != null) pauseRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePause()
    {
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (GameManager.Instance != null &&
            GameManager.Instance.currentState != GameManager.GameState.GameOver &&
            GameManager.Instance.currentState != GameManager.GameState.Victory)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetSubtitleText(string speaker, string dialogue)
    {
        if (string.IsNullOrEmpty(dialogue))
        {
            if (subtitleBox != null) subtitleBox.SetActive(false);
        }
        else
        {
            if (subtitleBox != null) subtitleBox.SetActive(true);
            if (speakerText != null) speakerText.text = speaker;
            if (dialogueText != null) dialogueText.text = dialogue;
        }
    }

    public void SetAlertText(string text)
    {
        if (alertText != null)
        {
            alertText.text = text;
            alertText.gameObject.SetActive(!string.IsNullOrEmpty(text));

            bool isLevelHeading = !string.IsNullOrEmpty(text) &&
                (text.StartsWith("LEVEL ") || text.StartsWith("FINAL LEVEL:"));
            RectTransform rect = alertText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = isLevelHeading ? Vector2.zero : new Vector2(0f, 120f);
            rect.sizeDelta = isLevelHeading ? new Vector2(1200f, 80f) : new Vector2(700f, 60f);
            alertText.fontSize = isLevelHeading ? 28 : 22;
            alertText.fontStyle = isLevelHeading ? FontStyle.Bold : FontStyle.Normal;
        }
    }

    public void ShowGameOver(string reason)
    {
        HideHUD();
        if (gameOverRoot != null) gameOverRoot.SetActive(true);
        if (gameOverReasonText != null) gameOverReasonText.text = reason;

        int lvl = GameManager.Instance != null ? GameManager.Instance.currentLevelIndex : 1;
        if (gameOverActionText != null)
        {
            gameOverActionText.text = lvl == 4
                ? "THE FINAL HUNT BEGINS AGAIN."
                : "THE NIGHT IS NOT OVER.";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SelectButton(gameOverRestartButton);
    }

    public void ShowVictory()
    {
        HideHUD();
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        if (victoryRoot != null) victoryRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SelectButton(continueButton);
    }

    public void ShowMainMenu()
    {
        HideHUD();
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (victoryRoot != null) victoryRoot.SetActive(false);
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (subtitleBox != null) subtitleBox.SetActive(false);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
        ShowMainMenuContent();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SelectButton(startGameButton);
    }

    private void BuildAllUI()
    {
        EnsureEventSystem();

        GameObject canvasObj = new GameObject("NIGHTBOUND_HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        Transform c = canvasObj.transform;
        BuildInGameHUD(c);
        BuildSubtitleBox(c);
        BuildGameOverUI(c);
        BuildVictoryUI(c);
        BuildPauseUI(c);
        BuildMainMenuUI(c);
    }

    private void BuildMainMenuUI(Transform parent)
    {
        mainMenuRoot = CreatePanel(parent, "MainMenuScreen", new Color(0.008f, 0.009f, 0.016f, 0.84f));
        Image backdrop = mainMenuRoot.GetComponent<Image>();
        backdrop.raycastTarget = false;

        AddMenuImage(mainMenuRoot.transform, glowSprite, new Color(0.48f, 0.025f, 0.045f, 0.52f),
            new Vector2(0.82f, 0.76f), new Vector2(820f, 820f));
        AddMenuImage(mainMenuRoot.transform, moonSprite, new Color(1f, 0.72f, 0.67f, 0.82f),
            new Vector2(0.83f, 0.79f), new Vector2(300f, 300f));
        AddMenuRule(mainMenuRoot.transform, new Vector2(0.5f, 0.875f), 180f, new Color(0.75f, 0.16f, 0.18f, 0.8f));

        Text edition = CreateAnchoredMenuText(mainMenuRoot.transform, "A NIGHTBOUND STORY", 14,
            new Vector2(0.5f, 0.875f), new Vector2(360f, 30f), TextAnchor.MiddleCenter);
        edition.color = new Color(0.87f, 0.7f, 0.62f);
        edition.fontStyle = FontStyle.Bold;

        Text title = CreateAnchoredMenuText(mainMenuRoot.transform, "NIGHTBOUND", 66,
            new Vector2(0.5f, 0.79f), new Vector2(700f, 105f), TextAnchor.MiddleCenter);
        title.color = new Color(0.94f, 0.88f, 0.84f);
        title.fontStyle = FontStyle.Bold;
        Outline titleOutline = title.gameObject.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.36f, 0.015f, 0.035f, 0.9f);
        titleOutline.effectDistance = new Vector2(2f, -3f);

        Text subtitle = CreateAnchoredMenuText(mainMenuRoot.transform, "THE NIGHT REMEMBERS.", 19,
            new Vector2(0.5f, 0.70f), new Vector2(580f, 42f), TextAnchor.MiddleCenter);
        subtitle.color = new Color(0.78f, 0.72f, 0.7f);
        subtitle.fontStyle = FontStyle.Italic;

        mainMenuContent = CreateMenuView(mainMenuRoot.transform, "MainMenuOptions");
        PositionMenuView(mainMenuContent, new Vector2(0.5f, 0.405f), new Vector2(460f, 360f));
        startGameButton = CreateMenuButton(mainMenuContent.transform, "BEGIN THE HUNT", 92f,
            () => GameManager.Instance?.StartPrologueCutscene());
        startGameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 56f);
        CreateMenuButton(mainMenuContent.transform, "SETTINGS", 22f, ShowSettingsContent);
        CreateMenuButton(mainMenuContent.transform, "CREDITS", -42f, ShowCreditsContent);
        CreateMenuButton(mainMenuContent.transform, "QUIT", -106f, Application.Quit);

        Text controls = CreateAnchoredMenuText(mainMenuRoot.transform, "MOUSE  /  ENTER TO SELECT",
            12, new Vector2(0.5f, 0.095f), new Vector2(430f, 28f), TextAnchor.MiddleCenter);
        controls.color = new Color(0.68f, 0.64f, 0.65f, 0.8f);
        AddMenuRule(mainMenuRoot.transform, new Vector2(0.5f, 0.125f), 290f, new Color(0.68f, 0.61f, 0.56f, 0.35f));

        settingsMenuContent = CreateMenuView(mainMenuRoot.transform, "SettingsOptions");
        CreateMenuHeading(settingsMenuContent.transform, "SETTINGS");
        Text volumeText = CreateMenuText(settingsMenuContent.transform, "MASTER VOLUME", 18, new Vector2(0f, 45f));
        volumeText.color = new Color(0.86f, 0.82f, 0.82f);
        Text volumeValue = CreateMenuText(settingsMenuContent.transform, "", 18, new Vector2(0f, -20f));
        Button volumeDown = CreateMenuButton(settingsMenuContent.transform, "VOLUME -", 0f, () => AdjustMasterVolume(-0.1f));
        volumeDown.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, -20f);
        Button volumeUp = CreateMenuButton(settingsMenuContent.transform, "VOLUME +", 0f, () => AdjustMasterVolume(0.1f));
        volumeUp.GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, -20f);
        volumeValue.rectTransform.sizeDelta = new Vector2(90f, 48f);
        volumeText.gameObject.name = "MasterVolumeLabel";
        volumeValue.gameObject.name = "MasterVolumeValue";
        masterVolumeValueText = volumeValue;
        UpdateMasterVolumeLabel();
        fullscreenButton = CreateMenuButton(settingsMenuContent.transform, "", -95f, ToggleFullscreen);
        UpdateFullscreenButton();
        CreateMenuButton(settingsMenuContent.transform, "BACK", -165f, ShowMainMenuContent);

        creditsMenuContent = CreateMenuView(mainMenuRoot.transform, "CreditsOptions");
        CreateMenuHeading(creditsMenuContent.transform, "CREDITS");
        Text creditLines = CreateMenuText(creditsMenuContent.transform,
            "NIGHTBOUND\nA story of Rayan Malik and Kaleem Rana\n\nThank you for playing.",
            20, new Vector2(0f, -5f));
        creditLines.rectTransform.sizeDelta = new Vector2(700f, 180f);
        CreateMenuButton(creditsMenuContent.transform, "BACK", -150f, ShowMainMenuContent);

        mainMenuRoot.SetActive(false);
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("NIGHTBOUND_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
        else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null &&
                 eventSystem.GetComponent<BaseInputModule>() == null)
        {
            InputSystemUIInputModule inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
    }

    private GameObject CreateMenuView(Transform parent, string name)
    {
        GameObject view = new GameObject(name, typeof(RectTransform));
        view.transform.SetParent(parent, false);
        Stretch(view.GetComponent<RectTransform>());
        return view;
    }

    private Button CreateMenuButton(Transform parent, string label, float y, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label.Replace(" ", string.Empty) + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, y);
        buttonRect.sizeDelta = new Vector2(270f, 42f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.035f, 0.05f, 0.96f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        bool isPrimary = label == "BEGIN THE HUNT" || label == "CONTINUE THE STORY";
        image.color = isPrimary
            ? new Color(0.19f, 0.025f, 0.04f, 0.94f)
            : new Color(0.025f, 0.022f, 0.03f, 0.64f);
        button.colors = new ColorBlock
        {
            normalColor = image.color,
            highlightedColor = new Color(0.38f, 0.055f, 0.08f, 1f),
            pressedColor = new Color(0.65f, 0.12f, 0.16f, 1f),
            selectedColor = new Color(0.32f, 0.045f, 0.07f, 0.96f),
            disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
        button.onClick.AddListener(onClick);

        GameObject accent = new GameObject("CrimsonAccent", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(buttonObject.transform, false);
        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0.16f);
        accentRect.anchorMax = new Vector2(0f, 0.84f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = new Vector2(8f, 0f);
        accentRect.sizeDelta = new Vector2(isPrimary ? 3f : 2f, 0f);
        accent.GetComponent<Image>().color = new Color(0.88f, 0.12f, 0.18f, isPrimary ? 1f : 0.72f);
        accent.GetComponent<Image>().raycastTarget = false;

        Text text = CreateText(buttonObject.transform, label, 16, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        text.rectTransform.sizeDelta = Vector2.zero;
        text.color = isPrimary ? new Color(1f, 0.92f, 0.87f) : new Color(0.84f, 0.8f, 0.78f);
        text.fontStyle = isPrimary ? FontStyle.Bold : FontStyle.Normal;
        text.raycastTarget = false;
        return button;
    }

    private Text CreateAnchoredMenuText(Transform parent, string content, int fontSize, Vector2 anchor,
        Vector2 size, TextAnchor alignment)
    {
        Text text = CreateText(parent, content, fontSize, alignment);
        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        return text;
    }

    private static void PositionMenuView(GameObject view, Vector2 anchor, Vector2 size)
    {
        RectTransform rect = view.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private void AddMenuImage(Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 size)
    {
        GameObject imageObject = new GameObject("MenuAtmosphere", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
    }

    private void AddMenuRule(Transform parent, Vector2 anchor, float width, Color color)
    {
        GameObject rule = new GameObject("MenuRule", typeof(RectTransform), typeof(Image));
        rule.transform.SetParent(parent, false);
        RectTransform rect = rule.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(width, 1f);
        Image image = rule.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private Text CreateMenuText(Transform parent, string content, int fontSize, Vector2 position)
    {
        Text text = CreateText(parent, content, fontSize, TextAnchor.MiddleCenter);
        text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = new Vector2(700f, 48f);
        return text;
    }

    private void CreateMenuHeading(Transform parent, string content)
    {
        Text heading = CreateMenuText(parent, content, 36, new Vector2(0f, 140f));
        heading.color = new Color(0.86f, 0.12f, 0.19f);
    }

    private void ShowMainMenuContent()
    {
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
        if (mainMenuContent != null) mainMenuContent.SetActive(true);
        if (settingsMenuContent != null) settingsMenuContent.SetActive(false);
        if (creditsMenuContent != null) creditsMenuContent.SetActive(false);
        SelectButton(startGameButton);
    }

    private void ShowSettingsContent()
    {
        if (mainMenuContent != null) mainMenuContent.SetActive(false);
        if (settingsMenuContent != null) settingsMenuContent.SetActive(true);
        if (creditsMenuContent != null) creditsMenuContent.SetActive(false);
        SelectButton(fullscreenButton);
    }

    private void ShowCreditsContent()
    {
        if (mainMenuContent != null) mainMenuContent.SetActive(false);
        if (settingsMenuContent != null) settingsMenuContent.SetActive(false);
        if (creditsMenuContent != null) creditsMenuContent.SetActive(true);
    }

    private void AdjustMasterVolume(float adjustment)
    {
        masterVolume = Mathf.Clamp01(masterVolume + adjustment);
        AudioListener.volume = masterVolume;
        PlayerPrefs.SetFloat("NightboundMasterVolume", masterVolume);
        PlayerPrefs.Save();
        UpdateMasterVolumeLabel();
    }

    private void UpdateMasterVolumeLabel()
    {
        if (masterVolumeValueText != null)
            masterVolumeValueText.text = $"{Mathf.RoundToInt(masterVolume * 100f)}%";
    }

    private void ToggleFullscreen()
    {
        fullscreen = !fullscreen;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("NightboundFullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        UpdateFullscreenButton();
    }

    private void UpdateFullscreenButton()
    {
        if (fullscreenButton == null) return;
        Text label = fullscreenButton.GetComponentInChildren<Text>();
        if (label != null) label.text = fullscreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF";
    }

    private static void SelectButton(Button button)
    {
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private void BuildSubtitleBox(Transform parent)
    {
        subtitleBox = CreatePanel(parent, "SubtitleBox", new Color(0.04f, 0.04f, 0.06f, 0.9f));
        RectTransform rt = subtitleBox.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 40f);
        rt.sizeDelta = new Vector2(850f, 90f);

        speakerText = CreateText(subtitleBox.transform, "SPEAKER", 16, TextAnchor.UpperLeft);
        speakerText.color = new Color(0.9f, 0.2f, 0.2f);
        speakerText.rectTransform.anchoredPosition = new Vector2(20f, -10f);

        dialogueText = CreateText(subtitleBox.transform, "Dialogue lines go here...", 18, TextAnchor.UpperLeft);
        dialogueText.rectTransform.anchoredPosition = new Vector2(20f, -34f);
        dialogueText.rectTransform.sizeDelta = new Vector2(810f, 50f);

        subtitleBox.SetActive(false);
    }

    private void BuildInGameHUD(Transform parent)
    {
        hudRoot = new GameObject("HUDContainer", typeof(RectTransform));
        hudRoot.transform.SetParent(parent, false);
        Stretch(hudRoot.GetComponent<RectTransform>());

        // Health Bar (Bottom Left)
        healthBarFill = CreateBar(hudRoot.transform, "HealthBar", new Vector2(160f, 78f), new Vector2(280f, 24f),
            new Color(0.15f, 0.02f, 0.02f, 0.85f), new Color(0.85f, 0.1f, 0.15f));

        // Blood Bar (Bottom Left)
        bloodBarFill = CreateBar(hudRoot.transform, "BloodBar", new Vector2(145f, 38f), new Vector2(220f, 15f),
            new Color(0.08f, 0.02f, 0.15f, 0.85f), new Color(0.6f, 0.08f, 0.85f));
        bloodLevelText = CreateText(hudRoot.transform, "BLOOD  70/100", 14, TextAnchor.MiddleLeft);
        bloodLevelText.rectTransform.anchorMin = new Vector2(1f, 0f);
        bloodLevelText.rectTransform.anchorMax = new Vector2(1f, 0f);
        bloodLevelText.rectTransform.pivot = new Vector2(1f, 0f);
        bloodLevelText.rectTransform.anchoredPosition = new Vector2(-28f, 68f);
        bloodLevelText.rectTransform.sizeDelta = new Vector2(260f, 28f);
        bloodLevelText.color = new Color(0.82f, 0.58f, 1f);
        bloodLevelText.fontSize = 16;
        bloodLevelText.fontStyle = FontStyle.Bold;

        // Serum Status (Bottom Left)
        serumStatusText = CreateText(hudRoot.transform, "VAMPIRE FORM - SERUM: 02:00", 16, TextAnchor.MiddleLeft);
        serumStatusText.rectTransform.anchoredPosition = new Vector2(170f, 100f);

        // Day/Night Indicator (Top Center)
        timeOfDayText = CreateText(hudRoot.transform, "NIGHT [SAFE]", 18, TextAnchor.MiddleCenter);
        timeOfDayText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        timeOfDayText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        timeOfDayText.rectTransform.pivot = new Vector2(0.5f, 1f);
        timeOfDayText.rectTransform.anchoredPosition = new Vector2(0f, -8f);
        timeOfDayText.rectTransform.sizeDelta = new Vector2(640f, 32f);

        // Ability Controls Help (Bottom Right)
        abilityHelpText = CreateText(hudRoot.transform, "[Q] Shadow Step  |  [Shift] Time Slow  |  [E] Bite  |  [L-Click] Attack", 14, TextAnchor.MiddleRight);
        abilityHelpText.rectTransform.anchorMin = new Vector2(1f, 0f);
        abilityHelpText.rectTransform.anchorMax = new Vector2(1f, 0f);
        abilityHelpText.rectTransform.pivot = new Vector2(1f, 0f);
        abilityHelpText.rectTransform.anchoredPosition = new Vector2(-20f, 24f);
        abilityHelpText.rectTransform.sizeDelta = new Vector2(760f, 28f);
        abilityHelpText.color = new Color(0.8f, 0.8f, 0.85f, 0.7f);

        // Crosshair
        GameObject crosshair = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
        crosshair.transform.SetParent(hudRoot.transform, false);
        RectTransform chRect = crosshair.GetComponent<RectTransform>();
        chRect.anchorMin = new Vector2(0.5f, 0.5f);
        chRect.anchorMax = new Vector2(0.5f, 0.5f);
        chRect.sizeDelta = new Vector2(6f, 6f);
        crosshair.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.65f);

        // Bite Prompt
        bitePromptText = CreateText(hudRoot.transform, "[E] BITE ENEMY (+Blood & Heal)", 22, TextAnchor.MiddleCenter);
        bitePromptText.rectTransform.anchoredPosition = new Vector2(0f, -70f);
        bitePromptText.color = new Color(1f, 0.2f, 0.2f);
        bitePromptText.gameObject.SetActive(false);

        // Alert Text
        alertText = CreateText(hudRoot.transform, "", 22, TextAnchor.MiddleCenter);
        alertText.rectTransform.anchoredPosition = new Vector2(0f, 120f);
        alertText.rectTransform.sizeDelta = new Vector2(1200f, 80f);
        alertText.color = new Color(1f, 0.3f, 0.15f);

        // Boss Bar (Top)
        bossBarRoot = new GameObject("BossBarContainer", typeof(RectTransform));
        bossBarRoot.transform.SetParent(hudRoot.transform, false);
        RectTransform bRect = bossBarRoot.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.5f, 1f);
        bRect.anchorMax = new Vector2(0.5f, 1f);
        bRect.pivot = new Vector2(0.5f, 1f);
        bRect.anchoredPosition = new Vector2(0f, -55f);
        bRect.sizeDelta = new Vector2(450f, 22f);

        bossBarFill = CreateBar(bossBarRoot.transform, "BossHealth", Vector2.zero, new Vector2(450f, 22f),
            new Color(0.2f, 0.05f, 0.05f, 0.9f), new Color(0.9f, 0.15f, 0.15f));
        Text bossTitle = CreateText(bossBarRoot.transform, "SYNDICATE BOSS", 14, TextAnchor.MiddleCenter);
        bossTitle.rectTransform.anchoredPosition = new Vector2(0f, 22f);
        bossBarRoot.SetActive(false);
    }

    private void BuildGameOverUI(Transform parent)
    {
        gameOverRoot = CreatePanel(parent, "GameOverScreen", new Color(0.008f, 0.009f, 0.016f, 0.86f));
        gameOverRoot.GetComponent<Image>().raycastTarget = false;
        AddMenuImage(gameOverRoot.transform, glowSprite, new Color(0.55f, 0.018f, 0.04f, 0.45f),
            new Vector2(0.5f, 0.58f), new Vector2(850f, 850f));
        AddMenuRule(gameOverRoot.transform, new Vector2(0.5f, 0.66f), 220f, new Color(0.82f, 0.13f, 0.17f, 0.8f));

        Text eyebrow = CreateAnchoredMenuText(gameOverRoot.transform, "THE HUNT ENDS HERE", 13,
            new Vector2(0.5f, 0.66f), new Vector2(420f, 30f), TextAnchor.MiddleCenter);
        eyebrow.color = new Color(0.83f, 0.62f, 0.57f);
        gameOverTitleText = CreateAnchoredMenuText(gameOverRoot.transform, "THE NIGHT TAKES YOU", 38,
            new Vector2(0.5f, 0.57f), new Vector2(900f, 68f), TextAnchor.MiddleCenter);
        gameOverTitleText.color = new Color(0.94f, 0.86f, 0.82f);
        gameOverTitleText.fontStyle = FontStyle.Bold;
        gameOverReasonText = CreateAnchoredMenuText(gameOverRoot.transform, "SLAIN BY THE SYNDICATE", 18,
            new Vector2(0.5f, 0.49f), new Vector2(760f, 44f), TextAnchor.MiddleCenter);
        gameOverReasonText.color = new Color(0.77f, 0.7f, 0.68f);
        gameOverActionText = CreateAnchoredMenuText(gameOverRoot.transform, "THE NIGHT IS NOT OVER.", 14,
            new Vector2(0.5f, 0.43f), new Vector2(700f, 34f), TextAnchor.MiddleCenter);
        gameOverActionText.color = new Color(0.79f, 0.18f, 0.2f);
        gameOverRestartButton = CreateMenuButton(gameOverRoot.transform, "RISE AGAIN", -50f,
            () =>
            {
                if (GameManager.Instance == null) return;
                int level = GameManager.Instance.currentLevelIndex == 4 ? 1 : GameManager.Instance.currentLevelIndex;
                GameManager.Instance.StartLevel(level);
            });
        gameOverRestartButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -55f);
        CreateMenuButton(gameOverRoot.transform, "RETURN TO MAIN MENU", -115f,
            () => GameManager.Instance?.ReturnToMainMenu());

        gameOverRoot.SetActive(false);
    }

    private void BuildVictoryUI(Transform parent)
    {
        victoryRoot = CreatePanel(parent, "VictoryScreen", new Color(0.008f, 0.009f, 0.016f, 0.86f));
        victoryRoot.GetComponent<Image>().raycastTarget = false;
        AddMenuImage(victoryRoot.transform, glowSprite, new Color(0.48f, 0.025f, 0.045f, 0.48f),
            new Vector2(0.5f, 0.68f), new Vector2(900f, 900f));
        AddMenuImage(victoryRoot.transform, moonSprite, new Color(1f, 0.72f, 0.67f, 0.56f),
            new Vector2(0.5f, 0.72f), new Vector2(220f, 220f));
        AddMenuRule(victoryRoot.transform, new Vector2(0.5f, 0.62f), 200f, new Color(0.85f, 0.16f, 0.18f, 0.85f));

        Text eyebrow = CreateAnchoredMenuText(victoryRoot.transform, "THE LAST CHAPTER", 13,
            new Vector2(0.5f, 0.62f), new Vector2(420f, 28f), TextAnchor.MiddleCenter);
        eyebrow.color = new Color(0.86f, 0.68f, 0.58f);
        Text title = CreateAnchoredMenuText(victoryRoot.transform, "THE END", 48,
            new Vector2(0.5f, 0.53f), new Vector2(760f, 74f), TextAnchor.MiddleCenter);
        title.color = new Color(0.96f, 0.91f, 0.85f);
        title.fontStyle = FontStyle.Bold;
        Text desc = CreateAnchoredMenuText(victoryRoot.transform,
            "THE SYNDICATE HAS FALLEN.\nBUT THE NIGHT ALWAYS RETURNS.", 17,
            new Vector2(0.5f, 0.43f), new Vector2(760f, 70f), TextAnchor.MiddleCenter);
        desc.color = new Color(0.78f, 0.71f, 0.68f);

        continueButton = CreateMenuButton(victoryRoot.transform, "CONTINUE THE STORY", -65f,
            () => GameManager.Instance?.ContinueAfterVictory());
        continueButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -35f);
        continueButton.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 52f);
        CreateMenuButton(victoryRoot.transform, "MAIN MENU", -105f,
            () => GameManager.Instance?.ReturnToMainMenu()).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);

        victoryRoot.SetActive(false);
    }

    private void BuildPauseUI(Transform parent)
    {
        pauseRoot = CreatePanel(parent, "PauseScreen", new Color(0.02f, 0.025f, 0.04f, 0.94f));

        Text title = CreateText(pauseRoot.transform, "PAUSED", 38, TextAnchor.MiddleCenter);
        title.rectTransform.anchoredPosition = new Vector2(0f, 80f);
        title.color = new Color(0.85f, 0.8f, 0.6f);

        Text actions = CreateText(pauseRoot.transform, "[ ESC ] RESUME\n[ R ] RESTART LEVEL\n[ Q ] QUIT", 20, TextAnchor.MiddleCenter);
        actions.rectTransform.anchoredPosition = new Vector2(0f, -15f);
        actions.rectTransform.sizeDelta = new Vector2(700f, 150f);
        actions.color = Color.white;

        pauseRoot.SetActive(false);
    }

    private Image CreateBar(Transform parent, string name, Vector2 pos, Vector2 size, Color bgColor, Color fillColor)
    {
        GameObject bgObj = new GameObject(name + "_BG", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(parent, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.zero;
        bgRect.anchoredPosition = pos;
        bgRect.sizeDelta = size;
        bgObj.GetComponent<Image>().color = bgColor;

        GameObject fillObj = new GameObject(name + "_Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(bgObj.transform, false);
        Stretch(fillObj.GetComponent<RectTransform>());
        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.sprite = solidSprite;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.color = fillColor;
        return fillImg;
    }

    private Text CreateText(Transform parent, string content, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        Text t = obj.GetComponent<Text>();
        t.font = gameFont;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.text = content;
        t.color = Color.white;
        RectTransform r = obj.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(700f, 60f);
        return t;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Stretch(panel.GetComponent<RectTransform>());
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateGlowSprite()
    {
        const int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.name = "NightboundMenuGlow";
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = (x + 0.5f) / resolution * 2f - 1f;
                float dy = (y + 0.5f) / resolution * 2f - 1f;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy)), 2.4f);
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateMoonSprite()
    {
        const int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.name = "NightboundMenuMoon";
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[resolution * resolution];
        Color core = new Color(0.72f, 0.12f, 0.15f);
        Color edge = new Color(0.13f, 0.018f, 0.035f);
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = (x + 0.5f) / resolution * 2f - 1f;
                float dy = (y + 0.5f) / resolution * 2f - 1f;
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((1f - radius) * resolution * 0.7f);
                float shade = Mathf.Clamp01(0.85f - radius * 0.45f + dy * 0.08f);
                Color color = Color.Lerp(edge, core, shade);
                pixels[y * resolution + x] = new Color(color.r, color.g, color.b, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
