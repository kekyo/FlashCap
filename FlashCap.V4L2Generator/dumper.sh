#!/bin/bash

#sudo apt install clang-13 build-essential mono-devel
#sudo apt install clang-11 build-essential mono-devel
#sudo apt install clang-10 build-essential mono-devel

Clang=clang-13; export Clang
#Clang=clang-11; export Clang
#Clang=clang-10; export Clang
Configuration=Debug; export Configuration

#===============================================================

Timestamp=`date`; export Timestamp

$Clang -v |& head -n 1 > clang_version.txt
gcc -v |& tail -n 1 > gcc_version.txt

echo "Step 1: Dump video2dev.h AST by Clang."
$Clang -Xclang -ast-dump=json -fsyntax-only /usr/include/linux/videodev2.h > videodev2.h.ast.json

echo "Step 2: Generate members dumper."
mono bin/$Configuration/mono/FlashCap.V4L2Generator.exe 1 /usr/include/linux/videodev2.h videodev2.h.ast.json videodev2.dumper.cpp `uname -m` "`cat clang_version.txt`" "`cat gcc_version.txt`" "$Timestamp"

echo "Step 3: Execute members dumper."
gcc -o videodev2.dumper videodev2.dumper.cpp
./videodev2.dumper > videodev2.h.members.json

echo "Step 4: Generate interop code."
mono bin/$Configuration/mono/FlashCap.V4L2Generator.exe 2 videodev2.h.members.json ../FlashCap.Core/Internal/V4L2/ "$Timestamp"

echo "Step 5: Generate base interop code."
mono bin/$Configuration/mono/FlashCap.V4L2Generator.exe 3 videodev2.h.members.json . "$Timestamp"

echo ""
echo "Done, you have to check './NativeMethods_V4L2_Interop.cs' to merge manually some symbols into '../FlashCap.Core/Internal/V4L2/NativeMethods_V4L2_Interop.cs'."

