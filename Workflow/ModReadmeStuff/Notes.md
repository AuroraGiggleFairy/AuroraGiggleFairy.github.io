# ModReadmeStuff — Notes & Discussion

This file is the canonical discussion and tracking space for the `ModReadmeStuff` work.

Guiding rules:
- I will not make further changes or act beyond creating this file until you give clear expectations.
- Use this file to state priorities, desired platforms, and any required sections or examples.

Suggested conversation starters (pick any or add your own):
1. Which platforms should we prioritize? (e.g., NexusMods, GitHub, Steam Workshop, ModDB)
2. Which sections must appear in every mod README? (short description, install, compatibility, changelog, credits, known issues, screenshots)
3. Do you want per-platform snippets (short vs. long descriptions) included in the same README or as separate files?
4. Any existing README examples I should base templates on? Provide paths or paste content.

Tracking / Log format
- Date — Author: Short note about action or decision

Example entry:
- 2026-06-17 — GitHub Copilot: Created this `Notes.md` as requested. Waiting for your expectations.

Next step (waiting on you):
- Tell me the expectations and priorities you want me to ask about or act on.


## Template Table of Contents (source: `Workflow/ReadmeSystem/Templates/TEMPLATE-ModReadMes.md`)
This is the canonical section order used by the template — we'll discuss reordering or consolidating these.

1. About Author
2. Mod Philosophy
3. Need Help?
4. Mod Type
5. Compatibility
6. Features Summary
7. Features Details
8. Changelog
9. Important Mod Details
10. Installation Guide
11. Removal Guide
12. Update Guide
13. Backup Guide

- 2026-06-17 — GitHub Copilot: Added template Table of Contents to aid discussion.


## Discussion: About/Philosophy placement

- Observation: `About Author` and `Mod Philosophy` function as an "About Me" section. They are useful, but may not be the first thing readers want when opening a mod README.
- Recommendation: Move author/philosophy later in the README (near Credits/Support) and lead with user-focused items: short description, compatibility, installation, quick-start, and features.


## Proposed Reordered Table of Contents (for discussion)
1. Short summary (one-line) & key badges/links
2. Quick compatibility summary (tested game version, EAC/required installs)
3. Quick install (one-paragraph steps and notable warnings)
4. Features Summary (bullet list) + screenshots
5. What changed (Changelog summary or latest highlights)
6. Detailed features (expanded details)
7. Known issues / Caveats / Removal notes
8. Full Installation / Update / Backup / Removal guides
9. Credits & Support (About Author, Mod Philosophy, Need Help?)
10. Technical details & compatibility table
11. Localization / Additional notes

- 2026-06-17 — GitHub Copilot: Added discussion and proposed reordered TOC. Reply with changes or approve and I'll update `TEMPLATE-ModReadMes.md` draft accordingly.
 

## Suggested order and what each section contains
Purpose: a clear, user-first layout and short description for each section.

1. Header — One-line summary & key links/badges: what the mod does and where to get it.
2. Compatibility (at-a-glance) — Tested game version, EAC/mod-type, `SAFE_TO_INSTALL`/`SAFE_TO_REMOVE` quick flags.
3. Quick Install — 1–2 step install + immediate warnings (backup/harmony requirement). Keep extremely short.
4. Features Summary — short bullet list of what users will notice immediately.
5. Changelog (latest) — one-line highlights for the current version.
6. Detailed Features — expanded descriptions, usage notes, examples.
7. Known Issues / Caveats / Removal Notes — important gotchas and safe removal guidance.
8. Full Guides — full Installation, Update, Backup, Removal steps (reference material).
9. Technical Details & Compatibility Table — `MOD_TYPE_ID`, dependencies, localization file names, CSV fields mapping.
10. Localization / Files — how to edit strings, where localization lives.
11. Support & Credits — `About Author`, `Mod Philosophy`, contact links, where to report issues.
12. Per-platform snippets — labeled copy-paste blocks for Nexus/GitHub/Steam descriptions (short vs long).
13. Automation & Markers — preserve `<!-- FEATURES-SUMMARY -->`, `<!-- FEATURES-DETAILED -->`, `<!-- CHANGELOG -->` for tooling.

Note: do not embed images in README files — link to an external gallery or GitHub-hosted images instead.

- 2026-06-17 — GitHub Copilot: Added suggested order and brief content descriptions.
