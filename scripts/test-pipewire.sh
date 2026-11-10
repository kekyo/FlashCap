#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

# The fixture never uses the user's PipeWire server or desktop portal.
make -C tests/FlashCap.PipeWire.Tests/native
test_runtime=$(mktemp -d /tmp/flashcap-pipewire-XXXXXX)
trap 'rm -rf "$test_runtime"' EXIT
export XDG_RUNTIME_DIR="$test_runtime"
export FLASHCAP_PIPEWIRE_NATIVE_DIRECTORY="$PWD/tests/FlashCap.PipeWire.Tests/native"
unset PIPEWIRE_REMOTE PIPEWIRE_RUNTIME_DIR PIPEWIRE_CONFIG_DIR PIPEWIRE_CONFIG_PREFIX PIPEWIRE_CONFIG_NAME
dbus-run-session --config-file="$FLASHCAP_PIPEWIRE_NATIVE_DIRECTORY/dbus.conf" -- dotnet test \
    --project tests/FlashCap.PipeWire.Tests/FlashCap.PipeWire.Tests.csproj \
    --configuration Release --max-parallel-test-modules 1 "$@"
