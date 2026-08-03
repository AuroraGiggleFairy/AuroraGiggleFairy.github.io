# WORKFLOW - SmeltTimerTotal DLL

## Working Methods
- Source of truth: `00_DLL-Projects/Projects/DLL_NoEAC-SmeltTimerTotal/`
- Build: `dotnet build 00_DLL-Projects/Projects/DLL_NoEAC-SmeltTimerTotal/SmeltTimerTotal.csproj -c Release`
- Output: `00_DLL-Projects/Projects/DLL_NoEAC-SmeltTimerTotal/SmeltTimerTotal.dll`
- Draft mod: `01_Draft/AGF-NoEAC-SmeltTimerTotal-v0.0.1/`

## Behavior
- Client-only UI (NoEAC). Preference: PlayerPrefs `AGF.SmeltTimerTotal.Mode`
- Single = vanilla per-item timer; Total = remaining + rest of stack (Weight, MeltTimePerUnit, bellows CraftingSmeltTime)
- Mode strip under forge Smelting header; Harmony shifts content for SmeltingPlus load-order safety

## Do-Not-Do
- Do not add netcode / server-required assets
- Do not edit ActiveBuild/ReleaseSource unless asked
- Do not deploy DLL while the game has the file locked
