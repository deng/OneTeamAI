#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DB_PATH="${1:-"$ROOT_DIR/Chatbot.Api/chatbot.dev.db"}"
BACKUP_DIR="${2:-"$ROOT_DIR/backups"}"
TIMESTAMP="$(date +"%Y%m%d-%H%M%S")"

if [[ ! -f "$DB_PATH" ]]; then
  echo "Database not found: $DB_PATH" >&2
  exit 1
fi

mkdir -p "$BACKUP_DIR"

DB_NAME="$(basename "$DB_PATH")"
BASE_NAME="${DB_NAME%.db}"
ARCHIVE_PATH="$BACKUP_DIR/${BASE_NAME}-${TIMESTAMP}.tar.gz"

DB_DIR="$(cd "$(dirname "$DB_PATH")" && pwd)"

TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TEMP_DIR"' EXIT

cp "$DB_PATH" "$TEMP_DIR/$DB_NAME"

if [[ -f "${DB_PATH}-shm" ]]; then
  cp "${DB_PATH}-shm" "$TEMP_DIR/${DB_NAME}-shm"
fi

if [[ -f "${DB_PATH}-wal" ]]; then
  cp "${DB_PATH}-wal" "$TEMP_DIR/${DB_NAME}-wal"
fi

tar -czf "$ARCHIVE_PATH" -C "$TEMP_DIR" .

echo "Backup created: $ARCHIVE_PATH"
echo "Source database: $DB_DIR/$DB_NAME"
