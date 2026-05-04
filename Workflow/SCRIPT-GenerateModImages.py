import argparse
import csv
import hashlib
import json
import os
import re
import sys
from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple
import xml.etree.ElementTree as ET

try:
    from PIL import Image, ImageDraw, ImageFont
except Exception as ex:
    print("Pillow is required. Install with: pip install pillow")
    print(f"Import error: {ex}")
    sys.exit(1)


WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
VS_CODE_ROOT = os.path.dirname(WORKFLOW_DIR)
RELEASE_SOURCE = os.path.join(VS_CODE_ROOT, "02_ActiveBuild")
COMPAT_CSV = os.path.join(VS_CODE_ROOT, "HELPER_ModCompatibility.csv")
DEFAULT_LAYOUT_PATH = os.path.join(VS_CODE_ROOT, "00_Images", "modimage-layout.json")


@dataclass
class ModMeta:
    folder: str
    base_name: str
    mod_name: str
    version: str
    description: str
    features: List[str]
    tested_version: str
    mod_type: str


def read_json(path: str) -> Dict[str, object]:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def get_base_mod_name(name: str) -> str:
    return re.sub(r"-v\d+\.\d+(\.\d+)*$", "", name)


def is_agf_mod(folder: str) -> bool:
    return folder.startswith("AGF-") or folder.startswith("zzzAGF-")


def parse_modinfo(modinfo_path: str, fallback_name: str) -> Tuple[str, str, str, str]:
    try:
        tree = ET.parse(modinfo_path)
        root = tree.getroot()
        name_tag = root.find("Name")
        display_name_tag = root.find("DisplayName") if root.find("DisplayName") is not None else name_tag
        version_tag = root.find("Version")
        desc_tag = root.find("Description")
        internal_name = name_tag.attrib.get("value", fallback_name) if name_tag is not None else fallback_name
        display_name = display_name_tag.attrib.get("value", internal_name) if display_name_tag is not None else internal_name
        version = version_tag.attrib.get("value", "0.0.0") if version_tag is not None else "0.0.0"
        desc = desc_tag.attrib.get("value", "") if desc_tag is not None else ""
        return display_name.strip(), internal_name.strip(), version.strip(), desc.strip()
    except Exception:
        return fallback_name, fallback_name, "0.0.0", ""


def split_beta_from_display_name(display_name: str, version: str) -> Tuple[str, str]:
    title = (display_name or "").strip()
    version_text = (version or "").strip()
    if not version_text:
        version_text = "0.0.0"
    if not version_text.lower().startswith("v"):
        version_text = f"v{version_text}"

    match = re.match(r"^\s*(BETA\s*-\s*)(.+)$", title, flags=re.IGNORECASE)
    if match:
        cleaned_title = match.group(2).strip()
        return cleaned_title, f"BETA - {version_text}"

    return title, version_text


def extract_features(readme_path: str) -> List[str]:
    if not os.path.isfile(readme_path):
        return []
    try:
        with open(readme_path, "r", encoding="utf-8") as f:
            content = f.read()
    except Exception:
        return []

    start = content.find("<!-- FEATURES-SUMMARY START -->")
    end = content.find("<!-- FEATURES-SUMMARY END -->")
    if start == -1 or end == -1 or end <= start:
        return []

    block = content[start + len("<!-- FEATURES-SUMMARY START -->"):end]
    lines: List[str] = []
    for raw in block.splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        line = re.sub(r"^[-*+]\s+", "", line)
        line = re.sub(r"`+", "", line)
        line = re.sub(r"\*\*|__|\*|_", "", line)
        line = re.sub(r"\[(.*?)\]\((.*?)\)", r"\1", line)
        line = line.strip()
        if line:
            lines.append(line)
    return lines


def load_compatibility(csv_path: str) -> Dict[str, Dict[str, str]]:
    out: Dict[str, Dict[str, str]] = {}
    if not os.path.isfile(csv_path):
        return out

    try:
        with open(csv_path, "r", encoding="utf-8-sig", newline="") as f:
            for row in csv.DictReader(f):
                key = (row.get("MOD_NAME") or "").strip()
                if key:
                    out[key] = {k: (v or "").strip() for k, v in row.items()}
    except Exception:
        return out
    return out


def extract_mod_type(readme_path: str) -> str:
    if not os.path.isfile(readme_path):
        return ""
    try:
        with open(readme_path, "r", encoding="utf-8") as f:
            content = f.read()
    except Exception:
        return ""
    match = re.search(r"^## 4\. Mod Type\s*\n(.*?)(?=\n##|\Z)", content, re.DOTALL | re.MULTILINE)
    if not match:
        return ""
    for raw in match.group(1).splitlines():
        line = raw.strip()
        if line.startswith("-") or line.startswith("*"):
            return re.sub(r"^[-*+]\s+", "", line).strip()
    return ""


