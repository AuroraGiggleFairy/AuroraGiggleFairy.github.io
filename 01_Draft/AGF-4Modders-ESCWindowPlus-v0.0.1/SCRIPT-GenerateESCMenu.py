"""
Generate ESC menu support files from one editable source JSON.

Outputs:
- Config/Generated/Localization.ESC.Generated.txt
- Config/Generated/News/*.xml

This script does NOT overwrite your main Localization.txt by default.
You can manually merge generated keys when you are ready.
"""

from __future__ import annotations

import argparse
import copy
import csv
import datetime as dt
import json
import re
import urllib.error
import urllib.request
from pathlib import Path
from xml.sax.saxutils import escape


DEFAULT_THEME_COLORS: dict[str, str] = {
    "headerBackground": "95, 89, 128",
    "pageBackground": "95, 89, 128",
    "headerTitle": "141, 181, 128",
    "headerMotto": "221, 205, 250",
    "tabButtonSelected": "74, 33, 150",
    "pageTitle": "141, 181, 128",
    "pageBody": "221, 205, 250",
    "textBolding": "141, 181, 128",
    "textHighlight": "255, 255, 255",
}

DEFAULT_AUTO_HIGH_CONTRAST_COLORS: dict[str, str] = {
    "headerBackground": "64, 64, 64",
    "pageBackground": "64, 64, 64",
    "headerTitle": "255, 255, 255",
    "headerMotto": "255, 255, 255",
    "tabButtonSelected": "200, 200, 200",
    "pageTitle": "255, 255, 255",
    "pageBody": "255, 255, 255",
    "textBolding": "255, 255, 255",
    "textHighlight": "255, 255, 255",
}

DEFAULT_TAB_LAYOUT: dict[str, int] = {
    "contentWidth": 1100,
    "contentPadding": 20,
    "buttonCellWidth": 150,
    "buttonHeight": 42,
    "buttonPosY": -20,
}

DEFAULT_PAGE_TITLE_FONT_SIZE = 36
DEFAULT_PAGE_BODY_FONT_SIZE = 28
DEFAULT_MAX_LINKS = 4


RGB_TRIPLET_RE = re.compile(r"^\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}\s*$")
BODY_BOLD_TOKEN_RE = re.compile(r"\{\{b:(.*?)\}\}|\*\*(.+?)\*\*", re.DOTALL)
BODY_HIGHLIGHT_TOKEN_RE = re.compile(r"\{\{h:(.*?)\}\}|==(.+?)==", re.DOTALL)
BODY_BOUNDARY_MARKER_RE = re.compile(r"^\s*<\!?--\s*body\b.*?(start|end).*?-->\s*$", re.IGNORECASE)
PAGE_BANNER_RE = re.compile(r"^\s*PAGE\s+\d+\s*$", re.IGNORECASE)
SEPARATOR_LINE_RE = re.compile(r"^\s*[-=]{5,}\s*$")


def _is_valid_rgb_triplet(value: str) -> bool:
    if not RGB_TRIPLET_RE.match(value):
        return False
    parts = [int(p.strip()) for p in value.split(",")]
    return all(0 <= part <= 255 for part in parts)


def _validate_rgb_theme_block(block: dict, block_name: str) -> None:
    rgb_keys = {
        "headerBackground",
        "headerBackgroundColor",
        "pageBackground",
        "pageBackgroundColor",
        "headerTitle",
        "headerTitleColor",
        "headerMotto",
        "headerMottoColor",
        "tabButtonSelected",
        "tabButtonSelectedColor",
        "pageTitle",
        "pageTitleColor",
        "pageBody",
        "pageBodyColor",
        "textBolding",
        "textBoldingColor",
        "textHighlight",
        "textHighlightColor",
    }
    allowed_list = ", ".join(sorted(rgb_keys))
    for key, raw_value in block.items():
        if key not in rgb_keys:
            raise ValueError(
                f"Source JSON '{block_name}.{key}' is not a supported color key. "
                f"Allowed keys: {allowed_list}"
            )
        value = str(raw_value)
        if not _is_valid_rgb_triplet(value):
            raise ValueError(
                f"Source JSON '{block_name}.{key}' must be RGB triplet like '255,255,255'."
            )


def _rgb_to_bracket_hex(value: str) -> str:
    parts = [int(p.strip()) for p in value.split(",")]
    return f"[{parts[0]:02X}{parts[1]:02X}{parts[2]:02X}]"


def _localization_color_value(value: str) -> str:
    if _is_valid_rgb_triplet(value):
        return _rgb_to_bracket_hex(value)
    return value


def _colorize_text(text: str, rgb_value: str) -> str:
    if not text:
        return text
    return f"{_localization_color_value(rgb_value)}{text}[-]"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Generate ESC localization + news files from one source config."
    )
    parser.add_argument(
        "--config",
        default="ESCMenu.source.json",
        help="Path to source JSON (default: ESCMenu.source.json)",
    )
    parser.add_argument(
        "--out-dir",
        default="Config/Generated",
        help="Output directory for generated files (default: Config/Generated)",
    )
    parser.add_argument(
        "--merge-localization",
        action="store_true",
        help="Merge generated keys into Config/Localization.txt (adds missing keys and updates existing).",
    )
    parser.add_argument(
        "--localization-file",
        default="Config/Localization.txt",
        help="Localization file to merge into when --merge-localization is used.",
    )
    parser.add_argument(
        "--update-windows-sources",
        action="store_true",
        help="Update active NewsWindow source URLs in windows.xml to generated local source path.",
    )
    parser.add_argument(
        "--windows-file",
        default="Config/XUi/windows.xml",
        help="XUi windows file to update when --update-windows-sources is used.",
    )
    parser.add_argument(
        "--windows-link-id",
        default="discord",
        help="Link id from source JSON used for NewsWindow source replacement (default: discord).",
    )
    parser.add_argument(
        "--windows-update-all-links",
        action="store_true",
        help="Update sources for all link ids in source JSON instead of only --windows-link-id.",
    )
    parser.add_argument(
        "--use-hc-dev-overrides",
        action="store_true",
        help=(
            "Use dev.highContrastPreview overrides from source JSON for temporary HC tuning. "
            "Default behavior keeps HC automatic."
        ),
    )
    parser.add_argument(
        "--texts-file",
        default="ESCMenu.texts.txt",
        help=(
            "Optional text-only overrides file for headers/page/link copy. "
            "If present, values are merged before generation (default: ESCMenu.texts.txt)."
        ),
    )
    parser.add_argument(
        "--init-texts-file",
        action="store_true",
        help="Write a starter text-only file from current source and exit.",
    )
    parser.add_argument(
        "--keep-obsolete-generated-keys",
        action="store_true",
        help=(
            "Keep old ESC-generated localization keys when merging. "
            "By default, obsolete ESC-generated keys are removed."
        ),
    )
    return parser.parse_args()


