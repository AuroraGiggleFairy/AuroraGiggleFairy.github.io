# Nexus Mod Details Packet

## 1) Mod Identity

| Field | Value |
|---|---|
| Mod Name (code) | `AGF-{Category}-{ModName}` *(e.g. AGF-VP-LargerStorageOption)* |
| Nexus Display Mod Name | `AGF - V{GameVer} - {Category} - {ModName}` *(e.g. AGF - V3 - VP - Larger Storage Option)* |
| Game | 7 Days to Die |
| Game Version Tested | `{version}` *(e.g. 3)* |
| Mod Version | `{x.y.z}` *(e.g. 1.0.1)* |
| Suggested Category | `{pick one}` *(see list below)* |
| Tags | `{comma separated}` *(e.g. storage, quality-of-life, eac-friendly)* |
| File Download Name | `{FileName}` *(e.g. AGF VP LargerStorageOption - shown on Nexus file list)* |
| Source README | `03_ReleaseSource/{folder}/README.md` |

### Suggested Categories (pick one)
- Quality of Life / Storage
- Quality of Life / Inventory
- HUD / UI
- HUD / Compass
- Audio / Sound
- Vehicles / Storage
- Vehicles / Performance
- Farming / Harvesting
- Crafting / Recipes
- Crafting / Smelting
- Combat / Weapons
- Combat / Ammo
- Building / Blocks
- Building / Decorations
- Skills / Progression
- Skills / Leveling
- Items / Food & Drink
- Items / Medical
- Items / Resources
- Mods / Slots
- Mods / Crafting
- Server / Admin Tools
- Client / QoL
- Client / Accessibility

### Copy/Paste: Nexus Mod Name
```text
AGF - V{GameVer} - {Category} - {ModName}
```

### Copy/Paste: Tags (comma-separated, for Nexus tags field)
```text
{tag1, tag2, tag3}
```

---

## 2) Short Description (Copy/Paste)
**Target:** Nexus → General → Short description  
**Format:** Single line of plain text. 350 character limit.  
**Rule:** One sentence describing what the mod does, plus last tested version.

Length: `{count}` / 350

```text
{One-line description of the mod. Last tested on 7d2d Version {X}.}
```

---

## 3) Full Description (Copy/Paste) — COMING SOON
**Target:** Nexus → General → Full description  
**Status:** Structure and formatting to be designed. This section needs the most work.
**Note:** Will include branded formatting, mod type explanation, feature highlights, compatibility, install/remove/update guides, and support links.

---

## 4) File Details (Copy/Paste)
**Target:** Nexus → Files → Edit File

### 4.1 Display Name
```text
AGF - V{GameVer} - {Category} - {ModName}
```

### 4.2 File Version
```text
{x.y.z}
```

### 4.3 File Category
```text
Main
```
*(Options: Main / Optional / Miscellaneous)*

### 4.4 File Description / Notes
**Target:** Nexus → Files → File options → Description  
**Format:** Two lines. Line 1 = one-line description of the mod. Line 2 = mod type sentence.

```text
{One-line description of what the mod does.}
{Mod Type}: {mod type explanation}
```

---

## 5) Changelog (Copy/Paste)
**Target:** Nexus → Files → Add changelog modal

### 5.1 Current Version Entry (v{x.y.z})
**Version field value:**
```text
{x.y.z}
```

**Changelog entry field value (one line per entry):**
```text
{Bullet 1}
{Bullet 2}
{Bullet 3}
```

### 5.2 Full Changelog Library (copy a block by version)

**v{prev1}** changelog entry field value:
```text
{Bullet 1}
{Bullet 2}
```

**v{prev2}** changelog entry field value:
```text
{Bullet 1}
{Bullet 2}
{Bullet 3}
```