def fit_and_crop(img: Image.Image, target_w: int, target_h: int) -> Image.Image:
    src_w, src_h = img.size
    scale = max(target_w / max(1, src_w), target_h / max(1, src_h))
    new_w = max(1, int(round(src_w * scale)))
    new_h = max(1, int(round(src_h * scale)))
    resized = img.resize((new_w, new_h), Image.Resampling.LANCZOS)
    left = max(0, (new_w - target_w) // 2)
    top = max(0, (new_h - target_h) // 2)
    return resized.crop((left, top, left + target_w, top + target_h))


def fit_single_line_font(
    draw: ImageDraw.ImageDraw,
    text: str,
    font_path: str,
    initial_size: int,
    min_size: int,
    max_width: int,
    max_height: int,
) -> ImageFont.ImageFont:
    for size in range(initial_size, min_size - 1, -1):
        font = load_font(font_path, size)
        bbox = draw.textbbox((0, 0), text, font=font, stroke_width=2)
        text_w = max(1, bbox[2] - bbox[0])
        text_h = max(1, bbox[3] - bbox[1])
        if text_w <= max_width and text_h <= max_height:
            return font
    return load_font(font_path, min_size)


def load_font(path: str, size: int) -> ImageFont.FreeTypeFont:
    if path and os.path.isfile(path):
        try:
            return ImageFont.truetype(path, size=size)
        except Exception:
            pass

    # Prefer common Windows fonts before falling back to PIL's tiny bitmap font.
    windows_font_candidates = [
        os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "segoeui.ttf"),
        os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "segoeuib.ttf"),
        os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "arial.ttf"),
        os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts", "arialbd.ttf"),
    ]
    for candidate in windows_font_candidates:
        if not os.path.isfile(candidate):
            continue
        try:
            return ImageFont.truetype(candidate, size=size)
        except Exception:
            continue

    return ImageFont.load_default()


def wrap_for_width(draw: ImageDraw.ImageDraw, text: str, font: ImageFont.ImageFont, max_width: int, max_lines: int) -> List[str]:
    if not text:
        return []

    words = text.split()
    if not words:
        return []

    lines: List[str] = []
    current = words[0]
    for word in words[1:]:
        candidate = f"{current} {word}"
        candidate_bbox = draw.textbbox((0, 0), candidate, font=font)
        width = max(1, candidate_bbox[2] - candidate_bbox[0])
        if width <= max_width:
            current = candidate
        else:
            lines.append(current)
            current = word
            if len(lines) >= max_lines:
                break

    if len(lines) < max_lines:
        lines.append(current)

    if len(lines) > max_lines:
        lines = lines[:max_lines]

    if lines and len(words) > 1:
        used = " ".join(lines)
        if len(used) < len(text):
            lines[-1] = lines[-1].rstrip(".") + "..."

    return lines


