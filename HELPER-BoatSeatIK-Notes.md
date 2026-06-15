# Boat Seat IK Helper Notes

Purpose: persistent reference for seat/IK tuning on boat seats (especially mirrored seat1).

## Confirmed Engine Behavior (Decompiled)

Source references:
- `_DLL-Projects/_Decompiled DLLs/Decompiled_AssemblyCSharp/VPSeat.cs`
- `_DLL-Projects/_Decompiled DLLs/Decompiled_AssemblyCSharp/VPSteering.cs`
- `_DLL-Projects/_Decompiled DLLs/Decompiled_AssemblyCSharp/VehiclePart.cs`
- `_DLL-Projects/_Decompiled DLLs/Decompiled_AssemblyCSharp/Vehicle.cs`
- `_DLL-Projects/_Decompiled DLLs/Decompiled_AssemblyCSharp/IKController.cs`

### 1) Seat IK targets are local to player rig space
- `VPSeat` initializes IK targets for hands/feet from seat properties.
- In `VehiclePart.InitIKTarget(..., parentT = null)`, seat IK stores raw `IKHand*Position`, `IKFoot*Position`, etc.
- `IKController` transforms these using player model local-to-world matrix.
- Practical result: movement feels centered on the avatar rig, not world/object surface directly.

### 2) Steering IK uses steering joint parent space
- `VPSteering` initializes hand IK with `parentT = steeringJoint`.
- Steering hand targets follow steering joint local transform and steering rotation.

### 3) Slot 0 vs slot 1 IK composition differs
- `Vehicle.GetIKTargets(slot)`:
  - slot 0: includes `handlebars` + `pedals` + `seat0` IK
  - slot > 0: includes only `seatN` IK
- Practical result: for `seat1`, handlebars IK is not part of the seat1 target list.

### 4) IK target order can override by same goal
- Multiple targets for same goal (example: left hand) can compete.
- For driver, later-applied targets can dominate visual outcome.

## Workflow That Works Best

### Position-first, rotation-second
1. Set target rotation(s) to neutral first (often `0,0,0`) to isolate translation.
2. Dial in position only until limb is where it should be.
3. Add small rotation adjustments after position is correct.

### Delta size recommendations
- Position passes: `0.02` to `0.06` per test on one axis at a time.
- Rotation passes: `4` to `12` degrees per test on one axis at a time.

### Mirrored seat caution
- Mirrored seats may feel direction-inverted on one axis from expected viewpoint.
- If a change moves opposite of expectation, reverse only that axis and continue.

## Seat1 Practical Targets

Current intent for `seat1`:
- Left hand: rest on/near left knee, slightly forward, natural wrist.
- Right foot: rest near box edge corner with slight angle and small realistic gap.

Use this quick loop:
1. Tune `IKHandLPosition` only until hand placement is correct.
2. Tune `IKFootRPosition` only until foot placement is correct.
3. Add `IKHandLRotation` to flatten/orient palm naturally.
4. Add `IKFootRRotation` to angle foot on edge naturally.

## Troubleshooting Notes

- If hand looks "wrong" at rotation `0,0,0`, that is normal: neutral is rig default orientation, not world-flat palm.
- If foot drifts inward unexpectedly, reverse the X-axis direction for that seat side.
- If edits appear ignored on seat1, verify test is actually using `seat1` block and not `seat0`.

## Keep/Do Not Keep

- Keep this as a living helper file and update after successful pose values.
- Do not delete handlebars class/properties in production mod unless confirmed safe for that specific vehicle setup.
