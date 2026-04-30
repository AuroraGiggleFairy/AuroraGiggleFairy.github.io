# ESCWindowPlus Single-Source Workflow (v1.1)

This adds a simple one-source editing model for content.

## Files
- Source config: ESCMenu.source.json
- Generator script: SCRIPT-GenerateESCMenu.py
- Generated outputs: Config + Links

## What gets generated
- Config/Localization.txt
- Config/XUi/windows.xml
- Config/XUi/xui.xml
- Links/*.xml

## Run
From this mod folder:

python SCRIPT-GenerateESCMenu.py

## Non-Technical Quick Start
This is the simplest workflow for people who do not want to use command flags.

1) Open this folder and double-click:
- RUN-Easy-GenerateESCWindow.bat

2) First run behavior:
- If ESCMenu.texts.txt does not exist yet, the script creates it and stops.
- Edit ESCMenu.texts.txt with your own title/motto/pages/links.

3) Generate your window:
- Double-click RUN-Easy-GenerateESCWindow.bat again.
- This automatically runs full generation with:
	- localization merge
	- windows.xml source update for all links
	- windows.xml tab/link layout update

Optional helper:
- RUN-CreateESCTextTemplate.bat only creates the starter ESCMenu.texts.txt file.

Optional custom paths:

python SCRIPT-GenerateESCMenu.py --config ESCMenu.source.json

Merge generated keys into the main localization file:

python SCRIPT-GenerateESCMenu.py --merge-localization

By default, merge removes obsolete generator-managed ESC keys that are no longer emitted.

Merge into a custom localization target:

python SCRIPT-GenerateESCMenu.py --merge-localization --localization-file Config/Localization.txt

Update active NewsWindow sources in windows.xml to local generated path:

python SCRIPT-GenerateESCMenu.py --update-windows-sources

Update sources for all link IDs in ESCMenu.source.json (discord/shop/vote/map/etc):

python SCRIPT-GenerateESCMenu.py --update-windows-sources --windows-update-all-links

Run full v1.1 flow in one command:

python SCRIPT-GenerateESCMenu.py --merge-localization --update-windows-sources

Run full multi-link flow:

python SCRIPT-GenerateESCMenu.py --merge-localization --update-windows-sources --windows-update-all-links

Run non-technical full flow in one command:

python SCRIPT-GenerateESCMenu.py --easy

## Easy text editing workflow
Users can edit localization copy in one text-only file and let the generator handle key placement, color tags, HV variants, and output formatting.

Create starter text file from current source:

python SCRIPT-GenerateESCMenu.py --init-texts-file

This creates `ESCMenu.texts.txt` with only editable text fields (`colors` + `headers` + `pages` + `links`).

Supported text file format:
- `.txt` (plain-text workflow)

In `.txt`, color scheme is editable at the top in `# Colors`.
Use RGB values like `141,181,128`.

`pages` format is intentionally simple:
- `id`: page number (1-based)
- `title`
- `titleFontSize` (optional, per page, default 36)
- `bodyFontSize` (optional, per page, default 28)
- `bodyLines` (recommended): list of lines, no `\\n` typing needed
- `body` (legacy): plain string if you still prefer it
- `bodySourceUrl` (optional): pull body text from an online plain-text source
- `@enabledPages: N` controls how many pages are active (1-8), while keeping all 8 page slots editable

Optional body columns in a page (up to 3):
- Set `bodyColumns: 1`, `bodyColumns: 2`, or `bodyColumns: 3` inside each page block
- `autoSplitColumns: true|false` (default false)
  - When true and `bodyColumns` is 2 or 3, generator auto-splits Column 1 body text across selected columns using a fit-based heuristic.
  - This lets you author only one body block and still render multi-column pages.
- `scrollableBody: true|false` (default false)
	- When true, generated page bodies are wrapped in `on_scroll="true"` panels.
	- Works for both single-column and multi-column pages.
	- If you need longer or shorter scroll range, tune `PAGE_BODY_SCROLL_CONTENT_HEIGHT` in the generator script.
- Use explicit body sections with comment markers:
	- `<!-- Body Column 1 starts below this line -->` ... `<!-- Body Column 1 ends -->`
	- `<!-- Body Column 2 starts below this line -->` ... `<!-- Body Column 2 ends -->`
	- `<!-- Body Column 3 starts below this line -->` ... `<!-- Body Column 3 ends -->`
- Also supported (legacy shorthand): `@columns`, `@body1`, `@body2`, `@body3`
- Optional source URLs:
	- `bodySourceUrl: https://...` (column 1)
	- `body2SourceUrl: https://...` (column 2)
	- `body3SourceUrl: https://...` (column 3)

Suggested page block shape:

```text
titleFontSize: 36
bodyFontSize: 28
titleLabel: Welcome
bodyColumns: 3
autoSplitColumns: false
scrollableBody: false
<!-- Body Column 1 starts below this line -->
Column 1 text...
<!-- Body Column 1 ends -->
<!-- Body Column 2 starts below this line -->
Column 2 text...
<!-- Body Column 2 ends -->
<!-- Body Column 3 starts below this line -->
Column 3 text...
<!-- Body Column 3 ends -->
```

Tabs can be added/removed directly by adding/removing page blocks in your texts file.
Generator enforces the max tab count that fits your current layout.

`links` format mirrors page-style indexing:
- `id`: link number (1-based, contiguous)
- `label`
- `url` (final destination opened in browser)
- `@enabledLinks: N` controls how many links are active (0-5), while still keeping all 5 slots editable.
- `@linksClientOrServer: client|server`
- `@linksXMLWebBaseUrl: https://...` (required helper base for server mode)

Global UI toggles in `# Toggles`:
- `showOptionsSection: true|false`
- `showAdminVersion: true|false`
- `showServerJoinSection: true|false`

Toggle behavior:
- `showOptionsSection`: controls visibility of OPTIONS panel.
- `showAdminVersion`: controls visibility of `windowESCAdmin` if present.
- `@enabledLinks`: controls LINKS behavior. `0` disables links (no links panel + no link XML files). `1-5` enables links.
- `showServerJoinSection`: controls visibility of the dedicated JOIN section (`joining` + `Join`/`Decline` buttons).
	When enabled, server-join options-style controls are hidden so this area stays join/decline focused.

Links can be added/removed directly by adding/removing lines in `# Links`.
Generator enforces a links fit limit (`ui.layout.maxLinks`, default 4).

Links format:
- `@enabledLinks: 3`
- `@linksClientOrServer: client`
- `@linksXMLWebBaseUrl: https://example.com/7d2d-links`
- `id | label | url`
- Example (client mode): `1 | Join Discord | https://discord.gg/yourInvite`
- Example (server mode): `1 | Join Discord | https://discord.gg/yourInvite`

Source mode behavior:
- `client`: generator writes NewsWindow sources as `@modfolder:Links/<id>.xml`.
- `server`: generator auto-builds NewsWindow sources as `<@linksXMLWebBaseUrl>/<id>.xml`.
- `server` with blank `@linksXMLWebBaseUrl`: generator writes `sources=""` (buttons remain visible but disabled / non-clickable).

Body text convenience features:
- Line breaks without escapes: use `bodyLines` array (one entry per line).
- Inline bold color: wrap text in `**like this**` or `{{b:like this}}`.
- Inline highlight color: wrap text in `==like this==` or `{{h:like this}}`.
- Online source: set `bodySourceUrl` to a plain-text URL.
- Body source directive: `@bodySourceUrl: https://example.com/file.txt`

Example page entry:

{
	"id": 2,
	"title": "Rules",
	"bodyLines": [
		"**Core Rules**",
		"1. Respect builds.",
		"2. ==Be kind.=="
	]
}

Button text auto-matches `title` during generation.

`optionsTitle` is intentionally not exposed in `ESCMenu.texts.txt`; the Options section text is feature-owned. Keep options appearance configurable through color settings (panel background in `windows.xml`, title color through theme text color).

Then run normal generation as usual:

python SCRIPT-GenerateESCMenu.py

If `ESCMenu.texts.txt` exists, its values are auto-applied before generation.

Use a custom text file path if needed:

python SCRIPT-GenerateESCMenu.py --texts-file ESCMenu.texts.txt

Use temporary High Visibility preview overrides while tuning:

python SCRIPT-GenerateESCMenu.py --use-hv-dev-overrides

## Why this helps
- One source for header text, tab text, and link labels.
- One source for any links you want (Discord/shop/vote/map/custom) including URL.
- News XML files are generated locally, so users do not need to host online files.

## Naming scheme (v2)
Tabs now support stable ID-based naming.

In `tabs`, add an `id` per tab (recommended):

```json
{
	"id": "welcome",
	"button": "Welcome",
	"title": "Welcome!!!",
	"body": "Welcome text..."
}
```

Generator will produce canonical keys like:
- `windowESC_color_page_1_button`
- `windowESC_color_page_1_title`
- `windowESC_color_page_1_body`
- `windowESC_highvisibility_page_1_button`
- `windowESC_highvisibility_page_1_title`
- `windowESC_highvisibility_page_1_body`

Header and link keys are also generated in the same namespace:
- `windowESC_color_header_title`
- `windowESC_color_header_motto`
- `windowESC_color_link_1_label`
- `windowESC_highvisibility_header_title`
- `windowESC_highvisibility_header_motto`
- `windowESC_highvisibility_link_1_label`

## Tab count and max fit
Use `ui.layout` in ESCMenu.source.json to keep tab count easy but bounded by what fits.

Tab button count is taken directly from how many items are in `tabs`.

```json
"ui": {
	"layout": {
		"contentWidth": 1100,
		"contentPadding": 20,
		"buttonCellWidth": 150,
		"buttonHeight": 42,
		"buttonPosY": -20,
		"maxTabs": 7
	}
}
```

Rules:
- Generator computes `maxFitTabs` from width/padding/button width.
- Effective max is `min(maxTabs, maxFitTabs)`.
- If your tabs exceed this, generator stops with a clear error.
- Generated tab grid is centered horizontally from layout values and current tab count.

## Simple color model
Keep all colors in `themes`.

Use `themes.color` for normal mode.

Use RGB triplets only (for example `141,181,128`).

Header/motto text keys are emitted with inline color tags (for example `[8DB580]Twilight Realm[-]`) so color still applies even when a label color is not wired in windows.xml.

Separate `esc_color_*` localization rows are no longer generated.

Generated tab title/body labels in windows.xml use localization-driven inline colors.
Sprite/button/background color attributes are still emitted normally.

In generated localization, data-row English values are wrapped in double quotes for safer comma handling.

High Visibility is now automatic and is not user-configurable. The generator derives HV values from your normal colors and flattens them into black/white-safe output for readability.

```json
"themes": {
	"color": {
		"headerBackground": "95,89,128",
		"pageBackground": "95,89,128",
		"headerTitleColor": "141,181,128",
		"headerMottoColor": "221,205,250",
		"pageTabButtonSelectedColor": "74,33,150",
		"pageTitleColor": "141,181,128",
		"pageBodyColor": "221,205,250",
		"textBoldingColor": "141,181,128",
		"textHighlightColor": "255,255,255"
	}
}
```

Meaning:
- `headerBackground`: header area background fill
- `pageBackground`: main content area background fill
- `headerTitle`: header title color and your standout title color base
- `headerMotto`: header motto and generic body-text base
- `pageTabButtonSelected`: selected page tab button color
- `pageTitle`: tab page title color
- `pageBody`: tab body text color
- `textBolding`: optional standout color for emphasis in your text
- `textHighlight`: optional high-contrast text highlight color (usually white)

Automatic HV behavior:
- Header/body backgrounds are auto-converted to grayscale (using the source color luminance).
- Text-like colors are then set to RGB black/white (`0,0,0` or `255,255,255`) from those grayscale backgrounds for readability.
- Selected tab color is auto-set to grayscale opposite the tab title color.
- Any `themes.highVisibility` object is ignored if present.

## Temporary HV tuning workspace (dev only)
If you want to experiment while working, use `dev.highVisibilityPreview` in ESCMenu.source.json.

Example:

```json
"dev": {
	"highVisibilityPreview": {
		"headerBackground": "64,64,64",
		"pageBackground": "48,48,48",
		"headerTitleColor": "255,255,255",
		"headerMottoColor": "255,255,255",
		"pageTabButtonSelectedColor": "220,220,220",
		"pageTitleColor": "0,0,0",
		"pageBodyColor": "255,255,255",
		"textBoldingColor": "255,255,255",
		"textHighlightColor": "255,255,255"
	}
}
```

Important:
- These values are only used when running with `--use-hv-dev-overrides`.
- Normal runs keep automatic HV behavior.
- This keeps HV non-configurable for end users while still allowing your local iteration.
- Dev preview colors are RGB-only, same as `themes.color`.

## Link control
People can decide exactly what links to use by editing the `links` array in ESCMenu.source.json.

- Add a link object to include it.
- Remove a link object to stop generating it.
- Set `"enabled": false` to keep it in the file but disable generation/update.

## Integration note
This workflow auto-updates your primary outputs directly:
- Config/Localization.txt
- Config/XUi/windows.xml
- Config/XUi/xui.xml
- Links/*.xml

Use --update-windows-sources to normalize NewsWindow sources to local mod links.

Example link object:

{
	"id": "shop",
	"enabled": true,
	"title": "Server Shop",
	"url": "https://example.com/shop"
}

## Starter link templates
Copy/paste these into the `links` array in ESCMenu.source.json and keep only what you want.

```json
[
	{
		"id": "discord",
		"enabled": true,
		"title": "Join Discord",
		"url": "https://discord.gg/REPLACE_ME"
	},
	{
		"id": "shop",
		"enabled": true,
		"title": "Server Shop",
		"url": "https://example.com/shop"
	},
	{
		"id": "vote",
		"enabled": true,
		"title": "Vote",
		"url": "https://example.com/vote"
	},
	{
		"id": "map",
		"enabled": true,
		"title": "Live Map",
		"url": "https://example.com/map"
	}
]
```

Tip: set `"enabled": false` on any link you want to keep in JSON but disable for generation/update.

Example source string format:

@AGF-4Modders-ESCWindowPlus-v0.0.1:Links/discord.xml

To update generated text, run with --merge-localization (included in --easy).

## Next step ideas
- Add optional generation of TabSelector blocks based on tab count.
- Add theme token replacement for color tags in long body text.