def draw_lines(
    draw: ImageDraw.ImageDraw,
    lines: List[str],
    box: Dict[str, int],
    font: ImageFont.ImageFont,
    color: str,
    line_spacing: float,
    align: str = "left",
    valign: str = "top",
    blank_line_ratio: float = 1.0,
) -> None:
    x = int(box["x"])
    w = int(box["w"])
    y = int(box["y"])
    h = int(box["h"])
    box_bottom = y + int(box["h"])
    line_bbox = draw.textbbox((0, 0), "Ag", font=font)
    line_h = max(1, line_bbox[3] - line_bbox[1])
    step = int(round(line_h * line_spacing))
    blank_step = int(round(line_h * line_spacing * blank_line_ratio))

    if not lines:
        return

    max_fit_lines = 1 + max(0, (h - line_h) // max(1, step))
    lines_to_draw = lines[: max(0, max_fit_lines)]
    if not lines_to_draw:
        return

    if blank_line_ratio != 1.0:
        total_h = line_h
        for _i in range(len(lines_to_draw) - 1):
            total_h += blank_step if lines_to_draw[_i] == "" else step
    else:
        total_h = line_h + step * (len(lines_to_draw) - 1)
    if valign == "center":
        y = int(box["y"]) + max(0, (h - total_h) // 2)
    elif valign == "bottom":
        y = int(box["y"]) + max(0, h - total_h)

    for line in lines_to_draw:
        if y + line_h > box_bottom:
            break
        line_bbox = draw.textbbox((0, 0), line, font=font)
        line_w = max(1, line_bbox[2] - line_bbox[0])
        line_top = line_bbox[1]
        if align == "center":
            x_draw = x + max(0, (w - line_w) // 2)
        elif align == "right":
            x_draw = x + max(0, w - line_w)
        else:
            x_draw = x
        # Compensate for glyph top-bearing so vertical centering is visually accurate.
        draw.text((x_draw, y - line_top), line, fill=color, font=font)
        y += blank_step if line == "" else step


def wrap_with_prefix(
    draw: ImageDraw.ImageDraw,
    text: str,
    font: ImageFont.ImageFont,
    max_width: int,
    first_prefix: str,
    cont_prefix: str,
    max_lines: int,
) -> List[str]:
    if not text:
        return []
    words = text.split()
    if not words:
        return []

    lines: List[str] = []
    current_words: List[str] = []

    def build_line(words_for_line: List[str], prefix: str) -> str:
        return f"{prefix}{' '.join(words_for_line)}".rstrip()

    for word in words:
        prefix = first_prefix if len(lines) == 0 else cont_prefix
        candidate_words = current_words + [word]
        candidate_line = build_line(candidate_words, prefix)
        candidate_bbox = draw.textbbox((0, 0), candidate_line, font=font)
        candidate_w = max(1, candidate_bbox[2] - candidate_bbox[0])
        if candidate_w <= max_width:
            current_words = candidate_words
            continue

        if current_words:
            lines.append(build_line(current_words, prefix))
            if len(lines) >= max_lines:
                return lines
            current_words = [word]
        else:
            lines.append(build_line([word], prefix))
            if len(lines) >= max_lines:
                return lines
            current_words = []

    if current_words and len(lines) < max_lines:
        prefix = first_prefix if len(lines) == 0 else cont_prefix
        lines.append(build_line(current_words, prefix))

    return lines


def compute_block_height(draw: ImageDraw.ImageDraw, font: ImageFont.ImageFont, line_spacing: float, line_count: int, lines: Optional[List[str]] = None, blank_line_ratio: float = 1.0) -> int:
    if line_count <= 0:
        return 0
    bbox = draw.textbbox((0, 0), "Ag", font=font)
    line_h = max(1, bbox[3] - bbox[1])
    step = max(1, int(round(line_h * line_spacing)))
    if lines is not None and blank_line_ratio != 1.0:
        blank_step = max(0, int(round(line_h * line_spacing * blank_line_ratio)))
        total = line_h
        for _i in range(len(lines) - 1):
            total += blank_step if lines[_i] == "" else step
        return total
    return line_h + step * (line_count - 1)


def fit_wrapped_text_to_box(
    draw: ImageDraw.ImageDraw,
    text: str,
    font_path: str,
    initial_size: int,
    min_size: int,
    max_width: int,
    max_height: int,
    max_lines: int,
    line_spacing: float,
) -> Tuple[ImageFont.ImageFont, List[str]]:
    for size in range(initial_size, min_size - 1, -1):
        font = load_font(font_path, size)
        lines = wrap_for_width(draw, text, font, max_width, max_lines)
        if compute_block_height(draw, font, line_spacing, len(lines)) <= max_height:
            return font, lines

    fallback_font = load_font(font_path, min_size)
    fallback_lines = wrap_for_width(draw, text, fallback_font, max_width, max_lines)
    return fallback_font, fallback_lines


def fit_feature_lines_to_box(
    draw: ImageDraw.ImageDraw,
    features: List[str],
    bullet: str,
    font_path: str,
    initial_size: int,
    min_size: int,
    max_width: int,
    max_height: int,
    max_lines: int,
    line_spacing: float,
    min_line_spacing: float,
    blank_line_ratio: float = 1.0,
    continuation_prefix_override: Optional[str] = None,
) -> Tuple[ImageFont.ImageFont, List[str], float, bool]:
    for size in range(initial_size, min_size - 1, -1):
        spacing = line_spacing
        while spacing >= min_line_spacing - 1e-9:
            font = load_font(font_path, size)
            _b_w = max(1, int(round(draw.textlength(bullet, font=font))))
            _n = 1
            while True:
                _sp_w = max(1, int(round(draw.textlength(" " * _n, font=font))))
                if _sp_w >= _b_w:
                    if _n > 1:
                        _prev_w = max(1, int(round(draw.textlength(" " * (_n - 1), font=font))))
                        if abs(_prev_w - _b_w) < abs(_sp_w - _b_w):
                            _n = _n - 1
                    break
                _n += 1
            continuation_prefix = " " * max(1, _n) if continuation_prefix_override is None else continuation_prefix_override
            lines: List[str] = []
            truncated = False
            for idx, feat in enumerate(features):
                remaining = max_lines - len(lines)
                if remaining <= 0:
                    truncated = True
                    break
                wrapped = wrap_with_prefix(
                    draw,
                    feat,
                    font,
                    max_width,
                    bullet,
                    continuation_prefix,
                    remaining,
                )
                lines.extend(wrapped)

                if idx < len(features) - 1:
                    if len(lines) < max_lines:
                        lines.append("")
                    else:
                        truncated = True
                        break

            if compute_block_height(draw, font, spacing, len(lines), lines=lines, blank_line_ratio=blank_line_ratio) <= max_height:
                return font, lines, spacing, truncated
            spacing = round(spacing - 0.05, 2)

    fallback_font = load_font(font_path, min_size)
    _b_w = max(1, int(round(draw.textlength(bullet, font=fallback_font))))
    _n = 1
    while True:
        _sp_w = max(1, int(round(draw.textlength(" " * _n, font=fallback_font))))
        if _sp_w >= _b_w:
            if _n > 1:
                _prev_w = max(1, int(round(draw.textlength(" " * (_n - 1), font=fallback_font))))
                if abs(_prev_w - _b_w) < abs(_sp_w - _b_w):
                    _n = _n - 1
            break
        _n += 1
    continuation_prefix = " " * max(1, _n) if continuation_prefix_override is None else continuation_prefix_override
    fallback_lines: List[str] = []
    fallback_truncated = False
    for idx, feat in enumerate(features):
        remaining = max_lines - len(fallback_lines)
        if remaining <= 0:
            fallback_truncated = True
            break
        wrapped = wrap_with_prefix(
            draw,
            feat,
            fallback_font,
            max_width,
            bullet,
            continuation_prefix,
            remaining,
        )
        fallback_lines.extend(wrapped)

        if idx < len(features) - 1:
            if len(fallback_lines) < max_lines:
                fallback_lines.append("")
            else:
                fallback_truncated = True
                break

    return fallback_font, fallback_lines, min_line_spacing, fallback_truncated


def fit_lines_to_box(
    draw: ImageDraw.ImageDraw,
    lines: List[str],
    font_path: str,
    initial_size: int,
    min_size: int,
    max_width: int,
    max_height: int,
    line_spacing: float,
) -> Tuple[ImageFont.ImageFont, List[str]]:
    if not lines:
        return load_font(font_path, initial_size), []

    for size in range(initial_size, min_size - 1, -1):
        font = load_font(font_path, size)
        too_wide = False
        for line in lines:
            bbox = draw.textbbox((0, 0), line, font=font)
            line_w = max(1, bbox[2] - bbox[0])
            if line_w > max_width:
                too_wide = True
                break
        if too_wide:
            continue

        if compute_block_height(draw, font, line_spacing, len(lines)) <= max_height:
            return font, lines

    return load_font(font_path, min_size), lines


def max_lines_for_box(draw: ImageDraw.ImageDraw, font: ImageFont.ImageFont, line_spacing: float, box_h: int) -> int:
    bbox = draw.textbbox((0, 0), "Ag", font=font)
    line_h = max(1, bbox[3] - bbox[1])
    step = max(1, int(round(line_h * line_spacing)))
    return 1 + max(0, (max(1, box_h) - line_h) // step)


def apply_features_overflow_notice(
    draw: ImageDraw.ImageDraw,
    lines: List[str],
    truncated: bool,
    font: ImageFont.ImageFont,
    line_spacing: float,
    box_w: int,
    box_h: int,
    bullet: str,
    notice_text: str,
) -> List[str]:
    if not truncated:
        return lines

    capacity = max_lines_for_box(draw, font, line_spacing, box_h)
    if capacity <= 0:
        return []

    cont_prefix = " " * max(4, len(bullet) + 1)
    notice_lines = wrap_with_prefix(draw, notice_text, font, box_w, bullet, cont_prefix, capacity)

    out = list(lines)
    while out and out[-1] == "":
        out.pop()

    def trim_last_item(buf: List[str]) -> List[str]:
        if not buf:
            return buf
        while buf and buf[-1] != "":
            buf.pop()
        while buf and buf[-1] == "":
            buf.pop()
        return buf

    need = len(notice_lines) + (1 if out else 0)
    while out and len(out) + need > capacity:
        out = trim_last_item(out)
        need = len(notice_lines) + (1 if out else 0)

    result: List[str] = []
    result.extend(out)
    if result and notice_lines:
        result.append("")
    result.extend(notice_lines)
    return result[:capacity]


def resolve_media_image_path(media_root: str, mod_base_name: str, legacy_source_root: str = "") -> Optional[str]:
    allowed_ext = [".png", ".jpg", ".jpeg", ".webp"]

    for ext in allowed_ext:
        candidate = os.path.join(media_root, f"{mod_base_name}{ext}")
        if os.path.isfile(candidate):
            return candidate

    if legacy_source_root:
        legacy_dir = os.path.join(legacy_source_root, mod_base_name)
        if os.path.isdir(legacy_dir):
            numbered: List[Tuple[int, str]] = []
            for name in os.listdir(legacy_dir):
                stem, ext = os.path.splitext(name)
                if ext.lower() not in allowed_ext:
                    continue
                if not stem.isdigit():
                    continue
                numbered.append((int(stem), os.path.join(legacy_dir, name)))
            numbered.sort(key=lambda t: t[0])
            if numbered:
                return numbered[0][1]

    return None


def read_manifest(path: str) -> Dict[str, Dict[str, object]]:
    if not os.path.isfile(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
        if isinstance(data, dict):
            return data
    except Exception:
        pass
    return {}


def write_manifest(path: str, manifest: Dict[str, Dict[str, object]]) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=True)


def write_media_status_csv(path: str, rows: List[Dict[str, str]]) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(
            f,
            fieldnames=["MOD_NAME", "HAS_IMAGE_PREPARED", "IMAGE_FILE"],
        )
        writer.writeheader()
        for row in rows:
            writer.writerow(row)


def build_mod_signature(mod: ModMeta, media_path: Optional[str]) -> str:
    media_stat = "missing"
    if media_path and os.path.isfile(media_path):
        st = os.stat(media_path)
        media_stat = f"{os.path.basename(media_path)}|{st.st_size}|{st.st_mtime_ns}"

    payload = {
        "base": mod.base_name,
        "name": mod.mod_name,
        "version": mod.version,
        "description": mod.description,
        "features": mod.features,
        "tested_version": mod.tested_version,
        "mod_type": mod.mod_type,
        "media": media_stat,
    }
    raw = json.dumps(payload, sort_keys=True, separators=(",", ":"), ensure_ascii=True)
    return hashlib.sha256(raw.encode("utf-8")).hexdigest()


def resolve_path(root: str, value: str) -> str:
    if os.path.isabs(value):
        return value
    return os.path.join(root, value.replace("/", os.sep))


def get_mods(layout: Dict[str, object], compatibility: Dict[str, Dict[str, str]]) -> List[ModMeta]:
    mods: List[ModMeta] = []
    if not os.path.isdir(RELEASE_SOURCE):
        return mods

    for folder in sorted(os.listdir(RELEASE_SOURCE), key=str.lower):
        full = os.path.join(RELEASE_SOURCE, folder)
        if not os.path.isdir(full) or not is_agf_mod(folder):
            continue

        modinfo = os.path.join(full, "ModInfo.xml")
        readme = os.path.join(full, "README.md")
        base = get_base_mod_name(folder)
        display_name, internal_name, version, desc = parse_modinfo(modinfo, base)
        feats = extract_features(readme)
        compat_row = compatibility.get(internal_name, {})
        tested_version = (compat_row.get("TESTED_GAME_VERSION") or "TBD").strip()
        mod_type = extract_mod_type(readme)
        mods.append(ModMeta(folder=folder, base_name=base, mod_name=display_name, version=version, description=desc, features=feats, tested_version=tested_version, mod_type=mod_type))

    return mods


def generate_for_mod(mod: ModMeta, layout: Dict[str, object], media_image_path: Optional[str], dry_run: bool = False) -> Tuple[bool, str]:
    paths = layout["paths"]
    zones = layout["zones"]
    typ = layout["typography"]
    colors = layout["colors"]
    limits = layout["limits"]
    output = layout["output"]

    template_path = resolve_path(VS_CODE_ROOT, paths["template"])
    logo_path = resolve_path(VS_CODE_ROOT, paths.get("logo", ""))
    generated_root = resolve_path(VS_CODE_ROOT, paths["generated_root"])
    regular_font_path = resolve_path(VS_CODE_ROOT, typ.get("font_regular", ""))
    bold_font_path = resolve_path(VS_CODE_ROOT, typ.get("font_bold", ""))

    missing_source_image = not media_image_path or not os.path.isfile(media_image_path)

    if not os.path.isfile(template_path):
        return False, f"skip {mod.base_name}: missing template {template_path}"

    if dry_run:
        if missing_source_image:
            return True, f"would-generate {mod.base_name} (placeholder media)"
        return True, f"would-generate {mod.base_name} ({os.path.basename(media_image_path or '')})"

    os.makedirs(generated_root, exist_ok=True)

    base_img = Image.open(template_path).convert("RGBA")
    media_zone = zones["media"]

    media_x = int(media_zone["x"])
    media_y = int(media_zone["y"])
    media_w = int(media_zone["w"])
    media_h = int(media_zone["h"])

    if not missing_source_image:
        primary_img = Image.open(media_image_path).convert("RGBA")
        fitted_primary = fit_and_crop(primary_img, media_w, media_h)
        base_img.paste(fitted_primary, (media_x, media_y))
    else:
        placeholder = Image.new("RGBA", (media_w, media_h), (0, 0, 0, 0))
        placeholder_draw = ImageDraw.Draw(placeholder)
        placeholder_draw.rectangle([(0, 0), (media_w, media_h)], fill=(22, 16, 28, 96))

        placeholder_text = str(typ.get("missing_media_text", "Image Work in-Progress"))
        placeholder_angle = float(typ.get("missing_media_angle", -28))
        placeholder_color = str(typ.get("missing_media_color", "#DDCDFA"))
        max_text_w = int(media_w * 0.82)
        max_text_h = int(media_h * 0.22)

        text_layer = Image.new("RGBA", (media_w, media_h), (0, 0, 0, 0))
        text_draw = ImageDraw.Draw(text_layer)
        placeholder_font = fit_single_line_font(
            text_draw,
            placeholder_text,
            bold_font_path,
            int(typ.get("missing_media_size", 132)),
            int(typ.get("missing_media_min_size", 36)),
            max_text_w,
            max_text_h,
        )
        text_bbox = text_draw.textbbox((0, 0), placeholder_text, font=placeholder_font, stroke_width=2)
        text_w = max(1, text_bbox[2] - text_bbox[0])
        text_h = max(1, text_bbox[3] - text_bbox[1])
        tx = (media_w - text_w) // 2 - text_bbox[0]
        ty = (media_h - text_h) // 2 - text_bbox[1]
        text_draw.text(
            (tx, ty),
            placeholder_text,
            fill=placeholder_color,
            font=placeholder_font,
            stroke_width=2,
            stroke_fill="#3D2645",
        )
        rotated_text = text_layer.rotate(
            placeholder_angle,
            resample=Image.Resampling.BICUBIC,
            center=(media_w // 2, media_h // 2),
            expand=False,
        )
        placeholder.alpha_composite(rotated_text)
        base_img.alpha_composite(placeholder, (media_x, media_y))

    if logo_path and os.path.isfile(logo_path):
        logo_zone = zones["logo"]
        logo_img = Image.open(logo_path).convert("RGBA")
        fitted_logo = fit_and_crop(logo_img, int(logo_zone["w"]), int(logo_zone["h"]))
        base_img.alpha_composite(fitted_logo, (int(logo_zone["x"]), int(logo_zone["y"])))

    draw = ImageDraw.Draw(base_img)

    title_font = load_font(bold_font_path, int(typ["title_size"]))
    version_font = load_font(bold_font_path, int(typ["version_size"]))

    line_spacing = float(typ.get("line_spacing", 1.15))
    bullet = str(typ.get("feature_bullet", ""))
    _cont_indent_n = int(typ.get("feature_continuation_indent", 0))
    continuation_prefix_override = " " * _cont_indent_n if _cont_indent_n > 0 or bullet == "" else None
    display_title, display_version = split_beta_from_display_name(mod.mod_name, mod.version)

    title_box = zones["title"]
    title_lines = wrap_for_width(draw, display_title, title_font, int(title_box["w"]), int(limits["title_lines"]))

    version_box = zones["version"]
    version_lines = wrap_for_width(draw, display_version, version_font, int(version_box["w"]), 1)

    desc_box = zones["description"]
    desc_font, desc_lines = fit_wrapped_text_to_box(
        draw,
        mod.description,
        regular_font_path,
        int(typ["description_size"]),
        int(typ.get("description_min_size", 20)),
        int(desc_box["w"]),
        int(desc_box["h"]),
        int(limits["description_lines"]),
        line_spacing,
    )

    top_even_spacing = bool(typ.get("top_even_spacing", True))
    if top_even_spacing:
        default_stack_top = min(int(title_box["y"]), int(version_box["y"]), int(desc_box["y"]))
        default_stack_bottom = max(
            int(title_box["y"]) + int(title_box["h"]),
            int(version_box["y"]) + int(version_box["h"]),
            int(desc_box["y"]) + int(desc_box["h"]),
        )
        stack_y = int(typ.get("top_stack_y", default_stack_top))
        stack_h = int(typ.get("top_stack_h", max(1, default_stack_bottom - default_stack_top)))

        title_h = compute_block_height(draw, title_font, line_spacing, len(title_lines))
        version_h = compute_block_height(draw, version_font, line_spacing, len(version_lines))
        desc_h = compute_block_height(draw, desc_font, line_spacing, len(desc_lines))

        total_text_h = title_h + version_h + desc_h
        free_h = max(0, stack_h - total_text_h)
        gap_base, gap_rem = divmod(free_h, 4)
        gaps = [gap_base + (1 if i < gap_rem else 0) for i in range(4)]

        title_y = stack_y + gaps[0]
        version_y = title_y + title_h + gaps[1]
        desc_y = version_y + version_h + gaps[2]

        title_box = {
            "x": int(title_box["x"]),
            "y": title_y,
            "w": int(title_box["w"]),
            "h": max(1, title_h),
        }
        version_box = {
            "x": int(version_box["x"]),
            "y": version_y,
            "w": int(version_box["w"]),
            "h": max(1, version_h),
        }
        desc_box = {
            "x": int(desc_box["x"]),
            "y": desc_y,
            "w": int(desc_box["w"]),
            "h": max(1, desc_h),
        }

    draw_lines(
        draw,
        title_lines,
        title_box,
        title_font,
        str(colors["title"]),
        line_spacing,
        align=str(typ.get("title_align", "left")),
        valign=str(typ.get("title_valign", "top" if top_even_spacing else "center")),
    )

    draw_lines(
        draw,
        version_lines,
        version_box,
        version_font,
        str(colors["version"]),
        line_spacing,
        align=str(typ.get("version_align", "left")),
        valign=str(typ.get("version_valign", "top" if top_even_spacing else "center")),
    )

    draw_lines(
        draw,
        desc_lines,
        desc_box,
        desc_font,
        str(colors["description"]),
        line_spacing,
        align=str(typ.get("description_align", "left")),
        valign=str(typ.get("description_valign", "top" if top_even_spacing else "center")),
    )

    compat_box = zones["compatibility"]
    tested_raw = mod.tested_version.strip()
    tested_label = f"7D2D v{tested_raw}" if tested_raw and tested_raw.upper() not in {"TBD", "UNKNOWN", ""} else "7D2D vTBD"
    tested_lines = ["Tested on:", tested_label]
    tested_font, tested_lines = fit_lines_to_box(
        draw,
        tested_lines,
        regular_font_path,
        int(typ.get("compatibility_size", 34)),
        int(typ.get("compatibility_min_size", 18)),
        int(compat_box["w"]),
        int(compat_box["h"]),
        line_spacing,
    )
    draw_lines(
        draw,
        tested_lines,
        compat_box,
        tested_font,
        str(colors["compatibility"]),
        line_spacing,
        align=str(typ.get("compatibility_align", "center")),
        valign=str(typ.get("compatibility_valign", "center")),
    )

    feats_box = zones["features"]
    _feats_pad_x = int(typ.get("features_padding_x", 12))
    _feats_pad_y = int(typ.get("features_padding_y", 10))
    feats_draw_box = {
        "x": int(feats_box["x"]) + _feats_pad_x,
        "y": int(feats_box["y"]) + _feats_pad_y,
        "w": int(feats_box["w"]) - _feats_pad_x * 2,
        "h": int(feats_box["h"]) - _feats_pad_y * 2,
    }
    features_line_spacing = float(typ.get("features_line_spacing", line_spacing))
    features_min_line_spacing = float(typ.get("features_min_line_spacing", features_line_spacing))
    features_gap_ratio = float(typ.get("features_gap_ratio", 0.3))
    features_font, feature_lines, features_draw_line_spacing, features_truncated = fit_feature_lines_to_box(
        draw,
        mod.features,
        bullet,
        regular_font_path,
        int(typ["features_size"]),
        int(typ.get("features_min_size", 16)),
        int(feats_draw_box["w"]),
        int(feats_draw_box["h"]),
        int(limits["feature_lines"]),
        features_line_spacing,
        features_min_line_spacing,
        blank_line_ratio=features_gap_ratio,
        continuation_prefix_override=continuation_prefix_override,
    )
    feature_notice_text = str(typ.get("features_overflow_notice", "and more! Full list in README."))
    feature_lines = apply_features_overflow_notice(
        draw,
        feature_lines,
        features_truncated,
        features_font,
        features_draw_line_spacing,
        int(feats_draw_box["w"]),
        int(feats_draw_box["h"]),
        bullet,
        feature_notice_text,
    )
    draw_lines(
        draw,
        feature_lines,
        feats_draw_box,
        features_font,
        str(colors["features"]),
        features_draw_line_spacing,
        align=str(typ.get("features_align", "left")),
        valign=str(typ.get("features_valign", "center")),
        blank_line_ratio=features_gap_ratio,
    )

    if zones.get("mod_type") and mod.mod_type:
        mod_type_box = zones["mod_type"]
        _mt_pad_x = int(typ.get("features_padding_x", 12))
        _mt_pad_y = int(typ.get("features_padding_y", 10))
        box_x = int(mod_type_box["x"]) + _mt_pad_x
        box_y = int(mod_type_box["y"]) + _mt_pad_y
        box_w = int(mod_type_box["w"]) - _mt_pad_x * 2
        box_h = int(mod_type_box["h"]) - _mt_pad_y * 2
        mt_color = str(colors.get("mod_type", colors["features"]))
        mt_label_size = int(typ.get("mod_type_size", 26))
        mt_min_size = int(typ.get("mod_type_min_size", 14))
        mt_line_spacing = float(typ.get("mod_type_line_spacing", 1.0))

        _parts = mod.mod_type.split(": ", 1)
        mt_label = _parts[0].strip()
        mt_explanation = _parts[1].strip() if len(_parts) > 1 else ""

        label_font = fit_single_line_font(
            draw, mt_label, regular_font_path,
            mt_label_size, mt_min_size,
            box_w, int(box_h * 0.5),
        )
        _lbbox = draw.textbbox((0, 0), "Ag", font=label_font)
        label_line_h = max(1, _lbbox[3] - _lbbox[1])
        label_step = int(round(label_line_h * mt_line_spacing))
        draw_lines(
            draw, [mt_label],
            {"x": box_x, "y": box_y, "w": box_w, "h": label_line_h},
            label_font, mt_color, mt_line_spacing, align="center", valign="top",
        )

        if mt_explanation:
            expl_y = box_y + label_step + 6
            expl_h = box_h - label_step - 6
            if expl_h > 0:
                expl_font, expl_lines = fit_wrapped_text_to_box(
                    draw, mt_explanation, regular_font_path,
                    mt_label_size, mt_min_size,
                    box_w, expl_h,
                    int(limits.get("mod_type_lines", 3)),
                    mt_line_spacing,
                )
                draw_lines(
                    draw, expl_lines,
                    {"x": box_x, "y": expl_y, "w": box_w, "h": expl_h},
                    expl_font, mt_color, mt_line_spacing, align="left", valign="top",
                )

    out_banner = os.path.join(generated_root, f"ModImage_{mod.base_name}.png")
    scaled = base_img.resize((int(output["width"]), int(output["height"])), Image.Resampling.LANCZOS)
    scaled.convert("RGB").save(out_banner, format="PNG", optimize=True)

    for name in os.listdir(generated_root):
        if name.startswith(f"{mod.base_name}_preview_") and name.endswith(".png"):
            try:
                os.remove(os.path.join(generated_root, name))
            except OSError:
                pass

    if not missing_source_image and int(limits.get("max_previews", 6)) > 0:
        src_img = Image.open(media_image_path).convert("RGBA")
        fitted = fit_and_crop(src_img, int(output["width"]), int(output["height"]))
        out_preview = os.path.join(generated_root, f"{mod.base_name}_preview_1.png")
        fitted.convert("RGB").save(out_preview, format="PNG", optimize=True)

    if missing_source_image:
        return True, f"generated {mod.base_name} (placeholder media)"
    return True, f"generated {mod.base_name}"


def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(description="Generate mod images and README preview images.")
    p.add_argument("--layout", default=DEFAULT_LAYOUT_PATH, help="Path to modimage layout JSON.")
    p.add_argument("--mod", default="", help="Generate only one base mod name.")
    p.add_argument("--changed-only", action="store_true", help="Generate only mods with changed text/media inputs.")
    p.add_argument("--dry-run", action="store_true", help="Report work without writing files.")
    return p


def main() -> int:
    args = build_parser().parse_args()

    if not os.path.isfile(args.layout):
        print(f"Layout config not found: {args.layout}")
        return 1

    try:
        layout = read_json(args.layout)
    except Exception as ex:
        print(f"Failed to read layout config: {ex}")
        return 1

    compatibility = load_compatibility(COMPAT_CSV)
    mods = get_mods(layout, compatibility)
    if args.mod:
        wanted = args.mod.strip().lower()
        mods = [m for m in mods if m.base_name.lower() == wanted]

    if not mods:
        print("No matching mods found.")
        return 0

    paths = layout.get("paths", {})
    media_root_value = str(paths.get("media_root", "00_Images/mod-media"))
    media_root = resolve_path(VS_CODE_ROOT, media_root_value)
    legacy_source_root = resolve_path(VS_CODE_ROOT, str(paths.get("source_root", ""))) if paths.get("source_root") else ""
    generated_root = resolve_path(VS_CODE_ROOT, str(paths.get("generated_root", "00_Images/_generated")))
    manifest_value = str(paths.get("manifest", "00_Images/_generated/_modimage-manifest.json"))
    manifest_path = resolve_path(VS_CODE_ROOT, manifest_value)
    media_status_value = str(paths.get("media_status_csv", "00_Images/media-status.csv"))
    media_status_path = resolve_path(VS_CODE_ROOT, media_status_value)

    os.makedirs(media_root, exist_ok=True)
    os.makedirs(generated_root, exist_ok=True)

    previous_manifest = read_manifest(manifest_path)
    next_manifest = dict(previous_manifest)

    work_items: List[Tuple[ModMeta, Optional[str], str]] = []
    media_status_rows: List[Dict[str, str]] = []
    unchanged_count = 0
    for mod in mods:
        media_image_path = resolve_media_image_path(media_root, mod.base_name, legacy_source_root)
        media_status_rows.append(
            {
                "MOD_NAME": mod.base_name,
                "HAS_IMAGE_PREPARED": "Yes" if media_image_path else "No",
                "IMAGE_FILE": os.path.basename(media_image_path) if media_image_path else "",
            }
        )
        signature = build_mod_signature(mod, media_image_path)
        prev = previous_manifest.get(mod.base_name, {}) if isinstance(previous_manifest.get(mod.base_name, {}), dict) else {}
        prev_sig = str(prev.get("signature", ""))
        banner_path = os.path.join(generated_root, f"ModImage_{mod.base_name}.png")
        should_skip = args.changed_only and (signature == prev_sig) and os.path.isfile(banner_path)
        if should_skip and not args.mod:
            unchanged_count += 1
            next_manifest[mod.base_name] = {
                "signature": signature,
                "version": mod.version,
                "has_media": bool(media_image_path),
                "media_file": os.path.basename(media_image_path) if media_image_path else "",
            }
            continue
        work_items.append((mod, media_image_path, signature))

    ok_count = 0
    skip_count = 0
    for mod, media_image_path, signature in work_items:
        ok, message = generate_for_mod(mod, layout, media_image_path, dry_run=args.dry_run)
        print(message)
        if ok:
            ok_count += 1
            next_manifest[mod.base_name] = {
                "signature": signature,
                "version": mod.version,
                "has_media": bool(media_image_path),
                "media_file": os.path.basename(media_image_path) if media_image_path else "",
            }
        else:
            skip_count += 1

    if unchanged_count:
        print(f"unchanged-skipped={unchanged_count}")

    write_media_status_csv(media_status_path, media_status_rows)
    print(f"media-status-csv={media_status_path}")

    if not args.dry_run:
        write_manifest(manifest_path, next_manifest)

    print(f"done: generated_or_planned={ok_count}, skipped={skip_count}, total={len(mods)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
