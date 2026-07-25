# Alter Autominers v1.4.0 (third-party update)

Updated port of Alter's abandoned Autominers v1.3 for the current 7 Days to Die build.

This package lives under `00_Support/Archive` only. It is **not** part of the AGF draft / ActiveBuild / DLL-Projects release pipeline.

## Contents

- `Alter_Autominers/` — deployable mod folder (copy into the game `Mods` folder)
- `Source/` — Harmony C# project used to rebuild `Autominers.dll`

## Requirements

- EAC off (`SkipWithAntiCheat`)
- TFP Harmony (`0_TFP_Harmony`)

## Build

```bat
dotnet build Source\Autominers.csproj -c Release
copy /Y Source\bin\Autominers.dll Alter_Autominers\Autominers.dll
```
