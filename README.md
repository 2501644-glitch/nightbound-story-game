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

## Orientation Theme Fit

Nightbound connects the themes of time and shadow to its story and mechanics:

- **Time:** Rayan uses the Chronos Gateway to travel to 1582, and can slow time during combat while spending blood.
- **Shadow:** Shadow Step is a movement ability, and the vampire's light-exposure rules make seeking cover part of survival.
- **Setting:** The opening moves from a modern setting to a 16th-century crypt before the four-level hunt.

Confirm any trailer or gameplay-video duration requirements against the current orientation brief; those presentation materials are not included in this repository.

## Asset Credits

The Graveyard Kit, Modular Dungeon Kit, and Retro Urban Kit each include a Kenney-provided CC0 1.0 license notice in the project. See [ASSET_CREDITS.md](ASSET_CREDITS.md) for the file paths and remaining source checks.

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
- **Escape** — skip a cutscene or story interlude; during gameplay, pause or resume; from the victory screen, return to the main menu; from game over, quit
- **R** — restart from the current level after defeat (level 1 if defeated in level 4); from victory, replay from the opening
