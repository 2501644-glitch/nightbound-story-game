using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    private static int nextVisualVariant;

    [Header("Combat Stats")]
    public float maxHealth = 60f;
    public float currentHealth = 60f;
    public float baseAttackDamage = 16f;
    public float damageMultiplier = 1.0f;
    public float attackRange = 1.8f;
    public float attackCooldown = 1.2f;

    [Header("Stealth & Sight")]
    public float sightRangeInLight = 18f;
    public float sightRangeInShadow = 5f;
    public float fieldOfView = 85f;
    public float patrolSpeed = 2.6f;
    public float chaseSpeed = 5.2f;

    [Header("State")]
    public bool isAlive = true;
    public bool isStunned = false;
    public bool isChasingPlayer = false;

    private NavMeshAgent agent;
    private PlayerController player;
    private Animator characterAnimator;
    private readonly List<Material> factionMaterials = new List<Material>();
    private readonly List<Color> originalMaterialColors = new List<Color>();

    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private float stunTimer = 0f;
    private float nextAttackTime = 0f;
    private bool animationStateInitialized;
    private bool isAnimatingAttack;
    private float attackAnimationEndTime;
    private Animator deathAnimator;
    private float deathAnimationEndTime;
    private bool despawnAfterDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 1.2f;
        }

        int variant = nextVisualVariant % 6 + 1;
        nextVisualVariant++;
        string modelResourcePath = $"Characters/Enemy{variant}";
        GameObject thugPrefab = Resources.Load<GameObject>(modelResourcePath);
        if (thugPrefab == null)
        {
            modelResourcePath = "Characters/SyndicateThug";
            thugPrefab = Resources.Load<GameObject>(modelResourcePath);
        }

        if (thugPrefab != null)
        {
            Renderer defR = GetComponent<Renderer>();
            if (defR != null) defR.enabled = false;

            GameObject model = Instantiate(thugPrefab, transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            CharacterModelUtility.PrepareVisual(model, modelResourcePath, 1.8f, transform.position.y);
            TintFactionModel(model, variant);
        }

        characterAnimator = CharacterModelUtility.FindAnimator(gameObject);
        CharacterModelUtility.PlayEnemyIdle(characterAnimator);
        animationStateInitialized = characterAnimator != null;
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        GameManager.Instance?.RegisterActiveEnemy(this);
    }

    public void SetPatrolRoute(Vector3[] points)
    {
        patrolPoints = ExpandPatrolRoute(points);
        currentPatrolIndex = 0;
        if (agent != null && agent.isOnNavMesh && patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0]);
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (!isAlive)
        {
            if (despawnAfterDeath && Time.time >= deathAnimationEndTime)
            {
                Destroy(gameObject);
                return;
            }

            if (deathAnimator != null && Time.time >= deathAnimationEndTime)
            {
                deathAnimator.speed = 0f;
                deathAnimator = null;
            }
            return;
        }

        if (isAnimatingAttack)
        {
            if (Time.time < attackAnimationEndTime)
            {
                FacePlayer();
                return;
            }

            isAnimatingAttack = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = false;
        }

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f) EndStun();
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
            return;
        }

        if (CanSeePlayer())
        {
            isChasingPlayer = true;
        }

        if (isChasingPlayer)
        {
            ChaseAndAttack();
        }
        else
        {
            Patrol();
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null || player.currentHealth <= 0f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 playerChest = player.transform.position + Vector3.up * 1.0f;
        Vector3 toPlayer = playerChest - eyePos;
        float dist = toPlayer.magnitude;

        // Shadow stealth vision
        float currentSight = player.isInLight ? sightRangeInLight : sightRangeInShadow;
        if (dist > currentSight) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        if (Physics.Raycast(eyePos, toPlayer.normalized, out RaycastHit hit, dist, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<PlayerController>() == player)
            {
                return true;
            }
        }
        return false;
    }

    private void ChaseAndAttack()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(player.transform.position);
            UpdateLocomotionAnimation(agent.velocity.sqrMagnitude > 0.04f);
        }
        else
        {
            Vector3 dir = (player.transform.position - transform.position);
            dir.y = 0f;
            if (dir.magnitude > attackRange)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), 8f * Time.deltaTime);
                transform.position += dir.normalized * (chaseSpeed * Time.deltaTime);
                UpdateLocomotionAnimation(true);
            }
            else UpdateLocomotionAnimation(false);
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            BeginAttackAnimation();
            ProceduralAudio.Instance?.PlayPunch();
            ProceduralAudio.Instance?.PlayVoiceBlip(0.85f);
            player.TakeDamage(baseAttackDamage * damageMultiplier);
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Vector3 target = patrolPoints[currentPatrolIndex];

        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = patrolSpeed;
            if (!agent.pathPending && agent.remainingDistance <= 0.6f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex]);
            }
            UpdateLocomotionAnimation(agent.velocity.sqrMagnitude > 0.04f);
        }
        else
        {
            Vector3 diff = target - transform.position;
            diff.y = 0f;
            if (diff.magnitude <= 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                UpdateLocomotionAnimation(false);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(diff.normalized), 5f * Time.deltaTime);
                transform.position += diff.normalized * (patrolSpeed * Time.deltaTime);
                UpdateLocomotionAnimation(true);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!isAlive) return;
        currentHealth -= amount;
        isChasingPlayer = true;

        SetFactionColor(new Color(0.82f, 0.78f, 1f));
        Invoke(nameof(RestoreColor), 0.15f);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void RestoreColor()
    {
        if (isAlive) RestoreFactionColors();
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        CharacterModelUtility.PlayEnemyIdle(characterAnimator);
        isWalkingAnimation = false;
        SetFactionColor(new Color(1f, 0.9f, 0.2f));
    }

    private void EndStun()
    {
        isStunned = false;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        RestoreFactionColors();
        animationStateInitialized = false;
    }

    public void InstantKill() => Die(true);

    private void Die(bool removeAfterDeath = false)
    {
        if (!isAlive) return;
        isAlive = false;
        despawnAfterDeath = removeAfterDeath;

        if (agent != null)
        {
            if (agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            agent.enabled = false;
        }

        bool hasDeathAnimation = StartDeathAnimation();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        GameManager.Instance?.OnEnemyDefeated(this);

        if (!hasDeathAnimation)
        {
            if (characterAnimator != null)
            {
                CharacterModelUtility.PlayEnemyIdle(characterAnimator);
                characterAnimator.speed = 0f;
            }

            deathAnimationEndTime = Time.time;
        }
    }

    private bool StartDeathAnimation()
    {
        float duration = CharacterModelUtility.PlayEnemyDeath(characterAnimator);
        if (duration <= 0f) return false;

        deathAnimator = characterAnimator;
        deathAnimationEndTime = Time.time + duration;
        return true;
    }

    private void BeginAttackAnimation()
    {
        FacePlayer();
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        float duration = CharacterModelUtility.PlayEnemyAttack(characterAnimator);
        if (duration <= 0f) duration = 0.55f;
        attackAnimationEndTime = Time.time + duration;
        isAnimatingAttack = true;
    }

    private void FacePlayer()
    {
        if (player == null) return;
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer.normalized), 8f * Time.deltaTime);
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

    private bool isWalkingAnimation;

    private void TintFactionModel(GameObject model, int variant)
    {
        Color[] palette =
        {
            new Color(0.57f, 0.47f, 0.92f),
            new Color(0.42f, 0.64f, 0.96f),
            new Color(0.70f, 0.48f, 0.91f),
            new Color(0.40f, 0.72f, 0.91f),
            new Color(0.55f, 0.54f, 0.98f),
            new Color(0.48f, 0.65f, 0.88f)
        };
        Color tint = palette[(variant - 1) % palette.Length];
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.materials)
            {
                if (material == null) continue;
                string colorProperty = material.HasProperty("_BaseColor") ? "_BaseColor" : (material.HasProperty("_Color") ? "_Color" : null);
                if (colorProperty == null) continue;
                Color baseColor = material.GetColor(colorProperty);
                Color factionColor = new Color(baseColor.r * tint.r, baseColor.g * tint.g, baseColor.b * tint.b, baseColor.a);
                material.SetColor(colorProperty, factionColor);
                factionMaterials.Add(material);
                originalMaterialColors.Add(factionColor);
            }
        }
    }

    private void SetFactionColor(Color color)
    {
        for (int i = 0; i < factionMaterials.Count; i++)
        {
            if (factionMaterials[i] != null) SetMaterialColor(factionMaterials[i], color);
        }
    }

    private void RestoreFactionColors()
    {
        for (int i = 0; i < factionMaterials.Count; i++)
        {
            if (factionMaterials[i] == null) continue;
            SetMaterialColor(factionMaterials[i], isStunned ? new Color(1f, 0.9f, 0.2f) : originalMaterialColors[i]);
        }
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static Vector3[] ExpandPatrolRoute(Vector3[] points)
    {
        if (points == null || points.Length != 2) return points;
        Vector3 direction = points[1] - points[0];
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return points;

        Vector3 side = Vector3.Cross(Vector3.up, direction.normalized) * Mathf.Min(1.5f, direction.magnitude * 0.18f);
        Vector3 midpoint = (points[0] + points[1]) * 0.5f;
        return new[] { points[0], midpoint + side, points[1], midpoint - side };
    }

    private void OnDestroy()
    {
        CharacterModelUtility.ReleaseAnimator(gameObject);
        for (int i = 0; i < factionMaterials.Count; i++)
        {
            if (factionMaterials[i] != null) Destroy(factionMaterials[i]);
        }
    }
}
