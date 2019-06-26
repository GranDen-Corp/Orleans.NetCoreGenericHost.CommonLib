#!/usr/bin/env pwsh
Get-ChildItem *.csproj -Recurse | ForEach-Object { dotnet build -c Debug $_.FullName }
