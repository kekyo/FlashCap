@echo off

rem FlashCap - Independent camera capture library.
rem Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
rem
rem Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo.
echo "==========================================================="
echo "Build FlashCap"
echo.

rem git clean -xfd

dotnet restore
dotnet build -c Release -p:Platform="Any CPU" FlashCap.sln
dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts FlashCap\FlashCap.csproj
