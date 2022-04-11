#!/bin/sh

Configuration=Debug; export Configuration

rm -rf bin/$Configuration/mono obj/$Configuration/mono
mkdir -p bin/$Configuration/mono
mkdir -p obj/$Configuration/mono

mcs -debug -out:bin/$Configuration/mono/FlashCap.V4L2Dumper.exe Program.cs

mono bin/$Configuration/mono/FlashCap.V4L2Dumper.exe
