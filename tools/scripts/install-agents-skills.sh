#!/usr/bin/env bash
# Copies .agents/skills from this repository into ~/.agents/skills/
# Run from repository root: chmod +x tools/scripts/install-agents-skills.sh && ./tools/scripts/install-agents-skills.sh

set -euo pipefail
REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SOURCE="${REPO_ROOT}/.agents/skills"
DEST="${HOME}/.agents/skills"

if [[ ! -d "${SOURCE}" ]]; then
  echo "Missing folder: ${SOURCE}" >&2
  exit 1
fi

mkdir -p "${DEST}"
cp -R "${SOURCE}/." "${DEST}/"
echo "Installed repo skills to: ${DEST}"
echo "Source: ${SOURCE}"
