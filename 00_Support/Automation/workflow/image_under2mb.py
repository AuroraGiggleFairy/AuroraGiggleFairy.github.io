"""Write Under2MB copies of ImagesFinal _01 thumbnails. Never overwrite originals.

7DaysToDieMods.com rejects thumbnail uploads over 2MB. Full-quality _01 PNGs stay
in 00_Images/02_ImagesFinal. Gallery slots (_02+) are left alone. When _01 is
over 2 MiB, a condensed sibling is written as <stem>_Under2MB.png.
"""

from __future__ import annotations

import io
import os
import re
from typing import List, Optional, Tuple

try:
    from PIL import Image
except Exception as ex:
    raise SystemExit(f"Pillow is required for Under2MB copies: {ex}")


SITE_IMAGE_MAX_BYTES = 2 * 1024 * 1024
UNDER2MB_SUFFIX = "_Under2MB"
LEGACY_UNDER2MB_DIRNAME = "Under2MB"
SKIP_DIR_NAMES = {LEGACY_UNDER2MB_DIRNAME.lower(), "thumbnails"}
SLOT_01_PNG_RE = re.compile(r"_01\.png$", re.IGNORECASE)


def under2mb_path(src_path: str) -> str:
    root, ext = os.path.splitext(src_path)
    if root.endswith(UNDER2MB_SUFFIX):
        return src_path
    return f"{root}{UNDER2MB_SUFFIX}{ext}"


def is_under2mb_name(name: str) -> bool:
    stem, _ext = os.path.splitext(name)
    return stem.endswith(UNDER2MB_SUFFIX)


def is_final_01_png(name: str) -> bool:
    if not name.lower().endswith(".png"):
        return False
    if is_under2mb_name(name):
        return False
    if name.startswith("Thumbnail_") or "_preview_" in name:
        return False
    return bool(SLOT_01_PNG_RE.search(name))


def _png_bytes(image: Image.Image) -> bytes:
    buffer = io.BytesIO()
    image.save(buffer, format="PNG", optimize=True, compress_level=9)
    return buffer.getvalue()


def _condense_png_bytes(src_path: str, max_bytes: int) -> bytes:
    rgb = Image.open(src_path).convert("RGB")
    best = b""

    for scale in (0.96, 0.92, 0.88, 0.80):
        width = max(1, int(round(rgb.width * scale)))
        height = max(1, int(round(rgb.height * scale)))
        resized = rgb.resize((width, height), Image.Resampling.LANCZOS)
        data = _png_bytes(resized)
        if not best or len(data) < len(best):
            best = data
        if len(data) <= max_bytes:
            return data

    palette = rgb.quantize(
        colors=256,
        method=Image.Quantize.MEDIANCUT,
        dither=Image.Dither.FLOYDSTEINBERG,
    )
    data = _png_bytes(palette)
    if not best or len(data) < len(best):
        best = data
    if len(data) <= max_bytes:
        return data

    for scale in (0.85, 0.75, 0.60, 0.50):
        width = max(1, int(round(rgb.width * scale)))
        height = max(1, int(round(rgb.height * scale)))
        resized = rgb.resize((width, height), Image.Resampling.LANCZOS)
        quant = resized.quantize(
            colors=256,
            method=Image.Quantize.MEDIANCUT,
            dither=Image.Dither.FLOYDSTEINBERG,
        )
        data = _png_bytes(quant)
        if not best or len(data) < len(best):
            best = data
        if len(data) <= max_bytes:
            return data

    return best


def _remove_if_exists(path: str) -> None:
    if os.path.isfile(path):
        try:
            os.remove(path)
        except OSError:
            pass


def ensure_under2mb_copy(
    src_path: str,
    max_bytes: int = SITE_IMAGE_MAX_BYTES,
    dry_run: bool = False,
) -> Optional[str]:
    """Leave src untouched. Write or remove the <stem>_Under2MB.png sibling.

    Returns the condensed path when a copy is written (or would be written),
    otherwise None.
    """
    if not os.path.isfile(src_path) or not is_final_01_png(os.path.basename(src_path)):
        return None

    dest_path = under2mb_path(src_path)
    legacy_dir_copy = os.path.join(
        os.path.dirname(src_path), LEGACY_UNDER2MB_DIRNAME, os.path.basename(src_path)
    )
    src_size = os.path.getsize(src_path)

    if src_size <= max_bytes:
        if not dry_run:
            _remove_if_exists(dest_path)
            _remove_if_exists(legacy_dir_copy)
        return None

    if dry_run:
        return dest_path

    data = _condense_png_bytes(src_path, max_bytes)
    with open(dest_path, "wb") as handle:
        handle.write(data)
    _remove_if_exists(legacy_dir_copy)
    return dest_path


def scan_final_images(
    generated_root: str,
    max_bytes: int = SITE_IMAGE_MAX_BYTES,
    dry_run: bool = False,
) -> List[Tuple[str, Optional[str], int]]:
    """Process every original _01 PNG in ImagesFinal and drop stale copies.

    Each result is (src_name, dest_path_or_none, src_bytes).
    """
    results: List[Tuple[str, Optional[str], int]] = []
    if not os.path.isdir(generated_root):
        return results

    stale = cleanup_stale_under2mb_copies(generated_root, max_bytes=max_bytes, dry_run=dry_run)
    if stale:
        print(f"under2mb-removed-stale={stale}")

    for name in sorted(os.listdir(generated_root)):
        if name.lower() in SKIP_DIR_NAMES:
            continue
        src_path = os.path.join(generated_root, name)
        if not os.path.isfile(src_path) or not is_final_01_png(name):
            continue
        dest = ensure_under2mb_copy(src_path, max_bytes=max_bytes, dry_run=dry_run)
        results.append((name, dest, os.path.getsize(src_path)))
    return results


def cleanup_stale_under2mb_copies(
    generated_root: str,
    max_bytes: int = SITE_IMAGE_MAX_BYTES,
    dry_run: bool = False,
) -> int:
    """Delete non-_01 Under2MB copies and _01 copies that are no longer needed."""
    removed = 0
    if not os.path.isdir(generated_root):
        return removed

    for name in list(os.listdir(generated_root)):
        if not is_under2mb_name(name):
            continue
        path = os.path.join(generated_root, name)
        if not os.path.isfile(path):
            continue

        stem, ext = os.path.splitext(name)
        original_name = stem[: -len(UNDER2MB_SUFFIX)] + ext
        original_path = os.path.join(generated_root, original_name)
        keep = (
            is_final_01_png(original_name)
            and os.path.isfile(original_path)
            and os.path.getsize(original_path) > max_bytes
        )
        if keep:
            continue
        if not dry_run:
            _remove_if_exists(path)
        removed += 1
    return removed


def report_scan(results: List[Tuple[str, Optional[str], int]]) -> int:
    created = 0
    for name, dest, src_size in results:
        if dest:
            created += 1
            print(
                f"under2mb {name} ({src_size / (1024 * 1024):.2f} MB) -> "
                f"{os.path.basename(dest)}"
            )
    if created:
        print(f"under2mb-created={created}")
    else:
        print("under2mb-created=0")
    return created
