#!/bin/sh

rm -r **/bin
rm -r **/obj

rm SS14.Launcher_Linux_x64.zip
rm SS14.Launcher_Windows_x64.zip
rm SS14.Launcher_macOS_x64.zip

dotnet publish --self-contained --runtime linux-x64 -c release /p:TrimUnusedDependencies=true
pushd SS14.Launcher/bin/Release/netcoreapp2.2/linux-x64/publish
zip -r ../../../../../../SS14.Launcher_Linux_x64.zip *
popd
dotnet publish --self-contained --runtime win-x64 -c release /p:TrimUnusedDependencies=true /p:LinkDuringPublish=false
pushd SS14.Launcher/bin/Release/netcoreapp2.2/win-x64/publish
zip -r ../../../../../../SS14.Launcher_Windows_x64.zip *
popd
dotnet publish --self-contained --runtime osx-x64 -c release /p:TrimUnusedDependencies=true /p:LinkDuringPublish=false
pushd SS14.Launcher/bin/Release/netcoreapp2.2/osx-x64/publish
zip -r ../../../../../../SS14.Launcher_macOS_x64.zip *
popd