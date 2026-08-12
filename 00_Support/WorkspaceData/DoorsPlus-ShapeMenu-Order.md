# DoorsPlus Shape Menu Order — EDIT THIS FILE

Rearrange by cutting/pasting model names. One name per line. Exact spelling required
(same as CSV `Name` / white-base keys).

After you edit, ask to **apply the shape order** (sync → rebuild → deploy).
Edits here do nothing until that apply step runs.

## How this maps to the menu

- Shape grid is **12 columns** wide.
- Section number = overall group order (`Sort Step 1`).
- Order of names inside a section = order within that group (`Sort Step 2`).
- **Compact** sections: pad only at section end. Within a model, Boarded then Plain
  stay adjacent (B, P, B, P…) with no row split.
- **Boarded-then-Plain** sections: all Boarded in the section first, then Plain rows.
- **Full blank rows before sections**: after finishing the previous section’s row,
  insert one completely empty 12-slot row before that section starts.

Wood / Iron / Steel / Powered helpers share this model order (only the AGF tier changes).

Overall: wood → iron → steel → wooden fence → chainlink/gates →
**garage doors** → *(blank row)* → **roll-ups** → house exterior →
screens+glass → sliding → house interior doors → *(blank row)* →
closets/cabinets → commercial exterior →
trailers/jail/elevators → bathroom stall → porta potty doors.

---

## Compact sections

```
1, 3, 5, 9, 10, 15
```

## Boarded-then-Plain sections

(none currently)

```

```

## Full blank rows before sections

Comma-separated section numbers. Inserts one empty 12-wide row before the section.

```
6, 7, 12
```

---

## Section 1 | Wood pattern (+ iron shutters/cellar at end of row 1)

```
oldWoodDoor
oldWoodDoorDouble
woodHatch
shuttersWood01
shuttersWood02
cellarDoorDoubleWood
shuttersIron01
shuttersIron02
cellarDoorDoubleIron
```

## Section 2 | Iron color doors / hatches

```
ironDoorWhite
ironDoorDoubleWhite
ironHatchWhite
```

## Section 3 | Steel / vault pattern

```
vaultDoor01
vaultDoor01Double
vaultHatch01
manholeHatch
shuttersSteel01
shuttersSteel02
cellarDoorDoubleSteel
```

## Section 4 | Wooden fence doors (row before chainlink)

```
woodenFenceDoorWhite
```

## Section 5 | Gates / chainlink

```
chainlinkFenceDoor
chainlinkFenceDoorDouble
chainlinkGateDouble
chainlinkGateDoubleWide
doorWoodLargeGate
```

## Section 6 | Garage doors

```
ironGarageDoorWhite
woodenGarageDoor3x3White
woodenGarageDoor4x3White
woodenGarageDoor5x3White
steelGarageDoor3x3White
steelGarageDoor4x3White
steelGarageDoor5x3White
```

## Section 7 | Roll-ups

```
rollUpDoor3x3White
rollUpDoor5x4White
rollUpDoor7x4White
rollUpGate3x3White
rollUpGate4x3White
rollUpGate4x3DiagonalWhite
rollUpGate5x3White
```

## Section 8 | House — exterior

```
exteriorHouseDoorOldWhite
exteriorHouseDoorDoubleOldWhite
exteriorHouseDoorWhite
exteriorHouseDoorDoubleWhite
exteriorHouseDoorSideLightWhite
exteriorHouseDoorSideLightSingleWhite
frenchDoorWhite
frenchDoorDoubleWhite
```

## Section 9 | Screen doors + glass doors (one row; glass B then P per door)

```
exteriorScreenDoor
exteriorScreenDoornNoFrame
commercialGlassDoor
commercialGlassDoorDouble
commercialBulletproofGlassDoor
commercialBulletproofGlassDoorDouble
```

## Section 10 | Sliding doors — house + commercial (one row; B then P per door)

```
houseSlidingDoorWhite
houseSlidingDoorNoScreenWhite
commercialSlidingDoor
commercialBulletproofSlidingDoor
```

## Section 11 | House — interior doors

```
interiorDoorOldWhite
interiorDoorOldDoubleWhite
interiorHouseDoorWhite
interiorHouseDoorDoubleWhite
```

## Section 12 | House — closets / cabinets

```
closetDoorWhite
closetDoorDoubleWhite
pantryDoorWhite
cntArmoireDoorsWhite
tallCabinetDoorWhite
```

## Section 13 | Commercial — exterior

```
commercialDoorV1White
commercialDoorDoubleV1White
commercialDoorV2White
commercialDoorDoubleV2White
commercialDoorV3White
commercialDoorDoubleV3White
commercialDoorV4White
commercialDoorDoubleV4White
```

## Section 14 | Commercial — interior (trailers, jail, elevators)

```
trailerDoorWhite
trailerDoorNoWindowWhite
jailDoorWhite
jailDoorDoubleWhite
elevatorDoor
elevatorDoorDouble
elevatorDoorTenthBlock
```

## Section 15 | Bathroom stall (own row, between jail and porta)

```
bathroomStallDoor
```

## Section 16 | Porta potty doors

```
portaPottyDoorWhite
```

---

## Notes / scratch (optional)

Use this area for comments while testing. Sync ignores everything below this heading.

- Porta potty *units* excluded from generation; only `portaPottyDoor*`.
- §9 screens+glass and §10 sliding are compact → interleaved B/P on one row.
- Blank rows before §6 garages, §7 roll-ups, and §12 closets/cabinets.
- Elevator double: CompositeFeatures normalized so activate opens (not lock toggle).
-
