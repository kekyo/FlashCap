#!/bin/bash
# Runs as root inside the disposable Ubuntu guest.
set -euo pipefail
export DEBIAN_FRONTEND=noninteractive
cat > /etc/apt/sources.list.d/ubuntu.sources <<'APT'
Types: deb
URIs: https://snapshot.ubuntu.com/ubuntu/20261101T000000Z/
Suites: noble noble-updates noble-security
Components: main universe
Check-Valid-Until: no
Signed-By: /usr/share/keyrings/ubuntu-archive-keyring.gpg
APT
apt-get -o Acquire::Retries=3 update
apt-get -o Acquire::Retries=3 install -y --no-install-recommends \
    linux-image-generic linux-headers-generic gdm3 gnome-session gnome-shell gjs gnome-control-center ibus \
    dbus-x11 xserver-xorg at-spi2-core libgl1-mesa-dri \
    pipewire pipewire-bin wireplumber rtkit libspa-0.2-modules xdg-desktop-portal xdg-desktop-portal-gnome \
    flatpak v4l-utils xdotool x11-utils imagemagick ffmpeg tesseract-ocr tesseract-ocr-eng nodejs \
    curl gcc make

# Build for the kernel the desktop will boot, which can differ from the cloud image's running kernel.
camera_build=$(mktemp -d)
camera_commit=d1243f277d0a10efd9fec04574ad6fcb4265f7b0
curl --fail --location --retry 3 --output "$camera_build/source.tar.gz" \
    "https://codeload.github.com/webcamoid/akvcam/tar.gz/$camera_commit"
echo "d70abfd2704dfd6623aaa93e0120cab51c4aff020b26c125cd34dc04d04d259b  $camera_build/source.tar.gz" | sha256sum -c -
tar -xf "$camera_build/source.tar.gz" -C "$camera_build"
kernel=$(basename "$(readlink -f /boot/vmlinuz)")
kernel=${kernel#vmlinuz-}
source_dir="$camera_build/akvcam-$camera_commit/src"
make -C "$source_dir" -j4 CC=x86_64-linux-gnu-gcc-13 KERNEL_DIR="/lib/modules/$kernel/build"
install -Dm644 "$source_dir/akvcam.ko" "/lib/modules/$kernel/extra/akvcam.ko"
depmod "$kernel"
rm -r "$camera_build"
mkdir -p /etc/akvcam
cat > /etc/akvcam/config.ini <<'CAMERA'
[Cameras]
cameras/size = 2
cameras/1/type = output
cameras/1/mode = rw
cameras/1/description = FlashCap pattern input
cameras/1/formats = 1
cameras/1/videonr = 1
cameras/2/type = capture
cameras/2/mode = mmap, userptr
cameras/2/description = FlashCap test camera
cameras/2/formats = 2
cameras/2/videonr = 0
[Formats]
formats/size = 2
formats/1/format = RGB24
formats/1/width = 320
formats/1/height = 180
formats/1/fps = 10
formats/2/format = YUY2
formats/2/width = 320
formats/2/height = 180
formats/2/fps = 10
[Connections]
connections/size = 1
connections/1/connection = 1:2
CAMERA
cat > /etc/systemd/system/flashcap-camera.service <<'UNIT'
[Unit]
Description=Feed moving color bars to the test camera
Before=display-manager.service
[Service]
ExecStartPre=/usr/sbin/modprobe akvcam
ExecStart=/usr/bin/ffmpeg -nostdin -hide_banner -loglevel error -re -f lavfi -i smptebars=size=320x180:rate=10,scroll=horizontal=0.01 -f v4l2 -pix_fmt rgb24 /dev/video1
[Install]
WantedBy=graphical.target
UNIT

# Both the runtime and its software GL driver must be available offline for each test boot.
while read -r ref commit; do
    flatpak install --system --noninteractive --no-deps --no-related "/mnt/flashcap-seed/$commit.flatpak"
    test "$(flatpak info --system --show-commit "$ref")" = "$commit"
done < /mnt/flashcap-seed/runtimes.lock

cat > /etc/gdm3/custom.conf <<'GDM'
[daemon]
AutomaticLoginEnable=True
AutomaticLogin=flashcap
WaylandEnable=false
DefaultSession=gnome-xorg.desktop
GDM
mkdir -p /var/lib/AccountsService/users
cat > /var/lib/AccountsService/users/flashcap <<'ACCOUNT'
[User]
XSession=gnome-xorg
Session=gnome-xorg
SystemAccount=false
ACCOUNT
mkdir -p /mnt/flashcap-input /mnt/flashcap-results
cat >> /etc/fstab <<'MOUNTS'
LABEL=FLASHCAPINPUT /mnt/flashcap-input iso9660 ro,nofail 0 0
LABEL=FLASHCAPRESULT /mnt/flashcap-results ext4 rw,nofail 0 0
MOUNTS
# Install the application before GDM starts so GNOME indexes the exported desktop entry.
cat > /etc/systemd/system/flashcap-install.service <<'UNIT'
[Unit]
Description=Install the FlashCap Flatpak being tested
Before=display-manager.service
RequiresMountsFor=/mnt/flashcap-input /mnt/flashcap-results
[Service]
Type=oneshot
ExecStart=/usr/bin/flatpak install --system --noninteractive --no-deps --no-related /mnt/flashcap-input/flashcap.flatpak
[Install]
WantedBy=graphical.target
UNIT
mkdir -p /etc/xdg/autostart
cat > /etc/xdg/autostart/flashcap-vm-test.desktop <<'DESKTOP'
[Desktop Entry]
Type=Application
Name=FlashCap portal test
Exec=/bin/bash /mnt/flashcap-input/session.sh
OnlyShowIn=GNOME;
DESKTOP
systemctl enable flashcap-camera.service flashcap-install.service gdm3
systemctl set-default graphical.target
systemctl disable apt-daily.timer apt-daily-upgrade.timer
touch /etc/cloud/cloud-init.disabled
mkdir -p /usr/local/share/flashcap-vm
dpkg-query -W -f='${Package}\t${Version}\n' > /usr/local/share/flashcap-vm/packages.tsv
apt-get clean
sync
