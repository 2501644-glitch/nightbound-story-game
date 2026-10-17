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
        EnvironmentStyler.Instance?.ApplyLevelTheme(1);
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
        familyDad.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        familyMom.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        familyKid.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        GameObject hitman1 = CreateActor("Hitman 1", new Vector3(-2f, 1f, -4f), new Color(0.15f, 0.15f, 0.15f), PrimitiveType.Capsule);
        GameObject hitman2 = CreateActor("Hitman 2", new Vector3(2f, 1f, -4f), new Color(0.15f, 0.15f, 0.15f), PrimitiveType.Capsule);

        CreateBlock("CoverCrate", new Vector3(-6f, 1f, -2f), new Vector3(2f, 2f, 2f), new Color(0.35f, 0.25f, 0.15f));
        CreateActor("HeroYoung", new Vector3(-6f, 0.7f, -3.5f), new Color(0.2f, 0.3f, 0.2f), PrimitiveType.Capsule);

        cutsceneCamera.transform.position = new Vector3(0f, 2.4f, -9f);
        cutsceneCamera.transform.LookAt(new Vector3(0f, 1.1f, 1f));

        bool dialogueDone = false;
        var scene1Dialogue = new List<DialogueLine>
        {
            new DialogueLine("SYNDICATE HITMAN", "Your family's time in this city is over. Kaleem Rana sends his regards.", new Color(0.85f, 0.45f, 0.2f), "💀", 0.85f, 4.0f),
            new DialogueLine("FATHER", "Take my savings! Take the deed to our home! Just spare my wife and child!", new Color(0.3f, 0.65f, 0.95f), "👤", 1.25f, 3.8f),
            new DialogueLine("HITMAN 2", "The Syndicate leaves zero loose ends. Squeeze the trigger.", new Color(0.85f, 0.45f, 0.2f), "🔫", 0.8f, 3.2f)
        };

        DialogueManager.Instance?.StartConversation(scene1Dialogue, () => dialogueDone = true);
        while (!dialogueDone) yield return null;

        // Hold the hitmen in idle while the dialogue is on screen, then let them approach.
        Coroutine approachOne = StartCoroutine(MoveActor(hitman1, new Vector3(-1.6f, 1f, 0.8f), 2.4f));
        Coroutine approachTwo = StartCoroutine(MoveActor(hitman2, new Vector3(0.8f, 1f, 0.8f), 2.4f));
        if (approachOne != null) yield return approachOne;
        if (approachTwo != null) yield return approachTwo;

        // Gunshots and execution
        FocusCamera(familyDad.transform.position);
        yield return new WaitForSeconds(0.4f);
        ProceduralAudio.Instance?.PlayGunshot();
        familyDad.transform.rotation = Quaternion.Euler(85f, 0f, 0f);
        familyDad.transform.position += Vector3.down * 0.7f;
        yield return new WaitForSeconds(0.5f);

        FocusCamera(familyMom.transform.position);
        yield return new WaitForSeconds(0.4f);
        ProceduralAudio.Instance?.PlayGunshot();
        familyMom.transform.rotation = Quaternion.Euler(85f, 40f, 0f);
        familyMom.transform.position += Vector3.down * 0.7f;
        yield return new WaitForSeconds(0.5f);

        FocusCamera(familyKid.transform.position);
        yield return new WaitForSeconds(0.4f);
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
        ApplyCutsceneMaterial(portal, new Color(0.1f, 0.8f, 1f));
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

        yield return StartCoroutine(MoveActor(hero, new Vector3(0f, 1f, 3.5f), 2.0f));
    }

    private IEnumerator PlayScene3_VampireSerum()
    {
        ClearStage();

        CreateBlock("SerumChamberFloor", new Vector3(0f, -0.5f, 0f), new Vector3(18f, 1f, 18f), new Color(0.09f, 0.1f, 0.14f));
        CreateBlock("SerumChamberBackWall", new Vector3(0f, 3f, 8f), new Vector3(18f, 6f, 0.8f), new Color(0.08f, 0.06f, 0.1f));
        CreateBlock("SerumChamberLeftWall", new Vector3(-8.6f, 3f, 0f), new Vector3(0.8f, 6f, 16f), new Color(0.08f, 0.06f, 0.1f));
        CreateBlock("SerumChamberRightWall", new Vector3(8.6f, 3f, 0f), new Vector3(0.8f, 6f, 16f), new Color(0.08f, 0.06f, 0.1f));
        CreateBlock("CryptPillarL", new Vector3(-4f, 2.5f, 3f), new Vector3(1.2f, 6f, 1.2f), new Color(0.2f, 0.15f, 0.12f));
        CreateBlock("CryptPillarR", new Vector3(4f, 2.5f, 3f), new Vector3(1.2f, 6f, 1.2f), new Color(0.2f, 0.15f, 0.12f));

        CreateBlock("SerumAltar", new Vector3(0f, 0.8f, 2f), new Vector3(1.2f, 1.6f, 1.2f), new Color(0.1f, 0.1f, 0.1f));
        GameObject serumVial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        serumVial.transform.position = new Vector3(0f, 1.8f, 2f);
        serumVial.transform.localScale = new Vector3(0.25f, 0.4f, 0.25f);
        ApplyCutsceneMaterial(serumVial, Color.red);
        stageActors.Add(serumVial);
        GameObject serumLightObject = new GameObject("SerumChamberLight");
        serumLightObject.transform.position = new Vector3(0f, 2.4f, 2f);
        Light serumLight = serumLightObject.AddComponent<Light>();
        serumLight.type = LightType.Point;
        serumLight.range = 7f;
        serumLight.color = new Color(0.55f, 0.08f, 0.18f);
        serumLight.intensity = 3.5f;
        stageActors.Add(serumLightObject);

        GameObject hero = CreateActor("HeroInLab", new Vector3(0f, 1f, -5f), new Color(0.18f, 0.05f, 0.05f), PrimitiveType.Capsule);

        cutsceneCamera.transform.position = new Vector3(0f, 2.5f, -9f);
        cutsceneCamera.transform.LookAt(new Vector3(0f, 1.2f, 1f));
        Coroutine heroEntry = StartCoroutine(MoveActor(hero, new Vector3(0f, 1f, 0.6f), 1.7f));

        bool dialogueDone = false;
        var scene3Dialogue = new List<DialogueLine>
        {
            new DialogueLine("CRYPT SPIRIT", "Mortal traveler from the future... You stand upon the tomb of the first vampires.", new Color(0.65f, 0.35f, 0.85f), "🦇", 0.72f, 4.0f),
            new DialogueLine("RAYAN MALIK", "My soul died ten years ago. Grant me the power of eternal shadows.", new Color(0.85f, 0.2f, 0.2f), "🩸", 0.95f, 3.5f),
            new DialogueLine("CRYPT SPIRIT", "Drink, then! But remember: daylight will burn your flesh! Now return, and feast upon the guilty!", new Color(0.65f, 0.35f, 0.85f), "🦇", 0.72f, 4.2f)
        };

        DialogueManager.Instance?.StartConversation(scene3Dialogue, () => dialogueDone = true);
        while (!dialogueDone) yield return null;
        if (heroEntry != null) yield return heroEntry;

        serumVial.SetActive(false);
        ProceduralAudio.Instance?.PlaySerumInject();
        ReplaceActorVisual(hero, "Characters/RayanAfterSerum", 1.8f);

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
        ApplyCutsceneMaterial(b, col);
        stageActors.Add(b);
        return b;
    }

    private void ApplyCutsceneMaterial(GameObject gameObject, Color color)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer == null) return;

        Material styledMaterial = EnvironmentStyler.Instance?.GetStyledMaterial("cutscene", color);
        if (styledMaterial != null)
        {
            renderer.material = styledMaterial;
        }
        else
        {
            renderer.material.color = color;
        }
    }

    private GameObject CreateActor(string name, Vector3 pos, Color col, PrimitiveType prim)
    {
        string familyModel = null;
        float familyHeight = 0f;
        Color familyTint = Color.white;
        if (name == "Father") { familyModel = "Characters/the father"; familyHeight = 1.78f; familyTint = new Color(0.68f, 0.76f, 0.88f); }
        else if (name == "Mother") { familyModel = "Characters/the mother"; familyHeight = 1.68f; familyTint = new Color(0.82f, 0.62f, 0.72f); }
        else if (name == "Child") { familyModel = "Characters/the little girl"; familyHeight = 1.18f; familyTint = new Color(0.86f, 0.78f, 0.58f); }
        if (familyModel != null)
        {
            GameObject familyPrefab = Resources.Load<GameObject>(familyModel);
            if (familyPrefab != null)
            {
                // Keep a stable pivot at the actor's center so the existing fall/rotation beats still work.
                GameObject actor = new GameObject(name);
                actor.transform.position = pos;
                GameObject visual = Instantiate(familyPrefab, actor.transform);
                visual.name = name + "Model";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    FitVisualToHeight(visual, familyHeight, actor.transform.position);
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material material in renderer.materials)
                        {
                            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", material.GetColor("_BaseColor") * familyTint);
                            else if (material.HasProperty("_Color")) material.SetColor("_Color", material.GetColor("_Color") * familyTint);
                        }
                    }
                }
                stageActors.Add(actor);
                return actor;
            }
            Debug.LogWarning("Family character model not found in Resources: " + familyModel);
        }

        string modelPath = null;
        if (name.Contains("Vampire")) modelPath = "Characters/VampireHero";
        else if (name.Contains("Hero")) modelPath = "Characters/HumanHero";
        else if (name.Contains("Hitman")) modelPath = "Characters/SyndicateThug";

        if (!string.IsNullOrEmpty(modelPath))
        {
            GameObject prefab = Resources.Load<GameObject>(modelPath);
            if (prefab != null)
            {
                GameObject actor = new GameObject(name);
                actor.transform.position = pos;
                GameObject visual = Instantiate(prefab, actor.transform);
                visual.name = name + "Model";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                float targetHeight = name == "HeroYoung" ? 1.45f : 1.8f;
                CharacterModelUtility.PrepareVisual(visual, modelPath, targetHeight, pos.y);
                CharacterModelUtility.PlayIdle(CharacterModelUtility.FindAnimator(visual));
                stageActors.Add(actor);
                return actor;
            }
        }

        GameObject a = GameObject.CreatePrimitive(prim);
        a.name = name;
        a.transform.position = pos;
        ApplyCutsceneMaterial(a, col);
        stageActors.Add(a);
        return a;
    }

    private static void FitVisualToHeight(GameObject visual, float targetHeight, Vector3 targetCenter)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y <= 0.01f) return;

        Vector3 originalCenter = bounds.center;
        float scale = targetHeight / bounds.size.y;
        visual.transform.localScale *= scale;
        visual.transform.position += targetCenter - (originalCenter + (originalCenter - visual.transform.position) * (scale - 1f));
    }

    private void ReplaceActorVisual(GameObject actor, string resourcePath, float targetHeight)
    {
        for (int i = actor.transform.childCount - 1; i >= 0; i--)
        {
            GameObject previous = actor.transform.GetChild(i).gameObject;
            CharacterModelUtility.ReleaseAnimator(previous);
            previous.SetActive(false);
            Destroy(previous);
        }

        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogWarning("Cutscene character model not found in Resources: " + resourcePath);
            return;
        }

        GameObject visual = Instantiate(prefab, actor.transform);
        visual.name = "TransformedHeroModel";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        if (resourcePath == "Characters/RayanAfterSerum")
            CharacterModelUtility.PrepareVisual(visual, resourcePath, targetHeight, actor.transform.position.y);
        else
            FitVisualToHeight(visual, targetHeight, actor.transform.position);
        CharacterModelUtility.PlayIdle(CharacterModelUtility.FindAnimator(visual));
    }

    private IEnumerator MoveActor(GameObject actor, Vector3 target, float duration)
    {
        if (actor == null) yield break;
        SetActorMoving(actor, true);
        Vector3 start = actor.transform.position;
        Vector3 direction = target - start;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            actor.transform.rotation = Quaternion.LookRotation(direction.normalized);

        float elapsed = 0f;
        while (elapsed < duration && actor != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            actor.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        if (actor != null) actor.transform.position = target;
        SetActorMoving(actor, false);
    }

    private static void SetActorMoving(GameObject actor, bool moving)
    {
        Animator animator = CharacterModelUtility.FindAnimator(actor);
        if (animator == null) return;
        if (moving) CharacterModelUtility.PlayWalk(animator);
        else CharacterModelUtility.PlayIdle(animator);
    }

    private void FocusCamera(Vector3 focus)
    {
        if (cutsceneCamera == null) return;
        cutsceneCamera.transform.position = new Vector3(focus.x * 0.25f, Mathf.Max(2f, focus.y + 1.4f), focus.z - 8f);
        cutsceneCamera.transform.LookAt(focus + Vector3.up * 0.25f);
    }

    private void ClearStage()
    {
        foreach (GameObject obj in stageActors)
        {
            if (obj != null)
            {
                CharacterModelUtility.ReleaseAnimator(obj);
                Destroy(obj);
            }
        }
        stageActors.Clear();
    }
}
