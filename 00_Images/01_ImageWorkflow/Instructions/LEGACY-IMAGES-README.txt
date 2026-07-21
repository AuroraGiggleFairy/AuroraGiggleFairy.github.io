============================================================
  AGF MOD BANNER IMAGE SYSTEM
============================================================

FOLDER LAYOUT
-------------
00_Images/
    modimage-layout.json        <- Coordinate/font/color preset used by the generator.

    _template-banner.png        <- YOU CREATE: the frame/background at 1920x1080
                                   This is the one shared template all banners use.

    _logo.png                   <- Optional logo added to top-left banner zone.

    _fonts/                     <- Optional: drop any .ttf font files here.
                                   If empty, the pipeline uses a system fallback.

    source/
        AGF-HUDPlus-1Main/      <- One subfolder per mod, named by BASE MOD NAME
            1.png               <- Primary mod screenshot/art (1920x1080)
            2.png               <- Optional second image
            3.png               <- Optional third image (etc.)
        AGF-VP-FarmingPlus/
            1.png
            ...

    _generated/                 <- AUTO-MANAGED by pipeline. Do not edit manually.
        AGF-HUDPlus-1Main_01.png          <- Full 1920x1080 composite: template + mod image + text
        AGF-HUDPlus-1Main_preview_1.png   <- Scaled source image 1
        AGF-HUDPlus-1Main_preview_2.png   <- Scaled source image 2 (if exists)
        ...


NAMING RULES
-------------
- Folder name  = base mod name, NO version suffix.
  Example: "AGF-HUDPlus-1Main"  (NOT "AGF-HUDPlus-1Main-v5.4.5")
- Source images = numbered 1.png, 2.png, 3.png inside the mod folder.
- The pipeline finds the base mod name automatically from the versioned folder.
- When a mod version changes, the banner regenerates automatically on next package run.


WHAT THE PIPELINE GENERATES
-----------------------------
For each mod that has a source image:

  1. _01.png     - Full 1920x1080 composite: template frame composited with the mod
                   screenshot + mod name/version/description/features text drawn over it.
                   GitHub README auto-scales this to fit the page for display.
                   Clicking the image opens the full 1920x1080 version.

  2. preview_N   - each source image scaled down for inline display (if max_previews > 0).

Mods with NO source image are still generated — they get a placeholder overlay on the
composite.


READEME IMAGE USAGE (how it works in GitHub)
---------------------------------------------
In the README, reference the _01.png directly in a clickable link:

    [![Mod Image](00_Images/_generated/AGF-ModName_01.png)](00_Images/_generated/AGF-ModName_01.png)

GitHub auto-scales the 1920x1080 image to fit the README column width.
Clicking the image opens it at full 1920x1080 resolution. Browser back returns to README.


GENERATION COMMANDS
-------------------
Generate banners and previews for all mods:

    python SCRIPT-GenerateModBanners.py

Dry-run what would be generated:

    python SCRIPT-GenerateModBanners.py --dry-run

Generate one mod only (base mod name):

    python SCRIPT-GenerateModBanners.py --mod AGF-HUDPlus-1Main

Run through your existing main script wrapper:

    python SCRIPT-Main.py --mode banners


TEMPLATE IMAGE TIPS
--------------------
- Size: 1920x1080 (16:9)
- Leave a clearly defined rectangular zone on one side for the mod screenshot.
- Leave a clearly defined text area for: mod name, version, description, features.
- Use solid or semi-transparent fill in the text area for readability.
- Save as PNG.
============================================================
