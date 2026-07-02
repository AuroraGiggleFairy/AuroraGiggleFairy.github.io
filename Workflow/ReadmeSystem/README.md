# README System

This folder centralizes README authoring assets for the workspace.

## Source of truth

- Main site README template:
  - `Templates/TEMPLATE-MainReadMe.md`
- Individual mod README template:
  - `Templates/TEMPLATE-ModReadMes.md`
- Main README section partials:
  - `Templates/TEMPLATE-MainReadMe-0GigglePack`
  - `Templates/TEMPLATE-MainReadMe-1ModCategory`
  - `Templates/TEMPLATE-MainReadMe-2ModEntry`
- Category descriptions:
  - `Templates/TEMPLATE-CategoryDescriptions.md`
- Compatibility metadata CSV:
  - `Data/HELPER_ModCompatibility.csv`
- Canonical guide snippets:
  - `Snippets/ABOUTME-Guide.md`
  - `Snippets/ABOUTME-Main-Guide.md`
  - `Snippets/MODTYPE-Guide.md`
  - `Snippets/INSTALL-Guide.md`
  - `Snippets/REMOVAL-Guide.md`
  - `Snippets/UPDATE-Guide.md`
  - `Snippets/BACKUP-Guide.md`

## Notes

- The publish-facing `README.md` remains in workspace root.
- Runtime scripts now read templates from this folder.
- Mod type wording for generated README entries is now sourced from `Snippets/MODTYPE-Guide.md` (Mod Types table).
- About Author and Mod Philosophy block in mod README output is now sourced from `Snippets/ABOUTME-Guide.md`.
- Main-page About block is sourced from `Snippets/ABOUTME-Main-Guide.md`.
