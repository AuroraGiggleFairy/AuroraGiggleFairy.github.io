# ESCWindowPlus Single-Source Workflow (v1.1)

This adds a simple one-source editing model for content.

## Files
- Source config: ESCMenu.source.json
- Generator script: SCRIPT-GenerateESCMenu.py
- Generated output root: Config/Generated

## What gets generated
- Config/Generated/Localization.ESC.Generated.txt
- Config/Generated/News/*.xml
- Config/Generated/XUi.Tabs.Generated.xml
- Config/Generated/ESC.Generated.Manifest.txt

## Run
From this mod folder:

python SCRIPT-GenerateESCMenu.py

Optional custom paths:

python SCRIPT-GenerateESCMenu.py --config ESCMenu.source.json --out-dir Config/Generated

Merge generated keys into the main localization file:

python SCRIPT-GenerateESCMenu.py --merge-localization

By default, merge also removes obsolete old-generation ESC keys that were previously generated but are no longer emitted.

Keep obsolete generated keys if you need a temporary compatibility pass:

python SCRIPT-GenerateESCMenu.py --merge-localization --keep-obsolete-generated-keys

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

## Easy text editing workflow
Users can edit localization copy in one text-only file and let the generator handle key placement, color tags, HC variants, and output formatting.

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

Tabs can be added/removed directly by adding/removing page blocks in your texts file.
Generator enforces the max tab count that fits your current layout.

`links` format mirrors page-style indexing:
- `id`: link number (1-based, contiguous)
- `label`
- `url`

Links can be added/removed directly by adding/removing lines in `# Links`.
Generator enforces a links fit limit (`ui.layout.maxLinks`, default 4).

Links format:
- `id | label | url`
- Example: `1 | Join Discord | https://discord.gg/yourInvite`

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

Use temporary High Contrast preview overrides while tuning:

python SCRIPT-GenerateESCMenu.py --use-hc-dev-overrides

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
- `windowESC_highcontrast_page_1_button`
- `windowESC_highcontrast_page_1_title`
- `windowESC_highcontrast_page_1_body`

Header and link keys are also generated in the same namespace:
- `windowESC_color_header_title`
- `windowESC_color_header_motto`
- `windowESC_color_link_1`
- `windowESC_highcontrast_header_title`
- `windowESC_highcontrast_header_motto`
- `windowESC_highcontrast_link_1`

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

In `XUi.Tabs.Generated.xml`, generated tab title/body labels now use `color=""` so localization inline colors drive text color.
Sprite/button/background color attributes are still emitted normally.

In generated localization, data-row English values are wrapped in double quotes for safer comma handling.

High Contrast is now automatic and is not user-configurable. The generator derives HC values from your normal colors and flattens them into black/white-safe output for readability.

```json
"themes": {
	"color": {
		"headerBackground": "95,89,128",
		"pageBackground": "95,89,128",
		"headerTitleColor": "141,181,128",
		"headerMottoColor": "221,205,250",
		"tabButtonSelectedColor": "74,33,150",
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
- `tabButtonSelected`: selected tab button color
- `pageTitle`: tab page title color
- `pageBody`: tab body text color
- `textBolding`: optional standout color for emphasis in your text
- `textHighlight`: optional high-contrast text highlight color (usually white)

Automatic HC behavior:
- Header/body backgrounds are auto-converted to grayscale (using the source color luminance).
- Text-like colors are then set to RGB black/white (`0,0,0` or `255,255,255`) from those grayscale backgrounds for readability.
- Selected tab color is auto-set to grayscale opposite the tab title color.
- Any `themes.highContrast` object is ignored if present.

## Temporary HC tuning workspace (dev only)
If you want to experiment while working, use `dev.highContrastPreview` in ESCMenu.source.json.

Example:

```json
"dev": {
	"highContrastPreview": {
		"headerBackground": "64,64,64",
		"pageBackground": "48,48,48",
		"headerTitleColor": "255,255,255",
		"headerMottoColor": "255,255,255",
		"tabButtonSelectedColor": "220,220,220",
		"pageTitleColor": "0,0,0",
		"pageBodyColor": "255,255,255",
		"textBoldingColor": "255,255,255",
		"textHighlightColor": "255,255,255"
	}
}
```

Important:
- These values are only used when running with `--use-hc-dev-overrides`.
- Normal runs keep automatic HC behavior.
- This keeps HC non-configurable for end users while still allowing your local iteration.
- Dev preview colors are RGB-only, same as `themes.color`.

## Link control
People can decide exactly what links to use by editing the `links` array in ESCMenu.source.json.

- Add a link object to include it.
- Remove a link object to stop generating it.
- Set `"enabled": false` to keep it in the file but disable generation/update.
- Add `legacySources` when you want updater replacement from old source strings.

## Integration note
This v1 does not auto-edit your XUi windows files.

It now generates a ready tab layout snippet file:

Config/Generated/XUi.Tabs.Generated.xml

Use that snippet to update tab button grid and tab page blocks in windows.xml when changing tab count/layout.

To use generated links in XUi, use the copy/paste mappings listed in:

Config/Generated/ESC.Generated.Manifest.txt

Or use --update-windows-sources to auto-update the active Discord source entries.

If your old sources are custom, add optional `legacySources` to each link in ESCMenu.source.json so updater knows what to replace.

Example link object:

{
	"id": "shop",
	"enabled": true,
	"title": "Server Shop",
	"url": "https://example.com/shop",
	"legacySources": [
		"https://raw.githubusercontent.com/olebem92/InfoPanelLinks/refs/heads/main/WitheredShop1"
	]
}

## Starter link templates
Copy/paste these into the `links` array in ESCMenu.source.json and keep only what you want.

```json
[
	{
		"id": "discord",
		"enabled": true,
		"title": "Join Discord",
		"url": "https://discord.gg/REPLACE_ME",
		"legacySources": [
			"https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/refs/heads/main/AGFDiscord.txt",
			"https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/refs/heads/main/DiscordLinkHells.txt",
			"https://raw.githubusercontent.com/olebem92/InfoPanelLinks/refs/heads/main/WitheredDiscord.xml"
		]
	},
	{
		"id": "shop",
		"enabled": true,
		"title": "Server Shop",
		"url": "https://example.com/shop",
		"legacySources": [
			"https://raw.githubusercontent.com/olebem92/InfoPanelLinks/refs/heads/main/WitheredShop1"
		]
	},
	{
		"id": "vote",
		"enabled": true,
		"title": "Vote",
		"url": "https://example.com/vote",
		"legacySources": [
			"https://raw.githubusercontent.com/olebem92/InfoPanelLinks/refs/heads/main/WitheredVoteSite"
		]
	},
	{
		"id": "map",
		"enabled": true,
		"title": "Live Map",
		"url": "https://example.com/map",
		"legacySources": [
			"https://raw.githubusercontent.com/olebem92/InfoPanelLinks/refs/heads/main/WitheredLiveMap"
		]
	}
]
```

Tip: set `"enabled": false` on any link you want to keep in JSON but disable for generation/update.

Example source string format:

@AGF-4Modders-ESCWindowPlus-v0.0.1:Config/Generated/News/discord.xml

To use generated text, either:
- run with --merge-localization, or
- manually merge keys from Localization.ESC.Generated.txt into Config/Localization.txt.

## Next step ideas
- Add optional generation of TabSelector blocks based on tab count.
- Add theme token replacement for color tags in long body text.
