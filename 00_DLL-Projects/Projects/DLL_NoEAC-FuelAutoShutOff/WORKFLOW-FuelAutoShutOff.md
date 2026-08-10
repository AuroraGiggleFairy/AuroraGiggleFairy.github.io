# WORKFLOW - FuelAutoShutOff DLL

## Working Methods
- Source of truth: `00_DLL-Projects/Projects/DLL_NoEAC-FuelAutoShutOff/`
- Build: `dotnet build 00_DLL-Projects/Projects/DLL_NoEAC-FuelAutoShutOff/FuelAutoShutOff.csproj -c Release`
- Output: `00_DLL-Projects/Projects/DLL_NoEAC-FuelAutoShutOff/FuelAutoShutOff.dll`
- Draft mod: `01_Draft/AGF-NoEAC-FuelAutoShutOff-v0.0.1/`

## Behavior
- NoEAC **server/host only** — joining clients do not need the DLL for shutoff
- No chat commands, no per-station options, no EnhancedAGF UI, no save file
- Extinguishes when productive craft/smelt work finishes
- Keep-burning only when craft queue is empty **and** primary smelt slots are empty
  (intentional heat buff / screamer attraction)
- Forge: leftover smelt input with full material bins extinguishes (not treated as heat-only)

## Do-Not-Do
- Do not add commands/options unless explicitly requested
- Do not edit ActiveBuild/ReleaseSource unless asked
- Do not deploy DLL while the game has the file locked
