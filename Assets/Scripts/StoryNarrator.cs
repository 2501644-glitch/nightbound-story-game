using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StoryNarrator : MonoBehaviour
{
    public static StoryNarrator Instance { get; private set; }

    public bool IsInterludePlaying => isPlayingInterlude;

    private GameObject letterboxCanvas;
    private RectTransform topBar;
    private RectTransform bottomBar;
    private Camera interludeCamera;
    private bool isPlayingInterlude = false;
    private Action onInterludeFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (FindAnyObjectByType<StoryNarrator>() == null)
        {
            GameObject go = new GameObject("NIGHTBOUND_StoryNarrator");
            go.AddComponent<StoryNarrator>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildLetterboxUI();
        SetupInterludeCamera();
    }

    private void Update()
    {
        if (isPlayingInterlude && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                StopAllCoroutines();
                DialogueManager.Instance?.StopConversation();
                FinishInterlude();
            }
        }
    }

    public void PlayLevelTransition(int fromLevel, Action onComplete)
    {
        onInterludeFinished = onComplete;
        if (fromLevel == 1)
        {
            StartCoroutine(Interlude1_ToGraveyard());
        }
        else if (fromLevel == 2)
        {
            StartCoroutine(Interlude2_ToWarehouse());
        }
        else if (fromLevel == 3)
        {
            StartCoroutine(Interlude3_ToPenthouse());
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    public void PlayEpilogue(Action onComplete)
    {
        onInterludeFinished = onComplete;
        StartCoroutine(EpilogueRoutine());
    }

    // ================= 1. ALLEY -> GRAVEYARD INTERLUDE =================
    private IEnumerator Interlude1_ToGraveyard()
    {
        BeginInterlude(new Vector3(0f, 3.5f, 35f), Quaternion.Euler(15f, 0f, 0f));

        var lines = new List<DialogueLine>
        {
            new DialogueLine("THE NARRATOR", "The alley grows cold. Two syndicate hitmen lie dead in the dirt, their blood feeding the ancient curse.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.2f),
            new DialogueLine("THE NARRATOR", "From a dying thug's radio, a panicked voice crackles: the perimeter scouts have retreated to St. Jude's Cemetery, praying holy ground will protect them.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.5f),
            new DialogueLine("THE NARRATOR", "They do not understand. A creature born of eternal shadow has no fear of the dead. The hunt moves to the cemetery.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.2f)
        };

        yield return StartCoroutine(PanCamera(new Vector3(0f, 6f, 48f), Quaternion.Euler(20f, 0f, 0f), 3.5f));

        bool dialogueDone = false;
        DialogueManager.Instance?.StartConversation(lines, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        FinishInterlude();
    }

    // ================= 2. GRAVEYARD -> WAREHOUSE INTERLUDE =================
    private IEnumerator Interlude2_ToWarehouse()
    {
        BeginInterlude(new Vector3(0f, 4.5f, 25f), Quaternion.Euler(20f, -30f, 0f));

        var lines = new List<DialogueLine>
        {
            new DialogueLine("THE NARRATOR", "Three scouts silenced beneath the weather-beaten gravestones. The trail leads back into the industrial heart of the city.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.2f),
            new DialogueLine("THE NARRATOR", "Inside the Syndicate's fortified shipping depot, elite enforcers arm themselves with high-caliber munitions and flood the yard with blinding light.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.5f),
            new DialogueLine("THE NARRATOR", "They know the beast burns in the light... but they do not know how deep the darkness can reach.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.0f)
        };

        yield return StartCoroutine(PanCamera(new Vector3(0f, 8f, 45f), Quaternion.Euler(25f, 15f, 0f), 3.5f));

        bool dialogueDone = false;
        DialogueManager.Instance?.StartConversation(lines, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        FinishInterlude();
    }

    // ================= 3. WAREHOUSE -> PENTHOUSE INTERLUDE =================
    private IEnumerator Interlude3_ToPenthouse()
    {
        BeginInterlude(new Vector3(0f, 6f, 20f), Quaternion.Euler(35f, 0f, 0f));

        var lines = new List<DialogueLine>
        {
            new DialogueLine("THE NARRATOR", "Four enforcers broken. With their lifeblood consumed, the primordial serum surges—Rayan's vampire power reaches full ascension.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.2f),
            new DialogueLine("THE NARRATOR", "At the summit of the Syndicate's high-rise tower, Kingpin Kaleem Rana paces in his gilded penthouse, surrounded by gold and terror.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.5f),
            new DialogueLine("THE NARRATOR", "The night is running out. But before the sun rises over this corrupt city... Kaleem Rana will answer for his crimes.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.2f)
        };

        yield return StartCoroutine(PanCamera(new Vector3(0f, 12f, 35f), Quaternion.Euler(45f, 0f, 0f), 3.5f));

        bool dialogueDone = false;
        DialogueManager.Instance?.StartConversation(lines, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        FinishInterlude();
    }

    // ================= 4. EPILOGUE (DEFEAT OF KALEEM RANA) =================
    private IEnumerator EpilogueRoutine()
    {
        BeginInterlude(new Vector3(0f, 4.5f, 20f), Quaternion.Euler(20f, 0f, 0f));

        var lines = new List<DialogueLine>
        {
            new DialogueLine("THE NARRATOR", "Kaleem Rana lies lifeless upon the cold obsidian floor. The criminal syndicate that terrorized this city for a decade is no more.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.5f),
            new DialogueLine("THE NARRATOR", "Through the shattered high-rise glass, the first golden rays of dawn begin to crest the horizon. The sunlight burns with merciless heat.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 4.5f),
            new DialogueLine("THE NARRATOR", "His family is avenged... but mortal humanity can never be reclaimed. As the city awakens, Rayan Malik retreats into the deep shadows... forever Nightbound.", new Color(1f, 0.82f, 0.35f), "📜", 0.78f, 5.0f)
        };

        yield return StartCoroutine(PanCamera(new Vector3(0f, 7f, 30f), Quaternion.Euler(30f, 0f, 0f), 4.0f));

        bool dialogueDone = false;
        DialogueManager.Instance?.StartConversation(lines, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        FinishInterlude();
    }

    private void BeginInterlude(Vector3 camPos, Quaternion camRot)
    {
        isPlayingInterlude = true;
        UIManager.Instance?.HideHUD();

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.gameObject.SetActive(false);

        if (interludeCamera != null)
        {
            interludeCamera.transform.position = camPos;
            interludeCamera.transform.rotation = camRot;
            interludeCamera.gameObject.SetActive(true);
        }

        StartCoroutine(AnimateLetterbox(true));
    }

    private void FinishInterlude()
    {
        isPlayingInterlude = false;
        StartCoroutine(AnimateLetterbox(false));

        if (interludeCamera != null)
        {
            interludeCamera.gameObject.SetActive(false);
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.gameObject.SetActive(true);

        onInterludeFinished?.Invoke();
    }

    private IEnumerator PanCamera(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        if (interludeCamera == null) yield break;

        Vector3 startP = interludeCamera.transform.position;
        Quaternion startR = interludeCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            interludeCamera.transform.position = Vector3.Lerp(startP, targetPos, t);
            interludeCamera.transform.rotation = Quaternion.Slerp(startR, targetRot, t);
            yield return null;
        }
    }

    private IEnumerator AnimateLetterbox(bool show)
    {
        if (topBar == null || bottomBar == null) yield break;

        float targetHeight = show ? 95f : 0f;
        float currentHeight = topBar.sizeDelta.y;
        float elapsed = 0f;

        while (elapsed < 0.35f)
        {
            elapsed += Time.unscaledDeltaTime;
            float h = Mathf.Lerp(currentHeight, targetHeight, elapsed / 0.35f);
            topBar.sizeDelta = new Vector2(0f, h);
            bottomBar.sizeDelta = new Vector2(0f, h);
            yield return null;
        }

        topBar.sizeDelta = new Vector2(0f, targetHeight);
        bottomBar.sizeDelta = new Vector2(0f, targetHeight);
    }

    private void SetupInterludeCamera()
    {
        if (interludeCamera == null)
        {
            GameObject camObj = new GameObject("StoryNarratorCamera");
            interludeCamera = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            interludeCamera.gameObject.SetActive(false);
        }
    }

    private void BuildLetterboxUI()
    {
        letterboxCanvas = new GameObject("LetterboxCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas c = letterboxCanvas.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 90;

        CanvasScaler cs = letterboxCanvas.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        topBar = CreateBlackBar("TopLetterbox", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        bottomBar = CreateBlackBar("BottomLetterbox", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f));
    }

    private RectTransform CreateBlackBar(string name, Vector2 aMin, Vector2 aMax, Vector2 pivot)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(letterboxCanvas.transform, false);
        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.sizeDelta = new Vector2(0f, 0f);
        bar.GetComponent<Image>().color = Color.black;
        return rt;
    }
}
