# 7DaysToDieMods publishing

This folder is the 7DaysToDieMods command center.

There is no public creator write API. Login is session + captcha — do not script it.
Official version updates are GitHub Release sync (one satellite repo per listing).

## Look here

| File | What it is |
|---|---|
| `PublishHelp/` | One generated `{ModName}.md` packet per ReleaseSource mod |
| `RUN-7DaysToDieMods-Update.bat` | Create missing satellite repos and missing GitHub Releases |
| `Workflow/7dtdmods-config.json` | Per-mod overrides (title, category, site id, last tag) |

## Typical flow

1. **Publish** (`RUN-Publish.bat`) regenerates `PublishHelp/*.md` and creates any missing empty satellite repos.
2. For a **new listing**: paste the packet on the site, upload the first zip once, connect `AuroraGiggleFairy/{ModName}`, then turn on Auto-sync / Auto-publish / Replace.
3. For **later versions**: `RUN-7DaysToDieMods-Update.bat` creates the GitHub Release from `04_DownloadZips\{Mod}.zip`. The site pulls version, changelog, and zip. It does **not** refresh description, images, title, or file Label/Description.

Category / GigglePack zips do not get satellite repos. Releases are not created on every git push.

## Behind the scenes

Everything under `Workflow/` — generator, update script, config.

GitHub CLI (`gh`) must be logged in as `AuroraGiggleFairy` for repo/Release create.
