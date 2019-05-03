#!/bin/sh

cd "$(dirname "$0")"

# Clear out previous build.
rm -r **/bin

# Run build.
nuget restore SS14.Launcher.sln
msbuild /p:Configuration=Release /p:TargetFramework=net472 SS14.Launcher.sln
# Delete PDB file.
rm SS14.Launcher/bin/Release/net472/*.pdb

# Create intermediate directories.
mkdir -p bin/publish/macOS
mkdir -p bin/publish/Windows
mkdir -p bin/publish/Linux

# Linux
cp PublishFiles/SS14.Launcher bin/publish/Linux
cp SS14.Launcher/bin/Release/net472/* bin/publish/Linux
pushd bin/publish/Linux
zip -r ../../../SS14.Launcher_Linux.zip *
popd

# Windows
cp SS14.Launcher/bin/Release/net472/* bin/publish/Windows
pushd bin/publish/Windows
zip -r ../../../SS14.Launcher_Windows.zip *
popd

# macOS
cp -r "PublishFiles/Space Station 14 Launcher.app" bin/publish/macOS
cp SS14.Launcher/bin/Release/net472/* "bin/publish/macOS/Space Station 14 Launcher.app/Contents/Resources/"
pushd bin/publish/macOS
zip -r ../../../SS14.Launcher_macOS.zip *
popd