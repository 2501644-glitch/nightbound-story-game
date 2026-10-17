using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
    public Color speakerColor;
    public string avatarSymbol;
    public float voicePitch;
    public float autoAdvanceTime;

    public DialogueLine(string speaker, string text, Color speakerColor, string avatarSymbol = "◆", float voicePitch = 1f, float autoAdvanceTime = 3.5f)
    {
        this.speaker = speaker;
        this.text = text;
        this.speakerColor = speakerColor;
        this.avatarSymbol = avatarSymbol;
        this.voicePitch = voicePitch;
        this.autoAdvanceTime = autoAdvanceTime;
    }
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public bool IsDialogueActive => isConversationRunning;

    private GameObject dialogueCanvas;
    private GameObject dialogueBoxRoot;
    private Image accentBorder;
    private Text avatarText;
    private Image avatarBg;
    private Text speakerBadgeText;
    private Image speakerBadgeBg;
    private Text dialogueBodyText;
    private Text promptContinueText;

    private Font gameFont;
    private Sprite solidSprite;

    private bool isConversationRunning = false;
    private Coroutine activeConversationCoroutine;
    private Coroutine activeTypewriterCoroutine;
    private bool isTyping = false;
    private string fullCurrentText = "";
    private bool advanceRequested = false;

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
        BuildDialogueUI();
    }

    private void Update()
    {
        if (!isConversationRunning) return;

        bool keyPressed = false;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                keyPressed = true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            keyPressed = true;
        }

        if (keyPressed)
        {
            if (isTyping)
            {
                // Instantly finish typing current line
                if (activeTypewriterCoroutine != null) StopCoroutine(activeTypewriterCoroutine);
                dialogueBodyText.text = fullCurrentText;
                isTyping = false;
                if (promptContinueText != null) promptContinueText.gameObject.SetActive(true);
            }
            else
            {
                // Advance to next line
                advanceRequested = true;
            }
        }

        // Gentle pulse for prompt text
        if (promptContinueText != null && promptContinueText.gameObject.activeSelf)
        {
            float alpha = 0.45f + Mathf.PingPong(Time.unscaledTime * 2f, 0.55f);
            Color c = promptContinueText.color;
            c.a = alpha;
            promptContinueText.color = c;
        }
    }

    public void StartConversation(List<DialogueLine> lines, Action onComplete = null)
    {
        if (activeConversationCoroutine != null)
            StopCoroutine(activeConversationCoroutine);

        activeConversationCoroutine = StartCoroutine(ConversationRoutine(lines, onComplete));
    }

    public void StopConversation()
    {
        if (activeConversationCoroutine != null)
        {
            StopCoroutine(activeConversationCoroutine);
            activeConversationCoroutine = null;
        }
        if (activeTypewriterCoroutine != null)
        {
            StopCoroutine(activeTypewriterCoroutine);
            activeTypewriterCoroutine = null;
        }
        isConversationRunning = false;
        isTyping = false;
        if (dialogueBoxRoot != null) dialogueBoxRoot.SetActive(false);
    }

    private IEnumerator ConversationRoutine(List<DialogueLine> lines, Action onComplete)
    {
        if (lines == null || lines.Count == 0) yield break;

        isConversationRunning = true;
        if (dialogueBoxRoot != null) dialogueBoxRoot.SetActive(true);

        for (int i = 0; i < lines.Count; i++)
        {
            DialogueLine line = lines[i];
            advanceRequested = false;

            ApplySpeakerStyle(line);

            fullCurrentText = line.text;
            if (activeTypewriterCoroutine != null) StopCoroutine(activeTypewriterCoroutine);
            activeTypewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text, line.voicePitch));

            float timer = 0f;
            float maxWait = Mathf.Max(2.5f, line.autoAdvanceTime);

            while (!advanceRequested && timer < maxWait)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Small gap between sentences
            yield return new WaitForSecondsRealtime(0.12f);
        }

        isConversationRunning = false;
        if (dialogueBoxRoot != null) dialogueBoxRoot.SetActive(false);
        onComplete?.Invoke();
    }

    private IEnumerator TypewriterRoutine(string targetText, float pitch)
    {
        isTyping = true;
        dialogueBodyText.text = "";
        if (promptContinueText != null) promptContinueText.gameObject.SetActive(false);

        float charDelay = 0.025f;
        for (int i = 0; i < targetText.Length; i++)
        {
            dialogueBodyText.text += targetText[i];

            if (i % 2 == 0 && targetText[i] != ' ')
            {
                ProceduralAudio.Instance?.PlayVoiceBlip(pitch);
            }

            yield return new WaitForSecondsRealtime(charDelay);
        }

        isTyping = false;
        if (promptContinueText != null) promptContinueText.gameObject.SetActive(true);
    }

    private void ApplySpeakerStyle(DialogueLine line)
    {
        if (speakerBadgeText != null)
        {
            speakerBadgeText.text = line.speaker.ToUpper();
            speakerBadgeText.color = Color.white;
        }

        if (speakerBadgeBg != null)
        {
            speakerBadgeBg.color = line.speakerColor;
        }

        if (accentBorder != null)
        {
            accentBorder.color = line.speakerColor;
        }

        if (avatarText != null)
        {
            avatarText.text = line.avatarSymbol;
            avatarText.color = line.speakerColor;
        }

        if (avatarBg != null)
        {
            avatarBg.color = new Color(line.speakerColor.r * 0.25f, line.speakerColor.g * 0.25f, line.speakerColor.b * 0.25f, 0.85f);
        }
    }

    private void BuildDialogueUI()
    {
        dialogueCanvas = new GameObject("DialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas c = dialogueCanvas.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 120; // Above HUD and Cutscene black bars

        CanvasScaler cs = dialogueCanvas.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;

        // Dialogue Box Root (Bottom Center)
        dialogueBoxRoot = new GameObject("DialogueBoxRoot", typeof(RectTransform), typeof(Image));
        dialogueBoxRoot.transform.SetParent(dialogueCanvas.transform, false);
        RectTransform rt = dialogueBoxRoot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 35f);
        rt.sizeDelta = new Vector2(1040f, 150f);

        Image bgImg = dialogueBoxRoot.GetComponent<Image>();
        bgImg.sprite = solidSprite;
        bgImg.color = new Color(0.04f, 0.04f, 0.07f, 0.94f);

        // Top Accent Stripe (3px)
        GameObject accentObj = new GameObject("AccentBorder", typeof(RectTransform), typeof(Image));
        accentObj.transform.SetParent(dialogueBoxRoot.transform, false);
        RectTransform aRect = accentObj.GetComponent<RectTransform>();
        aRect.anchorMin = new Vector2(0f, 1f);
        aRect.anchorMax = new Vector2(1f, 1f);
        aRect.pivot = new Vector2(0.5f, 1f);
        aRect.sizeDelta = new Vector2(0f, 4f);
        aRect.anchoredPosition = Vector2.zero;
        accentBorder = accentObj.GetComponent<Image>();
        accentBorder.sprite = solidSprite;
        accentBorder.color = new Color(0.85f, 0.15f, 0.2f);

        // Avatar Frame (Left side, 80x80)
        GameObject avObj = new GameObject("AvatarFrame", typeof(RectTransform), typeof(Image));
        avObj.transform.SetParent(dialogueBoxRoot.transform, false);
        RectTransform avRect = avObj.GetComponent<RectTransform>();
        avRect.anchorMin = new Vector2(0f, 0.5f);
        avRect.anchorMax = new Vector2(0f, 0.5f);
        avRect.pivot = new Vector2(0f, 0.5f);
        avRect.anchoredPosition = new Vector2(25f, -5f);
        avRect.sizeDelta = new Vector2(85f, 85f);
        avatarBg = avObj.GetComponent<Image>();
        avatarBg.sprite = solidSprite;
        avatarBg.color = new Color(0.2f, 0.05f, 0.05f, 0.9f);

        GameObject avTextObj = new GameObject("AvatarSymbol", typeof(RectTransform), typeof(Text));
        avTextObj.transform.SetParent(avObj.transform, false);
        RectTransform avtRect = avTextObj.GetComponent<RectTransform>();
        avtRect.anchorMin = Vector2.zero;
        avtRect.anchorMax = Vector2.one;
        avtRect.sizeDelta = Vector2.zero;
        avatarText = avTextObj.GetComponent<Text>();
        avatarText.font = gameFont;
        avatarText.fontSize = 42;
        avatarText.alignment = TextAnchor.MiddleCenter;
        avatarText.text = "V";
        avatarText.color = Color.red;

        // Speaker Badge Pill
        GameObject badgeObj = new GameObject("SpeakerBadge", typeof(RectTransform), typeof(Image));
        badgeObj.transform.SetParent(dialogueBoxRoot.transform, false);
        RectTransform sRect = badgeObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 1f);
        sRect.anchorMax = new Vector2(0f, 1f);
        sRect.pivot = new Vector2(0f, 1f);
        sRect.anchoredPosition = new Vector2(130f, -12f);
        sRect.sizeDelta = new Vector2(280f, 28f);
        speakerBadgeBg = badgeObj.GetComponent<Image>();
        speakerBadgeBg.sprite = solidSprite;
        speakerBadgeBg.color = new Color(0.75f, 0.1f, 0.15f);

        GameObject sTextObj = new GameObject("SpeakerText", typeof(RectTransform), typeof(Text));
        sTextObj.transform.SetParent(badgeObj.transform, false);
        RectTransform stRect = sTextObj.GetComponent<RectTransform>();
        stRect.anchorMin = Vector2.zero;
        stRect.anchorMax = Vector2.one;
        stRect.sizeDelta = Vector2.zero;
        speakerBadgeText = sTextObj.GetComponent<Text>();
        speakerBadgeText.font = gameFont;
        speakerBadgeText.fontSize = 15;
        speakerBadgeText.alignment = TextAnchor.MiddleCenter;
        speakerBadgeText.text = "PROTAGONIST";
        speakerBadgeText.color = Color.white;

        // Dialogue Body Text
        GameObject dTextObj = new GameObject("DialogueText", typeof(RectTransform), typeof(Text));
        dTextObj.transform.SetParent(dialogueBoxRoot.transform, false);
        RectTransform dtRect = dTextObj.GetComponent<RectTransform>();
        dtRect.anchorMin = new Vector2(0f, 1f);
        dtRect.anchorMax = new Vector2(1f, 0f);
        dtRect.pivot = new Vector2(0f, 1f);
        dtRect.anchoredPosition = new Vector2(130f, -48f);
        dtRect.sizeDelta = new Vector2(-155f, -65f);
        dialogueBodyText = dTextObj.GetComponent<Text>();
        dialogueBodyText.font = gameFont;
        dialogueBodyText.fontSize = 20;
        dialogueBodyText.lineSpacing = 1.15f;
        dialogueBodyText.alignment = TextAnchor.UpperLeft;
        dialogueBodyText.text = "";
        dialogueBodyText.color = new Color(0.92f, 0.92f, 0.95f);

        // Continue prompt (Bottom Right)
        GameObject promptObj = new GameObject("ContinuePrompt", typeof(RectTransform), typeof(Text));
        promptObj.transform.SetParent(dialogueBoxRoot.transform, false);
        RectTransform pRect = promptObj.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(1f, 0f);
        pRect.anchorMax = new Vector2(1f, 0f);
        pRect.pivot = new Vector2(1f, 0f);
        pRect.anchoredPosition = new Vector2(-20f, 12f);
        pRect.sizeDelta = new Vector2(240f, 22f);
        promptContinueText = promptObj.GetComponent<Text>();
        promptContinueText.font = gameFont;
        promptContinueText.fontSize = 13;
        promptContinueText.alignment = TextAnchor.MiddleRight;
        promptContinueText.text = "[SPACE] Continue ▶";
        promptContinueText.color = new Color(0.85f, 0.8f, 0.6f);

        dialogueBoxRoot.SetActive(false);
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Color[] px = new Color[] { Color.white, Color.white, Color.white, Color.white };
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
    }
}
