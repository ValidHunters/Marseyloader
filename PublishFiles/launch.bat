@echo off

echo %~dp0

SET DOTNET_ROOT=%~dp0\dotnet

echo %DOTNET_ROOT%

CALL bin\SS14.Launcher.exe