def load_source(config_path: Path) -> dict:
    if not config_path.exists():
        raise FileNotFoundError(f"Config not found: {config_path}")
    with config_path.open("r", encoding="utf-8") as f:
        data = json.load(f)

    if not isinstance(data.get("tabs"), list) or not data["tabs"]:
        raise ValueError("Source JSON must include a non-empty 'tabs' list.")
    if not isinstance(data.get("labels"), dict):
        raise ValueError("Source JSON must include a 'labels' object.")
    if not isinstance(data.get("links", []), list):
        raise ValueError("Source JSON 'links' must be a list.")

    ui = data.get("ui", {})
    if ui and not isinstance(ui, dict):
        raise ValueError("Source JSON 'ui' must be an object when provided.")

    layout = ui.get("layout", {}) if isinstance(ui, dict) else {}
    if layout and not isinstance(layout, dict):
        raise ValueError("Source JSON 'ui.layout' must be an object when provided.")

    themes = data.get("themes", {})
    if themes and not isinstance(themes, dict):
        raise ValueError("Source JSON 'themes' must be an object when provided.")

    for theme_name in ("color", "highContrast"):
        if theme_name in themes and not isinstance(themes.get(theme_name), dict):
            raise ValueError(f"Source JSON 'themes.{theme_name}' must be an object when provided.")

    if isinstance(themes, dict) and isinstance(themes.get("color"), dict):
        _validate_rgb_theme_block(themes["color"], "themes.color")

    dev = data.get("dev", {})
    if dev and not isinstance(dev, dict):
        raise ValueError("Source JSON 'dev' must be an object when provided.")
    if isinstance(dev, dict) and "highContrastPreview" in dev and not isinstance(
        dev.get("highContrastPreview"), dict
    ):
        raise ValueError("Source JSON 'dev.highContrastPreview' must be an object when provided.")
    if isinstance(dev, dict) and isinstance(dev.get("highContrastPreview"), dict):
        _validate_rgb_theme_block(dev["highContrastPreview"], "dev.highContrastPreview")

    return data


def normalize_text(value: str) -> str:
    """Store line breaks in localization-friendly escaped format."""
    return value.replace("\r\n", "\n").replace("\n", "\\n")


def _fetch_body_text_from_url(url: str, timeout_seconds: float = 8.0) -> str | None:
    try:
        with urllib.request.urlopen(url, timeout=timeout_seconds) as response:
            raw = response.read()
            encoding = response.headers.get_content_charset() or "utf-8"
            text = raw.decode(encoding, errors="replace")
            return text.replace("\ufeff", "")
    except (urllib.error.URLError, ValueError):
        print(f"Warning: unable to fetch page body from URL: {url}")
        return None


def _apply_inline_body_styles(text: str, bold_rgb: str, highlight_rgb: str) -> str:
    if not text:
        return text

    def _replace_bold(match: re.Match[str]) -> str:
        value = (match.group(1) or match.group(2) or "").strip()
        if not value:
            return ""
        return _colorize_text(value, bold_rgb)

    def _replace_highlight(match: re.Match[str]) -> str:
        value = (match.group(1) or match.group(2) or "").strip()
        if not value:
            return ""
        return _colorize_text(value, highlight_rgb)

    updated = BODY_BOLD_TOKEN_RE.sub(_replace_bold, text)
    updated = BODY_HIGHLIGHT_TOKEN_RE.sub(_replace_highlight, updated)
    return updated


def _resolve_page_body_text(row: dict) -> str | None:
    if "bodySourceUrl" in row:
        url = str(row.get("bodySourceUrl") or "").strip()
        if url:
            fetched = _fetch_body_text_from_url(url)
            if fetched is not None:
                return fetched

    if "bodyLines" in row:
        body_lines = row.get("bodyLines")
        if body_lines is None:
            return ""
        if not isinstance(body_lines, list):
            raise ValueError("Texts page 'bodyLines' must be a list when provided.")
        text = "\n".join(str(line) for line in body_lines)
        return text.replace("\\n", "\n")

    return None


def _safe_font_size(value: object, default: int) -> int:
    try:
        size = int(str(value).strip())
    except (ValueError, TypeError, AttributeError):
        return default
    return max(12, min(72, size))


def get_link_layout_limit(source: dict) -> int:
    ui = source.get("ui", {})
    layout = ui.get("layout", {}) if isinstance(ui, dict) else {}
    try:
        value = int(layout.get("maxLinks", DEFAULT_MAX_LINKS))
    except (TypeError, ValueError):
        value = DEFAULT_MAX_LINKS
    return max(1, min(16, value))


def build_texts_payload(source: dict) -> dict:
    """Create a user-editable payload containing only text content fields."""
    labels = source.get("labels", {}) if isinstance(source.get("labels", {}), dict) else {}
    tabs = source.get("tabs", []) if isinstance(source.get("tabs", []), list) else []

    label_keys = [
        "headerTitle",
        "headerMotto",
        "headerTitleHC",
        "headerMottoHC",
    ]

    payload_headers: dict[str, str] = {}
    for key in label_keys:
        if key in labels:
            payload_headers[key] = str(labels.get(key) or "")

    payload_pages: list[dict[str, str | int]] = []
    for idx, tab in enumerate(tabs, start=1):
        if not isinstance(tab, dict):
            continue
        body_text = str(tab.get("body") or "").replace("\r\n", "\n")
        body_text = body_text.replace("\\n", "\n")
        row: dict[str, str | int] = {
            "id": idx,
            "title": str(tab.get("title") or ""),
            "bodyLines": body_text.split("\n"),
            "titleFontSize": _safe_font_size(
                tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
                DEFAULT_PAGE_TITLE_FONT_SIZE,
            ),
            "bodyFontSize": _safe_font_size(
                tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
                DEFAULT_PAGE_BODY_FONT_SIZE,
            ),
        }
        payload_pages.append(row)

    source_links = source.get("links", []) if isinstance(source.get("links", []), list) else []
    payload_links: list[dict[str, str | int]] = []
    for link in source_links:
        if not isinstance(link, dict):
            continue
        if link.get("enabled", True) is False:
            continue
        url = str(link.get("url") or "").strip()
        if not url:
            continue
        idx = len(payload_links) + 1
        label = str(link.get("label") or link.get("title") or f"Link {idx}")
        payload_links.append({"id": idx, "label": label, "url": url})

    source_themes = source.get("themes", {}) if isinstance(source.get("themes", {}), dict) else {}
    theme_color = source_themes.get("color", {}) if isinstance(source_themes.get("color", {}), dict) else {}

    color_keys = [
        "headerBackgroundColor",
        "pageBackgroundColor",
        "headerTitleColor",
        "headerMottoColor",
        "tabButtonSelectedColor",
        "pageTitleColor",
        "pageBodyColor",
        "textBoldingColor",
        "textHighlightColor",
    ]
    payload_colors: dict[str, str] = {}
    for key in color_keys:
        if key in theme_color:
            payload_colors[key] = str(theme_color.get(key) or "")

    return {
        "_comment": "Edit text values here. Generator maps these into localized output automatically.",
        "colors": payload_colors,
        "headers": payload_headers,
        "pages": payload_pages,
        "links": payload_links,
    }


def _build_texts_markdown(payload: dict) -> str:
    colors = payload.get("colors", {}) if isinstance(payload.get("colors", {}), dict) else {}
    headers = payload.get("headers", {}) if isinstance(payload.get("headers", {}), dict) else {}
    pages = payload.get("pages", []) if isinstance(payload.get("pages", []), list) else []
    links = payload.get("links", []) if isinstance(payload.get("links", []), list) else []

    lines: list[str] = [
        "# Colors",
    ]
    for key, value in colors.items():
        lines.append(f"{key}: {str(value)}")

    lines.append("")
    lines.append("# Headers")

    for key, value in headers.items():
        lines.append(f"{key}: {str(value)}")

    lines.append("")
    lines.append("# Links")
    for row in links:
        if not isinstance(row, dict):
            continue
        link_id = row.get("id")
        label = str(row.get("label") or "")
        url = str(row.get("url") or "")
        lines.append(f"{link_id} | {label} | {url}")

    lines.append("")
    lines.append("# Pages")
    for row in pages:
        if not isinstance(row, dict):
            continue
        page_id = row.get("id")
        title = str(row.get("title") or "")
        lines.append("")
        lines.append(f"## {page_id}: {title}")
        if "titleFontSize" in row:
            lines.append(f"@titleFontSize: {row.get('titleFontSize')}")
        if "bodyFontSize" in row:
            lines.append(f"@bodyFontSize: {row.get('bodyFontSize')}")
        body_lines = row.get("bodyLines")
        if isinstance(body_lines, list):
            for body_line in body_lines:
                lines.append(str(body_line))
        else:
            lines.append(str(row.get("body") or ""))

    lines.append("")
    return "\n".join(lines)


