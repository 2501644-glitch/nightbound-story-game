using UnityEngine;

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
    private Renderer bossRenderer;
    private PlayerController player;

    private void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
        bossRenderer = GetComponentInChildren<Renderer>();

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
            Renderer mr = model.GetComponentInChildren<Renderer>();
            if (mr != null) bossRenderer = mr;
        }
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        nextSlamTime = Time.time + slamCooldown;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (!isAlive) return;

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
        }

        // Heavy Melee Strike
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
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

        if (bossRenderer != null && bossRenderer.material != null)
        {
            bossRenderer.material.color = Color.white;
            Invoke(nameof(RestoreColor), 0.15f);
        }

        if (currentHealth <= 0f)
        {
            DefeatBoss();
        }
    }

    private void RestoreColor()
    {
        if (bossRenderer != null && bossRenderer.material != null && isAlive)
        {
            bossRenderer.material.color = new Color(0.35f, 0.05f, 0.05f);
        }
    }

    private void DefeatBoss()
    {
        isAlive = false;
        ProceduralAudio.Instance?.PlayBossSlam();

        transform.rotation = Quaternion.Euler(85f, transform.eulerAngles.y, 0f);
        transform.position = new Vector3(transform.position.x, 0.3f, transform.position.z);

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
}
