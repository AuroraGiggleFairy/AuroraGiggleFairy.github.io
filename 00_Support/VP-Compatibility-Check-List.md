# VP Mods — Compatibility Check List

*Started: July 23, 2026*

Working list of Vanilla Plus (VP) mods to verify against current game files.
Statuses: `pending` · `in progress` · `pass` · `needs fix` · `blocked` · `Done`

Checked against: `...\7 Days To Die\Data\Config` (purple-book conditionals ignored).

Sorted by status, then short name.

---

## Queue

| # | Short name | Folder (01_Draft) | Status | Notes |
|---|------------|-------------------|--------|-------|
| 1 | AdminModdingSupport | AGF-VP-AdminModdingSupport-v1.0.5 | Done | MapColor fix. Block Replace: BurstRoundCount `0` (hold-fire), Action1 Delay `.1`, Action0 keeps vanilla `.83` (no shared RPM). Recopied. |
| 2 | AlternativeRecipes | AGF-VP-AlternativeRecipes-v1.0.2 | Done | Tested OK. |
| 3 | AmmoDisassembly | AGF-VP-AmmoDisassembly-v1.0.2 | Done | Bundle outputs matched to vanilla craft (base/x100/x1000). Keep scrap Weight 1:1 + vanilla scrap math as-is. Recopied. |
| 4 | ArcheryFeathersChange | AGF-VP-ArcheryFeathersChange-v1.0.3 | Done | Craft math OK. AmmoDisassembly conditional updated (fat/paper/yucca + feathers). Recopied. |
| 5 | DrinkableAcid | AGF-VP-DrinkableAcid-v2.0.2 | Done | Jar mesh. Water/fast/paint/jump/damage/trippy + dance via twitch_buffDance. Celebrate + Silly Sounds deferred (v3 sandbox/Twitch path unreliable). |
| 6 | DyesPlus | AGF-VP-DyesPlus-v3.1.1 | Done | Invisible dye commented out (no current purpose). Corrected inside comment to nested `property class="UMA"` (Mesh / ShowAltHair). Recipe commented too. Recopied to game Mods. |
| 7 | MiningPlus | AGF-VP-MiningPlus-v1.2.1 | Done | Brass removed. Clay/sand bundles + sand harvest. Purple Book inserts: Unlocks Misc overview (`unlockAllSectionGrid5`) + zoomed (`unlockMiscGrid`) after oil shale. Copied to game Mods. |
| 8 | PaintbrushPlus | AGF-VP-PaintbrushPlus-v2.1.1 | Done | Hold-paint: BurstRoundCount `0` + RPM `200` (Delay obsolete when RPM set). Recopied. |
| 9 | RecipeRottingFlesh | AGF-VP-RecipeRottingFlesh-v1.0.1 | Done | Tested OK. Copied to game Mods. |
| 10 | TacticalRiflePlus | AGF-VP-TacticalRiflePlus-v2.1.1 | Done | BurstRoundCount `1000`→`0` for full-auto (no 3-burst). Mag 36 unchanged. Recopied. |
| 11 | BuyTraderVendingMachines | AGF-VP-BuyTraderVendingMachines-v3.0.3 | pass | Ready for testing. Copied to game Mods. |
| 12 | CraftStackEngBattCells | AGF-VP-CraftStackEngBattCells-v3.3.1 | pass | Leave as-is for now (Draft only). Paths look OK; test with DoorsPlus later. |
| 13 | DoorsPlus | AGF-VP-DoorsPlus-v3.0.1 | pass | Leave as-is for now (Draft only). |
| 14 | SmeltingPlus | AGF-VP-SmeltingPlus-v2.4.1 | pass | Fixed forge templates xpaths: `/controls`→`/templates`, `@columns`→`@cols`. Recopied to game Mods. Confirm 3rd forge slot + material row layout. |

---

## Session log

- **2026-07-23** — List created from user queue (14 mods). All under `01_Draft`.
- **2026-07-23** — XPath/name check vs current vanilla Config. MiningPlus needs brass-bundle fix later.
- **2026-07-23** — Copied 11 mods into game `Mods` (Draft kept). Left CraftStack, DoorsPlus, MiningPlus in Draft only.
- **2026-07-23** — AdminModdingSupport: `Map.Color` → `MapColor` (blocks.xml load fail); Draft + game Mods updated.
- **2026-07-23** — DyesPlus: invisible dye commented out; UMA props corrected to nested class form inside comment; recipe commented; Draft + game Mods updated.
- **2026-07-23** — SmeltingPlus: forge templates patches updated for `<templates>` root + `cols` attribute; Draft + game Mods updated.
- **2026-07-23** — AdminModdingSupport + PaintbrushPlus: hold-to-use updated to BurstRoundCount `0` + RoundsPerMinute (old `1000` no longer means continuous fire).
- **2026-07-23** — TacticalRiflePlus: BurstRoundCount `1000`→`0` for full-auto.
- **2026-07-23** — AlternativeRecipes marked Done. AdminModdingSupport: reverted LMB/Action0 speed + full-auto; Action1 Delay `.1` only.
- **2026-07-23** — AmmoDisassembly: fixed missing/wrong OpenBundle ingredients vs vanilla; scrap ratio left as-is (vanilla math). AdminModdingSupport marked Done (hold-fire BurstRoundCount `0` + Action1 Delay `.1`).
