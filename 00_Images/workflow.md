# 00_Images Workflow

## Human Quick Guide

### Goal
- Keep source/post media at 1920x1080 PNG in mod-media.
- Keep merged GitHub thumbnail images in _generated.
- Keep naming consistent so update/publish scripts can use files automatically.

### One-Click Intake
1. Capture a game screenshot.
2. Run 00_Images/RUN-ImageIntake.bat.
3. Enter screenshot path.
4. Enter mod base name (no version suffix needed).
5. Enter slot number:
  - 01 = primary image
  - 02+ = numbered extra images
6. Confirm overwrite if asked.
7. Intake script processing:
  - Large full-screen captures: crop/fill to 16:9, then resize to 1920x1080.
  - Custom-size captures: place on a black 1920x1080 canvas, scaled to fit and centered.
  - Save PNG to 00_Images/mod-media.
8. Intake can immediately regenerate merged banner + thumbnail for that mod.

### Naming Contract
- Source media folder: 00_Images/mod-media
- Primary source image (used for merged banner):
  - AGF-ExampleMod_01.png
- Numbered extra source images:
  - AGF-ExampleMod_02.png
  - AGF-ExampleMod_03.png

### Merged Outputs (Generated)
- Folder: 00_Images/_generated
- GitHub thumbnail (used by README table):
  - Thumbnail_AGF-ExampleMod.png
  - Current size: 820x461
- Full merged image:
  - AGF-ExampleMod_01.png
  - Size: 1920x1080

### Which Image Should You Edit?
- Edit source media in 00_Images/mod-media.
- The generator reads the primary _01 media per mod and rebuilds merged outputs.

### Mod Folder Inclusion
- Pipeline copies mod image files into each mod subfolder:
  - AGF Mod Images (Not Required)
  - Full merged image: AGF-ExampleMod_01.png
  - Numbered extra images: AGF-ExampleMod_02.png, AGF-ExampleMod_03.png
- Thumbnails are not copied into mod folders:
  - Thumbnails stay in 00_Images/_generated only.
  - AGF-ExampleMod_01.png
  - AGF-ExampleMod_02.png
  - AGF-ExampleMod_03.png

---

## AI Operation Guide

### Pipeline Facts
- Primary media lookup uses base mod name in 00_Images/mod-media.
- Primary media lookup prefers <base>_01 naming in 00_Images/mod-media.
- Intake writes PNG source media using two-digit slot naming.
- Merged thumbnail naming is Thumbnail_<base>.png.
- Full merged naming is <base>_01.png.

### Deterministic Rules
1. Slot-01 media file for a mod is the authoritative source for merged generation.
2. Numbered files (_02, _03, ...) are extra media and not used as primary merge source.
3. README thumbnail references point to 00_Images/_generated/Thumbnail_<base>.png.
4. Keep mod-media files at 1920x1080 PNG with two-digit slot names.
5. In changed-only generation, never skip a mod when <base>_01.png is newer than Thumbnail_<base>.png; regenerate both together.

### Required Automation Behavior
1. Intake script must:
   - prompt for path, mod name, slot
  - for large full-screen images, crop/fill to 16:9 (2560x1600 => top 80 + bottom 80) then resize to 1920x1080
  - for custom-size images, fit/center on a black 1920x1080 canvas
  - save PNG output
   - prompt on overwrite
2. Generator must produce both:
   - thumbnail PNG for README
  - full merged PNG 1920x1080
  - changed-only mode must force regeneration when the full merged _01 is newer than its thumbnail
3. Engine sync step must copy to mod folders:
  - mod-media files matching base_<two_digit_number>
  - destination folder name: AGF Mod Images (Not Required)
  - must not copy thumbnails into mod folders

### File/Path Contract
- Intake script: 00_Images/SCRIPT-ImageIntake.py
- One-click launcher: 00_Images/RUN-ImageIntake.bat
- Generator: Workflow/SCRIPT-GenerateModImages.py
- Engine copy hooks: Workflow/05_pipeline_engine.py
