#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
GAME_APP="$PROJECT_PATH/Builds/Nightbound.app"

if [[ ! -d "$GAME_APP" ]]; then
  echo "[ERROR] Game not found: $GAME_APP" >&2
  echo "Run bash ./build_game_mac.sh first." >&2
  exit 1
fi

open "$GAME_APP"
