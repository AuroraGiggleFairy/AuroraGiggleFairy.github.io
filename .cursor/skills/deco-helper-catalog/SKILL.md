---
name: deco-helper-catalog
description: >-
  Batch-edit Decoration Block helper_categories.csv, deco-helper-block-list canvas,
  English localization names, parent/child tags, and SortOrder1. Use when the user
  recategorizes helper blocks, adds a Category 1/2 (Old West, Store/Coolers),
  changes sort ranges like 92-117, or renames helper localization / EnglishName.
---

# Deco helper catalog edits

Do **not** read `deco-helper-block-list.canvas.tsx` (2MB icons). Do **not** run `generate_deco_helper_canvas.py` (re-encodes every icon, minutes).

CSV is **not** the live working list. The canvas (`deco-helper-block-list`) is the sorting source of truth until the user asks to bake into CSV.

`00_DLL-Projects/Generators/DecorationBlock/helper_categories.csv`

Apply **all** items in the user message in **one** ops file and **one** script run.

## Steps

1. Write a small JSON file (e.g. `00_DLL-Projects/Generators/DecorationBlock/_ops.json`).
2. Run:

```
python 00_DLL-Projects/Generators/DecorationBlock/patch_helper_catalog.py 00_DLL-Projects/Generators/DecorationBlock/_ops.json
```

3. Delete `_ops.json`.
4. Before any catalog-wide CSV write, merge `edits_v1` from `deco-helper-block-list.canvas.data.json` (cat1/cat2/sort overlays). Canvas edits are the working list until the user asks to bake.
5. **Never** bump the canvas edits state key (`edits_v1`). Bumping it hides the user's Category 1/2 and sort work.
6. The canvas **Sort** button regroups by Category 1, then Category 2, then current order, and rewrites SortOrder1 1…N in `edits_v1`. Category 1/2 stay. Do not run a CSV-only reorder that ignores canvas overlays.
7. After the script write, header-only StrReplace on the `.canvas.tsx` so Cursor shows the Open chip above chat. Do not markdown-link the canvas. Do not Read the 2MB file.

`--dry-run` first only if the range/filter is ambiguous.

## Ops JSON

```json
{ "ops": [ { "op": "set_cat", "filter_parent": "Kitchen", "from": 92, "to": 117, "parent": "Store", "child": "Coolers", "needles": ["cntfreezer", "icemachine"] } ] }
```

| op | Fields |
|----|--------|
| `add_parent` | `name`, optional `after`, optional `children` [] |
| `add_child` | `parent`, `name` |
| `set_cat` | `parent`, `child`; select with `ids` [] **or** `filter_parent` / `filter_child` + `from`/`to` (1-based in that filtered list) **or** `family` **or** `name_contains`; optional `needles` [] prepends LOOK_RULES |
| `rename` | `english` + same selectors as `set_cat` (usually one `id`). Keeps `[hex](Kind)[-]` unless the new string already has tags. English loc only. |
| `set_kind` | `kind` (`loot` / `plain` / `campfire` / …) + same selectors as `set_cat`. Rewrites the English kind tag and the CSV `Kind` column. |
| `apply_excel_support` | `xlsx` path. Excel row order + Category/Category2 for **existing helper rows only**. Does not add Excel-only blocks. Campfire twins stay first (`Kitchen/Campfires`). v3-only items keep current cats and follow same-family Excel items. Writes `helper_look.csv` + canvas `edits_v1`. Do **not** click canvas Sort after (that regroups by category). |

`from`/`to` are **row numbers in the current filtered list**, not raw `00092` unless they said catalog/unfiltered (omit `filter_parent`).

If they clearly mean a family (all Wild West coffins, all `cntFreezer*`), use `ids` or `family` / `name_contains` and keep the rest of the family even if the number range clipped it.

New Category 1: `add_parent` before `set_cat`. New Category 2: `add_child` or let `set_cat` append the child.

## Do not

- Edit ModInfo / README
- Translate non-English loc unless asked (then follow localization-quality)
- Touch ICONS / regenerate the canvas
- One-off Python that rewrites the canvas by regex
- Explore taxonomy/canvas source to “discover” paths; use this script

## After

One sentence: what moved/renamed, where to click, reopen canvas.
