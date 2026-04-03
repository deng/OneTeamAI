#!/usr/bin/env bash

set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: bash scripts/restore-sqlite.sh <backup-archive> [target-db-path]" >&2
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARCHIVE_PATH="$1"
TARGET_DB_PATH="${2:-"$ROOT_DIR/Chatbot.Api/chatbot.dev.db"}"

if [[ ! -f "$ARCHIVE_PATH" ]]; then
  echo "Backup archive not found: $ARCHIVE_PATH" >&2
  exit 1
fi

TARGET_DIR="$(cd "$(dirname "$TARGET_DB_PATH")" && pwd)"
TARGET_NAME="$(basename "$TARGET_DB_PATH")"

mkdir -p "$TARGET_DIR"

TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TEMP_DIR"' EXIT

tar -xzf "$ARCHIVE_PATH" -C "$TEMP_DIR"

if [[ ! -f "$TEMP_DIR/$TARGET_NAME" ]]; then
  EXTRACTED_DB="$(find "$TEMP_DIR" -maxdepth 1 -name "*.db" | head -n 1)"
  if [[ -z "$EXTRACTED_DB" ]]; then
    echo "No database file found in archive." >&2
    exit 1
  fi

  TARGET_NAME="$(basename "$EXTRACTED_DB")"
  TARGET_DB_PATH="$TARGET_DIR/$TARGET_NAME"
fi

cp "$TEMP_DIR/$TARGET_NAME" "$TARGET_DB_PATH"

if [[ -f "$TEMP_DIR/${TARGET_NAME}-shm" ]]; then
  cp "$TEMP_DIR/${TARGET_NAME}-shm" "${TARGET_DB_PATH}-shm"
fi

if [[ -f "$TEMP_DIR/${TARGET_NAME}-wal" ]]; then
  cp "$TEMP_DIR/${TARGET_NAME}-wal" "${TARGET_DB_PATH}-wal"
fi

echo "Database restored to: $TARGET_DB_PATH"
