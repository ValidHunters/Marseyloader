#!/bin/sh

cd "$(dirname "$0")"

rm -r **/bin

nuget restore SS14.Launcher.sln
msbuild /p:Configuration=Release /p:TargetFramework=net472 SS14.Launcher.sln

cp PublishFiles/SS14.Launcher SS14.Launcher/bin/Release/net472/

pushd SS14.Launcher/bin/Release/net472/
rm *.pdb
zip -r ../../../../SS14.Launcher_all_platforms.zip *
popd