def _parse_texts_markdown(text: str) -> dict:
    colors: dict[str, str] = {}
    headers: dict[str, str] = {}
    links: list[dict[str, str | int]] = []
    pages: list[dict[str, str | int | list[str]]] = []

    section: str | None = None
    current_page: dict[str, str | int | list[str]] | None = None
    in_comment_block = False
    pending_page_settings: dict[str, int] = {}
    expecting_page_header = False

    def _trim_page_body_trailing_blanks(page: dict[str, str | int | list[str]] | None) -> None:
        if page is None:
            return
        body_lines = page.get("bodyLines")
        if not isinstance(body_lines, list):
            return
        while body_lines and not str(body_lines[-1]).strip():
            body_lines.pop()

    for raw_line in text.splitlines():
        line = raw_line.rstrip("\r")
        stripped = line.strip()

        if stripped.startswith("<!--"):
            in_comment_block = True
        if in_comment_block:
            if "-->" in stripped:
                in_comment_block = False
            continue

        if stripped.startswith("# "):
            if current_page is not None:
                _trim_page_body_trailing_blanks(current_page)
                pages.append(current_page)
                current_page = None
            heading = stripped[2:].strip().lower()
            if heading == "colors":
                section = "colors"
            elif heading == "headers":
                section = "headers"
            elif heading == "links":
                section = "links"
            elif heading == "pages":
                section = "pages"
            else:
                section = None
            continue

        if section == "pages" and stripped.startswith("## "):
            if current_page is not None:
                _trim_page_body_trailing_blanks(current_page)
                pages.append(current_page)
            page_header = stripped[3:].strip()
            page_id_text, _, page_title = page_header.partition(":")
            page_id_text = page_id_text.strip()
            page_title = page_title.strip()
            try:
                page_id = int(page_id_text)
            except ValueError:
                continue
            current_page = {"id": page_id, "title": page_title, "bodyLines": []}
            if "titleFontSize" in pending_page_settings:
                current_page["titleFontSize"] = pending_page_settings["titleFontSize"]
            if "bodyFontSize" in pending_page_settings:
                current_page["bodyFontSize"] = pending_page_settings["bodyFontSize"]
            pending_page_settings = {}
            expecting_page_header = False
            continue

        if section == "colors":
            if not stripped or stripped.startswith("//"):
                continue
            key, sep, value = line.partition(":")
            if not sep:
                continue
            colors[key.strip()] = value.strip()
            continue

        if section == "headers":
            if not stripped or stripped.startswith("//"):
                continue
            key, sep, value = line.partition(":")
            if not sep:
                continue
            headers[key.strip()] = value.strip()
            continue

        if section == "links":
            if not stripped or stripped.startswith("//"):
                continue

            if "|" in line:
                parts = [part.strip() for part in line.split("|", 2)]
                if len(parts) < 2:
                    continue
                id_text = parts[0]
                label_text = parts[1]
                url_text = parts[2] if len(parts) > 2 else ""
                try:
                    link_id = int(id_text.strip())
                except ValueError:
                    continue
                links.append({"id": link_id, "label": label_text, "url": url_text})
                continue

            id_text, sep, value = line.partition(":")
            if not sep:
                continue
            try:
                link_id = int(id_text.strip())
            except ValueError:
                continue
            links.append({"id": link_id, "label": value.strip()})
            continue

        if section == "pages" and current_page is not None:
            if stripped.startswith("//") or BODY_BOUNDARY_MARKER_RE.match(stripped):
                continue
            if PAGE_BANNER_RE.match(stripped) or SEPARATOR_LINE_RE.match(stripped):
                expecting_page_header = True
                continue
            if stripped.startswith("@bodySourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodySourceUrl"] = value.strip()
                continue
            if stripped.startswith("@titleFontSize:"):
                _, _, value = stripped.partition(":")
                parsed_size = _safe_font_size(value.strip(), DEFAULT_PAGE_TITLE_FONT_SIZE)
                if expecting_page_header:
                    pending_page_settings["titleFontSize"] = parsed_size
                else:
                    current_page["titleFontSize"] = parsed_size
                continue
            if stripped.startswith("@bodyFontSize:"):
                _, _, value = stripped.partition(":")
                parsed_size = _safe_font_size(value.strip(), DEFAULT_PAGE_BODY_FONT_SIZE)
                if expecting_page_header:
                    pending_page_settings["bodyFontSize"] = parsed_size
                else:
                    current_page["bodyFontSize"] = parsed_size
                continue
            body_lines = current_page.get("bodyLines")
            if isinstance(body_lines, list):
                body_lines.append(line)

        if section == "pages" and current_page is None:
            if stripped.startswith("@titleFontSize:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["titleFontSize"] = _safe_font_size(
                    value.strip(),
                    DEFAULT_PAGE_TITLE_FONT_SIZE,
                )
                continue
            if stripped.startswith("@bodyFontSize:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyFontSize"] = _safe_font_size(
                    value.strip(),
                    DEFAULT_PAGE_BODY_FONT_SIZE,
                )
                continue
            if stripped.startswith("//") or BODY_BOUNDARY_MARKER_RE.match(stripped):
                continue
            if PAGE_BANNER_RE.match(stripped) or SEPARATOR_LINE_RE.match(stripped):
                expecting_page_header = True
                continue

    if current_page is not None:
        _trim_page_body_trailing_blanks(current_page)
        pages.append(current_page)

    return {
        "colors": colors,
        "headers": headers,
        "pages": pages,
        "links": links,
    }


def _load_texts_payload(texts_path: Path) -> dict:
    suffix = texts_path.suffix.lower()
    if suffix == ".txt":
        text = texts_path.read_text(encoding="utf-8")
        return _parse_texts_markdown(text)

    raise ValueError(
        f"Unsupported texts file format: {texts_path}. Use .txt"
    )


def write_texts_file(texts_path: Path, source: dict) -> None:
    texts_path.parent.mkdir(parents=True, exist_ok=True)
    payload = build_texts_payload(source)
    if texts_path.suffix.lower() == ".txt":
        texts_path.write_text(_build_texts_markdown(payload), encoding="utf-8")
        return

    raise ValueError(f"Unsupported texts file format for init: {texts_path}. Use .txt")


