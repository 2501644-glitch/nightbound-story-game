# Nightbound

Nightbound is a Unity first-person action game about Rayan Malik, a survivor seeking revenge on the syndicate that murdered his family. After finding Dr. Vance's Chronos Gateway, Rayan travels to the 16th century, takes an ancient vampire serum, and returns to hunt the syndicate.

## Requirements

- Unity **6000.6.4f1**
- Windows x64 or macOS for the standalone build targets

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

Open this folder in Unity Hub using version 6000.6.4f1, open `Assets/Scenes/SampleScene.unity`, and press **Play**. Build a player first to create the platform-specific output under `Builds/`.

## Build

### Windows

Install Unity 6000.6.4f1 with **Windows Build Support (IL2CPP)**, then run `build_game.bat` from Command Prompt or PowerShell. It creates `Builds/Nightbound.exe`; launch it with `run_game.bat` or directly with `Builds\Nightbound.exe` in Command Prompt (`.\Builds\Nightbound.exe` in PowerShell). `build_and_run.bat` builds and launches in one step. The scripts use the default Unity Hub install path; set `UNITY_EDITOR_PATH` if Unity is installed elsewhere.

You can also build from the Editor using **Nightbound > Build Windows Standalone**.

### macOS

Install Unity 6000.6.4f1 with **Mac Build Support**, then run `bash ./build_game_mac.sh` in Terminal. It creates `Builds/Nightbound.app`; launch it with `bash ./run_game_mac.sh` or build and launch with `bash ./build_and_run_mac.sh`. Set `UNITY_EDITOR_PATH` if Unity is installed outside the default Unity Hub path. The macOS menu item is **Nightbound > Build macOS Standalone**.

`Builds/` is ignored by Git, so each platform's player is built locally and is not part of the source repository. The macOS build must be made with the Mac build module installed.

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
