#!/bin/sh

cd "$(dirname "$0")"

# Clear out previous build.
rm -r **/bin

dotnet publish SS14.Launcher/SS14.Launcher.csproj -c Release --self-contained -r osx-x64 /nologo

# Create intermediate directories.
mkdir -p bin/publish/macOS

cp -r "PublishFiles/Space Station 14 Launcher.app" bin/publish/macOS
cp SS14.Launcher/bin/Release/netcoreapp3.0/osx-x64/publish/* "bin/publish/macOS/Space Station 14 Launcher.app/Contents/Resources/"
pushd bin/publish/macOS
zip -r ../../../SS14.Launcher_macOS.zip *
popd