def apply_text_overrides(source: dict, texts_path: Path) -> tuple[dict, int]:
    """Merge optional text-only overrides into source structure.

    Returns updated source and count of applied fields.
    """
    if not texts_path.exists():
        return source, 0

    payload = _load_texts_payload(texts_path)

    updated = copy.deepcopy(source)
    applied = 0

    colors_overrides = payload.get("colors", {})
    if colors_overrides and not isinstance(colors_overrides, dict):
        raise ValueError(f"Texts file 'colors' must be an object: {texts_path}")

    if isinstance(colors_overrides, dict):
        themes = updated.setdefault("themes", {})
        theme_color = themes.setdefault("color", {})
        for key, value in colors_overrides.items():
            if key in {
                "headerBackgroundColor",
                "pageBackgroundColor",
                "headerTitleColor",
                "headerMottoColor",
                "tabButtonSelectedColor",
                "pageTitleColor",
                "pageBodyColor",
                "textBoldingColor",
                "textHighlightColor",
            }:
                theme_color[key] = str(value)
                applied += 1

    headers_overrides = payload.get("headers", {})
    if headers_overrides and not isinstance(headers_overrides, dict):
        raise ValueError(f"Texts file 'headers' must be an object: {texts_path}")

    labels = updated.setdefault("labels", {})
    for key, value in headers_overrides.items() if isinstance(headers_overrides, dict) else []:
        if key in {"optionsTitle", "optionsTitleHC", "linkLabel", "linkLabelHC"}:
            # Options section title is feature-owned, not part of user text overrides.
            continue
        labels[key] = str(value)
        applied += 1

    pages_overrides_raw = payload.get("pages")
    if pages_overrides_raw is not None and not isinstance(pages_overrides_raw, list):
        raise ValueError(f"Texts file 'pages' must be a list: {texts_path}")

    existing_tabs = updated.get("tabs", []) if isinstance(updated.get("tabs", []), list) else []
    if isinstance(pages_overrides_raw, list):
        if not pages_overrides_raw:
            raise ValueError("Texts file must include at least one page in 'pages'.")

        new_tabs: list[dict] = []
        for i, row in enumerate(pages_overrides_raw):
            if not isinstance(row, dict):
                continue

            target_idx: int | None = None
            row_id = row.get("id")
            try:
                if row_id is not None and str(row_id).strip() != "":
                    parsed = int(str(row_id).strip())
                    if parsed >= 1:
                        target_idx = parsed - 1
            except ValueError:
                target_idx = None

            if target_idx is None:
                target_idx = i

            source_tab: dict = {}
            if 0 <= target_idx < len(existing_tabs) and isinstance(existing_tabs[target_idx], dict):
                source_tab = existing_tabs[target_idx]

            page_num = len(new_tabs) + 1
            title_value = str(row.get("title") or source_tab.get("title") or source_tab.get("button") or f"Page {page_num}")
            resolved_body = _resolve_page_body_text(row)
            body_value = resolved_body if resolved_body is not None else str(source_tab.get("body") or "")

            new_tab = {
                "id": str(source_tab.get("id") or f"page_{page_num}"),
                "button": title_value,
                "title": title_value,
                "body": body_value,
                "titleFontSize": _safe_font_size(
                    row.get("titleFontSize", source_tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE)),
                    DEFAULT_PAGE_TITLE_FONT_SIZE,
                ),
                "bodyFontSize": _safe_font_size(
                    row.get("bodyFontSize", source_tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE)),
                    DEFAULT_PAGE_BODY_FONT_SIZE,
                ),
            }
            new_tabs.append(new_tab)
            applied += 5

        tab_layout = get_tab_layout(updated)
        validate_tab_count(len(new_tabs), tab_layout)
        updated["tabs"] = new_tabs

    links_overrides = payload.get("links", [])
    if links_overrides and not isinstance(links_overrides, list):
        raise ValueError(f"Texts file 'links' must be a list: {texts_path}")

    existing_links = updated.get("links", []) if isinstance(updated.get("links", []), list) else []
    active_existing_links: list[dict] = []
    for link in existing_links:
        if not isinstance(link, dict):
            continue
        if link.get("enabled", True) is False:
            continue
        if not str(link.get("url") or "").strip():
            continue
        active_existing_links.append(link)

    if isinstance(links_overrides, list):
        max_links = get_link_layout_limit(updated)
        if len(links_overrides) > max_links:
            raise ValueError(
                f"Too many links for configured layout: links={len(links_overrides)}, allowed={max_links}. "
                "Reduce links in texts file or increase ui.layout.maxLinks."
            )

        new_links: list[dict] = []
        for i, row in enumerate(links_overrides):
            if not isinstance(row, dict):
                continue

            target_pos: int | None = None
            row_id = row.get("id")
            try:
                if row_id is not None and str(row_id).strip() != "":
                    parsed = int(str(row_id).strip())
                    if parsed >= 1:
                        target_pos = parsed - 1
            except ValueError:
                target_pos = None
            if target_pos is None:
                target_pos = i

            source_link: dict = {}
            if 0 <= target_pos < len(active_existing_links):
                source_link = active_existing_links[target_pos]

            link_num = len(new_links) + 1
            label = str(row.get("label") or source_link.get("label") or source_link.get("title") or f"Link {link_num}")
            url = str(row.get("url") or source_link.get("url") or "").strip()
            if not url:
                continue

            stable_id = str(source_link.get("id") or slugify_token(label) or f"link_{link_num}")
            new_links.append(
                {
                    "id": stable_id,
                    "enabled": True,
                    "title": str(source_link.get("title") or label),
                    "label": label,
                    "url": url,
                }
            )
            applied += 2

        updated["links"] = new_links

    return updated, applied


def slugify_token(value: str) -> str:
    token = re.sub(r"[^a-z0-9]+", "_", value.strip().lower())
    token = re.sub(r"_+", "_", token).strip("_")
    return token or "tab"


def build_tab_models(source: dict) -> list[dict]:
    tabs = source.get("tabs", [])
    models: list[dict] = []
    used_ids: set[str] = set()

    for idx, tab in enumerate(tabs, start=1):
        base_id = str(tab.get("id") or "").strip() or str(tab.get("button") or f"tab_{idx}")
        tab_id = slugify_token(base_id)
        if tab_id in used_ids:
            suffix = 2
            while f"{tab_id}_{suffix}" in used_ids:
                suffix += 1
            tab_id = f"{tab_id}_{suffix}"
        used_ids.add(tab_id)

        button = str(tab.get("button") or f"Tab {idx}")
        title = str(tab.get("title") or button)
        body = str(tab.get("body") or "")
        title_font_size = _safe_font_size(
            tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
            DEFAULT_PAGE_TITLE_FONT_SIZE,
        )
        body_font_size = _safe_font_size(
            tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
            DEFAULT_PAGE_BODY_FONT_SIZE,
        )

        button_hc = str(tab.get("buttonHC") or button)
        title_hc = str(tab.get("titleHC") or title)
        body_hc = str(tab.get("bodyHC") or tab.get("body") or "")

        models.append(
            {
                "idx": idx,
                "id": tab_id,
                "button": button,
                "title": title,
                "body": body,
                "title_font_size": title_font_size,
                "body_font_size": body_font_size,
                "button_hc": button_hc,
                "title_hc": title_hc,
                "body_hc": body_hc,
            }
        )

    return models


def _parse_rgb_value(value: str) -> tuple[int, int, int] | None:
    token_map: dict[str, tuple[int, int, int]] = {
        "[white]": (255, 255, 255),
        "[black]": (0, 0, 0),
        "[lightgrey]": (211, 211, 211),
        "[lightgray]": (211, 211, 211),
        "[grey]": (128, 128, 128),
        "[gray]": (128, 128, 128),
        "[darkgrey]": (64, 64, 64),
        "[darkgray]": (64, 64, 64),
        "[darkestgrey]": (32, 32, 32),
        "[darkestgray]": (32, 32, 32),
    }

    text = value.strip().lower()
    if text in token_map:
        return token_map[text]

    parts = [p.strip() for p in value.split(",")]
    if len(parts) != 3:
        return None
    try:
        rgb = tuple(max(0, min(255, int(part))) for part in parts)
    except ValueError:
        return None
    return rgb[0], rgb[1], rgb[2]


