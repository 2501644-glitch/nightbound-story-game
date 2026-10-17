using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameObject hudRoot;
    private GameObject gameOverRoot;
    private GameObject victoryRoot;
    private GameObject pauseRoot;

    // Subtitle & Dialogue Box
    private GameObject subtitleBox;
    private Text speakerText;
    private Text dialogueText;

    // HUD Elements
    private Image healthBarFill;
    private Image bloodBarFill;
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

    private Font gameFont;
    private Sprite solidSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gameFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (gameFont == null) gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        solidSprite = CreateSolidSprite();
        BuildAllUI();
    }

    private void Update()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && hudRoot != null && hudRoot.activeSelf)
        {
            if (healthBarFill != null) healthBarFill.fillAmount = player.currentHealth / player.maxHealth;
            if (bloodBarFill != null) bloodBarFill.fillAmount = player.currentBlood / player.maxBlood;

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
            gameOverActionText.text = (lvl == 4)
                ? "[ PRESS 'R' TO RESTART FROM LEVEL 1 ]  |  [ ESC TO QUIT ]"
                : "[ PRESS 'R' TO RESTART LEVEL ]  |  [ ESC TO QUIT ]";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowVictory()
    {
        HideHUD();
        if (victoryRoot != null) victoryRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void BuildAllUI()
    {
        GameObject canvasObj = new GameObject("NIGHTBOUND_HUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        Transform c = canvasObj.transform;
        BuildInGameHUD(c);
        BuildSubtitleBox(c);
        BuildGameOverUI(c);
        BuildVictoryUI(c);
        BuildPauseUI(c);
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
        healthBarFill = CreateBar(hudRoot.transform, "HealthBar", new Vector2(140f, 65f), new Vector2(220f, 22f),
            new Color(0.15f, 0.02f, 0.02f, 0.85f), new Color(0.85f, 0.1f, 0.15f));

        // Blood Bar (Bottom Left)
        bloodBarFill = CreateBar(hudRoot.transform, "BloodBar", new Vector2(140f, 36f), new Vector2(220f, 15f),
            new Color(0.08f, 0.02f, 0.15f, 0.85f), new Color(0.6f, 0.08f, 0.85f));

        // Serum Status (Bottom Left)
        serumStatusText = CreateText(hudRoot.transform, "VAMPIRE FORM - SERUM: 02:00", 16, TextAnchor.MiddleLeft);
        serumStatusText.rectTransform.anchoredPosition = new Vector2(170f, 100f);

        // Day/Night Indicator (Top Center)
        timeOfDayText = CreateText(hudRoot.transform, "NIGHT [SAFE]", 18, TextAnchor.MiddleCenter);
        timeOfDayText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        timeOfDayText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        timeOfDayText.rectTransform.pivot = new Vector2(0.5f, 1f);
        timeOfDayText.rectTransform.anchoredPosition = new Vector2(0f, -20f);

        // Ability Controls Help (Bottom Right)
        abilityHelpText = CreateText(hudRoot.transform, "[Q] Shadow Step  |  [Shift] Time Slow  |  [E] Bite  |  [L-Click] Attack", 14, TextAnchor.MiddleRight);
        abilityHelpText.rectTransform.anchorMin = new Vector2(1f, 0f);
        abilityHelpText.rectTransform.anchorMax = new Vector2(1f, 0f);
        abilityHelpText.rectTransform.pivot = new Vector2(1f, 0f);
        abilityHelpText.rectTransform.anchoredPosition = new Vector2(-20f, 30f);
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
        gameOverRoot = CreatePanel(parent, "GameOverScreen", new Color(0.12f, 0.02f, 0.02f, 0.95f));

        gameOverTitleText = CreateText(gameOverRoot.transform, "Y O U   P E R I S H E D", 36, TextAnchor.MiddleCenter);
        gameOverTitleText.rectTransform.anchoredPosition = new Vector2(0f, 80f);
        gameOverTitleText.color = new Color(0.9f, 0.1f, 0.1f);

        gameOverReasonText = CreateText(gameOverRoot.transform, "SLAIN BY SYNDICATE", 20, TextAnchor.MiddleCenter);
        gameOverReasonText.rectTransform.anchoredPosition = new Vector2(0f, 20f);

        gameOverActionText = CreateText(gameOverRoot.transform, "[ PRESS 'R' TO RESTART LEVEL ]", 20, TextAnchor.MiddleCenter);
        gameOverActionText.rectTransform.anchoredPosition = new Vector2(0f, -70f);
        gameOverActionText.color = new Color(1f, 0.8f, 0.3f);

        gameOverRoot.SetActive(false);
    }

    private void BuildVictoryUI(Transform parent)
    {
        victoryRoot = CreatePanel(parent, "VictoryScreen", new Color(0.04f, 0.06f, 0.12f, 0.96f));

        Text title = CreateText(victoryRoot.transform, "V E N G E A N C E   S A T I S F I E D", 34, TextAnchor.MiddleCenter);
        title.rectTransform.anchoredPosition = new Vector2(0f, 80f);
        title.color = new Color(0.9f, 0.8f, 0.2f);

        Text desc = CreateText(victoryRoot.transform, "The Syndicate Boss is dead. Your family is avenged in the dark.", 18, TextAnchor.MiddleCenter);
        desc.rectTransform.anchoredPosition = new Vector2(0f, 20f);

        Text replay = CreateText(victoryRoot.transform, "[ PRESS 'R' TO PLAY AGAIN ]  |  [ ESC TO QUIT ]", 20, TextAnchor.MiddleCenter);
        replay.rectTransform.anchoredPosition = new Vector2(0f, -70f);
        replay.color = new Color(0.3f, 0.85f, 1f);

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

    private static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
