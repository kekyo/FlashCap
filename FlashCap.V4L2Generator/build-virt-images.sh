#!/bin/sh

# sudo apt install virt-manager qemu-system

wget https://cdimage.debian.org/debian-cd/12.5.0/amd64/iso-cd/debian-12.5.0-amd64-netinst.iso
wget https://cdimage.debian.org/debian-cd/12.5.0/i386/iso-cd/debian-12.5.0-i386-netinst.iso
wget https://cdimage.debian.org/debian-cd/12.5.0/arm64/iso-cd/debian-12.5.0-arm64-netinst.iso
wget https://cdimage.debian.org/debian-cd/12.5.0/armhf/iso-cd/debian-12.5.0-armhf-netinst.iso
wget https://cdimage.debian.org/debian-cd/12.5.0/mipsel/iso-cd/debian-12.5.0-mipsel-netinst.iso

qemu-img create -f qcow2 debian-amd64.qcow2 20G
qemu-img create -f qcow2 debian-i386.qcow2 20G
qemu-img create -f qcow2 debian-arm64.qcow2 20G
qemu-img create -f qcow2 debian-armhf.qcow2 20G
qemu-img create -f qcow2 debian-mipsel.qcow2 20G

virt-install --connect qemu:///system --name Debian_amd64 --vcpus 2 --ram 4096 --hvm --virt-type qemu --arch x86_64 --os-variant debian12 --import --noreboot --disk path=`pwd`/debian-12.5.0-amd64-netinst.iso,device=cdrom --disk path=`pwd`/debian-amd64.qcow2

virt-install --connect qemu:///system --name Debian_i386 --vcpus 2 --ram 2048 --hvm --virt-type qemu --arch i686 --os-variant debian12 --import --noreboot --disk path=`pwd`/debian-12.5.0-i386-netinst.iso,device=cdrom --disk path=`pwd`/debian-i386.qcow2

virt-install --connect qemu:///system --name Debian_arm64 --vcpus 2 --ram 4096 --hvm --virt-type qemu --arch aarch64 --os-variant debian12 --import --noreboot --disk path=`pwd`/debian-12.5.0-arm64-netinst.iso,device=cdrom --disk path=`pwd`/debian-arm64.qcow2

# https://bugzilla.redhat.com/show_bug.cgi?id=2077680

virt-install --connect qemu:///system --name Debian_armhf --vcpus 2 --ram 2048 --hvm --virt-type qemu --arch armv7l --os-variant debian12 --import --noreboot --disk path=`pwd`/debian-12.5.0-armhf-netinst.iso,device=cdrom --disk path=`pwd`/debian-armhf.qcow2 --tpm none


