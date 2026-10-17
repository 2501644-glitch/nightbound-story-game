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
    private Renderer bodyRenderer;
    private Color originalColor = Color.gray;

    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private float stunTimer = 0f;
    private float nextAttackTime = 0f;
    private Animator deathAnimator;
    private AnimatorOverrideController deathOverrideController;
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

        bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer != null && bodyRenderer.material != null)
        {
            originalColor = bodyRenderer.material.color;
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
            Renderer mr = model.GetComponentInChildren<Renderer>();
            if (mr != null)
            {
                bodyRenderer = mr;
                if (mr.material != null) originalColor = mr.material.color;
            }
        }
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        GameManager.Instance?.RegisterActiveEnemy(this);
    }

    public void SetPatrolRoute(Vector3[] points)
    {
        patrolPoints = points;
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
            if (deathAnimator != null && Time.time >= deathAnimationEndTime)
            {
                deathAnimator.speed = 0f;
                deathAnimator = null;
                if (despawnAfterDeath) Destroy(gameObject);
            }
            else if (despawnAfterDeath && deathAnimator == null)
            {
                Destroy(gameObject);
            }
            return;
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
        }
        else
        {
            Vector3 dir = (player.transform.position - transform.position);
            dir.y = 0f;
            if (dir.magnitude > attackRange)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), 8f * Time.deltaTime);
                transform.position += dir.normalized * (chaseSpeed * Time.deltaTime);
            }
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
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
        }
        else
        {
            Vector3 diff = target - transform.position;
            diff.y = 0f;
            if (diff.magnitude <= 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(diff.normalized), 5f * Time.deltaTime);
                transform.position += diff.normalized * (patrolSpeed * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!isAlive) return;
        currentHealth -= amount;
        isChasingPlayer = true;

        if (bodyRenderer != null && bodyRenderer.material != null)
        {
            bodyRenderer.material.color = Color.white;
            Invoke(nameof(RestoreColor), 0.15f);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void RestoreColor()
    {
        if (bodyRenderer != null && bodyRenderer.material != null && isAlive)
        {
            bodyRenderer.material.color = isStunned ? new Color(1f, 0.9f, 0.2f) : originalColor;
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (bodyRenderer != null && bodyRenderer.material != null)
            bodyRenderer.material.color = new Color(1f, 0.9f, 0.2f);
    }

    private void EndStun()
    {
        isStunned = false;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        if (bodyRenderer != null && bodyRenderer.material != null)
            bodyRenderer.material.color = originalColor;
    }

    public void InstantKill()
    {
        despawnAfterDeath = true;
        Die();
        if (deathAnimator == null) Destroy(gameObject);
    }

    private void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;

        if (bodyRenderer != null && bodyRenderer.material != null)
            bodyRenderer.material.color = new Color(0.2f, 0.02f, 0.02f);

        bool hasDeathAnimation = PlayDeathAnimation();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        GameManager.Instance?.OnEnemyDefeated(this);

        if (!hasDeathAnimation)
        {
            Animator animator = GetComponentInChildren<Animator>(true);
            if (animator != null) animator.speed = 0f;
        }
    }

    private bool PlayDeathAnimation()
    {
        Animator animator = GetComponentInChildren<Animator>(true);
        RuntimeAnimatorController sourceController = animator != null ? animator.runtimeAnimatorController : null;
        AnimationClip[] walkClips = sourceController != null ? sourceController.animationClips : null;
        AnimationClip[] deathClips = Resources.LoadAll<AnimationClip>("Animations/exo_red@Death");
        if (deathClips.Length == 0)
            deathClips = Resources.LoadAll<AnimationClip>("Animations/sword and shield death");
        if (animator == null || walkClips == null || walkClips.Length == 0 || deathClips.Length == 0)
            return false;

        AnimationClip deathClip = deathClips[0];
        deathOverrideController = new AnimatorOverrideController(sourceController);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(walkClips.Length);
        deathOverrideController.GetOverrides(overrides);
        if (overrides.Count == 0)
        {
            Destroy(deathOverrideController);
            deathOverrideController = null;
            return false;
        }

        int walkClipIndex = overrides.FindIndex(pair => pair.Key != null && pair.Key.name == "Walk");
        if (walkClipIndex < 0) walkClipIndex = 0;
        overrides[walkClipIndex] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[walkClipIndex].Key, deathClip);
        deathOverrideController.ApplyOverrides(overrides);
        if (agent != null)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            agent.enabled = false;
        }
        animator.runtimeAnimatorController = deathOverrideController;
        animator.applyRootMotion = true;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.speed = 1f;
        animator.enabled = true;
        animator.Play("Walk", 0, 0f);
        animator.Update(0f);

        deathAnimator = animator;
        deathAnimationEndTime = Time.time + deathClip.length;
        return true;
    }

    private void OnDestroy()
    {
        if (deathOverrideController != null)
            Destroy(deathOverrideController);
    }
}
