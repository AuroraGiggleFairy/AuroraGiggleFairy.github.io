# Publication automation checklist

Started 2026-10-07. One step at a time. The current step is the first unchecked box.

Tracker: this file. Say the step number to continue. Do not skip ahead.

## Order

- [ ] **1. Finalize the naming theme.** Locked title and display name: `AGF - V# - SCOPE - ModName`. No `0` or `z` prefix on that name. `V#` is the game line (V3 now, V4 later). `SCOPE` is SERVER, CLIENT, BOTH, or ADMIN. A casual fix uses SERVER, CLIENT, or BOTH. An overhaul-only fix uses ADMIN. The readme Mod Scope holds the rest, including EAC. Folder includes the game `V#` and ends with the mod version `-vX.Y.Z`. Folders may prefix AGF with `0` to load earlier, or some number of `z` to load later. COMPAT uses five `z`s. Every other late mod uses three. Draft: `AGF-Category-Prefix-Overhaul.md`.
- [ ] **2. Apply the naming theme.** Folders, listing titles, and anything else the theme covers. Only after step 1 is locked.
- [ ] **3. One example mod for Publish Help.** Pick one mod and check how its packet is built. Known before that pick: packets come from ReleaseSource; 7daystodiemods section 4 is HTML for the Import tab; Nexus source is BBCode; the tractor zip (`zzzAGF-V3-CLIENT-LawnTractorV3Fix` plus `0AGF-V3-CLIENT-LawnTractorPatchGuard`) is two folders in one download and has no packet yet.
- [ ] **4. Fix that packet, then 7daystodiemods.** Get the example packet right, then see what the site fill does with it. Last logged create reached the publish glance. The live page layout has not been rechecked since then.
- [ ] **5. Nexus, including Playwright.** Browser fill for a new page and for the description and images the API cannot write. The API can still push a zip and changelog onto a page that already exists.
- [ ] **6. Next after both sites.** Decide when step 5 is done. Open items already noted: remaining mods after the example, and the tractor companion listing.

## Current

**Step 1.** Finalize the naming theme. Display name: `AGF - V# - SCOPE - ModName`, with no load-order prefix. Folder includes `V#` and ends with `-vX.Y.Z`. Casual fixes use SERVER, CLIENT, or BOTH. Overhaul-only fixes use ADMIN. COMPAT is in the theme. The compatibility split is in ActiveBuild. COMPAT ModInfo Name is `AGF-COMPAT-` plus the token. COMPAT uses five `z`s. Every other late mod uses three. LocalizationPatches is split into BDubVehicles, IZYWeapons, and GSVanillaCookBook in ActiveBuild. The folder and ModInfo rename is applied in ActiveBuild and the scoped Draft mods. TerrainLeveler is TBD and was not renamed. The other Draft folders were not renamed. ReleaseSource and the game Mods folder were not renamed. WMM12SlotToolbelt display name stays `AGF COMPAT WMM12SlotToolbelt`. VanillaExtended food patches are in GSVanillaCookBook.
