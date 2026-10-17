using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpace3DUI : MonoBehaviour
{
    public static WorldSpace3DUI Instance { get; private set; }

    private class TrackedEnemy
    {
        public EnemyAI enemy;
        public GameObject plateRoot;
        public Image healthBarFill;
        public Text statusText;
        public float lastHealth;
    }

    private class FloatingText
    {
        public GameObject root;
        public Text textComponent;
        public Vector3 position;
        public Vector3 velocity;
        public float lifeTime;
        public float maxLife;
        public Color baseColor;
    }

    private readonly List<TrackedEnemy> trackedEnemies = new List<TrackedEnemy>();
    private readonly List<FloatingText> activePopups = new List<FloatingText>();

    private PlayerController player;
    private Camera mainCamera;
    private Font gameFont;
    private Sprite solidSprite;

    // Diegetic 3D Serum Gauge (In First-Person View)
    private GameObject serumVialRoot;
    private Transform serumFluidTransform;
    private Material serumFluidMaterial;
    private TextMesh serum3DText;
    private float lastPlayerHealth;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (FindAnyObjectByType<WorldSpace3DUI>() == null)
        {
            GameObject obj = new GameObject("NIGHTBOUND_3D_UI_UX");
            obj.AddComponent<WorldSpace3DUI>();
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

        gameFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (gameFont == null) gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        solidSprite = CreateSolidSprite();
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        mainCamera = Camera.main;
        if (player != null)
        {
            lastPlayerHealth = player.currentHealth;
            Build3DSerumVial();
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
            if (player != null && serumVialRoot == null)
            {
                lastPlayerHealth = player.currentHealth;
                Build3DSerumVial();
            }
        }

        UpdateEnemyPlates();
        UpdateFloatingPopups();
        UpdateDiegeticVial();
        CheckPlayerDamage();
    }

    // ================= 1. 3D ENEMY OVERHEAD BILLBOARD BADGES =================
    private void UpdateEnemyPlates()
    {
        // Auto-discover active enemies in scene
        EnemyAI[] currentEnemies = FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude);
        foreach (EnemyAI e in currentEnemies)
        {
            if (e != null && e.isAlive && !trackedEnemies.Exists(t => t.enemy == e))
            {
                CreateEnemyPlate(e);
            }
        }

        for (int i = trackedEnemies.Count - 1; i >= 0; i--)
        {
            TrackedEnemy t = trackedEnemies[i];
            if (t.enemy == null || !t.enemy.isAlive)
            {
                if (t.plateRoot != null) Destroy(t.plateRoot);
                trackedEnemies.RemoveAt(i);
                continue;
            }

            // Detect damage and spawn 3D floating damage number
            if (t.enemy.currentHealth < t.lastHealth)
            {
                float diff = t.lastHealth - t.enemy.currentHealth;
                bool isCrit = diff >= 35f;
                SpawnPopup(t.enemy.transform.position + Vector3.up * 1.8f,
                    isCrit ? $"-{Mathf.RoundToInt(diff)} CRIT!" : $"-{Mathf.RoundToInt(diff)}",
                    isCrit ? new Color(1f, 0.15f, 0.15f) : new Color(1f, 0.85f, 0.2f), isCrit ? 26 : 20);
                t.lastHealth = t.enemy.currentHealth;
            }

            // Position and billboard to camera
            Vector3 worldPos = t.enemy.transform.position + Vector3.up * 2.2f;
            t.plateRoot.transform.position = worldPos;
            t.plateRoot.transform.rotation = Quaternion.LookRotation(t.plateRoot.transform.position - mainCamera.transform.position);

            // Update health bar fill
            if (t.healthBarFill != null)
            {
                t.healthBarFill.fillAmount = Mathf.Clamp01(t.enemy.currentHealth / t.enemy.maxHealth);
            }

            // Update status text
            if (t.statusText != null)
            {
                if (t.enemy.isStunned)
                {
                    t.statusText.text = "★ STUNNED [E] BITE ★";
                    t.statusText.color = new Color(1f, 0.85f, 0.1f);
                }
                else if (player != null && player.targetedEnemy == t.enemy && player.canBiteTarget)
                {
                    t.statusText.text = "🩸 [E] BITE TARGET 🩸";
                    t.statusText.color = new Color(1f, 0.2f, 0.2f);
                }
                else if (t.enemy.isChasingPlayer)
                {
                    t.statusText.text = "⚠ ALERTED [CHASE]";
                    t.statusText.color = new Color(1f, 0.4f, 0.2f);
                }
                else
                {
                    t.statusText.text = "PATROL";
                    t.statusText.color = new Color(0.7f, 0.7f, 0.75f, 0.8f);
                }
            }
        }
    }

    private void CreateEnemyPlate(EnemyAI enemy)
    {
        GameObject canvasObj = new GameObject("OverheadBadge_" + enemy.name, typeof(Canvas));
        Canvas c = canvasObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        RectTransform rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180f, 45f);
        rt.localScale = Vector3.one * 0.007f;

        // Background
        GameObject bg = CreateUIBox(canvasObj.transform, "BG", Vector2.zero, new Vector2(180f, 45f), new Color(0.04f, 0.04f, 0.06f, 0.85f));

        // Health Bar BG & Fill
        GameObject hbBg = CreateUIBox(bg.transform, "HealthBG", new Vector2(0f, -8f), new Vector2(160f, 10f), new Color(0.2f, 0.05f, 0.05f, 0.9f));
        GameObject hbFill = CreateUIBox(hbBg.transform, "HealthFill", Vector2.zero, new Vector2(160f, 10f), new Color(0.85f, 0.15f, 0.15f));
        Image fillImg = hbFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;

        // Name and Status Text
        Text nameTxt = CreateUIText(bg.transform, enemy.name.ToUpper(), 11, TextAnchor.UpperCenter);
        nameTxt.rectTransform.anchoredPosition = new Vector2(0f, 16f);
        nameTxt.color = new Color(0.9f, 0.9f, 0.95f);

        Text stTxt = CreateUIText(bg.transform, "PATROL", 10, TextAnchor.LowerCenter);
        stTxt.rectTransform.anchoredPosition = new Vector2(0f, -19f);

        trackedEnemies.Add(new TrackedEnemy
        {
            enemy = enemy,
            plateRoot = canvasObj,
            healthBarFill = fillImg,
            statusText = stTxt,
            lastHealth = enemy.currentHealth
        });
    }

    // ================= 2. 3D FLOATING COMBAT NUMBERS =================
    public void SpawnPopup(Vector3 worldPos, string text, Color color, int fontSize = 20)
    {
        GameObject pObj = new GameObject("PopupText", typeof(Canvas));
        Canvas c = pObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        RectTransform rt = pObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250f, 60f);
        rt.localScale = Vector3.one * 0.012f;
        pObj.transform.position = worldPos;

        Text t = CreateUIText(pObj.transform, text, fontSize, TextAnchor.MiddleCenter);
        t.color = color;

        activePopups.Add(new FloatingText
        {
            root = pObj,
            textComponent = t,
            position = worldPos,
            velocity = new Vector3(Random.Range(-0.3f, 0.3f), 1.5f, Random.Range(-0.3f, 0.3f)),
            lifeTime = 0f,
            maxLife = 0.9f,
            baseColor = color
        });
    }

    private void UpdateFloatingPopups()
    {
        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            FloatingText p = activePopups[i];
            p.lifeTime += Time.unscaledDeltaTime;
            if (p.lifeTime >= p.maxLife || p.root == null)
            {
                if (p.root != null) Destroy(p.root);
                activePopups.RemoveAt(i);
                continue;
            }

            p.position += p.velocity * Time.unscaledDeltaTime;
            p.root.transform.position = p.position;
            p.root.transform.rotation = Quaternion.LookRotation(p.position - mainCamera.transform.position);

            float progress = p.lifeTime / p.maxLife;
            Color c = p.baseColor;
            c.a = 1f - Mathf.Pow(progress, 2f);
            p.textComponent.color = c;
        }
    }

    // ================= 3. DIEGETIC 3D FIRST-PERSON SERUM VIAL =================
    private void Build3DSerumVial()
    {
        if (mainCamera == null) return;

        serumVialRoot = new GameObject("3D_SerumVial_Gauge");
        serumVialRoot.transform.SetParent(mainCamera.transform, false);
        serumVialRoot.transform.localPosition = new Vector3(0.48f, -0.32f, 0.85f);
        serumVialRoot.transform.localRotation = Quaternion.Euler(15f, -25f, 5f);

        // Glass Outer Tube
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glass.name = "GlassTube";
        glass.transform.SetParent(serumVialRoot.transform, false);
        glass.transform.localScale = new Vector3(0.045f, 0.16f, 0.045f);
        Destroy(glass.GetComponent<Collider>());
        glass.GetComponent<Renderer>().material.color = new Color(0.2f, 0.6f, 0.9f, 0.35f);

        // Metallic End Caps
        GameObject topCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        topCap.transform.SetParent(serumVialRoot.transform, false);
        topCap.transform.localPosition = new Vector3(0f, 0.17f, 0f);
        topCap.transform.localScale = new Vector3(0.052f, 0.02f, 0.052f);
        Destroy(topCap.GetComponent<Collider>());
        topCap.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.18f);

        GameObject btmCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        btmCap.transform.SetParent(serumVialRoot.transform, false);
        btmCap.transform.localPosition = new Vector3(0f, -0.17f, 0f);
        btmCap.transform.localScale = new Vector3(0.052f, 0.02f, 0.052f);
        Destroy(btmCap.GetComponent<Collider>());
        btmCap.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.18f);

        // Glowing Crimson Fluid Cylinder
        GameObject fluid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fluid.name = "SerumFluid";
        fluid.transform.SetParent(serumVialRoot.transform, false);
        fluid.transform.localPosition = new Vector3(0f, 0f, 0f);
        fluid.transform.localScale = new Vector3(0.038f, 0.15f, 0.038f);
        Destroy(fluid.GetComponent<Collider>());
        serumFluidTransform = fluid.transform;
        serumFluidMaterial = fluid.GetComponent<Renderer>().material;
        serumFluidMaterial.color = new Color(0.95f, 0.08f, 0.12f);

        // 3D Text Label
        GameObject label = new GameObject("3D_VialText");
        label.transform.SetParent(serumVialRoot.transform, false);
        label.transform.localPosition = new Vector3(0f, -0.23f, 0f);
        label.transform.localRotation = Quaternion.identity;
        label.transform.localScale = Vector3.one * 0.008f;
        serum3DText = label.AddComponent<TextMesh>();
        serum3DText.fontSize = 24;
        serum3DText.alignment = TextAlignment.Center;
        serum3DText.anchor = TextAnchor.MiddleCenter;
        serum3DText.color = Color.white;
        serum3DText.text = "SERUM 120s";
    }

    private void UpdateDiegeticVial()
    {
        if (player == null || serumVialRoot == null) return;

        if (player.isVampire)
        {
            float ratio = Mathf.Clamp01(player.serumTimer / player.serumDuration);
            serumFluidTransform.localScale = new Vector3(0.038f, 0.15f * ratio, 0.038f);
            serumFluidTransform.localPosition = new Vector3(0f, -0.15f * (1f - ratio) * 0.5f, 0f);
            serumFluidMaterial.color = new Color(0.95f, 0.08f, 0.12f);
            serum3DText.text = $"SERUM {Mathf.CeilToInt(player.serumTimer)}s";
            serum3DText.color = Color.white;
        }
        else
        {
            // Human cooldown mode
            float pulse = Mathf.PingPong(Time.unscaledTime * 4f, 1f);
            serumFluidTransform.localScale = new Vector3(0.038f, 0.02f, 0.038f);
            serumFluidMaterial.color = Color.Lerp(Color.black, Color.red, pulse);

            if (player.serumCooldownTimer > 0f)
            {
                serum3DText.text = $"COOLING {Mathf.CeilToInt(player.serumCooldownTimer)}s";
                serum3DText.color = new Color(1f, 0.5f, 0.2f);
            }
            else
            {
                serum3DText.text = "PRESS [V] INJECT!";
                serum3DText.color = Color.Lerp(Color.yellow, Color.red, pulse);
            }
        }
    }

    private void CheckPlayerDamage()
    {
        if (player == null) return;

        if (player.currentHealth < lastPlayerHealth)
        {
            float loss = lastPlayerHealth - player.currentHealth;
            if (loss >= 1.5f)
            {
                Vector3 headPos = player.transform.position + Vector3.up * 1.5f;
                SpawnPopup(headPos, $"-{Mathf.RoundToInt(loss)} HP", new Color(1f, 0.25f, 0.25f), 22);
            }
            lastPlayerHealth = player.currentHealth;
        }
        else if (player.currentHealth > lastPlayerHealth)
        {
            float gain = player.currentHealth - lastPlayerHealth;
            if (gain >= 5f)
            {
                Vector3 headPos = player.transform.position + Vector3.up * 1.5f;
                SpawnPopup(headPos, $"+{Mathf.RoundToInt(gain)} HEAL", new Color(0.3f, 1f, 0.4f), 20);
            }
            lastPlayerHealth = player.currentHealth;
        }
    }

    private GameObject CreateUIBox(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.GetComponent<Image>();
        img.sprite = solidSprite;
        img.color = color;
        return go;
    }

    private Text CreateUIText(Transform parent, string content, int fontSize, TextAnchor alignment)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.font = gameFont;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.text = content;
        t.color = Color.white;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180f, 30f);
        return t;
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Color[] px = { Color.white, Color.white, Color.white, Color.white };
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
    }
}
