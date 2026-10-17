using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Cutscene, Level1, Level2, Level3, Level4, GameOver, Victory }

    public GameState currentState = GameState.Cutscene;
    public int currentLevelIndex = 1;

    private PlayerController player;
    private readonly List<EnemyAI> activeEnemies = new List<EnemyAI>();
    private bool bossEpilogueTriggered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitializeGame()
    {
        if (FindAnyObjectByType<GameManager>() == null)
        {
            GameObject root = new GameObject("NIGHTBOUND_GameManager");
            root.AddComponent<GameManager>();
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

        // Ensure all required core subsystems exist
        if (FindAnyObjectByType<ProceduralAudio>() == null)
            gameObject.AddComponent<ProceduralAudio>();

        if (FindAnyObjectByType<DayNightCycle>() == null)
            gameObject.AddComponent<DayNightCycle>();

        if (FindAnyObjectByType<LevelBuilder>() == null)
            gameObject.AddComponent<LevelBuilder>();

        if (FindAnyObjectByType<UIManager>() == null)
            gameObject.AddComponent<UIManager>();

        if (FindAnyObjectByType<DialogueManager>() == null)
            gameObject.AddComponent<DialogueManager>();

        if (FindAnyObjectByType<WorldSpace3DUI>() == null)
            gameObject.AddComponent<WorldSpace3DUI>();

        if (FindAnyObjectByType<EnvironmentStyler>() == null)
            gameObject.AddComponent<EnvironmentStyler>();

        if (FindAnyObjectByType<CutsceneDirector>() == null)
            gameObject.AddComponent<CutsceneDirector>();

        if (FindAnyObjectByType<StoryNarrator>() == null)
            gameObject.AddComponent<StoryNarrator>();
    }

    private void Start()
    {
        EnsurePlayerExists();
        StartPrologueCutscene();
    }

    private void Update()
    {
        // Monitor Level 4 Boss Defeat -> Seamless Story Epilogue
        if (currentState == GameState.Level4 && !bossEpilogueTriggered)
        {
            if (BossController.Instance != null && !BossController.Instance.isAlive)
            {
                if (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive)
                {
                    bossEpilogueTriggered = true;
                    GameObject victoryScreen = GameObject.Find("VictoryScreen");
                    if (victoryScreen != null) victoryScreen.SetActive(false);

                    if (StoryNarrator.Instance != null)
                    {
                        StoryNarrator.Instance.PlayEpilogue(() =>
                        {
                            currentState = GameState.Victory;
                            UIManager.Instance?.ShowVictory();
                        });
                    }
                    else
                    {
                        currentState = GameState.Victory;
                        UIManager.Instance?.ShowVictory();
                    }
                }
            }
        }

        // Restart & Quit Controls
        if (currentState == GameState.GameOver || currentState == GameState.Victory)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    if (currentState == GameState.Victory)
                    {
                        StartLevel(1);
                    }
                    else
                    {
                        // Level 4 (Boss) death restarts from Level 1 per design rules
                        if (currentLevelIndex == 4)
                        {
                            StartLevel(1);
                        }
                        else
                        {
                            StartLevel(currentLevelIndex);
                        }
                    }
                }
                else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    Application.Quit();
                }
            }
        }
    }

    public void StartPrologueCutscene()
    {
        currentState = GameState.Cutscene;
        UIManager.Instance?.HideHUD();
        if (player != null) player.gameObject.SetActive(false);
        CutsceneDirector.Instance?.PlayIntroCutscene();
    }

    public void OnPlayerDefeated()
    {
        currentState = GameState.GameOver;
    }

    public void StartLevel(int level)
    {
        currentLevelIndex = level;
        bossEpilogueTriggered = false;
        EnsurePlayerExists();
        player.gameObject.SetActive(true);

        activeEnemies.Clear();
        player.currentHealth = player.maxHealth;
        player.currentBlood = 70f;
        player.ResetToVampire();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1.0f;

        UIManager.Instance?.ShowHUD();

        if (level == 1)
        {
            currentState = GameState.Level1;
            UIManager.Instance?.SetAlertText("LEVEL 1: THE ALLEY — ELIMINATE 2 SYNDICATE THUGS");
            LevelBuilder.Instance?.BuildLevel1(player);
            PlayLevel1Dialogue();
        }
        else if (level == 2)
        {
            currentState = GameState.Level2;
            UIManager.Instance?.SetAlertText("LEVEL 2: THE GRAVEYARD — ELIMINATE 3 SYNDICATE SCOUTS");
            LevelBuilder.Instance?.BuildLevel2_Graveyard(player);
            PlayLevel2Dialogue();
        }
        else if (level == 3)
        {
            currentState = GameState.Level3;
            UIManager.Instance?.SetAlertText("LEVEL 3: THE WAREHOUSE — ELIMINATE 4 ENFORCERS (1.3x ATK)");
            LevelBuilder.Instance?.BuildLevel3_Warehouse(player);
            PlayLevel3Dialogue();
        }
        else if (level == 4)
        {
            currentState = GameState.Level4;
            UIManager.Instance?.SetAlertText("FINAL LEVEL: DEFEAT THE SYNDICATE BOSS!");
            LevelBuilder.Instance?.BuildLevel4_Boss(player);
            PlayLevel4Dialogue();
        }
    }

    private void PlayLevel1Dialogue()
    {
        var lines = new List<DialogueLine>
        {
            new DialogueLine("SYNDICATE THUG", "Hey, stay sharp! Dispatch said someone broke into Dr. Vance's lab.", new Color(0.85f, 0.45f, 0.2f), "💀", 0.9f, 3.5f),
            new DialogueLine("ROOKIE THUG", "Relax. Who would be suicidal enough to walk into Rana's alley?", new Color(0.8f, 0.5f, 0.3f), "🔫", 1.15f, 3.5f),
            new DialogueLine("VAMPIRE RAYAN", "(The ones who murdered my family... Your blood belongs to the dark.)", new Color(0.9f, 0.15f, 0.15f), "🧛", 0.65f, 3.8f)
        };
        DialogueManager.Instance?.StartConversation(lines);
    }

    private void PlayLevel2Dialogue()
    {
        var lines = new List<DialogueLine>
        {
            new DialogueLine("GRAVEYARD SCOUT", "Alley patrol! Respond! Dammit, the radios went completely dead!", new Color(0.85f, 0.5f, 0.2f), "💀", 0.9f, 3.5f),
            new DialogueLine("SCOUT SNIPER", "Look around, fool! It's a cemetery at midnight! Keep the lanterns burning high!", new Color(0.8f, 0.55f, 0.25f), "🔫", 1.1f, 3.5f),
            new DialogueLine("VAMPIRE RAYAN", "(Lanterns won't save you. The shadows will drag you into the earth.)", new Color(0.9f, 0.15f, 0.15f), "🧛", 0.65f, 3.8f)
        };
        DialogueManager.Instance?.StartConversation(lines);
    }

    private void PlayLevel3Dialogue()
    {
        var lines = new List<DialogueLine>
        {
            new DialogueLine("WAREHOUSE CAPTAIN", "All units! The scouts are slaughtered! The intruder moves faster than human eyes can track!", new Color(0.9f, 0.35f, 0.2f), "💀", 0.85f, 3.5f),
            new DialogueLine("ENFORCER", "Captain! Boss says he burns in the light! Power on every warehouse floodlamp!", new Color(0.85f, 0.5f, 0.2f), "🔫", 1.05f, 3.5f),
            new DialogueLine("VAMPIRE RAYAN", "(Burn every bulb you have. Nothing can protect your boss from the night.)", new Color(0.9f, 0.15f, 0.15f), "🧛", 0.65f, 3.8f)
        };
        DialogueManager.Instance?.StartConversation(lines);
    }

    private void PlayLevel4Dialogue()
    {
        var lines = new List<DialogueLine>
        {
            new DialogueLine("KALEEM RANA", "You... You're the Malik boy from the alley! What kind of demon are you?!", new Color(0.95f, 0.75f, 0.2f), "👑", 0.7f, 4.0f),
            new DialogueLine("VAMPIRE RAYAN", "Ten years of torment, Rana. I took ancient blood and crossed centuries to end you.", new Color(0.9f, 0.15f, 0.15f), "🧛", 0.65f, 4.0f),
            new DialogueLine("KALEEM RANA", "You think a pair of fangs scares me?! I run this city! Kill him and burn the ashes!", new Color(0.95f, 0.75f, 0.2f), "👑", 0.68f, 4.2f)
        };
        DialogueManager.Instance?.StartConversation(lines);
    }

    public void RegisterActiveEnemy(EnemyAI enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void OnEnemyDefeated(EnemyAI enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
        }

        // Check if all enemies in level are eliminated
        if (activeEnemies.Count == 0)
        {
            if (currentLevelIndex == 1)
            {
                UIManager.Instance?.SetAlertText("ALLEY CLEARED! THE HUNT CONTINUES TO ST. JUDE'S CEMETERY...");
                if (StoryNarrator.Instance != null)
                {
                    StoryNarrator.Instance.PlayLevelTransition(1, () => StartLevel(2));
                }
                else
                {
                    Invoke(nameof(GoToLevel2), 2.5f);
                }
            }
            else if (currentLevelIndex == 2)
            {
                UIManager.Instance?.SetAlertText("GRAVEYARD CLEARED! PURSUING THE SYNDICATE TO THE WAREHOUSE...");
                if (StoryNarrator.Instance != null)
                {
                    StoryNarrator.Instance.PlayLevelTransition(2, () => StartLevel(3));
                }
                else
                {
                    Invoke(nameof(GoToLevel3), 2.5f);
                }
            }
            else if (currentLevelIndex == 3)
            {
                player.ApplyPowerBoost();
                UIManager.Instance?.SetAlertText("WAREHOUSE CLEARED! VAMPIRE ASCENSION UNLOCKED (+HP & +DAMAGE)!");
                if (StoryNarrator.Instance != null)
                {
                    StoryNarrator.Instance.PlayLevelTransition(3, () => StartLevel(4));
                }
                else
                {
                    Invoke(nameof(GoToLevel4), 3.0f);
                }
            }
        }
    }

    private void GoToLevel2() => StartLevel(2);
    private void GoToLevel3() => StartLevel(3);
    private void GoToLevel4() => StartLevel(4);

    private void EnsurePlayerExists()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "VampirePlayer";
            player = playerObj.AddComponent<PlayerController>();
        }
    }
}
