#!/bin/sh

# https://github.com/kekyo/qemu-debian-mipsel-setup

qemu-system-mipsel \
  -M malta \
  -m 2048 \
  -hda debian-mipsel.qcow2 \
  -kernel vmlinuz \
  -initrd initrd.gz \
  -nographic \
  -append "root=/dev/sda1 console=ttyS0 nokaslr" \
  -netdev user,id=net0,hostfwd=tcp:127.0.0.1:2222-:22 \
  -device e1000,netdev=net0,id=net0,mac=52:54:00:12:34:56

