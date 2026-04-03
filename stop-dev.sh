#!/usr/bin/env bash

set -euo pipefail

stop_port() {
  local port="$1"
  local pids

  pids="$(lsof -ti tcp:"$port" 2>/dev/null || true)"
  if [[ -z "$pids" ]]; then
    echo "No process found on port $port."
    return
  fi

  echo "Stopping processes on port $port: $pids"
  kill $pids 2>/dev/null || true
}

stop_port 5173
stop_port 5078
stop_port 7278
