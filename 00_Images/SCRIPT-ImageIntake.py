import io
import os
import re
import subprocess
import sys
import argparse
from pathlib import Path
from typing import Optional, Tuple

try:
    from PIL import Image, ImageGrab
except Exception as ex:
    print("Pillow is required. Install with: pip install pillow")
    print(f"Import error: {ex}")
    sys.exit(1)


IMAGES_ROOT = Path(__file__).resolve().parent
VS_CODE_ROOT = IMAGES_ROOT.parent
IMAGE_WORKFLOW_ROOT = IMAGES_ROOT / "01_ImageWorkflow"
PRIMARY_IMAGE_SOURCES_ROOT = IMAGE_WORKFLOW_ROOT / "PrimaryImageSources"
FINAL_IMAGES_ROOT = IMAGES_ROOT / "02_ImagesFinal"
GENERATE_SCRIPT = VS_CODE_ROOT / "Workflow" / "SCRIPT-GenerateModImages.py"

TARGET_W = 1920
TARGET_H = 1080
TARGET_RATIO = TARGET_W / TARGET_H
FULLSCREEN_RATIO_16_10 = 16 / 10


def find_latest_screenshot_path() -> Optional[Path]:
    candidate_dirs = []

    appdata = os.environ.get("APPDATA")
    if appdata:
        candidate_dirs.append(Path(appdata) / "7DaysToDie" / "Screenshots")

    user_profile = os.environ.get("USERPROFILE")
    if user_profile:
        candidate_dirs.append(Path(user_profile) / "Pictures" / "7 Days To Die")
        candidate_dirs.append(Path(user_profile) / "Pictures" / "7DaysToDie")

    latest_file: Optional[Path] = None
    latest_mtime = -1.0
    for folder in candidate_dirs:
        if not folder.is_dir():
            continue
        for ext in ("*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp"):
            for file in folder.glob(ext):
                try:
                    mtime = file.stat().st_mtime
                except OSError:
                    continue
                if mtime > latest_mtime:
                    latest_mtime = mtime
                    latest_file = file

    return latest_file


def resolve_auto_source_image(source_arg: str) -> Tuple[Optional[Image.Image], str]:
    arg = source_arg.strip()
    if arg != "":
        path = Path(strip_wrapping_quotes(arg))
        if not path.is_file():
            return None, f"Source image not found: {path}"
        return Image.open(path).convert("RGB"), str(path)

    try:
        clip = ImageGrab.grabclipboard()
    except Exception:
        clip = None

    if isinstance(clip, Image.Image):
        return clip.convert("RGB"), "Clipboard image"

    if isinstance(clip, list) and len(clip) > 0:
        first = Path(str(clip[0]))
        if first.is_file() and first.suffix.lower() in {".png", ".jpg", ".jpeg", ".bmp", ".webp"}:
            return Image.open(first).convert("RGB"), str(first)

    latest = find_latest_screenshot_path()
    if latest and latest.is_file():
        return Image.open(latest).convert("RGB"), str(latest)

    return None, (
        "No source image found. Copy an image to clipboard first, or ensure a screenshot exists in "
        "AppData\\Roaming\\7DaysToDie\\Screenshots / Pictures\\7 Days To Die."
    )


def strip_wrapping_quotes(value: str) -> str:
    text = value.strip()
    if len(text) >= 2 and text[0] == '"' and text[-1] == '"':
        return text[1:-1]
    return text


def prompt_text(prompt: str, default: Optional[str] = None) -> str:
    if default is not None and default != "":
        raw = input(f"{prompt} [{default}]: ").strip()
        return default if raw == "" else raw
    return input(f"{prompt}: ").strip()


def prompt_yes_no(prompt: str, default_yes: bool = True) -> bool:
    suffix = "[Y/n]" if default_yes else "[y/N]"
    raw = input(f"{prompt} {suffix}: ").strip().lower()
    if raw == "":
        return default_yes
    return raw in {"y", "yes"}


def sanitize_base_mod_name(raw_name: str) -> str:
    text = strip_wrapping_quotes(raw_name)
    text = text.strip()
    text = re.sub(r"-v\d+\.\d+(?:\.\d+)*$", "", text, flags=re.IGNORECASE)
    text = text.replace(" ", "")
    text = re.sub(r"[^A-Za-z0-9._-]", "", text)
    return text


def parse_slot(raw_slot: str) -> int:
    text = raw_slot.strip()
    if text == "":
        return 1
    try:
        value = int(text)
    except ValueError:
        return -1
    return value if value >= 1 else -1


def build_output_filename(base_name: str, slot: int) -> str:
    return f"{base_name}_{slot:02d}.png"


def choose_output_path(base_name: str, slot: int) -> Optional[Path]:
    while True:
        file_name = build_output_filename(base_name, slot)
        output_root = PRIMARY_IMAGE_SOURCES_ROOT if slot == 1 else FINAL_IMAGES_ROOT
        out_path = output_root / file_name
        if not out_path.exists():
            return out_path

        print(f"File already exists: {out_path}")
        choice = input("Choose [O]verwrite, [R]ename slot, or [C]ancel: ").strip().lower()
        if choice in {"o", "overwrite"}:
            return out_path
        if choice in {"r", "rename"}:
            next_slot = parse_slot(prompt_text("Enter slot number (01 for primary, 02+ for numbered)"))
            if next_slot == -1:
                print("Invalid slot number. Try again.")
                continue
            slot = next_slot
            continue
        if choice in {"c", "cancel"}:
            return None
        print("Invalid choice. Please type O, R, or C.")


