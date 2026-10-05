# Interaction prompt details

## Status types

- Slots `used/total`
- Loot refresh `Day N HH:MM`
- Queue `used/total`
- Fuel `mm:ss`
- Smelting `used/total`
- Output `used/total`
- Seats
- Durability `hp/max`
- Gas `fuel/max`
- Ammo `current/magazine`
- Power On / Off
- Lock
- Owner

Tools / cookware names are not shown.

## Layout slots

Details are one column. Lock is first when that block can lock. Owner is last. Other rows use slot 0, then 1, then 2… so their slot numbers are the visual order after lock. Idle values still occupy a row (`0/4`, `00:00`). Unused slots collapse.

The card width is one column. The header pill sizes to the name and can be narrower or wider than the card.

| Slot | Typical | Visual order |
|---|---|---|
| 6 | Lock | First, when present |
| 0–5 | Fuel / Queue / … | After lock, in slot number order |
| 7 | Owner | Last, when present |

## By block

**Storage** (player crates / touched `TEFeatureStorage`)

```
[  (XL) Wood Storage Crate (Empty)  ]
[Lock]
[Slots]
[Loot refresh day + time]
[Owner]
```

World loot (not player-owned / not player-placed): first open starts the sandbox loot-respawn clock. Later opens and walking up do not restart it. If items are still inside when that time hits, loot stays until those items are removed, then the container becomes fresh. Refresh row is hidden when loot respawn is off, on player storage, or when the loot list `destroy_on_close` is `true` (airdrop / twitch crates vanish) or `empty` (backpacks, nests, junk piles, mailboxes, safes that become a non-loot Open model). Icon is `ui_game_symbol_scrap`. Text is `Day N, HH:MM`.

**Forge**

```
[  Forge / Press (F) to use  ]
[Lock]
[Fuel]
[Crafting]
[Smelting]
[Output]
[Owner]
```

**Campfire** (and other fuel workstations)

```
[  Campfire / Press (F) to use  ]
[Lock]
[Fuel]
[Crafting]
[Output]
[Owner]
```

**Other workstations** (workbench, chemistry station, cement mixer, …)

```
[  Workbench / Press (F) to use  ]
[Lock]
[Crafting]
[Output]
[Owner]
```

**Vehicle**

```
[  Hold (E) to interact with Motorcycle  ]
[Lock]
[Storage]
[Durability]
[Gas]
[Seats]
[Owner]
```

Gas only if the vehicle has a fuel tank.

**Junk sledge** (`junkTurretSledge`)

```
[  turret name / …  ]
[Durability]
[Owner]
```

**Junk turret** (`junkTurretGun`, magazine)

```
[  turret name / …  ]
[Ammo]
[Durability]
[Owner]
```

**Junk drone** (`entityJunkDrone`)

```
[  drone name / …  ]
[Storage]
[Durability]
[Owner]
```

Lock / Unlocked use matching lock vs unlock icons. Locked uses deco Campfire red (`f0beb9`); Unlocked uses deco loot green (`c8e6be`).

**Dew collector**

```
[  Dew Collector / …  ]
[Lock]
[Owner]
```

**Generator**

```
[  (E) to interact with Generator Bank  ]
[Lock]
[On / Off]
[Gas]
[Max output]
[Power]
[Owner]
```

**Power bank / solar**

```
[  Battery Bank / …  ]
[Lock]
[On / Off]
[Slots]
[Max output]
[Power]
[Owner]
```

**Shot turret** / **Rocket turret**

```
[  turret name / …  ]
[Lock]
[Owner]
```
