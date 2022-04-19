#!/bin/sh

Configuration=Debug; export Configuration

rm -rf bin/$Configuration/mono obj/$Configuration/mono
mkdir -p bin/$Configuration/mono
mkdir -p obj/$Configuration/mono

mcs -debug -r:external/Newtonsoft.Json.dll -out:bin/$Configuration/mono/FlashCap.V4L2Generator.exe ClangASTJsonSchema.cs StructureDumpedJsonSchema.cs Utilities.cs external/RelaxVersioner.cs Program.cs

cp external/Newtonsoft.Json.dll bin/$Configuration/mono/
#cp videodev2.h.* bin/$Configuration/mono/
cp dumper.sh bin/$Configuration/mono/

