#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
API_DIR="$ROOT_DIR/Chatbot.Api"
WEB_DIR="$ROOT_DIR/Chatbot.Web"
WEB_ENV_FILE="$WEB_DIR/.env"
WEB_ENV_EXAMPLE="$WEB_DIR/.env.example"

cleanup() {
  if [[ -n "${API_PID:-}" ]] && kill -0 "$API_PID" >/dev/null 2>&1; then
    kill "$API_PID" >/dev/null 2>&1 || true
  fi
}

trap cleanup EXIT INT TERM

if [[ ! -f "$WEB_ENV_FILE" ]]; then
  cp "$WEB_ENV_EXAMPLE" "$WEB_ENV_FILE"
  echo "Created $WEB_ENV_FILE from template."
fi

echo "Starting Chatbot.Api on http://127.0.0.1:5078 ..."
(
  cd "$API_DIR"
  ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5078 dotnet run --no-launch-profile
) &
API_PID=$!

echo "Waiting for Chatbot.Api to be ready ..."
for _ in {1..30}; do
  if curl -fsS http://127.0.0.1:5078/health >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

if ! curl -fsS http://127.0.0.1:5078/health >/dev/null 2>&1; then
  echo "Chatbot.Api did not become ready in time."
  exit 1
fi

echo "Starting Chatbot.Web on http://127.0.0.1:5173 ..."
cd "$WEB_DIR"
npm run dev -- --host 127.0.0.1
