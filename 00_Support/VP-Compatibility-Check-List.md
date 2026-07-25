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
| 1 | BuyTraderVendingMachines | AGF-VP-BuyTraderVendingMachines-v3.0.3 | pass | Ready for testing. Copied to game Mods. |
| 2 | CraftStackEngBattCells | AGF-VP-CraftStackEngBattCells-v3.3.1 | pass | Leave as-is for now (Draft only). Paths look OK; test with DoorsPlus later. |
| 3 | DoorsPlus | AGF-VP-DoorsPlus-v3.0.1 | pass | Leave as-is for now (Draft only). |
| 4 | SmeltingPlus | AGF-VP-SmeltingPlus-v2.4.1 | pass | Fixed forge templates xpaths: `/controls`→`/templates`, `@columns`→`@cols`. Recopied to game Mods. Confirm 3rd forge slot + material row layout. |

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
- **2026-07-23** — Cleared 10 Done mods from queue (Admin through TacticalRiflePlus). Remaining: BuyTrader, CraftStack, DoorsPlus, SmeltingPlus.
