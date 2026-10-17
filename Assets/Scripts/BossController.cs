using UnityEngine;
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    public static BossController Instance { get; private set; }

    [Header("Boss Stats")]
    public float maxHealth = 220f;
    public float currentHealth = 220f;
    public bool isAlive = true;

    [Header("Attacks")]
    public float moveSpeed = 4.2f;
    public float attackRange = 2.4f;
    public float attackDamage = 26f;
    public float attackCooldown = 1.6f;
    public float slamCooldown = 6.0f;

    private float nextAttackTime = 0f;
    private float nextSlamTime = 0f;
    private Animator characterAnimator;
    private readonly List<Material> bossMaterials = new List<Material>();
    private readonly List<Color> originalMaterialColors = new List<Color>();
    private PlayerController player;
    private bool animationStateInitialized;
    private bool isWalkingAnimation;
    private bool isAnimatingAttack;
    private float attackAnimationEndTime;
    private Animator deathAnimator;
    private float deathAnimationEndTime;

    private void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
        string modelResourcePath = "Characters/Enemy6";
        GameObject bossPrefab = Resources.Load<GameObject>(modelResourcePath);
        if (bossPrefab == null)
        {
            modelResourcePath = "Characters/SyndicateBoss";
            bossPrefab = Resources.Load<GameObject>(modelResourcePath);
        }

        if (bossPrefab != null)
        {
            Renderer defR = GetComponent<Renderer>();
            if (defR != null) defR.enabled = false;

            GameObject model = Instantiate(bossPrefab, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            CharacterModelUtility.PrepareVisual(model, modelResourcePath, 2.7f, transform.position.y);
            TintBossModel(model);
        }

        characterAnimator = CharacterModelUtility.FindAnimator(gameObject);
        CharacterModelUtility.PlayEnemyIdle(characterAnimator);
        animationStateInitialized = characterAnimator != null;
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        nextSlamTime = Time.time + slamCooldown;
    }

    private void Update()
    {
        if (!isAlive)
        {
            if (deathAnimator != null && Time.time >= deathAnimationEndTime)
            {
                deathAnimator.speed = 0f;
                deathAnimator = null;
            }
            return;
        }
        if (Time.timeScale <= 0f) return;

        if (isAnimatingAttack)
        {
            if (Time.time < attackAnimationEndTime)
            {
                FacePlayer();
                return;
            }
            isAnimatingAttack = false;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
            return;
        }

        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer.normalized), 5f * Time.deltaTime);
        }

        // Move towards player
        if (dist > attackRange)
        {
            transform.position += toPlayer.normalized * (moveSpeed * Time.deltaTime);
            UpdateLocomotionAnimation(true);
        }
        else
        {
            UpdateLocomotionAnimation(false);
        }

        // Heavy Melee Strike
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            float attackDuration = CharacterModelUtility.PlayEnemyAttack(characterAnimator);
            if (attackDuration > 0f)
            {
                isAnimatingAttack = true;
                attackAnimationEndTime = Time.time + attackDuration;
            }
            ProceduralAudio.Instance?.PlayPunch();
            ProceduralAudio.Instance?.PlayVoiceBlip(0.6f);
            player.TakeDamage(attackDamage);
        }

        // Ground Slam Area Shockwave
        if (Time.time >= nextSlamTime)
        {
            nextSlamTime = Time.time + slamCooldown;
            PerformGroundSlam();
        }
    }

    private void PerformGroundSlam()
    {
        ProceduralAudio.Instance?.PlayBossSlam();
        UIManager.Instance?.SetAlertText("BOSS SHOCKWAVE!");

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= 8f)
            {
                player.TakeDamage(18f);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!isAlive) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        SetBossColor(new Color(0.82f, 0.78f, 1f));
        Invoke(nameof(RestoreColor), 0.15f);

        if (currentHealth <= 0f)
        {
            DefeatBoss();
        }
    }

    private void RestoreColor()
    {
        if (isAlive) RestoreBossColors();
    }

    private void DefeatBoss()
    {
        isAlive = false;
        isAnimatingAttack = false;
        ProceduralAudio.Instance?.PlayBossSlam();

        float deathDuration = CharacterModelUtility.PlayEnemyDeath(characterAnimator);
        if (deathDuration > 0f)
        {
            deathAnimator = characterAnimator;
            deathAnimationEndTime = Time.time + deathDuration;
        }
        else if (characterAnimator != null)
        {
            CharacterModelUtility.PlayEnemyIdle(characterAnimator);
            characterAnimator.speed = 0f;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var victoryDialog = new System.Collections.Generic.List<DialogueLine>
        {
            new DialogueLine("KALEEM RANA", "Impossible... The Syndicate was invincible... You're a monster...", new Color(0.95f, 0.75f, 0.2f), "👑", 0.68f, 4.0f),
            new DialogueLine("VAMPIRE RAYAN", "You made me a monster ten years ago in that alley. Rest in peace, mother... father. It's done.", new Color(0.9f, 0.15f, 0.15f), "🧛", 0.65f, 4.5f)
        };

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartConversation(victoryDialog);
        }
        else
        {
            UIManager.Instance?.ShowVictory();
        }
    }

    private void UpdateLocomotionAnimation(bool moving)
    {
        if (characterAnimator == null || isAnimatingAttack || !isAlive) return;
        if (!animationStateInitialized || moving != isWalkingAnimation)
        {
            if (moving) CharacterModelUtility.PlayEnemyWalk(characterAnimator);
            else CharacterModelUtility.PlayEnemyIdle(characterAnimator);
            isWalkingAnimation = moving;
            animationStateInitialized = true;
        }
    }

    private void FacePlayer()
    {
        if (player == null) return;
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer.normalized), 5f * Time.deltaTime);
    }

    private void TintBossModel(GameObject model)
    {
        Color tint = new Color(0.58f, 0.42f, 0.88f);
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.materials)
            {
                if (material == null) continue;
                string property = material.HasProperty("_BaseColor") ? "_BaseColor" : (material.HasProperty("_Color") ? "_Color" : null);
                if (property == null) continue;
                Color baseColor = material.GetColor(property);
                Color factionColor = new Color(baseColor.r * tint.r, baseColor.g * tint.g, baseColor.b * tint.b, baseColor.a);
                material.SetColor(property, factionColor);
                bossMaterials.Add(material);
                originalMaterialColors.Add(factionColor);
            }
        }
    }

    private void SetBossColor(Color color)
    {
        foreach (Material material in bossMaterials)
            if (material != null) SetMaterialColor(material, color);
    }

    private void RestoreBossColors()
    {
        for (int i = 0; i < bossMaterials.Count; i++)
            if (bossMaterials[i] != null) SetMaterialColor(bossMaterials[i], originalMaterialColors[i]);
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private void OnDestroy()
    {
        CharacterModelUtility.ReleaseAnimator(gameObject);
        foreach (Material material in bossMaterials)
            if (material != null) Destroy(material);
    }
}
