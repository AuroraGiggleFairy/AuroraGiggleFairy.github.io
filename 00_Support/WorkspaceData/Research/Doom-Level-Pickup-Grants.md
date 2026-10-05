# Doom level pickup grants

Checked against the MandaServer game copy on 2026-09-27.

Walking over a pickup in a Doom level either puts an item in the backpack or applies a buff immediately. It does not use the item. Health packs, armor, and powerups have to be used afterward. Ammo is just ammo. Keys, the health bonus, and the armor bonus never become inventory items.

Almost every pickup also plays a short screen flash (`doomBuffPickup`, about 0.17 seconds). The berserk pack does not.

Weapons placed in the levels do not give the weapon. They give ammo.

## Sources

Live grants (what the levels actually spawn):

- `7 Days to Die - MandaServer\Mods\zzzz_DoomClassicMaps\Config\blocks.xml`
- Pickup class: `DoomLevels.Item` in `zzzz_DoomClassicMaps\DoomLevels.dll`
- Item effects: `ZZZ_DoomMod_Standalone\Config\items.xml`
- Immediate buffs: `zzzz_DoomClassicMaps\Config\buffs.xml`

The repo file `00_Support/DoomLevelsSource/DoomLevels/content/pickups/pickups.xml` is older than the loaded levels. It still lists `stimPack` / `mediKit` and 10 pistol rounds for a clip. The MandaServer levels use `stimPackweak` / `mediKitweak` and 15 pistol rounds. This note follows the loaded levels.

## Health and armor

| On the map | Thing | You receive | What that does |
|---|---|---|---|
| Stimpack | 2011 | 1 `stimPackweak` | Use it: +5 health (up to +10 with Medic 5), only while at or under 50% health. Cures bleeding. Costs 1 food. |
| Medikit | 2012 | 1 `mediKitweak` | Use it: +13 health (up to +25 with Medic 5), same 50% cap. Cures bleeding, laceration, and concussion. Costs 2 food. |
| Health bonus | 2014 | `doomBuffHealthPotion` immediately | +3 health right away. No item. |
| Soulsphere | 2013 | 1 `soulSphere` | Use it: +100 health. |
| Megasphere | 83 | 1 `megaSphere` | Use it: +200 health and +200 armor. Equips mega armor if you are not already in blue armor. |
| Green armor | 2018 | 1 `armourSecurityGreenPowerUp` | Use it: +100 armor. Equips security armor (40% protection). If you are in mega armor, this downgrades you to security. |
| Blue megaarmor | 2019 | 1 `armourMegaBluePowerUp` | Use it: +200 armor and upgrades you to mega armor (60% protection). |
| Armor bonus | 2015 | `doomBuffArmourShard` immediately | +3 armor right away. If you have no armor, it also equips security armor. |

Health numbers above are Medic rank 0. Each Medic rank raises the stimpack and medikit heal. The 50% health requirement is on the item's use action, so they cannot be used while health is above that.

## Powerups

| On the map | Thing | You receive | What that does when used |
|---|---|---|---|
| Berserk | 2023 | 1 `berserkPack` | Heals up to 50% health, knuckle/knife damage ×5 for 150 seconds, 20% damage reduction. |
| Invulnerability | 2022 | 1 `InvulnerabilityPowerup` | Immortal for 40 seconds. Does not stack. Does not cover storms or biome hazards. |
| Radiation suit | 2025 | 1 `HazmatSuitPowerup` | Hazard-floor protection in Doom levels for 30 minutes. It wears down while you stand in the hazard. |
| Light-amp visor | 2045 | 1 `LightAmplificationPowerup` | Full visibility for 5 minutes, stackable up to 3 uses. |
| Computer map | 2026 | 1 `LightAmplificationPowerup` | Same item as the light-amp visor. |
| Invisibility | 2024 | 1 `LightAmplificationPowerup` | Same item as the light-amp visor. |

## Keys

These are buffs, not items. They are lost on death. A skull and a keycard of the same color are the same buff.

| On the map | Thing | Buff |
|---|---|---|
| Blue keycard | 5 | `buffBlueKeyHeld` |
| Blue skull | 40 | `buffBlueKeyHeld` |
| Yellow keycard | 6 | `buffYellowKeyHeld` |
| Yellow skull | 39 | `buffYellowKeyHeld` |
| Red keycard | 13 | `buffRedKeyHeld` |
| Red skull | 38 | `buffRedKeyHeld` |

## Ammo

| On the map | Thing | You receive |
|---|---|---|
| Clip | 2007 | 15 `9mmBulletDM` |
| Box of bullets | 2048 | 25 `762mmBulletDM` |
| Shells | 2008 | 2 `ShotgunShellDM` |
| Box of shells | 2049 | 8 `ShotgunShellDM` |
| Rocket | 2010 | 1 `RocketFragDM` |
| Box of rockets | 2046 | 3 `RocketFragDM` |
| Cell | 2047 | 10 `PlasmaCellDM` |
| Cell pack | 17 | 50 `PlasmaCellDM` |
| Backpack | 8 | 1 `BackpackFullAmmo` |

Opening `BackpackFullAmmo` gives:

| Item | Count |
|---|---|
| `9mmBulletDM` | 15 |
| `ShotgunShellDM` | 2 |
| `CrossbowBoltDM` | 4 |
| `TurretAmmoDM` | 20 |
| `ammoGasCan` | 60 |
| `762mmBulletDM` | 15 |
| `RocketFragDM` | 1 |
| `PlasmaCellDM` | 10 |

## Weapon sprites

These look like weapons. Picking one up gives ammo only.

| On the map | Thing | You receive |
|---|---|---|
| Shotgun | 2001 | 4 `ShotgunShellDM` |
| Super shotgun | 82 | 4 `ShotgunShellDM` |
| Chaingun | 2002 | 25 `762mmBulletDM` |
| Rocket launcher | 2003 | 1 `RocketFragDM` |
| Plasma rifle | 2004 | 20 `PlasmaCellDM` |
| BFG | 2006 | 20 `PlasmaCellDM` |
| Chainsaw | 2005 | 15 or 50 `9mmBulletDM` |

The chainsaw amount is not one value. In `blocks.xml`, the plain chainsaw block (`doomPdoomCLIPA0T2005` and the Doom 2 copy) gives 15. These light-bucket copies give 50: `L0`, `L2`, `L6`, `L15`, and Doom 2 `L0` / `L4`. Doom 2 `L3` and `L5` stay at 15. The clip pickup (thing 2007) stays at 15.
