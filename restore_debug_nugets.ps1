#!/usr/bin/env pwsh
& ./build_debug_nugets.ps1
Get-ChildItem example/*.csproj -Recurse | ForEach-Object { dotnet restore --no-cache --force-evaluate $_.FullName }
