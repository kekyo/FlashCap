@echo off

rem FlashCap - Independent camera capture library.
rem Copyright (c) Kouji Matsui (@kekyo@mi.kekyo.net)
rem
rem Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo.
echo "==========================================================="
echo "Build FlashCap"
echo.

rem git clean -xfd

dotnet restore

dotnet build -p:Configuration=Release -p:Platform=AnyCPU FlashCap.Core\FlashCap.Core.csproj
dotnet build -p:Configuration=Release -p:Platform=AnyCPU FlashCap\FlashCap.csproj
dotnet build -p:Configuration=Release -p:Platform=AnyCPU FSharp.FlashCap\FSharp.FlashCap.fsproj

dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts FlashCap.Core\FlashCap.Core.csproj
dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts FlashCap\FlashCap.csproj
dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts FSharp.FlashCap\FSharp.FlashCap.fsproj
