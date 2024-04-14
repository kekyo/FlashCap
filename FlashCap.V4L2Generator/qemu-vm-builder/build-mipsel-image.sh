#!/bin/sh

# sudo apt install qemu-system bsdtar

wget https://cdimage.debian.org/debian-cd/12.5.0/mipsel/iso-cd/debian-12.5.0-mipsel-netinst.iso

qemu-img create -f qcow2 debian-mipsel.qcow2 20G

# https://github.com/kekyo/qemu-debian-mipsel-setup

bsdtar -xf debian-12.5.0-mipsel-netinst.iso install/malta/\*

qemu-system-mipsel \
  -M malta \
  -m 2048 \
  -cdrom `pwd`/debian-12.5.0-mipsel-netinst.iso \
  -hda debian-mipsel.qcow2 \
  -kernel install/malta/netboot/vmlinuz-* \
  -initrd install/malta/netboot/initrd.gz \
  -boot d \
  -nographic \
  -no-reboot \
  -append "root=/dev/sda1 nokaslr" \
  -netdev user,id=net0 \
  -device e1000,netdev=net0,id=net0,mac=52:54:00:12:34:56

