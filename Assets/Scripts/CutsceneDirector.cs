using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneDirector : MonoBehaviour
{
    public static CutsceneDirector Instance { get; private set; }

    public bool isCutsceneRunning = false;
    private Camera cutsceneCamera;
    private readonly List<GameObject> stageActors = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void PlayIntroCutscene()
    {
        StartCoroutine(CutsceneSequence());
    }

    private void Update()
    {
        if (isCutsceneRunning && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopAllCoroutines();
            DialogueManager.Instance?.StopConversation();
            EndCutscene();
        }
    }

    private IEnumerator CutsceneSequence()
    {
        isCutsceneRunning = true;
        SetupCamera();

        // SCENE 1: The Syndicate Ambush
        yield return StartCoroutine(PlayScene1_Tragedy());

        // SCENE 2: Dr. Vance's Chronos Gateway
        yield return StartCoroutine(PlayScene2_TimeMachine());

        // SCENE 3: The 16th Century Crypt & Transformation
        yield return StartCoroutine(PlayScene3_VampireSerum());

        EndCutscene();
    }

    private IEnumerator PlayScene1_Tragedy()
    {
        ClearStage();

        CreateBlock("Street", new Vector3(0f, -0.5f, 0f), new Vector3(25f, 1f, 25f), new Color(0.12f, 0.12f, 0.14f));
        CreateBlock("BrickWall", new Vector3(0f, 3f, 8f), new Vector3(25f, 6f, 0.8f), new Color(0.2f, 0.1f, 0.1f));

        GameObject familyDad = CreateActor("Father", new Vector3(-1.5f, 0.9f, 2f), new Color(0.3f, 0.5f, 0.8f), PrimitiveType.Capsule);
        GameObject familyMom = CreateActor("Mother", new Vector3(0f, 0.85f, 2f), new Color(0.7f, 0.3f, 0.5f), PrimitiveType.Capsule);
        GameObject familyKid = CreateActor("Child", new Vector3(1.2f, 0.6f, 2f), new Color(0.8f, 0.7f, 0.3f), PrimitiveType.Capsule);
        familyKid.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

        GameObject hitman1 = CreateActor("Hitman 1", new Vector3(-2f, 1f, -4f), new Color(0.15f, 0.15f, 0.15f), PrimitiveType.Capsule);
        GameObject hitman2 = CreateActor("Hitman 2", new Vector3(2f, 1f, -4f), new Color(0.15f, 0.15f, 0.15f), PrimitiveType.Capsule);

        CreateBlock("CoverCrate", new Vector3(-6f, 1f, -2f), new Vector3(2f, 2f, 2f), new Color(0.35f, 0.25f, 0.15f));
        CreateActor("HeroYoung", new Vector3(-6f, 0.7f, -3.5f), new Color(0.2f, 0.3f, 0.2f), PrimitiveType.Capsule);

        cutsceneCamera.transform.position = new Vector3(0f, 2.5f, -8f);
        cutsceneCamera.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

        bool dialogueDone = false;
        var scene1Dialogue = new List<DialogueLine>
        {
            new DialogueLine("SYNDICATE HITMAN", "Your family's time in this city is over. Kaleem Rana sends his regards.", new Color(0.85f, 0.45f, 0.2f), "💀", 0.85f, 4.0f),
            new DialogueLine("FATHER", "Take my savings! Take the deed to our home! Just spare my wife and child!", new Color(0.3f, 0.65f, 0.95f), "👤", 1.25f, 3.8f),
            new DialogueLine("HITMAN 2", "The Syndicate leaves zero loose ends. Squeeze the trigger.", new Color(0.85f, 0.45f, 0.2f), "🔫", 0.8f, 3.2f)
        };

        DialogueManager.Instance?.StartConversation(scene1Dialogue, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        // Gunshots and execution
        ProceduralAudio.Instance?.PlayGunshot();
        familyDad.transform.rotation = Quaternion.Euler(85f, 0f, 0f);
        familyDad.transform.position += Vector3.down * 0.7f;
        yield return new WaitForSeconds(0.5f);

        ProceduralAudio.Instance?.PlayGunshot();
        familyMom.transform.rotation = Quaternion.Euler(85f, 40f, 0f);
        familyMom.transform.position += Vector3.down * 0.7f;
        yield return new WaitForSeconds(0.5f);

        ProceduralAudio.Instance?.PlayGunshot();
        familyKid.transform.rotation = Quaternion.Euler(85f, -30f, 0f);
        familyKid.transform.position += Vector3.down * 0.5f;

        dialogueDone = false;
        var scene1Aftermath = new List<DialogueLine>
        {
            new DialogueLine("YOUNG RAYAN", "NO! FATHER! MOTHER! ... Monsters! You slaughtered them all!", new Color(0.4f, 0.8f, 0.5f), "👤", 1.35f, 3.5f),
            new DialogueLine("SYNDICATE HITMAN", "Check the alley... Just a kid cowering in the trash. Leave him to rot in the cold.", new Color(0.85f, 0.45f, 0.2f), "💀", 0.85f, 3.8f),
            new DialogueLine("YOUNG RAYAN", "I swear on their graves... I will destroy every single one of you. Whatever it takes.", new Color(0.9f, 0.15f, 0.15f), "🩸", 1.05f, 4.2f)
        };

        DialogueManager.Instance?.StartConversation(scene1Aftermath, () => dialogueDone = true);
        while (!dialogueDone) yield return null;
    }

    private IEnumerator PlayScene2_TimeMachine()
    {
        ClearStage();

        CreateBlock("LabFloor", new Vector3(0f, -0.5f, 0f), new Vector3(20f, 1f, 20f), new Color(0.08f, 0.12f, 0.18f));
        CreateBlock("LabWallBack", new Vector3(0f, 3f, 8f), new Vector3(20f, 7f, 0.6f), new Color(0.06f, 0.08f, 0.12f));

        // Time Machine Portal Ring
        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portal.name = "ChronosPortal";
        portal.transform.position = new Vector3(0f, 2.5f, 4f);
        portal.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        portal.transform.localScale = new Vector3(4.5f, 0.2f, 4.5f);
        portal.GetComponent<Renderer>().material.color = new Color(0.1f, 0.8f, 1f);
        stageActors.Add(portal);

        GameObject pLight = new GameObject("PortalLight");
        pLight.transform.position = new Vector3(0f, 2.5f, 3f);
        Light pl = pLight.AddComponent<Light>();
        pl.type = LightType.Point;
        pl.range = 15f;
        pl.color = new Color(0.2f, 0.8f, 1f);
        pl.intensity = 4f;
        stageActors.Add(pLight);

        GameObject hero = CreateActor("HeroInLab", new Vector3(0f, 1f, -3f), new Color(0.2f, 0.4f, 0.6f), PrimitiveType.Capsule);

        cutsceneCamera.transform.position = new Vector3(0f, 2.2f, -6f);
        cutsceneCamera.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        bool dialogueDone = false;
        var scene2Dialogue = new List<DialogueLine>
        {
            new DialogueLine("RAYAN MALIK", "Ten years of searching... The Syndicate owns the police and courts. Conventional weapons can't touch them.", new Color(0.3f, 0.7f, 1f), "👤", 1.0f, 4.0f),
            new DialogueLine("DR. VANCE (ARCHIVE)", "Chronos Gateway activated. If you hear this, Kaleem Rana's thugs have silenced me.", new Color(0.2f, 0.85f, 0.95f), "⏳", 1.45f, 4.0f),
            new DialogueLine("DR. VANCE (ARCHIVE)", "You cannot rewrite history. But in the 16th century, an ancient Vampire strain possessed godlike power.", new Color(0.2f, 0.85f, 0.95f), "⏳", 1.45f, 4.2f),
            new DialogueLine("DR. VANCE (ARCHIVE)", "WARNING: Traversing time degrades the formula! The serum will only hold for 2 minutes before human cooldown!", new Color(1f, 0.75f, 0.2f), "⚠", 1.35f, 4.5f),
            new DialogueLine("RAYAN MALIK", "Two minutes in the dark is all I need. Opening the rift to 1582!", new Color(0.9f, 0.2f, 0.2f), "🩸", 1.0f, 3.5f)
        };

        DialogueManager.Instance?.StartConversation(scene2Dialogue, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        ProceduralAudio.Instance?.PlayShadowStep();

        float elapsed = 0f;
        Vector3 startP = hero.transform.position;
        Vector3 targetP = new Vector3(0f, 1f, 3.5f);
        while (elapsed < 2.0f)
        {
            elapsed += Time.deltaTime;
            hero.transform.position = Vector3.Lerp(startP, targetP, elapsed / 2.0f);
            yield return null;
        }
    }

    private IEnumerator PlayScene3_VampireSerum()
    {
        ClearStage();

        CreateBlock("CryptFloor", new Vector3(0f, -0.5f, 0f), new Vector3(18f, 1f, 18f), new Color(0.15f, 0.12f, 0.1f));
        CreateBlock("CryptPillarL", new Vector3(-4f, 2.5f, 3f), new Vector3(1.2f, 6f, 1.2f), new Color(0.2f, 0.15f, 0.12f));
        CreateBlock("CryptPillarR", new Vector3(4f, 2.5f, 3f), new Vector3(1.2f, 6f, 1.2f), new Color(0.2f, 0.15f, 0.12f));

        CreateBlock("SerumAltar", new Vector3(0f, 0.8f, 2f), new Vector3(1.2f, 1.6f, 1.2f), new Color(0.1f, 0.1f, 0.1f));
        GameObject serumVial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        serumVial.transform.position = new Vector3(0f, 1.8f, 2f);
        serumVial.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
        serumVial.GetComponent<Renderer>().material.color = Color.red;
        stageActors.Add(serumVial);

        GameObject hero = CreateActor("HeroVampireTransformed", new Vector3(0f, 1f, 0f), new Color(0.18f, 0.05f, 0.05f), PrimitiveType.Capsule);

        cutsceneCamera.transform.position = new Vector3(0f, 2.0f, -3.5f);
        cutsceneCamera.transform.rotation = Quaternion.Euler(14f, 0f, 0f);

        bool dialogueDone = false;
        var scene3Dialogue = new List<DialogueLine>
        {
            new DialogueLine("CRYPT SPIRIT", "Mortal traveler from the future... You stand upon the tomb of the first vampires.", new Color(0.65f, 0.35f, 0.85f), "🦇", 0.72f, 4.0f),
            new DialogueLine("RAYAN MALIK", "My soul died ten years ago. Grant me the power of eternal shadows.", new Color(0.85f, 0.2f, 0.2f), "🩸", 0.95f, 3.5f),
            new DialogueLine("CRYPT SPIRIT", "Drink, then! But remember: daylight will burn your flesh! Now return, and feast upon the guilty!", new Color(0.65f, 0.35f, 0.85f), "🦇", 0.72f, 4.2f)
        };

        DialogueManager.Instance?.StartConversation(scene3Dialogue, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        serumVial.SetActive(false);
        ProceduralAudio.Instance?.PlaySerumInject();
        hero.GetComponent<Renderer>().material.color = new Color(0.85f, 0.08f, 0.12f);

        dialogueDone = false;
        var scene3Climax = new List<DialogueLine>
        {
            new DialogueLine("VAMPIRE RAYAN", "The serum... My veins are burning with immortal shadow! Time bends to my fangs!", new Color(0.95f, 0.15f, 0.15f), "🧛", 0.65f, 3.8f),
            new DialogueLine("VAMPIRE RAYAN", "Kaleem Rana... Your empire of blood ends tonight in 2026. The hunt begins!", new Color(0.95f, 0.15f, 0.15f), "🧛", 0.6f, 4.0f)
        };

        DialogueManager.Instance?.StartConversation(scene3Climax, () => dialogueDone = true);
        while (!dialogueDone) yield return null;
    }

    private void EndCutscene()
    {
        isCutsceneRunning = false;
        ClearStage();

        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
        }

        DialogueManager.Instance?.StopConversation();
        GameManager.Instance?.StartLevel(1);
    }

    private void SetupCamera()
    {
        if (cutsceneCamera == null)
        {
            GameObject camObj = new GameObject("CutsceneCamera");
            cutsceneCamera = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cutsceneCamera.gameObject.SetActive(true);
    }

    private GameObject CreateBlock(string name, Vector3 pos, Vector3 scale, Color col)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = name;
        b.transform.position = pos;
        b.transform.localScale = scale;
        b.GetComponent<Renderer>().material.color = col;
        stageActors.Add(b);
        return b;
    }

    private GameObject CreateActor(string name, Vector3 pos, Color col, PrimitiveType prim)
    {
        string modelPath = null;
        if (name.Contains("Vampire")) modelPath = "Characters/VampireHero";
        else if (name.Contains("Hero")) modelPath = "Characters/HumanHero";
        else if (name.Contains("Hitman")) modelPath = "Characters/SyndicateThug";

        if (!string.IsNullOrEmpty(modelPath))
        {
            GameObject prefab = Resources.Load<GameObject>(modelPath);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = name;
                instance.transform.position = pos;
                stageActors.Add(instance);
                return instance;
            }
        }

        GameObject a = GameObject.CreatePrimitive(prim);
        a.name = name;
        a.transform.position = pos;
        a.GetComponent<Renderer>().material.color = col;
        stageActors.Add(a);
        return a;
    }

    private void ClearStage()
    {
        foreach (GameObject obj in stageActors)
        {
            if (obj != null) Destroy(obj);
        }
        stageActors.Clear();
    }
}
