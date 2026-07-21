# Flight Level Assist (Prototype)

Adds a press-activate flight-level assist for flying vehicles:

- Pressing the activation button enables flat-lock mode (latched).
- Pressing again while flat-lock is already active does nothing.
- Flat-lock remains active until manual pitch input is used.
- Only pitch input cancels flat-lock; turning, strafing, and wobble are allowed to continue.
- In forced flat-lock mode, assist first performs natural pitch correction toward level, then captures and hard-locks the settled world Y plane.
- Pitch correction uses vanilla-like step torque, honors sprint speed-up, and scales with actual forward speed plus a short progressive ramp.
- Extra roll-rate damping is applied to reduce side-to-side wobble.
- Forced flat-lock uses smooth velocity-based altitude hold (no hard position snapping).

## Build

```powershell
dotnet build .\FlightLevelAssist\FlightLevelAssist.csproj -v minimal
```

## Install

Build output goes directly to this mod folder:

- `FlightLevelAssist/FlightLevelAssist.dll`

No Release packaging step is required.

## Client-side Hotkey Config

Edit:

- `FlightLevelAssist/Config/FlightLevelAssistConfig.txt`

This file is optional. If it is missing or invalid, the DLL uses default `Z` automatically.

Setting:

- `LevelAssistHotkey=Z`
- `ControllerActivationAction=None`

Use any valid Unity `KeyCode` name, for example:

- `LevelAssistHotkey=Z`
- `LevelAssistHotkey=V`
- `LevelAssistHotkey=G`
- `LevelAssistHotkey=Mouse4`

Controller action options:

- `ControllerActivationAction=None`
- `ControllerActivationAction=HonkHorn`
- `ControllerActivationAction=ToggleFlashlight`
- `ControllerActivationAction=ToggleTurnMode`
- `ControllerActivationAction=Scoreboard`
- `ControllerActivationAction=Inventory`
- `ControllerActivationAction=Activate`

## Z Key Check (Vehicle Context)

From decompiled input flow:

- `Z` is bound in local actions (`SelectionSet`) and not in vehicle actions.
- Vehicle keyboard defaults include `W`, `A`, `S`, `D`, `LeftShift`, `Space`, `C`, and `X`.
- Vehicle movement reads `VehicleActions` in `EntityVehicle.MoveByAttachedEntity`.

Conclusion: by default, `Z` is not used for vehicle controls and is a good candidate for tap/hold level assist.

## Notes

This is a first-pass implementation intended to validate feel and compatibility.

## Plane Momentum Gate (Y-Lock Safety)

To avoid pseudo-hover behavior on plane-like vehicles, hard Y-lock now waits for minimum forward momentum before engaging.

Rule used by the patch:

- `requiredMomentum = max(7.0 m/s, VelocityMaxForward * 0.34)`
- Hard lock engages only when `CurrentForwardVelocity >= requiredMomentum`
- If hard lock is active and momentum stays below threshold for ~0.5s, Y-lock disengages automatically.

Examples:

- If `VelocityMaxForward = 18`, threshold is `max(7.0, 6.12) = 7.0 m/s`.
- If `VelocityMaxForward = 28`, threshold is `max(7.0, 9.52) = 9.52 m/s`.
- If `VelocityMaxForward = 42`, threshold is `max(7.0, 14.28) = 14.28 m/s`.

When below threshold, assist keeps leveling behavior but delays hard Y-lock, so the craft must build real plane momentum first.