def _bw_rgb_for_readability(value: str, fallback: str = "255, 255, 255") -> str:
    rgb = _parse_rgb_value(value)
    if rgb is None:
        return fallback

    # Perceived luminance; bright colors become black text, dark colors become white text.
    luminance = int((0.299 * rgb[0]) + (0.587 * rgb[1]) + (0.114 * rgb[2]))
    return "0, 0, 0" if luminance >= 180 else "255, 255, 255"


def _grayscale_from_color(value: str, fallback: str = "64, 64, 64") -> str:
    rgb = _parse_rgb_value(value)
    if rgb is None:
        return fallback

    luminance = int((0.299 * rgb[0]) + (0.587 * rgb[1]) + (0.114 * rgb[2]))
    # Keep HC background in a practical mid-range so it is not blown-out or crushed.
    grey = max(32, min(224, luminance))
    return f"{grey}, {grey}, {grey}"


def derive_auto_high_contrast_colors(theme_color: dict[str, str]) -> dict[str, str]:
    hc = dict(DEFAULT_AUTO_HIGH_CONTRAST_COLORS)

    hc["headerBackground"] = _grayscale_from_color(
        theme_color.get("headerBackground", hc["headerBackground"]),
        fallback=hc["headerBackground"],
    )
    hc["pageBackground"] = _grayscale_from_color(
        theme_color.get("pageBackground", hc["pageBackground"]),
        fallback=hc["pageBackground"],
    )

    header_text = _bw_rgb_for_readability(hc["headerBackground"], fallback="255, 255, 255")
    body_text = _bw_rgb_for_readability(hc["pageBackground"], fallback="255, 255, 255")

    hc["headerTitle"] = header_text
    hc["headerMotto"] = header_text
    hc["pageTitle"] = body_text
    hc["pageBody"] = body_text
    hc["textBolding"] = body_text
    hc["textHighlight"] = "0, 0, 0" if body_text == "255, 255, 255" else "255, 255, 255"

    # Keep selected tab fill in grayscale and opposite of page title text color for stronger contrast.
    hc["tabButtonSelected"] = "35, 35, 35" if hc["pageTitle"] == "0, 0, 0" else "220, 220, 220"
    return hc


def get_theme_colors(
    source: dict,
    theme_name: str,
    use_hc_dev_overrides: bool = False,
) -> dict[str, str]:
    if theme_name == "highContrast":
        # High Contrast is automatic and intentionally not user-configurable.
        derived = derive_auto_high_contrast_colors(get_theme_colors(source, "color"))

        if not use_hc_dev_overrides:
            return derived

        dev = source.get("dev", {})
        preview = dev.get("highContrastPreview", {}) if isinstance(dev, dict) else {}
        if not isinstance(preview, dict):
            return derived

        aliases = {
            "headerBackground": ["headerBackground", "headerBackgroundColor"],
            "pageBackground": ["pageBackground", "pageBackgroundColor"],
            "headerTitle": ["headerTitle", "headerTitleColor"],
            "headerMotto": ["headerMotto", "headerMottoColor"],
            "tabButtonSelected": ["tabButtonSelected", "tabButtonSelectedColor"],
            "pageTitle": ["pageTitle", "pageTitleColor"],
            "pageBody": ["pageBody", "pageBodyColor"],
            "textBolding": ["textBolding", "textBoldingColor"],
            "textHighlight": ["textHighlight", "textHighlightColor"],
        }

        merged = dict(derived)
        for canonical, names in aliases.items():
            for name in names:
                if name in preview:
                    merged[canonical] = str(preview[name])
                    break
        return merged

    themes = source.get("themes", {})
    theme_values = themes.get(theme_name, {}) if isinstance(themes, dict) else {}
    merged = dict(DEFAULT_THEME_COLORS)

    # Source uses strict v1 naming.
    aliases = {
        "headerBackground": ["headerBackground", "headerBackgroundColor"],
        "pageBackground": ["pageBackground", "pageBackgroundColor"],
        "headerTitle": ["headerTitle", "headerTitleColor"],
        "headerMotto": ["headerMotto", "headerMottoColor"],
        "tabButtonSelected": ["tabButtonSelected", "tabButtonSelectedColor"],
        "pageTitle": ["pageTitle", "pageTitleColor"],
        "pageBody": ["pageBody", "pageBodyColor"],
        "textBolding": ["textBolding", "textBoldingColor"],
        "textHighlight": ["textHighlight", "textHighlightColor"],
    }

    for canonical, names in aliases.items():
        for name in names:
            if name in theme_values:
                merged[canonical] = str(theme_values[name])
                break

    return merged


