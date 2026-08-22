# WORKFLOW - CombatGlitchMitigations DLL

## Working Methods
- Source of truth: `00_DLL-Projects/Projects/DLL_NoEAC-CombatGlitchMitigations/`
- Research: `00_Support/WorkspaceData/Research/7D2D-v3.1-Vanilla-Bugs-Possible-Fixes.md`
- Share list: `00_Support/WorkspaceData/Research/7D2D-v3.1-CombatGlitchMitigations-Simple-List.md`
- Build: `dotnet build 00_DLL-Projects/Projects/DLL_NoEAC-CombatGlitchMitigations/CombatGlitchMitigations.csproj -c Release`
- Output: `00_DLL-Projects/Projects/DLL_NoEAC-CombatGlitchMitigations/CombatGlitchMitigations.dll`
- Draft mod: `01_Draft/AGF-NoEAC-CombatGlitchMitigations-v0.0.1/` (copy the dll here; do **not** edit ModInfo.xml or README unless the user asks)

## Behavior (v1 target)
- NoEAC **server/host** Harmony helper
- Stop punch damage through a closed door/wall/floor/hatch/bars, or if the zombie is inside that block
- Stop punch damage if they never swung
- Re-check reach when the punch would land
- After a short wait, move them up out of the floor or back through the wall they glitched through
- Face-hit pain anim (hands on face / legs buckle) must not close the gap. Not PainResistPerHit.
- Player swing through loose grass/plant if the zombie is on this ray
- Get-up: melee and ranged count through chest/pelvis. Do not turn their colliders back on.

## Do-Not-Do
- Do not edit draft ModInfo.xml or README unless the user asks
- Do not hook PainResistPerHit for B06
- Do not shorten XML Range in v1
- Do not disable crouch/crawl
- Do not re-enable `PhysicsTransform` during ragdoll / get-up
- Do not deploy the dll while the game has the file locked
