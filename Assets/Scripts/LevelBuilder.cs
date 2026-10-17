using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public static LevelBuilder Instance { get; private set; }

    private readonly List<GameObject> activeLevelObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ClearCurrentLevel()
    {
        foreach (GameObject go in activeLevelObjects)
        {
            if (go != null) Destroy(go);
        }
        activeLevelObjects.Clear();
    }

    // ========== LEVEL 1: Urban Noir Alley ==========
    public void BuildLevel1(PlayerController player)
    {
        ClearCurrentLevel();
        EnvironmentStyler.Instance?.ApplyLevelTheme(1);

        // Wet Reflective Asphalt Floor
        SpawnBlock("AlleyFloor", new Vector3(0f, -0.5f, 25f), new Vector3(20f, 1f, 65f), "wet_asphalt");

        // 4 Enclosing Walls with Weathered Brick
        SpawnBlock("NorthWall", new Vector3(0f, 4f, 57f), new Vector3(20f, 8f, 1.2f), "brick_dark");
        SpawnBlock("SouthWall", new Vector3(0f, 4f, -7f), new Vector3(20f, 8f, 1.2f), "brick_dark");
        SpawnBlock("WestWall", new Vector3(-10f, 4f, 25f), new Vector3(1.2f, 8f, 65f), "brick_shadow");
        SpawnBlock("EastWall", new Vector3(10f, 4f, 25f), new Vector3(1.2f, 8f, 65f), "brick_shadow");

        // Modular urban wall facades
        for (float z = 0f; z <= 50f; z += 10f)
        {
            SpawnProp("Environment/UrbanWall", new Vector3(-9.2f, 0f, z), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.5f);
            SpawnProp("Environment/UrbanWall", new Vector3(9.2f, 0f, z), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 2.5f);
        }

        // Glowing Neon Accent Bars (Urban Noir Aesthetic)
        SpawnBlock("NeonBar1", new Vector3(-9.3f, 4f, 15f), new Vector3(0.2f, 0.4f, 6f), "neon_cyan");
        SpawnBlock("NeonBar2", new Vector3(9.3f, 5f, 35f), new Vector3(0.2f, 0.4f, 5f), "neon_red");

        // Overhead Fire Escape Shadow Beams
        SpawnBlock("ShadowBeam1", new Vector3(0f, 6f, 15f), new Vector3(20f, 0.5f, 4f), "brick_shadow");
        SpawnBlock("ShadowBeam2", new Vector3(0f, 6f, 38f), new Vector3(20f, 0.5f, 4f), "brick_shadow");

        // Cover props
        SpawnDumpster(new Vector3(6f, 0f, 10f), Quaternion.Euler(0f, -30f, 0f));
        SpawnDumpster(new Vector3(6f, 0f, 40f), Quaternion.Euler(0f, 15f, 0f));
        SpawnProp("Environment/UrbanBarrier", new Vector3(-6f, 0f, 24f), Quaternion.identity, Vector3.one * 2.0f);

        // 3 Sodium Vapor Street Lamps
        SpawnStreetLamp(new Vector3(7f, 0f, 8f), 12f, new Color(1f, 0.85f, 0.55f));
        SpawnStreetLamp(new Vector3(-7f, 0f, 26f), 12f, new Color(1f, 0.82f, 0.5f));
        SpawnStreetLamp(new Vector3(7f, 0f, 44f), 12f, new Color(1f, 0.85f, 0.55f));

        // 2 Syndicate Thugs
        SpawnThug(new Vector3(0f, 0f, 18f), new Vector3[] { new Vector3(-4f, 0f, 18f), new Vector3(4f, 0f, 18f) }, 1.0f);
        SpawnThug(new Vector3(0f, 0f, 36f), new Vector3[] { new Vector3(4f, 0f, 36f), new Vector3(-4f, 0f, 36f) }, 1.0f);

        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.1f, -2f);
            player.transform.rotation = Quaternion.identity;
        }
    }

    // ========== LEVEL 2: Haunted Cemetery ==========
    public void BuildLevel2_Graveyard(PlayerController player)
    {
        ClearCurrentLevel();
        EnvironmentStyler.Instance?.ApplyLevelTheme(2);

        // Damp Dark Earth Ground
        SpawnBlock("GraveyardGround", new Vector3(0f, -0.5f, 25f), new Vector3(50f, 1f, 60f), "graveyard_earth");

        // 4 Weathered Perimeter Walls
        SpawnBlock("GY_Wall_N", new Vector3(0f, 3f, 55f), new Vector3(50f, 6f, 1.5f), "mossy_stone");
        SpawnBlock("GY_Wall_S", new Vector3(0f, 3f, -5f), new Vector3(50f, 6f, 1.5f), "mossy_stone");
        SpawnBlock("GY_Wall_W", new Vector3(-25f, 3f, 25f), new Vector3(1.5f, 6f, 60f), "mossy_stone");
        SpawnBlock("GY_Wall_E", new Vector3(25f, 3f, 25f), new Vector3(1.5f, 6f, 60f), "mossy_stone");

        // Stone Wall Facades
        for (float z = -2f; z <= 52f; z += 8f)
        {
            SpawnProp("Environment/stone-wall", new Vector3(-24f, 0f, z), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.8f);
            SpawnProp("Environment/stone-wall", new Vector3(24f, 0f, z), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 2.8f);
        }

        // Iron Gate Entrance
        SpawnProp("Environment/iron-fence-border-gate", new Vector3(0f, 0f, -3.5f), Quaternion.identity, Vector3.one * 2.5f);
        SpawnProp("Environment/iron-fence-border", new Vector3(-8f, 0f, -3.5f), Quaternion.identity, Vector3.one * 2.5f);
        SpawnProp("Environment/iron-fence-border", new Vector3(8f, 0f, -3.5f), Quaternion.identity, Vector3.one * 2.5f);

        // Gravestones with atmospheric offsets
        float[] gZ = { 6f, 13f, 20f, 27f, 34f, 41f };
        string[] gModels = { "Environment/gravestone-cross", "Environment/gravestone-decorative", "Environment/gravestone-round", "Environment/gravestone-wide", "Environment/gravestone-broken" };
        int idx = 0;
        foreach (float z in gZ)
        {
            for (float x = -17f; x <= 17f; x += 7f)
            {
                float rOff = Mathf.Sin(x * 13.7f + z * 7.3f) * 0.5f;
                float rRot = Mathf.Sin(x * 3.1f + z * 11.2f) * 12f;
                SpawnProp(gModels[idx % gModels.Length], new Vector3(x + rOff, 0f, z + rOff), Quaternion.Euler(0f, rRot, 0f), Vector3.one * 2.2f);
                idx++;
            }
        }

        // Crypts with dark shadow roofs
        SpawnProp("Environment/crypt-a", new Vector3(-15f, 0f, 25f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 3.5f);
        SpawnProp("Environment/crypt-small", new Vector3(16f, 0f, 38f), Quaternion.identity, Vector3.one * 3.0f);
        SpawnBlock("CryptRoof1", new Vector3(-15f, 4.5f, 25f), new Vector3(6f, 0.6f, 8f), "mossy_stone");
        SpawnBlock("CryptRoof2", new Vector3(16f, 3.8f, 38f), new Vector3(5f, 0.5f, 5f), "mossy_stone");

        // Dead Crooked Pines & Rocks
        SpawnProp("Environment/pine-crooked", new Vector3(-8f, 0f, 8f), Quaternion.identity, Vector3.one * 3.5f);
        SpawnProp("Environment/pine", new Vector3(12f, 0f, 14f), Quaternion.Euler(0f, 45f, 0f), Vector3.one * 3.0f);
        SpawnProp("Environment/pine-crooked", new Vector3(-18f, 0f, 42f), Quaternion.Euler(0f, 130f, 0f), Vector3.one * 4.0f);
        SpawnProp("Environment/coffin-old", new Vector3(-14f, 0f, 28f), Quaternion.Euler(0f, 20f, 0f), Vector3.one * 2.5f);
        SpawnProp("Environment/altar-stone", new Vector3(0f, 0f, 48f), Quaternion.identity, Vector3.one * 3.0f);

        // Flickering Torches & Eerie Lanterns
        SpawnFlickeringLantern(new Vector3(-10f, 0f, 18f), new Color(1f, 0.6f, 0.2f), 10f);
        SpawnFlickeringLantern(new Vector3(10f, 0f, 36f), new Color(1f, 0.6f, 0.2f), 10f);
        SpawnFlickeringLantern(new Vector3(0f, 0f, 48f), new Color(0.3f, 0.9f, 0.8f), 8f); // Spectral altar flame

        // 3 Syndicate Scouts
        SpawnThug(new Vector3(-6f, 0f, 12f), new Vector3[] { new Vector3(-12f, 0f, 12f), new Vector3(-6f, 0f, 20f) }, 1.15f);
        SpawnThug(new Vector3(8f, 0f, 28f), new Vector3[] { new Vector3(14f, 0f, 28f), new Vector3(8f, 0f, 36f) }, 1.15f);
        SpawnThug(new Vector3(0f, 0f, 44f), new Vector3[] { new Vector3(-8f, 0f, 44f), new Vector3(8f, 0f, 44f) }, 1.15f);

        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.1f, -1f);
            player.transform.rotation = Quaternion.identity;
        }
    }

    // ========== LEVEL 3: Industrial Warehouse ==========
    public void BuildLevel3_Warehouse(PlayerController player)
    {
        ClearCurrentLevel();
        EnvironmentStyler.Instance?.ApplyLevelTheme(3);

        // Polished Industrial Concrete Floor
        SpawnBlock("WarehouseFloor", new Vector3(0f, -0.5f, 25f), new Vector3(45f, 1f, 55f), "industrial_floor");

        // 4 High Warehouse Perimeter Walls
        SpawnBlock("WH_N", new Vector3(0f, 5f, 52f), new Vector3(45f, 10f, 1.5f), "brick_dark");
        SpawnBlock("WH_S", new Vector3(0f, 5f, -2f), new Vector3(45f, 10f, 1.5f), "brick_dark");
        SpawnBlock("WH_W", new Vector3(-22f, 5f, 25f), new Vector3(1.5f, 10f, 55f), "brick_dark");
        SpawnBlock("WH_E", new Vector3(22f, 5f, 25f), new Vector3(1.5f, 10f, 55f), "brick_dark");

        // Steel Roof Beams
        for (int z = 5; z <= 45; z += 12)
            SpawnBlock($"RoofBeam_{z}", new Vector3(0f, 9.5f, z), new Vector3(45f, 1f, 3f), "brick_shadow");

        // Color-Coded Shipping Containers
        SpawnBlock("Container1", new Vector3(-10f, 2f, 14f), new Vector3(3.5f, 4f, 12f), "container_rust");
        SpawnBlock("Container2", new Vector3(10f, 2f, 18f), new Vector3(3.5f, 4f, 12f), "container_blue");
        SpawnBlock("Container3", new Vector3(-6f, 2f, 36f), new Vector3(14f, 4f, 3.5f), "container_gold");
        SpawnBlock("Container4", new Vector3(10f, 2f, 40f), new Vector3(3.5f, 4f, 10f), "container_rust");

        // Industrial Halogen Floodlamps
        SpawnStreetLamp(new Vector3(-16f, 0f, 25f), 16f, new Color(0.95f, 0.95f, 1f));
        SpawnStreetLamp(new Vector3(16f, 0f, 25f), 16f, new Color(0.95f, 0.95f, 1f));

        // 4 Enforcers (1.3x)
        SpawnThug(new Vector3(-8f, 0f, 12f), new Vector3[] { new Vector3(-8f, 0f, 8f), new Vector3(-8f, 0f, 20f) }, 1.3f);
        SpawnThug(new Vector3(8f, 0f, 15f), new Vector3[] { new Vector3(8f, 0f, 8f), new Vector3(8f, 0f, 24f) }, 1.3f);
        SpawnThug(new Vector3(-4f, 0f, 32f), new Vector3[] { new Vector3(-12f, 0f, 32f), new Vector3(4f, 0f, 32f) }, 1.3f);
        SpawnThug(new Vector3(6f, 0f, 44f), new Vector3[] { new Vector3(0f, 0f, 44f), new Vector3(14f, 0f, 44f) }, 1.3f);

        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.1f, 2f);
            player.transform.rotation = Quaternion.identity;
        }
    }

    // ========== LEVEL 4: Syndicate Boss Penthouse ==========
    public void BuildLevel4_Boss(PlayerController player)
    {
        ClearCurrentLevel();
        EnvironmentStyler.Instance?.ApplyLevelTheme(4);

        // Reflective Obsidian Marble Floor
        SpawnBlock("PenthouseFloor", new Vector3(0f, -0.5f, 20f), new Vector3(40f, 1f, 40f), "marble_black");

        // Regal Crimson Center Carpet
        SpawnBlock("CenterCarpet", new Vector3(0f, 0.02f, 20f), new Vector3(8f, 0.05f, 36f), "velvet_crimson");

        // Perimeter Enclosing Walls
        SpawnBlock("P_N", new Vector3(0f, 5f, 40f), new Vector3(40f, 10f, 1.5f), "brick_shadow");
        SpawnBlock("P_S", new Vector3(0f, 5f, 0f), new Vector3(40f, 10f, 1.5f), "brick_shadow");
        SpawnBlock("P_W", new Vector3(-20f, 5f, 20f), new Vector3(1.5f, 10f, 40f), "brick_shadow");
        SpawnBlock("P_E", new Vector3(20f, 5f, 20f), new Vector3(1.5f, 10f, 40f), "brick_shadow");

        // 4 Ornate Gold & Marble Pillars
        Vector3[] pillarPos = { new Vector3(-10f, 0f, 10f), new Vector3(10f, 0f, 10f), new Vector3(-10f, 0f, 30f), new Vector3(10f, 0f, 30f) };
        foreach (Vector3 p in pillarPos)
        {
            SpawnProp("Environment/DungeonPillar", p, Quaternion.identity, new Vector3(2.5f, 4.5f, 2.5f));
            SpawnBlock("PillarTrim", p + Vector3.up * 8f, new Vector3(3.2f, 0.8f, 3.2f), "gold_ornate");
        }

        // Gothic Gate behind Throne
        SpawnProp("Environment/DungeonGate", new Vector3(0f, 0f, 38.5f), Quaternion.identity, Vector3.one * 3f);

        // Boss Rana
        SpawnBoss(new Vector3(0f, 0.1f, 26f));

        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.1f, 4f);
            player.transform.rotation = Quaternion.identity;
        }
    }

    // ==================== HELPERS ====================
    private GameObject SpawnBlock(string name, Vector3 pos, Vector3 scale, string materialType)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = name;
        b.transform.position = pos;
        b.transform.localScale = scale;

        Material m = EnvironmentStyler.Instance != null ? EnvironmentStyler.Instance.GetStyledMaterial(materialType) : null;
        if (m != null) b.GetComponent<Renderer>().material = m;

        activeLevelObjects.Add(b);
        return b;
    }

    private GameObject SpawnProp(string resourcePath, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab != null)
        {
            GameObject inst = Instantiate(prefab, pos, rot);
            inst.transform.localScale = scale;
            activeLevelObjects.Add(inst);
            return inst;
        }
        return null;
    }

    private void SpawnDumpster(Vector3 pos, Quaternion rot)
    {
        GameObject d = SpawnProp("Environment/Dumpster", pos, rot, Vector3.one * 1.8f);
        if (d == null) SpawnBlock("Dumpster", pos + Vector3.up * 1f, new Vector3(2.5f, 2f, 4f), "container_rust");
    }

    private void SpawnStreetLamp(Vector3 basePos, float range, Color lightColor)
    {
        GameObject lampObj = new GameObject("StreetLamp");
        lampObj.transform.position = basePos;

        GameObject meshPrefab = Resources.Load<GameObject>("Environment/StreetLight");
        if (meshPrefab != null)
        {
            GameObject mesh = Instantiate(meshPrefab, lampObj.transform);
            mesh.transform.localScale = Vector3.one * 1.6f;
        }

        GameObject head = new GameObject("LampLight");
        head.transform.SetParent(lampObj.transform);
        head.transform.localPosition = new Vector3(0f, 4.5f, 0.5f);
        head.transform.rotation = Quaternion.Euler(65f, 0f, 0f);

        Light l = head.AddComponent<Light>();
        l.type = LightType.Spot;
        l.range = range;
        l.spotAngle = 75f;
        l.intensity = 5f;
        l.color = lightColor;
        l.shadows = LightShadows.Soft;

        activeLevelObjects.Add(lampObj);
    }

    private void SpawnFlickeringLantern(Vector3 basePos, Color color, float range)
    {
        GameObject lampObj = new GameObject("GraveyardLantern");
        lampObj.transform.position = basePos;

        GameObject head = new GameObject("LanternLight");
        head.transform.SetParent(lampObj.transform);
        head.transform.localPosition = new Vector3(0f, 2.5f, 0f);

        Light l = head.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = range;
        l.color = color;
        l.intensity = 3.5f;
        l.shadows = LightShadows.Soft;

        EnvironmentStyler.Instance?.RegisterFlickerLight(l);
        activeLevelObjects.Add(lampObj);
    }

    private void SpawnThug(Vector3 pos, Vector3[] patrol, float damageMultiplier)
    {
        GameObject thug = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        thug.name = "Syndicate Thug";
        thug.transform.position = pos + Vector3.up * 1f;

        EnemyAI ai = thug.AddComponent<EnemyAI>();
        ai.damageMultiplier = damageMultiplier;
        ai.SetPatrolRoute(patrol);

        activeLevelObjects.Add(thug);
    }

    private void SpawnBoss(Vector3 pos)
    {
        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boss.name = "Syndicate Boss";
        boss.transform.position = pos + Vector3.up * 1.5f;
        boss.transform.localScale = new Vector3(1.6f, 1.8f, 1.6f);

        boss.AddComponent<BossController>();
        activeLevelObjects.Add(boss);
    }
}
