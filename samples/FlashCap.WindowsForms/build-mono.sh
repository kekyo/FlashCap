#!/bin/sh

Configuration=Debug; export Configuration

rm -rf bin/$Configuration/mono obj/$Configuration/mono
mkdir bin/$Configuration/mono
mkdir obj/$Configuration/mono

resgen /compile MainForm.resx,obj/$Configuration/mono/MainForm.resources
mcs -debug -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:../../FlashCap/bin/$Configuration/net45/FlashCap.dll -res:obj/$Configuration/mono/MainForm.resources -out:bin/$Configuration/mono/FlashCap.WindowsForms.exe MainForm.Designer.cs MainForm.cs Program.cs
cp ../../FlashCap/bin/$Configuration/net45/FlashCap.dll bin/$Configuration/mono/

mono bin/$Configuration/mono/FlashCap.WindowsForms.exe &
