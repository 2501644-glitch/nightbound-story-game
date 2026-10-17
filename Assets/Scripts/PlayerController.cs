using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Health & Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxBlood = 100f;
    public float currentBlood = 50f;
    public bool hasPowerBoost = false;

    [Header("Serum Transformation Rules")]
    public bool isVampire = true;
    public float serumDuration = 120f;  // 2 minutes active
    public float serumTimer = 120f;
    public float serumCooldownMax = 15f; // 15 seconds cooldown
    public float serumCooldownTimer = 0f;

    [Header("Mechanics")]
    public bool isInLight = false;
    public bool isTimeSlowActive = false;
    public float burnDamagePerSecond = 8f;
    public float shadowRegenPerSecond = 2f;
    public float timeSlowBloodCostPerSec = 14f;

    [Header("Speeds")]
    public float mouseSensitivity = 0.12f;
    public float humanSpeed = 4.2f;
    public float vampireNormalSpeed = 6.8f;
    public float vampireFastSpeed = 9.5f;
    public float gravity = -20f;

    [Header("Shadow Step")]
    public float shadowStepDistance = 7f;
    public float shadowStepCooldown = 1.5f;
    private float nextShadowStepTime = 0f;

    private CharacterController controller;
    private Camera playerCamera;
    private float pitch = 0f;
    private float yaw = 0f;
    private float verticalVelocity = 0f;
    private float lightCheckTimer = 0f;

    public EnemyAI targetedEnemy = null;
    public bool canBiteTarget = false;

    private GameObject humanVisual;
    private GameObject vampireVisual;

    private void Awake()
    {
        gameObject.layer = 2; // Ignore Raycast
        controller = GetComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.38f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        SetupCamera();
        SetupVisuals();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yaw = transform.eulerAngles.y;
        ResetToVampire();
    }

    private void SetupVisuals()
    {
        GameObject humanPrefab = Resources.Load<GameObject>("Characters/HumanHero");
        if (humanPrefab != null)
        {
            humanVisual = Instantiate(humanPrefab, transform);
            humanVisual.transform.localPosition = Vector3.zero;
            humanVisual.transform.localRotation = Quaternion.identity;
            if (!CharacterModelUtility.PrepareVisual(humanVisual, "Characters/HumanHero",
                    controller.height, transform.TransformPoint(controller.center).y))
            {
                Destroy(humanVisual);
                humanVisual = null;
            }
        }

        GameObject vampirePrefab = Resources.Load<GameObject>("Characters/RayanAfterSerum");
        string vampireResourcePath = "Characters/RayanAfterSerum";
        if (vampirePrefab == null)
        {
            vampirePrefab = Resources.Load<GameObject>("Characters/VampireHero");
            vampireResourcePath = "Characters/VampireHero";
        }
        if (vampirePrefab != null)
        {
            vampireVisual = Instantiate(vampirePrefab, transform);
            vampireVisual.transform.localPosition = Vector3.zero;
            vampireVisual.transform.localRotation = Quaternion.identity;
            if (!CharacterModelUtility.PrepareVisual(vampireVisual, vampireResourcePath,
                    controller.height, transform.TransformPoint(controller.center).y))
            {
                Destroy(vampireVisual);
                vampireVisual = null;
            }
        }

        if (humanVisual != null || vampireVisual != null)
        {
            Renderer capsuleRenderer = GetComponent<Renderer>();
            if (capsuleRenderer != null) capsuleRenderer.enabled = false;
        }

        UpdateVisualForm();
    }

    public void UpdateVisualForm()
    {
        if (humanVisual != null) humanVisual.SetActive(!isVampire);
        if (vampireVisual != null) vampireVisual.SetActive(isVampire);
    }

    public void ResetToVampire()
    {
        if (isTimeSlowActive) StopTimeSlow();
        isVampire = true;
        serumTimer = serumDuration;
        serumCooldownTimer = 0f;
        ProceduralAudio.Instance?.PlayHumanHeartbeat(false);
        UpdateVisualForm();
    }

    public void ApplyPowerBoost()
    {
        if (hasPowerBoost) return;

        hasPowerBoost = true;
        maxHealth += 40f;
        currentHealth = maxHealth;
        currentBlood = maxBlood;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Paused) return;
        if (currentHealth <= 0f) return;

        HandleMouseLook();
        HandleSerumLifecycle();
        HandleMovement();
        HandleAbilities();

        // Check light exposure every 0.1s
        lightCheckTimer += Time.unscaledDeltaTime;
        if (lightCheckTimer >= 0.1f)
        {
            lightCheckTimer = 0f;
            CheckLightExposure();
            UpdateBiteTarget();
        }

        // Sunlight / Artificial Light Burning rules
        bool burnsFromEnvironment = false;
        if (isVampire)
        {
            // At Night: only artificial lights burn. In Day: sunlight + artificial lights burn if exposed!
            bool isDay = (DayNightCycle.Instance != null && DayNightCycle.Instance.isDay);
            if (isInLight || (isDay && !IsInShadowCover()))
            {
                burnsFromEnvironment = true;
            }
        }

        if (burnsFromEnvironment)
        {
            currentHealth -= burnDamagePerSecond * Time.unscaledDeltaTime;
            ProceduralAudio.Instance?.PlayBurnSizzle(true);
            if (isTimeSlowActive) StopTimeSlow();
        }
        else
        {
            if (isVampire)
            {
                currentHealth += shadowRegenPerSecond * Time.unscaledDeltaTime;
            }
            ProceduralAudio.Instance?.PlayBurnSizzle(false);
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentBlood = Mathf.Clamp(currentBlood, 0f, maxBlood);

        if (currentHealth <= 0f)
        {
            Die("DEFEATED IN BATTLE");
        }
    }

    private void HandleSerumLifecycle()
    {
        if (isVampire)
        {
            serumTimer -= Time.deltaTime;
            if (serumTimer <= 0f)
            {
                // Revert to human form!
                isVampire = false;
                serumCooldownTimer = serumCooldownMax;
                if (isTimeSlowActive) StopTimeSlow();
                ProceduralAudio.Instance?.PlayHumanHeartbeat(true);
                UpdateVisualForm();
                UIManager.Instance?.SetAlertText("SERUM DEGRADED! REVERTED TO HUMAN (15s COOLDOWN) — EVADE IN SHADOWS!");
                WorldSpace3DUI.Instance?.SpawnPopup(transform.position + Vector3.up * 1.6f, "HUMAN FORM (15s CD)", new Color(1f, 0.4f, 0.1f), 24);
            }
        }
        else
        {
            // In human form: count down 15s cooldown
            if (serumCooldownTimer > 0f)
            {
                serumCooldownTimer -= Time.deltaTime;
            }
            else
            {
                // Ready to inject! Press V
                if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
                {
                    ProceduralAudio.Instance?.PlaySerumInject();
                    ResetToVampire();
                    UIManager.Instance?.SetAlertText("SERUM INJECTED! VAMPIRE POWERS RESTORED! (120s)");
                    WorldSpace3DUI.Instance?.SpawnPopup(transform.position + Vector3.up * 1.6f, "VAMPIRE RESTORED!", new Color(0.9f, 0.1f, 0.15f), 24);
                }
            }
        }
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
        }

        Vector3 moveDirection = (transform.right * input.x + transform.forward * input.y).normalized;

        float moveSpeed = humanSpeed;
        if (isVampire)
        {
            moveSpeed = (currentBlood >= 67f || hasPowerBoost) ? vampireFastSpeed : vampireNormalSpeed;
        }

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.unscaledDeltaTime;
        }

        Vector3 velocity = moveDirection * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.unscaledDeltaTime);
    }

    private void HandleAbilities()
    {
        // Shadow Step (Q key) - Vampire only
        if (isVampire && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Time.time >= nextShadowStepTime)
            {
                PerformShadowStep();
            }
        }

        // Time Slow (Hold Left Shift) - Vampire only
        if (isVampire && !isInLight && currentBlood > 0f)
        {
            bool holdShift = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            if (holdShift)
            {
                if (!isTimeSlowActive) StartTimeSlow();
                currentBlood -= timeSlowBloodCostPerSec * Time.unscaledDeltaTime;
                if (currentBlood <= 0f) StopTimeSlow();
            }
            else if (isTimeSlowActive)
            {
                StopTimeSlow();
            }
        }
        else if (isTimeSlowActive)
        {
            StopTimeSlow();
        }

        // Melee Attack (Left Click)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformMelee();
        }

        // Bite (E key) - Vampire only
        if (isVampire && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (canBiteTarget && targetedEnemy != null)
            {
                PerformBite(targetedEnemy);
            }
        }
    }

    private void PerformShadowStep()
    {
        nextShadowStepTime = Time.time + shadowStepCooldown;
        ProceduralAudio.Instance?.PlayShadowStep();

        Vector3 dashDir = transform.forward;
        if (Keyboard.current != null)
        {
            Vector3 inputDir = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) inputDir += transform.forward;
            if (Keyboard.current.sKey.isPressed) inputDir -= transform.forward;
            if (Keyboard.current.dKey.isPressed) inputDir += transform.right;
            if (Keyboard.current.aKey.isPressed) inputDir -= transform.right;
            if (inputDir.sqrMagnitude > 0.01f) dashDir = inputDir.normalized;
        }

        controller.Move(dashDir * shadowStepDistance);
    }

    private void PerformMelee()
    {
        ProceduralAudio.Instance?.PlayPunch();

        Vector3 rayOrigin = playerCamera != null ? playerCamera.transform.position : transform.position + Vector3.up * 1.6f;
        Vector3 rayDir = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        float damage = isVampire ? (hasPowerBoost ? 40f : 28f) : 8f;

        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, 2.8f))
        {
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null && enemy.isAlive)
            {
                enemy.TakeDamage(damage);
                if (isVampire) enemy.Stun(3.0f);
            }

            BossController boss = hit.collider.GetComponentInParent<BossController>();
            if (boss != null && boss.isAlive)
            {
                boss.TakeDamage(damage);
            }
        }
    }

    private void UpdateBiteTarget()
    {
        targetedEnemy = null;
        canBiteTarget = false;
        if (!isVampire) return;

        Vector3 eyePos = playerCamera != null ? playerCamera.transform.position : transform.position + Vector3.up * 1.6f;
        Vector3 eyeFwd = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        if (Physics.Raycast(eyePos, eyeFwd, out RaycastHit hit, 3.2f))
        {
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null && enemy.isAlive)
            {
                targetedEnemy = enemy;
                float dot = Vector3.Dot(enemy.transform.forward, transform.forward);
                bool isUnaware = dot > 0.3f && !enemy.isChasingPlayer;
                if (enemy.isStunned || isUnaware)
                {
                    canBiteTarget = true;
                }
            }
        }
    }

    private void PerformBite(EnemyAI enemy)
    {
        ProceduralAudio.Instance?.PlayBite();
        currentBlood = Mathf.Min(maxBlood, currentBlood + 35f);
        currentHealth = Mathf.Min(maxHealth, currentHealth + 30f);
        enemy.InstantKill();
        canBiteTarget = false;
        targetedEnemy = null;
    }

    private void StartTimeSlow()
    {
        isTimeSlowActive = true;
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        ProceduralAudio.Instance?.SetPitch(0.65f);
    }

    private void StopTimeSlow()
    {
        isTimeSlowActive = false;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        ProceduralAudio.Instance?.SetPitch(1.0f);
    }

    public void CheckLightExposure()
    {
        Vector3 chest = transform.position + Vector3.up * 1.0f;
        Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        bool exposed = false;

        foreach (Light l in lights)
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy || l.intensity <= 0.05f) continue;
            if (l.type == LightType.Directional) continue; // Handled separately by DayNightCycle

            Vector3 toLight = l.transform.position - chest;
            float dist = toLight.magnitude;
            if (dist > l.range) continue;

            if (l.type == LightType.Spot)
            {
                Vector3 fromLight = (chest - l.transform.position).normalized;
                float angle = Vector3.Angle(l.transform.forward, fromLight);
                if (angle > l.spotAngle * 0.5f) continue;
            }

            if (Physics.Raycast(chest, toLight.normalized, out RaycastHit hit, dist))
            {
                if (hit.distance >= dist - 0.25f)
                {
                    exposed = true;
                    break;
                }
            }
            else
            {
                exposed = true;
                break;
            }
        }

        isInLight = exposed;
    }

    private bool IsInShadowCover()
    {
        // Raycast upwards towards sky/sun to see if a ceiling or high wall blocks sunlight
        Vector3 chest = transform.position + Vector3.up * 1.0f;
        Vector3 sunDir = Vector3.up + Vector3.forward * 0.4f;
        return Physics.Raycast(chest, sunDir.normalized, out RaycastHit hit, 40f);
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (currentHealth <= 0f)
        {
            Die("SLAIN BY SYNDICATE");
        }
    }

    private void Die(string reason)
    {
        StopTimeSlow();
        ProceduralAudio.Instance?.PlayBurnSizzle(false);
        ProceduralAudio.Instance?.PlayHumanHeartbeat(false);
        GameManager.Instance?.OnPlayerDefeated();
        UIManager.Instance?.ShowGameOver(reason);
    }

    private void SetupCamera()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Camera mainCam = FindAnyObjectByType<Camera>();
            if (mainCam != null)
            {
                playerCamera = mainCam;
                playerCamera.transform.SetParent(transform);
            }
            else
            {
                GameObject camObj = new GameObject("PlayerCamera");
                camObj.transform.SetParent(transform);
                playerCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
        }
        playerCamera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        playerCamera.transform.localRotation = Quaternion.identity;
        playerCamera.clearFlags = CameraClearFlags.Skybox;
        playerCamera.fieldOfView = 58f;
        playerCamera.farClipPlane = 80f;
    }

    private void HandleMouseLook()
    {
        if (Mouse.current == null || playerCamera == null) return;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        yaw += mouseDelta.x;
        pitch -= mouseDelta.y;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
