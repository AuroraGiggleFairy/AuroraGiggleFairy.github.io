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
  - `Snippets/ModReadme-ABOUTME-md-Snippet.md`
  - `Snippets/MainReadme-1-ABOUTME-md-Snippet.md` (main README About section; includes short lines + extra-details dropdown)
  - `Snippets/MainReadme-DONATION-md-Snippet.md`
  - `Snippets/MainReadme-LANGUAGE-md-Snippet.md` (main README collapsible language support section)
  - `Snippets/MainReadme-3-ASKFORHELP-md-Snippet.md` (main README ask-for-help body content)
  - `Snippets/MainReadme-2-MODGUIDE-md-Snippet.md` (main README collapsible AGF guide; excludes support section)
  - `Snippets/ModReadme-EAC-md-Snippet.md`
  - `Snippets/ModReadme-MODGUIDE-txt-Snippet.txt` (preferred source for per-mod README.txt quick guide block)
  - `Snippets/ModReadme-MODSCOPE-md-Snippet.md`
  - `Snippets/ModReadme-MODTYPE-md-Snippet.md` (legacy fallback)
  - `Snippets/ModReadme-MODTYPE-txt-Snippet.txt`
  - `Snippets/MainReadme-MODTYPE-md-Snippet.md` (main README mod-type guide + mod card type wording)
  - `Snippets/MainReadme-4-SUPPORT-md-Snippet.md` (main README collapsible support section)
  - `Snippets/LongGuides/ModReadme-INSTALL-md-Snippet.md`
  - `Snippets/LongGuides/ModReadme-REMOVAL-md-Snippet.md`
  - `Snippets/LongGuides/ModReadme-UPDATE-md-Snippet.md`
  - `Snippets/LongGuides/ModReadme-BACKUP-md-Snippet.md`

## Notes

- The publish-facing `README.md` remains in workspace root.
- Runtime scripts now read templates from this folder.
- Main README sections 1-4 are snippet-driven for display formatting/content; template sections call snippet placeholders.
- Mod type wording for generated per-mod MOD SCOPE blocks is sourced from `Snippets/ModReadme-MODTYPE-txt-Snippet.txt`.
- Main README mod-type guide body and main mod-card type wording are sourced from `Snippets/MainReadme-MODTYPE-md-Snippet.md`.
- About Author and Mod Philosophy block in mod README output is now sourced from `Snippets/ModReadme-ABOUTME-md-Snippet.md`.
- Main-page About block is sourced from `Snippets/MainReadme-1-ABOUTME-md-Snippet.md`.
