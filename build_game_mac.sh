#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
UNITY_PATH="${UNITY_EDITOR_PATH:-/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/MacOS/Unity}"
BUILD_LOG="$PROJECT_PATH/build-macos.log"
GAME_APP="$PROJECT_PATH/Builds/Nightbound.app"

if [[ ! -x "$UNITY_PATH" ]]; then
  echo "[ERROR] Unity Editor not found or not executable: $UNITY_PATH" >&2
  echo "Set UNITY_EDITOR_PATH to the Unity executable and try again." >&2
  exit 1
fi

"$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" \
  -executeMethod BuildScript.BuildMacOS -logFile "$BUILD_LOG"

if [[ ! -d "$GAME_APP" ]]; then
  echo "[ERROR] Unity returned success, but Builds/Nightbound.app was not created." >&2
  echo "See $BUILD_LOG" >&2
  exit 1
fi

echo "[SUCCESS] Built $GAME_APP"