def get_tab_layout(source: dict) -> dict[str, int]:
    ui = source.get("ui", {})
    layout = ui.get("layout", {}) if isinstance(ui, dict) else {}

    merged = dict(DEFAULT_TAB_LAYOUT)
    for key in DEFAULT_TAB_LAYOUT:
        if key in layout:
            merged[key] = int(layout[key])

    available_width = max(1, merged["contentWidth"] - (2 * merged["contentPadding"]))
    max_fit_tabs = max(1, available_width // max(1, merged["buttonCellWidth"]))

    requested_max = int(layout.get("maxTabs", max_fit_tabs))
    merged["maxTabs"] = min(max_fit_tabs, max(1, requested_max))
    merged["maxFitTabs"] = max_fit_tabs

    return merged


def validate_tab_count(tab_count: int, tab_layout: dict[str, int]) -> None:
    allowed = tab_layout["maxTabs"]
    fit_limit = tab_layout["maxFitTabs"]
    if tab_count > allowed:
        raise ValueError(
            "Too many tabs for configured layout: "
            f"tabs={tab_count}, allowed={allowed}, fit_limit={fit_limit}. "
            "Increase ui.layout.contentWidth, decrease ui.layout.buttonCellWidth, "
            "or reduce number of tabs."
        )


def build_localization_rows(source: dict, use_hc_dev_overrides: bool = False) -> list[list[str]]:
    labels = source["labels"]
    tab_models = build_tab_models(source)
    theme_color = get_theme_colors(source, "color")
    theme_hc = get_theme_colors(source, "highContrast", use_hc_dev_overrides=use_hc_dev_overrides)

    header_title = labels.get("headerTitle", "")
    header_motto = labels.get("headerMotto", "")
    options_title = labels.get("optionsTitle", "Options")
    # HC text mirrors Color text; only colors differ.
    header_title_hc = header_title
    header_motto_hc = header_motto
    options_title_hc = options_title

    active_links: list[dict] = []
    for link in source.get("links", []):
        if not isinstance(link, dict):
            continue
        if link.get("enabled", True) is False:
            continue
        if not str(link.get("url") or "").strip():
            continue
        active_links.append(link)

    header_title_colored = _colorize_text(header_title, theme_color["headerTitle"])
    header_motto_colored = _colorize_text(header_motto, theme_color["headerMotto"])
    header_title_hc_colored = _colorize_text(header_title_hc, theme_hc["headerTitle"])
    header_motto_hc_colored = _colorize_text(header_motto_hc, theme_hc["headerMotto"])
    options_title_colored = _colorize_text(options_title, theme_color["textBolding"])
    options_title_hc_colored = _colorize_text(options_title_hc, theme_hc["textBolding"])

    rows: list[list[str]] = [
        ["Key", "Context", "English"],
        ["windowESC_color_header_title", "UI", header_title_colored],
        ["windowESC_color_header_motto", "UI", header_motto_colored],
        ["windowESC_color_options_title", "UI", options_title_colored],
        ["windowESC_highcontrast_header_title", "UI", header_title_hc_colored],
        ["windowESC_highcontrast_header_motto", "UI", header_motto_hc_colored],
        ["windowESC_highcontrast_options_title", "UI", options_title_hc_colored],
    ]

    if active_links:
        for link_idx, link in enumerate(active_links, start=1):
            fallback_label = str(link.get("title") or f"Link {link_idx}")
            link_label = str(link.get("label") or link.get("title") or fallback_label)
            link_label_hc = link_label
            rows.append(
                [
                    f"windowESC_color_link_{link_idx}",
                    "UI",
                    _colorize_text(link_label, theme_color["textHighlight"]),
                ]
            )
            rows.append(
                [
                    f"windowESC_highcontrast_link_{link_idx}",
                    "UI",
                    _colorize_text(link_label_hc, theme_hc["textHighlight"]),
                ]
            )
    else:
        # Keep link_1 keys present even when no active links are configured.
        fallback_label = labels.get("linkLabel", "")
        fallback_label_hc = fallback_label
        rows.append(
            [
                "windowESC_color_link_1",
                "UI",
                _colorize_text(fallback_label, theme_color["textHighlight"]),
            ]
        )
        rows.append(
            [
                "windowESC_highcontrast_link_1",
                "UI",
                _colorize_text(fallback_label_hc, theme_hc["textHighlight"]),
            ]
        )

    for tab in tab_models:
        idx = tab["idx"]
        body_color = normalize_text(
            _apply_inline_body_styles(
                tab["body"],
                theme_color["textBolding"],
                theme_color["textHighlight"],
            )
        )
        body_hc = normalize_text(
            _apply_inline_body_styles(
                tab["body"],
                theme_hc["textBolding"],
                theme_hc["textHighlight"],
            )
        )

        # Canonical window-scoped keys for readability in XUi and localization.
        rows.append(
            [
                f"windowESC_color_page_{idx}_button",
                "UI",
                _colorize_text(tab["button"], theme_color["textBolding"]),
            ]
        )
        rows.append(
            [
                f"windowESC_color_page_{idx}_title",
                "UI",
                _colorize_text(tab["title"], theme_color["pageTitle"]),
            ]
        )
        rows.append([f"windowESC_color_page_{idx}_body", "UI", body_color])
        rows.append(
            [
                f"windowESC_highcontrast_page_{idx}_button",
                "UI",
                _colorize_text(tab["button"], theme_hc["textBolding"]),
            ]
        )
        rows.append(
            [
                f"windowESC_highcontrast_page_{idx}_title",
                "UI",
                _colorize_text(tab["title"], theme_hc["pageTitle"]),
            ]
        )
        rows.append([f"windowESC_highcontrast_page_{idx}_body", "UI", body_hc])

    return rows


def build_tabs_ui_snippet(source: dict, use_hc_dev_overrides: bool = False) -> str:
    tab_models = build_tab_models(source)
    tab_layout = get_tab_layout(source)
    validate_tab_count(len(tab_models), tab_layout)
    theme_color = get_theme_colors(source, "color")
    theme_hc = get_theme_colors(source, "highContrast", use_hc_dev_overrides=use_hc_dev_overrides)

    tab_count = max(1, len(tab_models))
    cell_width = tab_layout["buttonCellWidth"]
    total_width = tab_count * cell_width
    start_x = max(
        tab_layout["contentPadding"],
        (tab_layout["contentWidth"] - total_width) // 2,
    )

    def build_pages(is_hc: bool) -> str:
        pages: list[str] = []
        for tab in tab_models:
            idx = tab["idx"]
            mode_prefix = "windowESC_highcontrast" if is_hc else "windowESC_color"
            title_key = f"{mode_prefix}_page_{idx}_title"
            body_key = f"{mode_prefix}_page_{idx}_body"
            button_key = f"{mode_prefix}_page_{idx}_button"
            page_name = f"page_{idx}_hc" if is_hc else f"page_{idx}"
            title_name = f"page_{idx}_hc_title" if is_hc else f"page_{idx}_title"
            body_name = f"page_{idx}_hc_body" if is_hc else f"page_{idx}_body"
            pages.extend(
                [
                    f'\t\t<rect name="{page_name}" controller="TabSelectorTab" tab_key="{button_key}">',
                    f'\t\t\t<label name="{title_name}" depth="10" pos="0,-102" justify="center" effect="Outline8" effect_color="0,0,0,255" effect_distance="1,1" color="" font_size="{tab["title_font_size"]}" text_key="{title_key}" foregroundlayer="true"/>',
                    f'\t\t\t<label name="{body_name}" depth="10" pos="20,-164" width="1060" justify="left" effect="Outline8" effect_color="0,0,0,255" effect_distance="1,1" color="" font_size="{tab["body_font_size"]}" text_key="{body_key}" foregroundlayer="true"/>',
                    "\t\t</rect>",
                ]
            )
        return "\n".join(pages)

    button_width = max(1, tab_layout["buttonCellWidth"] - 4)
    lines = [
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
        "<generatedTabs>",
        "<!-- Generated tab layout snippet -->",
        f"<!-- tab_count={tab_count}, max_tabs={tab_layout['maxTabs']}, max_fit_tabs={tab_layout['maxFitTabs']} -->",
        "",
        "<!-- Color: tabsHeader -->",
        "<rect name=\"tabsHeader\">",
        f'\t<grid name="tabButtons" pos="{start_x},{tab_layout["buttonPosY"]}" depth="3" rows="1" cols="{tab_count}" cell_width="{tab_layout["buttonCellWidth"]}" cell_height="{tab_layout["buttonHeight"]}" repeat_content="true" arrangement="horizontal">',
        "\t\t<rect controller=\"TabSelectorButton\">",
        f'\t\t\t<simplebutton name="tabButton" depth="8" pos="5,0" width="{button_width}" height="{tab_layout["buttonHeight"]}" font_size="20" sprite="ui_game_header_fill" bordercolor="[black]" defaultcolor="[darkestGrey]" selectedsprite="ui_game_header_fill" selectedcolor="{theme_color["tabButtonSelected"]}" foregroundlayer="false" caption="{{tab_name_localized}}"/>',
        "\t\t</rect>",
        "\t</grid>",
        "</rect>",
        "",
        "<!-- Color: tabsContents -->",
        "<rect name=\"tabsContents\">",
        build_pages(is_hc=False),
        "</rect>",
        "",
        "<!-- HighContrast: tabsHeader -->",
        "<rect name=\"tabsHeader\">",
        f'\t<grid name="tabButtons" pos="{start_x},{tab_layout["buttonPosY"]}" depth="3" rows="1" cols="{tab_count}" cell_width="{tab_layout["buttonCellWidth"]}" cell_height="{tab_layout["buttonHeight"]}" repeat_content="true" arrangement="horizontal">',
        "\t\t<rect controller=\"TabSelectorButton\">",
        f'\t\t\t<simplebutton name="tabButton" depth="8" pos="5,0" width="{button_width}" height="{tab_layout["buttonHeight"]}" font_size="20" sprite="ui_game_header_fill" bordercolor="[black]" defaultcolor="[darkestGrey]" selectedsprite="ui_game_header_fill" selectedcolor="{theme_hc["tabButtonSelected"]}" foregroundlayer="false" caption="{{tab_name_localized}}"/>',
        "\t\t</rect>",
        "\t</grid>",
        "</rect>",
        "",
        "<!-- HighContrast: tabsContents -->",
        "<rect name=\"tabsContents\">",
        build_pages(is_hc=True),
        "</rect>",
        "</generatedTabs>",
    ]
    return "\n".join(lines) + "\n"


def write_tabs_ui_snippet(source: dict, out_dir: Path, use_hc_dev_overrides: bool = False) -> Path:
    snippet_path = out_dir / "XUi.Tabs.Generated.xml"
    snippet_path.write_text(
        build_tabs_ui_snippet(source, use_hc_dev_overrides=use_hc_dev_overrides),
        encoding="utf-8",
    )
    return snippet_path


def write_localization(rows: list[list[str]], out_dir: Path) -> Path:
    out_file = out_dir / "Localization.ESC.Generated.txt"
    out_dir.mkdir(parents=True, exist_ok=True)

    ordered_rows = rows
    if rows:
        header = rows[0]
        body = rows[1:]
        body_sorted = sorted(body, key=lambda r: str(r[0] if len(r) > 0 else "").lower())
        ordered_rows = [header, *body_sorted]

    with out_file.open("w", encoding="utf-8", newline="") as f:
        for idx, row in enumerate(ordered_rows):
            key = row[0] if len(row) > 0 else ""
            context = row[1] if len(row) > 1 else ""
            english = row[2] if len(row) > 2 else ""
            key = str(key).replace("\n", " ")
            context = str(context).replace("\n", " ")
            english = str(english)

            if idx == 0:
                # Keep header plain: Key,Context,English
                f.write(f"{key},{context},{english}\n")
                continue

            english = english.replace('"', '""')
            f.write(f"{key},{context},\"{english}\"\n")

    return out_file


def _is_esc_generated_managed_key(key: str) -> bool:
    """Keys managed by this ESC generator, including older generations."""
    if key.startswith("windowESC_"):
        return True
    if key.startswith("esc_tab_"):
        return True
    if key in {
        "esc_header_title",
        "esc_header_motto",
        "esc_link_label",
        "esc_header_title_hc",
        "esc_header_motto_hc",
        "esc_link_label_hc",
        "esc_headerTitle",
        "esc_headerMotto",
        "esc_headerTitleHC",
        "esc_headerMottoHC",
    }:
        return True
    if re.match(r"^tab(ButtonTitle|Title|Body)\d+(HC)?$", key):
        return True
    return False


def merge_localization(
    rows: list[list[str]],
    target_file: Path,
    remove_obsolete_generated_keys: bool = True,
) -> tuple[int, int, int]:
    """Merge generated rows into target localization file.

    Returns:
    - number of updated keys
    - number of added keys
    - number of removed obsolete generated keys
    """
    target_file.parent.mkdir(parents=True, exist_ok=True)

    existing_rows: list[list[str]] = []
    if target_file.exists():
        with target_file.open("r", encoding="utf-8", newline="") as f:
            existing_rows = [row for row in csv.reader(f) if row]

    if not existing_rows:
        existing_rows = [["Key", "Context", "English"]]

    header = existing_rows[0]
    generated_header = rows[0]
    generated_body = rows[1:]

    key_col = 0
    context_col = 1
    english_col = 2

    index_by_key: dict[str, int] = {}
    for i in range(1, len(existing_rows)):
        row = existing_rows[i]
        if len(row) > key_col and row[key_col]:
            index_by_key[row[key_col]] = i

    updated = 0
    added = 0
    removed = 0

    for grow in generated_body:
        key = grow[key_col] if len(grow) > key_col else ""
        if not key:
            continue

        if key in index_by_key:
            idx = index_by_key[key]
            row = existing_rows[idx]
            while len(row) < len(header):
                row.append("")

            new_context = grow[context_col] if len(grow) > context_col else ""
            new_english = grow[english_col] if len(grow) > english_col else ""

            changed = False
            if row[context_col] != new_context:
                row[context_col] = new_context
                changed = True
            if row[english_col] != new_english:
                row[english_col] = new_english
                changed = True
            if changed:
                updated += 1
        else:
            new_row = [""] * len(header)
            new_row[key_col] = key
            new_row[context_col] = grow[context_col] if len(grow) > context_col else ""
            new_row[english_col] = grow[english_col] if len(grow) > english_col else ""
            existing_rows.append(new_row)
            index_by_key[key] = len(existing_rows) - 1
            added += 1

    if remove_obsolete_generated_keys:
        generated_keys = {
            grow[key_col]
            for grow in generated_body
            if len(grow) > key_col and grow[key_col]
        }

        filtered_rows = [existing_rows[0]]
        for row in existing_rows[1:]:
            key = row[key_col] if len(row) > key_col else ""
            if key and _is_esc_generated_managed_key(key) and key not in generated_keys:
                removed += 1
                continue
            filtered_rows.append(row)
        existing_rows = filtered_rows

    with target_file.open("w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f, quoting=csv.QUOTE_MINIMAL)
        writer.writerows(existing_rows)

    return updated, added, removed


def news_xml_content(title: str, url: str, timestamp: str) -> str:
    safe_title = escape(title)
    safe_url = escape(url)

    return (
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        "<news>\n"
        "  <entry>\n"
        f"    <link>{safe_url}</link>\n"
        f"    <date>{timestamp}</date>\n"
        f"    <title>{safe_title}</title>\n"
        "    <text></text>\n"
        "    <imagerelpath></imagerelpath>\n"
        "  </entry>\n"
        "</news>\n"
    )


def write_news_files(source: dict, out_dir: Path) -> list[Path]:
    created: list[Path] = []
    news_dir = out_dir / "News"
    news_dir.mkdir(parents=True, exist_ok=True)

    now_utc = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d %H:%M:%SZ")

    for link in source.get("links", []):
        if link.get("enabled", True) is False:
            continue

        link_id = (link.get("id") or "link").strip()
        title = (link.get("title") or "Open Link").strip()
        url = (link.get("url") or "").strip()
        if not url:
            continue

        safe_id = "".join(ch for ch in link_id if ch.isalnum() or ch in ("-", "_")) or "link"
        out_file = news_dir / f"{safe_id}.xml"
        out_file.write_text(news_xml_content(title, url, now_utc), encoding="utf-8")
        created.append(out_file)

    return created


def _safe_link_id(link_id: str) -> str:
    return "".join(ch for ch in link_id if ch.isalnum() or ch in ("-", "_")) or "link"


def resolve_mod_folder_name(source: dict, config_path: Path) -> str:
    """Resolve @modfolder token for local NewsWindow references.

    Priority:
    1) source.meta.modFolder (explicit)
    2) folder containing ESCMenu.source.json
    3) source.meta.name
    """
    meta = source.get("meta", {}) if isinstance(source.get("meta", {}), dict) else {}
    explicit = str(meta.get("modFolder") or "").strip()
    if explicit:
        return explicit

    folder_name = config_path.parent.name.strip()
    if folder_name:
        return folder_name

    return str(meta.get("name") or "AGF-4Modders-ESCWindowPlus").strip()


def build_local_news_source_map(source: dict, mod_folder_name: str) -> dict[str, str]:
    source_map: dict[str, str] = {}
    for link in source.get("links", []):
        if link.get("enabled", True) is False:
            continue

        link_id = (link.get("id") or "").strip()
        if not link_id:
            continue
        url = (link.get("url") or "").strip()
        if not url:
            # Do not map links that are not currently active/generated.
            continue
        source_map[link_id] = f"@{mod_folder_name}:Config/Generated/News/{_safe_link_id(link_id)}.xml"
    return source_map


def build_legacy_sources_by_id(source: dict) -> dict[str, list[str]]:
    legacy: dict[str, list[str]] = {}
    for link in source.get("links", []):
        if link.get("enabled", True) is False:
            continue

        link_id = (link.get("id") or "").strip()
        if not link_id:
            continue
        extra = link.get("legacySources", [])
        if not isinstance(extra, list):
            continue
        merged = list(legacy.get(link_id, []))
        for item in extra:
            text = str(item).strip()
            if text and text not in merged:
                merged.append(text)
        legacy[link_id] = merged
    return legacy


def update_windows_news_sources(
    windows_file: Path,
    local_sources_by_id: dict[str, str],
    legacy_sources_by_id: dict[str, list[str]],
    link_ids: list[str],
) -> dict[str, int]:
    """Update NewsWindow source values in windows XML for the requested link ids."""
    if not windows_file.exists():
        raise FileNotFoundError(f"Windows file not found: {windows_file}")

    text = windows_file.read_text(encoding="utf-8")
    replaced_by_id: dict[str, int] = {}
    total_replacements = 0

    for link_id in link_ids:
        if link_id not in local_sources_by_id:
            continue

        new_source = local_sources_by_id[link_id]
        safe_id = _safe_link_id(link_id)
        replaced = 0

        for legacy_source in legacy_sources_by_id.get(link_id, []):
            text, count_legacy = re.subn(
                r'(controller="NewsWindow"\s+sources=")'
                + re.escape(legacy_source)
                + r'(")',
                rf'\1{new_source}\2',
                text,
            )
            replaced += count_legacy

        # Normalize any existing local mapping for this link id to the latest mod name/path.
        local_pattern = re.compile(
            r'(controller="NewsWindow"\s+sources=")(@[^\"]+:Config/Generated/News/'
            + re.escape(safe_id)
            + r'\.xml)(")'
        )

        changed_local = 0

        def replace_local(match: re.Match[str]) -> str:
            nonlocal changed_local
            current_source = match.group(2)
            if current_source == new_source:
                return match.group(0)
            changed_local += 1
            return f"{match.group(1)}{new_source}{match.group(3)}"

        text = local_pattern.sub(replace_local, text)
        replaced += changed_local

        if replaced > 0:
            replaced_by_id[link_id] = replaced
            total_replacements += replaced

    if total_replacements > 0:
        windows_file.write_text(text, encoding="utf-8")

    return replaced_by_id


def build_news_source_mappings(news_paths: list[Path], mod_folder_name: str) -> list[str]:
    """Create ready-to-paste XUi NewsWindow source mapping lines."""
    mappings: list[str] = []
    for p in news_paths:
        link_id = p.stem
        rel = p.as_posix()
        mappings.append(f"{link_id} => @{mod_folder_name}:{rel}")
    return mappings


def write_manifest(
    source_path: Path,
    out_dir: Path,
    localization_path: Path,
    news_paths: list[Path],
    tabs_ui_snippet_path: Path,
    mod_folder_name: str,
    merge_target: Path | None,
    merge_result: tuple[int, int, int] | None,
) -> Path:
    manifest_path = out_dir / "ESC.Generated.Manifest.txt"
    mappings = build_news_source_mappings(news_paths, mod_folder_name)

    lines = [
        f"Generated: {dt.datetime.now().isoformat()}",
        f"Source: {source_path}",
        f"Mod Folder Token: @{mod_folder_name}:",
        f"Localization: {localization_path}",
        f"XUi Tabs Snippet: {tabs_ui_snippet_path}",
        "News Files:",
    ]
    if news_paths:
        lines.extend([f"- {p}" for p in news_paths])
    else:
        lines.append("- none")

    lines.append("")
    lines.append("NewsWindow Sources (copy/paste):")
    if mappings:
        lines.extend([f"- {line}" for line in mappings])
    else:
        lines.append("- none")

    lines.append("")
    lines.append("Localization Merge:")
    if merge_target is not None and merge_result is not None:
        updated, added, removed = merge_result
        lines.append(f"- Target: {merge_target}")
        lines.append(f"- Updated Keys: {updated}")
        lines.append(f"- Added Keys: {added}")
        lines.append(f"- Removed Obsolete Generated Keys: {removed}")
    else:
        lines.append("- Not run (use --merge-localization to apply).")

    lines.extend(
        [
            "",
            "Next Step:",
            "- If you want dynamic tab count/layout, paste XUi.Tabs.Generated.xml snippets into windows.xml tab sections.",
            "- Point NewsWindow sources in XUi to the mapping lines above.",
            "- Optionally run with --merge-localization to update your main localization file.",
        ]
    )

    manifest_path.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return manifest_path


def main() -> None:
    args = parse_args()

    config_path = Path(args.config)
    out_dir = Path(args.out_dir)
    texts_path = Path(args.texts_file)

    source = load_source(config_path)

    if args.init_texts_file:
        if texts_path.exists():
            raise FileExistsError(
                f"Texts file already exists: {texts_path}. Move/delete it first or choose --texts-file."
            )
        write_texts_file(texts_path, source)
        print(f"Created texts file: {texts_path}")
        return

    source, text_override_count = apply_text_overrides(source, texts_path)
    mod_folder_name = resolve_mod_folder_name(source, config_path)

    rows = build_localization_rows(source, use_hc_dev_overrides=args.use_hc_dev_overrides)
    localization_path = write_localization(rows, out_dir)
    news_paths = write_news_files(source, out_dir)
    tabs_ui_snippet_path = write_tabs_ui_snippet(
        source,
        out_dir,
        use_hc_dev_overrides=args.use_hc_dev_overrides,
    )

    merge_target: Path | None = None
    merge_result: tuple[int, int, int] | None = None
    if args.merge_localization:
        merge_target = Path(args.localization_file)
        merge_result = merge_localization(
            rows,
            merge_target,
            remove_obsolete_generated_keys=not args.keep_obsolete_generated_keys,
        )

    windows_file: Path | None = None
    windows_update_by_id: dict[str, int] = {}
    if args.update_windows_sources:
        windows_file = Path(args.windows_file)
        local_sources_by_id = build_local_news_source_map(source, mod_folder_name)
        legacy_sources_by_id = build_legacy_sources_by_id(source)

        if args.windows_update_all_links:
            link_ids = sorted(local_sources_by_id.keys())
        else:
            link_ids = [args.windows_link_id]

        windows_update_by_id = update_windows_news_sources(
            windows_file,
            local_sources_by_id,
            legacy_sources_by_id,
            link_ids,
        )

    manifest_path = write_manifest(
        config_path,
        out_dir,
        localization_path,
        news_paths,
        tabs_ui_snippet_path,
        mod_folder_name,
        merge_target,
        merge_result,
    )

    print(f"Generated localization: {localization_path}")
    print(f"Generated tabs snippet: {tabs_ui_snippet_path}")
    print(f"Generated news files: {len(news_paths)}")
    if text_override_count > 0:
        print(f"Applied text overrides: {text_override_count} from {texts_path}")
    if merge_target is not None and merge_result is not None:
        updated, added, removed = merge_result
        print(
            "Merged localization target: "
            f"{merge_target} (updated={updated}, added={added}, removed={removed})"
        )
    if windows_file is not None:
        total_windows_updates = sum(windows_update_by_id.values())
        print(f"Updated windows sources: {total_windows_updates} in {windows_file}")
        if windows_update_by_id:
            details = ", ".join(f"{k}={v}" for k, v in sorted(windows_update_by_id.items()))
            print(f"Windows source updates by link: {details}")
    print(f"Generated manifest: {manifest_path}")


if __name__ == "__main__":
    main()
