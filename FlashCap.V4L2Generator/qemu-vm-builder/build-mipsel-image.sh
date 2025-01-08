#!/bin/sh

# sudo apt install qemu-system qemu-utils bsdtar

wget https://cdimage.debian.org/debian-cd/12.5.0/mipsel/iso-cd/debian-12.5.0-mipsel-netinst.iso
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

sudo modprobe nbd max_part=8

sudo qemu-nbd --connect=/dev/nbd0 `pwd`/debian-mipsel.qcow2
mkdir mnt
sudo mount -r /dev/nbd0p1 `pwd`/mnt
cp mnt/boot/vmlinuz-6.1.0-20-4kc-malta vmlinuz-mipsel
cp mnt/boot/initrd.img-6.1.0-20-4kc-malta initrd-mipsel.img
sudo umount /dev/nbd0p1
sudo qemu-nbd --disconnect /dev/nbd0