def compute_16x9_crop_box(width: int, height: int) -> Tuple[int, int, int, int]:
    if width == 2560 and height == 1600:
        return 0, 80, width, height - 80

    ratio = width / height
    if abs(ratio - TARGET_RATIO) < 1e-6:
        return 0, 0, width, height

    if ratio < TARGET_RATIO:
        target_h = int(round(width / TARGET_RATIO))
        crop = max(0, height - target_h)
        top = crop // 2
        bottom = crop - top
        return 0, top, width, height - bottom

    target_w = int(round(height * TARGET_RATIO))
    crop = max(0, width - target_w)
    left = crop // 2
    right = crop - left
    return left, 0, width - right, height


def is_large_fullscreen_capture(width: int, height: int) -> bool:
    if width == 2560 and height == 1600:
        return True

    ratio = width / height
    return (width >= TARGET_W and height >= TARGET_H) and (
        abs(ratio - TARGET_RATIO) <= 0.01 or abs(ratio - FULLSCREEN_RATIO_16_10) <= 0.01
    )


def transform_image(img: Image.Image) -> Tuple[Image.Image, str]:

    if is_large_fullscreen_capture(img.width, img.height):
        crop_box = compute_16x9_crop_box(img.width, img.height)
        cropped = img.crop(crop_box)
        return cropped.resize((TARGET_W, TARGET_H), Image.Resampling.LANCZOS), "fullscreen"

    scale = min(TARGET_W / img.width, TARGET_H / img.height)
    new_w = max(1, int(round(img.width * scale)))
    new_h = max(1, int(round(img.height * scale)))
    resized = img.resize((new_w, new_h), Image.Resampling.LANCZOS)

    canvas = Image.new("RGB", (TARGET_W, TARGET_H), (0, 0, 0))
    x = (TARGET_W - new_w) // 2
    y = (TARGET_H - new_h) // 2
    canvas.paste(resized, (x, y))
    return canvas, "custom"


def save_png(image: Image.Image, destination: Path) -> float:
    buffer = io.BytesIO()
    image.save(buffer, format="PNG", optimize=True, compress_level=9)
    data = buffer.getvalue()
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(data)
    return len(data) / 1024.0


def run_merge_generation(base_name: str) -> int:
    if not GENERATE_SCRIPT.is_file():
        print(f"Merge generation script not found: {GENERATE_SCRIPT}")
        return 1

    cmd = [sys.executable, str(GENERATE_SCRIPT), "--mod", base_name]
    print(f"Running merge generation: {' '.join(cmd)}")
    return subprocess.run(cmd, check=False).returncode


def main() -> int:
    parser = argparse.ArgumentParser(add_help=False)
    parser.add_argument("source", nargs="?", default="")
    args, _unknown = parser.parse_known_args()

    print("AGF Image Intake")
    print("- Large full-screen capture: crop/fill to 16:9 then resize to 1920x1080")
    print("- Custom capture size: fit/center on black 1920x1080 canvas")
    print("- Slot 01 source folder: 00_Images/01_ImageWorkflow/PrimaryImageSources")
    print("- Slot 02+ final folder: 00_Images/02_ImagesFinal")
    print("- Source image auto order: clipboard image -> latest detected screenshot")
    print("- Slot 01 saves as the primary image (used by merged generation)")
    print("- Slots 02+ save as numbered images for additional media posts")
    print()

    source_image, source_label = resolve_auto_source_image(args.source if args.source else "")
    if source_image is None:
        print(source_label)
        return 1
    print(f"Auto source image: {source_label}")

    base_raw = prompt_text("Enter base mod name (example: AGF-NoEAC-ScreamerAlert)")
    base_name = sanitize_base_mod_name(base_raw)
    if base_name == "":
        print("Invalid mod name after sanitization.")
        return 1

    slot = parse_slot(prompt_text("Enter image slot number (01 = primary, 02+ = numbered)", "01"))
    if slot == -1:
        print("Invalid slot number.")
        return 1

    output_path = choose_output_path(base_name, slot)
    if output_path is None:
        print("Cancelled.")
        return 0

    try:
        final_img, mode = transform_image(source_image)
        size_kb = save_png(final_img, output_path)
    except Exception as ex:
        print(f"Image processing failed: {ex}")
        return 1

    print()
    print(f"Saved: {output_path}")
    if mode == "fullscreen":
        print("Transform mode: full-screen crop/fill")
    else:
        print("Transform mode: custom contain on black")
    print(f"Final dimensions: {TARGET_W}x{TARGET_H}")
    print(f"PNG file size: {size_kb:.1f} KB")

    if slot == 1:
        print("This is the primary media image used by merged banner generation.")
    else:
        print("This is a numbered extra media image for additional posts.")

    print()
    if slot == 1 and prompt_yes_no("Regenerate merged banner + thumbnail for this mod now?", default_yes=True):
        exit_code = run_merge_generation(base_name)
        if exit_code != 0:
            print(f"Merge generation exited with code {exit_code}")
            return exit_code
        print("Merge generation completed.")

    return 0


if __name__ == "__main__":
    sys.exit(main())
