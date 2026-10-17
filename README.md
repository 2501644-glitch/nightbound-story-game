# Nightbound

Nightbound is a Unity first-person action game about Rayan Malik, a survivor seeking revenge on the syndicate that murdered his family. After finding Dr. Vance's Chronos Gateway, Rayan travels to the 16th century, takes an ancient vampire serum, and returns to hunt the syndicate.

## Requirements

- Unity **6000.6.4f1**
- Windows x64 for the included standalone build target

## Story

The opening cutscene follows Rayan's family tragedy, the discovery of the Chronos Gateway, and his transformation in an ancient crypt. The hunt then spans four levels:

1. **The Alley** — eliminate two syndicate thugs.
2. **The Graveyard** — eliminate three scouts.
3. **The Warehouse** — eliminate four enforcers.
4. **The Penthouse** — defeat syndicate boss Kaleem Rana.

## Run

Open this folder in Unity Hub using version 6000.6.4f1, open `Assets/Scenes/SampleScene.unity`, and press **Play**. After building, run `Builds/Nightbound.exe` or use `run_game.bat` on Windows.

## Build

In the Unity Editor, select **Nightbound > Build Windows Standalone**. The Windows x64 player is written to `Builds/Nightbound.exe`. The included `build_game.bat` is an alternative for Windows installations using the Unity Editor path set in that script.

## Controls

- **WASD** — move; **mouse** — look
- **Left mouse button** — melee attack
- **Q** — Shadow Step
- **Left Shift** — slow time (vampire form; consumes blood)
- **E** — bite a vulnerable target (vampire form)
- **V** — inject serum after it expires and the cooldown ends
- **Space, E, Enter, or left click** — advance dialogue
- **Escape** — skip the opening cutscene or a story interlude; on the game-over/victory screen, quit
- **R** — restart from the current level after defeat, or from level 1 after the final boss
