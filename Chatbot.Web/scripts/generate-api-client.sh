#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WEB_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
OUTPUT_DIR="${CHATBOT_OPENAPI_OUTPUT_DIR:-${WEB_ROOT}/src/generated/api}"
SOURCE="${1:-${CHATBOT_OPENAPI_URL:-http://127.0.0.1:5078/swagger/v1/swagger.json}}"
TMP_SPEC=""

cleanup() {
  if [[ -n "${TMP_SPEC}" && -f "${TMP_SPEC}" ]]; then
    rm -f "${TMP_SPEC}"
  fi
}

trap cleanup EXIT

if command -v openapi-generator >/dev/null 2>&1; then
  GENERATOR_CMD=("$(command -v openapi-generator)" "generate")
elif [[ -x "${WEB_ROOT}/node_modules/.bin/openapi-generator-cli" ]]; then
  GENERATOR_CMD=("${WEB_ROOT}/node_modules/.bin/openapi-generator-cli" "generate")
elif command -v openapi-generator-cli >/dev/null 2>&1; then
  GENERATOR_CMD=("$(command -v openapi-generator-cli)" "generate")
else
  echo "openapi-generator tool not found. Install openapi-generator or run npm install in Chatbot.Web first." >&2
  exit 1
fi

if [[ -f "${SOURCE}" ]]; then
  SPEC_PATH="${SOURCE}"
  echo "Using local OpenAPI spec: ${SPEC_PATH}"
else
  TMP_SPEC="$(mktemp "${TMPDIR:-/tmp}/chatbot-openapi.XXXXXX.json")"
  SPEC_PATH="${TMP_SPEC}"

  echo "Fetching OpenAPI spec: ${SOURCE}"
  if [[ "${SOURCE}" =~ ^https?://(127\.0\.0\.1|localhost)(:[0-9]+)?/ ]]; then
    env -u all_proxy -u ALL_PROXY -u http_proxy -u https_proxy -u HTTP_PROXY -u HTTPS_PROXY -u no_proxy -u NO_PROXY \
      curl -fsSL "${SOURCE}" -o "${SPEC_PATH}"
  else
    curl -fsSL "${SOURCE}" -o "${SPEC_PATH}"
  fi
fi

echo "Generating API client into ${OUTPUT_DIR}"
"${GENERATOR_CMD[@]}" \
  -i "${SPEC_PATH}" \
  -g typescript-fetch \
  -o "${OUTPUT_DIR}" \
  --additional-properties=typescriptThreePlus=true,useSingleRequestParameter=true

echo "OpenAPI client generation completed."
