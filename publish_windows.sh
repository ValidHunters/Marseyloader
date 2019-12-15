#!/bin/sh

cd "$(dirname "$0")"

./download_net_runtime.py windows

# Clear out previous build.
rm -r **/bin bin/
rm SS14.Launcher_Windows.zip

dotnet publish SS14.Launcher/SS14.Launcher.csproj -c Release --no-self-contained -r win-x64 /nologo

# Create intermediate directories.
mkdir -p bin/publish/Windows/bin
mkdir -p bin/publish/Windows/dotnet

cp -r Dependencies/dotnet/windows/* bin/publish/Windows/dotnet
cp PublishFiles/launch.bat bin/publish/Windows
cp SS14.Launcher/bin/Release/netcoreapp3.0/win-x64/publish/* bin/publish/Windows/bin

pushd bin/publish/Windows
zip -r ../../../SS14.Launcher_Windows.zip *
popd
