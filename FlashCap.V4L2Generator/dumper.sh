#!/bin/bash

#sudo apt install clang mono

echo "Step 1: Dump video2dev.h AST by Clang."
clang -Xclang -ast-dump=json -fsyntax-only /usr/include/linux/videodev2.h > videodev2.h.ast.json

echo "Step 2: Generate members dumper."
mono FlashCap.V4L2Generator.exe 1 /usr/include/linux/videodev2.h videodev2.h.ast.json videodev2.dumper.cpp `uname -m`

echo "Step 3: Execute members dumper."
gcc -o videodev2.dumper videodev2.dumper.cpp
./videodev2.dumper > videodev2.h.members.json

echo "Step 4: Generate interop code."
mono FlashCap.V4L2Generator.exe 2 videodev2.h.members.json ../../../../FlashCap/Internal/V4L2/

echo "Step 5: Generate base interop code."
mono FlashCap.V4L2Generator.exe 3 videodev2.h.members.json ../../../../FlashCap/Internal/V4L2/

