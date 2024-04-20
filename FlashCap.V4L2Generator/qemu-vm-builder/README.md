# qemu virtual machines builder

This directory contains some bash scripts for building qemu-based vm images.
We can use these vm environments for generating FlashCap V4L2 interop fragments.

* `build-virt-images.sh`:
  Construct virt-manager based Debian 12 vm images for `x86_64`, `i686`, `aarch64` and `armv7l` architectures.
* `build-mipsel-image.sh`:
  Build Debian 12 vm image for `mipsel` architecture.
* `run-mipsel-image.sh`:
  Run Debian 12 vm image for `mipsel` architecture.

