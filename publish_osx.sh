#!/bin/sh

cd "$(dirname "$0")"

./download_net_runtime.py mac

# Clear out previous build.
rm -r **/bin bin/
rm SS14.Launcher_macOS.zip

dotnet publish SS14.Launcher/SS14.Launcher.csproj -c Release --no-self-contained -r osx-x64 /nologo

# Create intermediate directories.
mkdir -p bin/publish/macOS

cp -r "PublishFiles/Space Station 14 Launcher.app" bin/publish/macOS
cp -r Dependencies/dotnet/mac/* "bin/publish/macOS/Space Station 14 Launcher.app/Contents/Resources/dotnet/"
cp -r SS14.Launcher/bin/Release/netcoreapp3.1/osx-x64/publish/* "bin/publish/macOS/Space Station 14 Launcher.app/Contents/Resources/bin/"
pushd bin/publish/macOS
zip -r ../../../SS14.Launcher_macOS.zip *
popd
