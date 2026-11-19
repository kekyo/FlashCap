#!/bin/bash
# Launched inside the active GNOME X11 session by its autostart desktop entry.
set -euo pipefail
input=/mnt/flashcap-input
results=/mnt/flashcap-results/results
mkdir -p "$results"
scenario=$(cat "$input/scenario")
exec > "$results/session.log" 2>&1
finish() {
    status=$?
    trap - EXIT
    set +e
    if ((status != 0)); then
        timeout 5 import -window root "$results/failure.png"
        timeout 5 pw-dump > "$results/pipewire.log"
        systemctl status flashcap-camera.service flashcap-install.service --no-pager
    fi
    journalctl --user -b --no-pager > "$results/session-journal.log"
    if ((status == 0)); then echo "PASS $scenario"; else echo "FAIL $scenario ($status)"; fi > "$results/result"
    sync
    systemctl --no-ask-password poweroff
}
trap finish EXIT
export LC_ALL=C.UTF-8
echo "Desktop: $XDG_CURRENT_DESKTOP, session: $XDG_SESSION_TYPE"
test "$XDG_SESSION_TYPE" = x11
[[ ":${XDG_CURRENT_DESKTOP}:" = *:GNOME:* ]]
gsettings set org.gnome.desktop.session idle-delay 0
gsettings set org.gnome.desktop.screensaver lock-enabled false
cp /usr/local/share/flashcap-vm/packages.tsv "$results/"
modinfo -F version akvcam
flatpak list --columns=ref,active > "$results/flatpaks.tsv"
flatpak info --show-permissions io.github.kekyo.FlashCap.PipeWireTest
flatpak run --command=sh io.github.kekyo.FlashCap.PipeWireTest -ec '
    test -f /.flatpak-info
    for device in /dev/video*; do test ! -e "$device"; done
    test ! -S "$XDG_RUNTIME_DIR/pipewire-0"
'
echo "Verified that video devices and the PipeWire socket are hidden in the sandbox."
v4l2-ctl -d /dev/video0 --info
node "$input/test-desktop.mjs" "$scenario" "$results"
