============================================================
  AGF MOD BANNER IMAGE SYSTEM
============================================================

FOLDER LAYOUT
-------------
00_Images/
    banner-layout.json          <- Coordinate/font/color preset used by the generator.

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
        AGF-HUDPlus-1Main_banner.png      <- Composite: template + mod image + text (820x461)
        AGF-HUDPlus-1Main_preview_1.png   <- Scaled source image 1 (820x461)
        AGF-HUDPlus-1Main_preview_2.png   <- Scaled source image 2 (820x461, if exists)
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
For each mod that has a source/ subfolder:

  1. banner       - template frame composited with 1.png + mod name/version/description
                    text drawn over it. Used as the main mod header image in the README.

  2. preview_N    - each source image scaled down to 820x461 for inline display.
                    Linked to the full 1920x1080 original so users can click to expand.

Mods with NO source subfolder are skipped — their README entry renders as text only,
exactly as it does today.


CLICK TO ENLARGE (how it works in GitHub)
------------------------------------------
The README will render each image like this:

    [![description](00_Images/_generated/AGF-HUDPlus-1Main_preview_1.png)](00_Images/source/AGF-HUDPlus-1Main/1.png)

Clicking the thumbnail opens the full 1920x1080 image in the browser.
The browser back button returns to the README page.


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
