"""
Generate ESC menu support files from one editable source JSON.

Outputs:
- Config/Localization.txt
- Config/XUi/windows.xml
- Config/XUi/xui.xml
- Links/*.xml
"""

from __future__ import annotations

import argparse
import copy
import csv
import datetime as dt
import json
import math
import re
import shutil
import sys
import urllib.error
import urllib.request
from pathlib import Path
from xml.dom import minidom
from xml.sax.saxutils import escape


SCRIPT_DIR = Path(__file__).resolve().parent
if SCRIPT_DIR.name.lower() == "code" and SCRIPT_DIR.parent.name.lower() == "_generator":
    GENERATOR_DIR = SCRIPT_DIR.parent
    PROJECT_ROOT = GENERATOR_DIR.parent
elif SCRIPT_DIR.name.lower() == "_generator":
    GENERATOR_DIR = SCRIPT_DIR
    PROJECT_ROOT = SCRIPT_DIR.parent
else:
    GENERATOR_DIR = SCRIPT_DIR
    PROJECT_ROOT = SCRIPT_DIR

DEFAULT_CONFIG_PATH = SCRIPT_DIR / "ESCMenu.source.json"
DEFAULT_TEXTS_FILE = GENERATOR_DIR / "2-EditESCMenuConfig.txt"
DEFAULT_LINKS_DIR = PROJECT_ROOT / "Links"
DEFAULT_LOCALIZATION_FILE = PROJECT_ROOT / "Config" / "Localization.txt"
DEFAULT_WINDOWS_FILE = PROJECT_ROOT / "Config" / "XUi" / "windows.xml"
DEFAULT_XUI_FILE = PROJECT_ROOT / "Config" / "XUi" / "xui.xml"

DEFAULT_OPTIONS_INNER_TEMPLATE = """
<sprite name="optionsBackground" depth="2" pos="5,-5" sprite="ui_game_header_fill" color="[optionsBackgroundColor]" width="310" height="540" type="sliced"/>
<sprite name="optionsBorder" depth="1" sprite="ui_game_header_fill" color="[panelBorderColor]" width="320" height="550" type="sliced"/>
<label name="options_title" depth="10" pos="160,-40" height="40" width="180" justify="center" pivot="center" effect="Outline8" effect_color="0,0,0,255" effect_distance="1,1" color="[textBoldingColor]" font_size="38" text_key="windowESC_color_options_title" foregroundlayer="true"/>
<sprite name="optionsDivider" pos="15,-75" depth="10" width="290" height="5" sprite="menu_empty3px" color="[panelBorderColor]" type="sliced" fillcenter="false" globalopacity="false" />
<rect name="stabilityPublic" depth="200" pos="60,-92" controller="InGameDebugMenu" disableautobackground="true">
    <togglebutton depth="210" name="toggleDebugShaders" pos="0,0" width="200" height="32" caption_key="xuiDebugMenuShowStability" visible="true"/>
</rect>
<grid name="optionsList" pos="20,-140" depth="10" rows="6" cols="1" cell_width="300" cell_height="100" arrangement="vertical">
    <conditional>
        <if cond="mod_loaded('AGF-NoEAC-OptionsPlus') and mod_loaded('AGF-NoEAC-ScreamerAlert')">
            <entry name="screamerAlert">
                <rect name="agfOptionsScreamer" controller="OptionsAgfUi, optionsAGF" pos="0,0" width="280" height="120" disableautobackground="true">
                    <label name="optionsScreamerTitle" pos="0,-5" depth="10" width="280" height="28" font_size="26" justify="center" color="[textBoldingColor]" effect="outline" effect_color="0,0,0,255" effect_distance="1,1" text_key="windowESC_color_options_screamer_title" visible="{opt_row_1_visible}" />
                    <sprite name="optionsDivider" pos="-5,5" depth="10" width="290" height="5" sprite="menu_empty3px" color="[panelBorderColor]" type="sliced" fillcenter="false" globalopacity="false" />
                    <simplebutton name="btnScreamerOff" pos="0,-41" depth="11" width="84" height="32" font_size="24" sprite="ui_game_header_fill" bordercolor="[pageTabButtonBorderColor]" defaultcolor="[pageTabButtonBackgroundColor]" selectedsprite="menu_empty3px" selectedcolor="[pageTabButtonSelectedColor]" selected="{opt_row_1_off_selected_visible}" caption_key="windowESC_color_options_screamer_off" tooltip_key="windowESC_options_screamer_tooltip_off" visible="{opt_row_1_visible}" gamepad_selectable="false" foregroundlayer="false" on_press="true" />
                    <simplebutton name="btnScreamerOn" pos="96,-41" depth="11" width="84" height="32" font_size="24" sprite="ui_game_header_fill" bordercolor="[pageTabButtonBorderColor]" defaultcolor="[pageTabButtonBackgroundColor]" selectedsprite="menu_empty3px" selectedcolor="[pageTabButtonSelectedColor]" selected="{opt_row_1_on_selected_visible}" caption_key="windowESC_color_options_screamer_on" tooltip_key="windowESC_options_screamer_tooltip_on" visible="{opt_row_1_visible}" gamepad_selectable="false" foregroundlayer="false" on_press="true" />
                    <simplebutton name="btnScreamerNum" pos="192,-41" depth="11" width="84" height="32" font_size="24" sprite="ui_game_header_fill" bordercolor="[pageTabButtonBorderColor]" defaultcolor="[pageTabButtonBackgroundColor]" selectedsprite="menu_empty3px" selectedcolor="[pageTabButtonSelectedColor]" selected="{opt_row_1_num_selected_visible}" caption_key="windowESC_color_options_screamer_on_num" tooltip_key="windowESC_options_screamer_tooltip_on_num" visible="{opt_row_1_visible}" gamepad_selectable="false" foregroundlayer="false" on_press="true" />
                </rect>
            </entry>
        </if>
    </conditional>
    <entry name="END">
        <sprite name="optionsDivider" pos="-5,15" depth="10" width="290" height="5" sprite="menu_empty3px" color="[panelBorderColor]" type="sliced" fillcenter="false" globalopacity="false" />
    </entry>
</grid>
"""

# Keep bytecode cache inside generator code area.
if sys.pycache_prefix is None:
    _pycache_root = SCRIPT_DIR / "__pycache__"
    _pycache_root.mkdir(parents=True, exist_ok=True)
    sys.pycache_prefix = str(_pycache_root)


DEFAULT_THEME_COLORS: dict[str, str] = {
    "headerBackground": "95, 89, 128",
    "pageBackground": "95, 89, 128",
    "linkBackground": "95, 89, 128",
    "optionsBackground": "95, 89, 128",
    "panelBorder": "0, 0, 0",
    "headerTitle": "141, 181, 128",
    "headerMotto": "221, 205, 250",
    "pageTabButtonBackground": "32, 32, 32",
    "pageTabButtonSelected": "74, 33, 150",
    "pageTabButtonText": "141, 181, 128",
    "pageTabButtonBorder": "0, 0, 0",
    "pageTitle": "141, 181, 128",
    "pageBody": "221, 205, 250",
    "textBolding": "141, 181, 128",
    "textHighlight": "255, 255, 255",
    "linkButtonBackground": "64, 64, 64",
    "linkButtonText": "255, 255, 255",
    "linkButtonHover": "50, 50, 50",
    "linkButtonBorder": "0, 0, 0",
}

DEFAULT_AUTO_HIGH_VISIBILITY_COLORS: dict[str, str] = {
    "headerBackground": "40, 40, 40",
    "pageBackground": "40, 40, 40",
    "linkBackground": "40, 40, 40",
    "optionsBackground": "40, 40, 40",
    "panelBorder": "0, 0, 0",
    "headerTitle": "255, 255, 255",
    "headerMotto": "200, 200, 200",
    "pageTabButtonBackground": "20, 20, 20",
    "pageTabButtonSelected": "100, 100, 100",
    "pageTabButtonText": "255, 255, 255",
    "pageTabButtonBorder": "0, 0, 0",
    "pageTitle": "255, 255, 255",
    "pageBody": "200, 200, 200",
    "textBolding": "255, 255, 255",
    "textHighlight": "225, 225, 225",
    "linkButtonBackground": "20, 20, 20",
    "linkButtonText": "255, 255, 255",
    "linkButtonHover": "70, 70, 70",
    "linkButtonBorder": "0, 0, 0",
}

XML_STYLE_COLOR_NAMES: dict[str, str] = {
    "headerBackground": "headerBackgroundColor",
    "pageBackground": "pageBackgroundColor",
    "linkBackground": "linkBackgroundColor",
    "optionsBackground": "optionsBackgroundColor",
    "panelBorder": "panelBorderColor",
    "headerTitle": "headerTitleColor",
    "headerMotto": "headerMottoColor",
    "pageTabButtonBackground": "pageTabButtonBackgroundColor",
    "pageTabButtonBorder": "pageTabButtonBorderColor",
    "pageTabButtonSelected": "pageTabButtonSelectedColor",
    "pageTitle": "pageTitleColor",
    "pageBody": "pageBodyColor",
    "textBolding": "textBoldingColor",
    "linkButtonBackground": "linkButtonBackgroundColor",
    "linkButtonHover": "linkButtonHoverColor",
    "linkButtonBorder": "linkButtonBorderColor",
}

DEFAULT_TAB_LAYOUT: dict[str, int] = {
    "contentWidth": 1100,
    "contentPadding": 20,
    "buttonCellWidth": 150,
    "buttonHeight": 42,
    "buttonPosY": -20,
}

TAB_BUTTON_X_NUDGE = 2
TAB_BUTTON_INNER_POS = "0,0"
TAB_BUTTON_SELECTED_SPRITE = "menu_empty3px"
PAGE_TITLE_POS = "0,-82"
PAGE_BODY_POS = "26,-144"
PAGE_BODY_WIDTH = 1048
PAGE_BODY_HEIGHT = 544
PAGE_BODY_MAX_COLUMNS = 3
PAGE_BODY_COLUMN_GAP = 24
PAGE_BODY_SCROLL_CONTENT_HEIGHT = 4096
DEFAULT_BODY_WIDTH_PERCENT = 100
DEFAULT_BODY_ALIGN = "left"
TEMP_BODY_LABEL_BG_ENABLED = False
TEMP_BODY_LABEL_BG_SPRITE = "menu_empty"
TEMP_BODY_LABEL_BG_COLOR = "0,0,0,255"

DEFAULT_LINK_LAYOUT: dict[str, int] = {
    "containerWidth": 1100,
    "maxLinks": 5,
    "cellWidth": 210,
    "buttonWidth": 200,
    "buttonHeight": 36,
    "buttonPosY": -12,
    "buttonDepth": 10,
    "posXForMaxLinks": 30,
}

DEFAULT_PAGE_TITLE_FONT_SIZE = 36
DEFAULT_PAGE_BODY_FONT_SIZE = 28
DEFAULT_MAX_LINKS = 5
DEFAULT_MAX_PAGES = 8
DEFAULT_LINK_SOURCE_MODE = "client"


RGB_TRIPLET_RE = re.compile(r"^\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}\s*$")
BODY_BOLD_TOKEN_RE = re.compile(r"\{\{b:(.*?)\}\}|\*\*(.+?)\*\*", re.DOTALL)
BODY_HIGHLIGHT_TOKEN_RE = re.compile(r"\{\{h:(.*?)\}\}|==(.+?)==", re.DOTALL)
FULL_COLOR_WRAPPER_RE = re.compile(r"^\s*(\[[0-9A-Fa-f]{6}\])(.*)\[-\]\s*$", re.DOTALL)
HAS_ANY_COLOR_TAG_RE = re.compile(r"\[[0-9A-Fa-f]{6}\].*\[-\]", re.DOTALL)
BODY_BOUNDARY_MARKER_RE = re.compile(r"^\s*<\!?--\s*body\b.*?(start|end).*?-->\s*$", re.IGNORECASE)
BODY_COLUMN_START_MARKER_RE = re.compile(
    r"^\s*<!--\s*Body\s*(?:Column\s*)?(?P<col>[123])\s*(?:,\s*if\s*Enabled,?)?\s*starts\s*below\s*this\s*line\s*-->\s*$",
    re.IGNORECASE,
)
BODY_COLUMN_END_MARKER_RE = re.compile(
    r"^\s*<!--\s*Body\s*(?:Column\s*)?(?P<col>[123])\s*ends\s*-->\s*$",
    re.IGNORECASE,
)
SERVER_JOIN_BODY_START_MARKER_RE = re.compile(
    r"^\s*<!--\s*Server\s*Join\s*Body\s*starts\s*below\s*this\s*line\s*-->\s*$",
    re.IGNORECASE,
)
SERVER_JOIN_BODY_END_MARKER_RE = re.compile(
    r"^\s*<!--\s*Server\s*Join\s*Body\s*ends\s*-->\s*$",
    re.IGNORECASE,
)
ADMIN_BODY_START_MARKER_RE = re.compile(
    r"^\s*<!--\s*Admin\s*Body\s*starts\s*below\s*this\s*line\s*-->\s*$",
    re.IGNORECASE,
)
ADMIN_BODY_END_MARKER_RE = re.compile(
    r"^\s*<!--\s*Admin\s*Body\s*ends\s*-->\s*$",
    re.IGNORECASE,
)
PAGE_BANNER_RE = re.compile(r"^\s*PAGE\s+(?P<num>\d+)\s*$", re.IGNORECASE)
ADMIN_PAGE_BANNER_RE = re.compile(r"^\s*ADMIN\s+PAGE\s+(?P<num>\d+)\s*$", re.IGNORECASE)
SEPARATOR_LINE_RE = re.compile(r"^\s*[-=]{5,}\s*$")

LOCALIZATION_LANGUAGE_COLUMNS = [
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
]


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
        "linkBackground",
        "linkBackgroundColor",
        "optionsBackground",
        "optionsBackgroundColor",
        "panelBorder",
        "panelBorderColor",
        "borderColor",
        "headerTitle",
        "headerTitleColor",
        "headerMotto",
        "headerMottoColor",
        "pageTabButtonBackground",
        "pageTabButtonBackgroundColor",
        "pageTabButtonBorder",
        "pageTabButtonBorderColor",
        "sectionButtonBorder",
        "sectionButtonBorderColor",
        "pageTabButtonSelected",
        "pageTabButtonSelectedColor",
        "pageTabButtonText",
        "pageTabButtonTextColor",
        "tabButtonSelected",
        "tabButtonSelectedColor",
        "pageButtonText",
        "pageButtonTextColor",
        "pageTitle",
        "pageTitleColor",
        "pageBody",
        "pageBodyColor",
        "textBolding",
        "textBoldingColor",
        "textHighlight",
        "textHighlightColor",
        "linkButtonBackground",
        "linkButtonBackgroundColor",
        "linkButtonBorder",
        "linkButtonBorderColor",
        "linkButtonText",
        "linkButtonTextColor",
        "linkButtonHover",
        "linkButtonHoverColor",
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
        "--easy",
        action="store_true",
        help=(
            "Run the non-technical full flow. If texts file is missing, create it and exit. "
            "If texts exists, auto-runs merge-localization + windows source updates for all links "
            "+ windows tabs/links layout updates."
        ),
    )
    parser.add_argument(
        "--config",
        default=str(DEFAULT_CONFIG_PATH),
        help="Path to source JSON (default: _Generator/Code/ESCMenu.source.json)",
    )
    parser.add_argument(
        "--links-dir",
        default=str(DEFAULT_LINKS_DIR),
        help="Output directory for generated link XML files (default: Links)",
    )
    parser.add_argument(
        "--merge-localization",
        action="store_true",
        help=(
            "Merge generated keys into existing localization file instead of rewriting it."
        ),
    )
    parser.add_argument(
        "--no-merge-localization",
        action="store_true",
        help="Skip updating the main localization file.",
    )
    parser.add_argument(
        "--localization-file",
        default=str(DEFAULT_LOCALIZATION_FILE),
        help="Localization file to merge generated keys into (default: Config/Localization.txt).",
    )
    parser.add_argument(
        "--update-windows-sources",
        action="store_true",
        help="Update active NewsWindow source URLs in windows.xml to generated local source path.",
    )
    parser.add_argument(
        "--windows-file",
        default=str(DEFAULT_WINDOWS_FILE),
        help="XUi windows file to update when --update-windows-sources is used.",
    )
    parser.add_argument(
        "--xui-file",
        default=str(DEFAULT_XUI_FILE),
        help="XUi root file used for admin window registration (default: Config/XUi/xui.xml).",
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
        "--update-windows-links-layout",
        action="store_true",
        help=(
            "Regenerate active linkButtons/linkButtons_hv grids in windows.xml using current links, "
            "with auto-centering when active links are fewer than 5."
        ),
    )
    parser.add_argument(
        "--use-hv-dev-overrides",
        action="store_true",
        help=(
            "Use dev.highVisibilityPreview overrides from source JSON for temporary HV tuning. "
            "Default behavior keeps HV automatic."
        ),
    )
    parser.add_argument(
        "--dev-body-label-bg",
        action="store_true",
        help=(
            "Enable temporary body label background sprites for layout/dev testing. "
            "Default keeps these disabled in generated output."
        ),
    )
    parser.add_argument(
        "--texts-file",
        default=str(DEFAULT_TEXTS_FILE),
        help=(
            "Optional text-only overrides file for headers/page/link copy. "
            "If present, values are merged before generation (default: _Generator/2-EditESCMenuConfig.txt)."
        ),
    )
    parser.add_argument(
        "--init-texts-file",
        action="store_true",
        help="Write a starter text-only file from current source and exit.",
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

    for theme_name in ("color", "highVisibility"):
        if theme_name in themes and not isinstance(themes.get(theme_name), dict):
            raise ValueError(f"Source JSON 'themes.{theme_name}' must be an object when provided.")

    if isinstance(themes, dict) and isinstance(themes.get("color"), dict):
        _validate_rgb_theme_block(themes["color"], "themes.color")

    dev = data.get("dev", {})
    if dev and not isinstance(dev, dict):
        raise ValueError("Source JSON 'dev' must be an object when provided.")
    if isinstance(dev, dict) and "highVisibilityPreview" in dev and not isinstance(
        dev.get("highVisibilityPreview"), dict
    ):
        raise ValueError("Source JSON 'dev.highVisibilityPreview' must be an object when provided.")
    if isinstance(dev, dict) and isinstance(dev.get("highVisibilityPreview"), dict):
        _validate_rgb_theme_block(dev["highVisibilityPreview"], "dev.highVisibilityPreview")

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


def _resolve_page_column_text(
    row: dict,
    lines_key: str,
    text_key: str,
    source_url_key: str | None = None,
) -> str | None:
    if source_url_key and source_url_key in row:
        url = str(row.get(source_url_key) or "").strip()
        if url:
            fetched = _fetch_body_text_from_url(url)
            if fetched is not None:
                return fetched

    if lines_key in row:
        body_lines = row.get(lines_key)
        if body_lines is None:
            return ""
        if not isinstance(body_lines, list):
            raise ValueError(f"Texts page '{lines_key}' must be a list when provided.")
        text = "\n".join(str(line) for line in body_lines)
        return text.replace("\\n", "\n")

    if text_key in row:
        return str(row.get(text_key) or "").replace("\\n", "\n")

    return None


def _trim_page_columns_trailing_blanks(page: dict[str, str | int | list[str]] | None) -> None:
    if page is None:
        return
    for key in ("bodyLines", "bodyColumn2Lines", "bodyColumn3Lines"):
        lines = page.get(key)
        if not isinstance(lines, list):
            continue
        while lines and not str(lines[0]).strip():
            lines.pop(0)
        while lines and not str(lines[-1]).strip():
            lines.pop()


def _safe_font_size(value: object, default: int) -> int:
    try:
        size = int(str(value).strip())
    except (ValueError, TypeError, AttributeError):
        return default
    return max(12, min(72, size))


def _safe_bool(value: object, default: bool = False) -> bool:
    if isinstance(value, bool):
        return value
    if value is None:
        return default
    token = str(value).strip().lower()
    if token in {"1", "true", "yes", "y", "on"}:
        return True
    if token in {"0", "false", "no", "n", "off"}:
        return False
    return default


def _safe_link_source_mode(value: object, default: str = DEFAULT_LINK_SOURCE_MODE) -> str:
    token = str(value or "").strip().lower()
    if token in {"client", "server"}:
        return token
    return default


def _normalize_web_base_url(value: object) -> str:
    text = str(value or "").strip()
    return text.rstrip("/")


def get_link_source_settings(source: dict) -> tuple[str, str]:
    meta = source.get("meta", {}) if isinstance(source.get("meta", {}), dict) else {}
    mode = _safe_link_source_mode(
        source.get("linksClientOrServer", meta.get("linksClientOrServer", DEFAULT_LINK_SOURCE_MODE)),
        DEFAULT_LINK_SOURCE_MODE,
    )
    web_base_url = _normalize_web_base_url(
        source.get("linksXMLWebBaseUrl", meta.get("linksXMLWebBaseUrl", ""))
    )
    return mode, web_base_url


def _resolve_link_source_xml_url(link: dict, mode: str, web_base_url: str) -> str:
    link_id = _safe_link_id(str(link.get("id") or "link"))
    if mode == "server":
        if web_base_url:
            return f"{web_base_url}/{link_id}.xml"
        # Pre-setup server mode: keep button visible but non-clickable until web XML hosting is configured.
        return ""

    return f"@modfolder:Links/{link_id}.xml"


def _safe_body_width_percent(value: object, default: int = DEFAULT_BODY_WIDTH_PERCENT) -> int:
    try:
        parsed = int(str(value).strip())
    except (ValueError, TypeError, AttributeError):
        return default
    return max(40, min(100, parsed))


def _safe_align(value: object, default: str = DEFAULT_BODY_ALIGN) -> str:
    token = str(value or "").strip().lower()
    if token in {"left", "center", "right"}:
        return token
    return default


def _estimate_text_box_width(text: str, font_size: int, max_width: int) -> int:
    lines = str(text or "").replace("\r\n", "\n").split("\n")
    # Remove lightweight authoring markers that do not consume visible width.
    normalized_lines = [
        re.sub(r"\[[^\]]+\]|\*\*|==|\{\{b:|\{\{h:|\}\}", "", ln).strip()
        for ln in lines
    ]

    # Box-centering is based on the longest visible line in the column.
    longest = max((len(ln) for ln in normalized_lines), default=0)
    # Relaxed estimate so long lines keep a safer non-wrapping margin in-game.
    avg_char_px = max(5.2, font_size * 0.34)
    horizontal_padding = 36
    estimated = int(longest * avg_char_px) + horizontal_padding
    min_width = min(max_width, 220)
    return max(min_width, min(max_width, estimated))


def _resolve_body_block_layout(
    pos_value: str,
    width_value: int,
    text: str,
    font_size: int,
    box_centered: bool,
) -> tuple[str, int]:
    if not box_centered:
        return pos_value, width_value

    parts = [p.strip() for p in str(pos_value).split(",", 1)]
    try:
        base_x = int(parts[0])
    except (ValueError, TypeError, IndexError):
        base_x = 26
    base_y = parts[1] if len(parts) > 1 else "-144"

    inner_width = _estimate_text_box_width(text, font_size, width_value)
    # Keep very small side breathing room inside the column bounds.
    inner_width = min(inner_width, max(220, width_value - 6))
    x_shift = (width_value - inner_width) // 2
    return f"{base_x + x_shift},{base_y}", inner_width


def _build_body_label_xml(
    *,
    indent: str,
    name: str,
    depth: int,
    pos: str,
    width: int,
    height: int,
    justify: str,
    color: str,
    font_size: int,
    text_key: str,
) -> str:
    attrs = (
        f'name="{name}" depth="{depth}" pos="{pos}" width="{width}" height="{height}" '
        f'justify="{justify}" effect="Outline8" effect_color="0,0,0,255" effect_distance="1,1" '
        f'color="{color}" font_size="{font_size}" text_key="{text_key}" foregroundlayer="true"'
    )
    if not TEMP_BODY_LABEL_BG_ENABLED:
        return f"{indent}<label {attrs}/>"

    return "\n".join(
        [
            f"{indent}<label {attrs}>",
            (
                f'{indent}\t<sprite depth="0" name="{name}_bg" sprite="{TEMP_BODY_LABEL_BG_SPRITE}" '
                f'color="{TEMP_BODY_LABEL_BG_COLOR}" type="sliced" fillcenter="false" globalopacity="false"/>'
            ),
            f"{indent}</label>",
        ]
    )


def _single_body_layout(tab_model: dict, body_text: str) -> tuple[str, int]:
    width_percent = _safe_body_width_percent(
        tab_model.get("body_width_percent", DEFAULT_BODY_WIDTH_PERCENT),
        DEFAULT_BODY_WIDTH_PERCENT,
    )
    width_value = max(1, int(PAGE_BODY_WIDTH * width_percent / 100))
    width_value = min(PAGE_BODY_WIDTH, width_value)
    x_offset = (PAGE_BODY_WIDTH - width_value) // 2
    base_pos = f"{26 + x_offset},-144"
    box_centered = _safe_bool(tab_model.get("body_box_centered"), default=False)
    return _resolve_body_block_layout(
        base_pos,
        width_value,
        body_text,
        _safe_font_size(tab_model.get("body_font_size"), DEFAULT_PAGE_BODY_FONT_SIZE),
        box_centered,
    )


def _theme_style_color(theme_key: str, is_hv: bool) -> str | None:
    style_name = XML_STYLE_COLOR_NAMES.get(theme_key)
    if not style_name:
        return None
    suffix = "HV" if is_hv else ""
    return f"[{style_name}{suffix}]"


def _theme_xml_color(theme: dict[str, str], theme_key: str, is_hv: bool) -> str:
    return _theme_style_color(theme_key, is_hv) or theme[theme_key]


def _large_tab_button_xml(*, is_hv: bool) -> str:
    default_color = (
        _theme_style_color("pageTabButtonBackground", True) or "[pageTabButtonBackgroundColorHV]"
        if is_hv
        else "[mediumGrey]"
    )
    selected_color = (
        _theme_style_color("pageTabButtonSelected", True) or "[pageTabButtonSelectedColorHV]"
        if is_hv
        else "[lightGrey]"
    )
    return (
        '<simplebutton name="tabButton" depth="12" pos="0,0" width="170" height="36" '
        'font_size="30" sprite="ui_game_header_fill" '
        f'bordercolor="{_theme_style_color("pageTabButtonBorder", is_hv) or ("[pageTabButtonBorderColorHV]" if is_hv else "[pageTabButtonBorderColor]")}" '
        f'defaultcolor="{default_color}" '
        'selectedsprite="menu_empty3px" '
        f'selectedcolor="{selected_color}" '
        'foregroundlayer="false" caption="{tab_name_localized}"/>'
    )


def _split_body_text_by_fit(text: str, columns_count: int, font_size: int) -> list[str]:
    """Split one body text into N columns using a simple fit-aware heuristic.

    This is an approximation because exact in-game wrapping depends on runtime font metrics.
    """
    count = max(1, min(PAGE_BODY_MAX_COLUMNS, columns_count))
    normalized = str(text or "").replace("\r\n", "\n").strip("\n")
    if count <= 1 or not normalized:
        return [normalized] + [""] * (PAGE_BODY_MAX_COLUMNS - 1)

    paragraphs = [p.strip("\n") for p in normalized.split("\n\n")]
    paragraphs = [p for p in paragraphs if p.strip()]
    if not paragraphs:
        return [normalized] + [""] * (PAGE_BODY_MAX_COLUMNS - 1)

    total_gap = PAGE_BODY_COLUMN_GAP * (count - 1)
    available_width = max(1, PAGE_BODY_WIDTH - total_gap)
    column_width = max(1, available_width // count)

    # Rough estimate for average glyph width / line-height in the in-game UI label.
    avg_char_px = max(6.5, font_size * 0.55)
    line_height_px = max(12.0, font_size * 1.35)
    est_chars_per_line = max(18, int(column_width / avg_char_px))
    est_lines_per_col = max(8, int(PAGE_BODY_HEIGHT / line_height_px))
    est_capacity_per_col = max(200, est_chars_per_line * est_lines_per_col)

    para_sizes = [len(p) + 2 for p in paragraphs]
    total_size = sum(para_sizes)

    columns: list[list[str]] = [[] for _ in range(count)]
    idx = 0
    remaining_size = total_size

    for col in range(count):
        remaining_cols = count - col
        if idx >= len(paragraphs):
            break

        target_size = int(remaining_size / max(1, remaining_cols))
        target_size = min(target_size, est_capacity_per_col)
        if col == count - 1:
            target_size = max(target_size, remaining_size)

        used = 0
        while idx < len(paragraphs):
            remaining_paragraphs = len(paragraphs) - idx
            must_leave = max(0, remaining_cols - 1)
            if remaining_paragraphs <= must_leave:
                break

            para = paragraphs[idx]
            para_size = para_sizes[idx]
            will_exceed = used > 0 and (used + para_size) > target_size
            if will_exceed:
                break

            columns[col].append(para)
            used += para_size
            idx += 1

        remaining_size = max(0, remaining_size - used)

    # Put any remainder into the last column.
    while idx < len(paragraphs):
        columns[-1].append(paragraphs[idx])
        idx += 1

    column_texts = ["\n\n".join(col).strip("\n") for col in columns]

    # Fallback: when the content has no blank-paragraph boundaries, the fit pass can keep
    # everything in column 1. In that case, split by lines so auto columns still populate.
    non_empty_columns = [c for c in column_texts if c.strip()]
    if count > 1 and len(non_empty_columns) <= 1:
        lines = [ln for ln in normalized.split("\n")]
        chunks: list[list[str]] = [[] for _ in range(count)]
        if lines:
            per_col = max(1, math.ceil(len(lines) / count))
            start = 0
            for col_idx in range(count):
                end = min(len(lines), start + per_col)
                chunks[col_idx] = lines[start:end]
                start = end
        column_texts = ["\n".join(chunk).strip("\n") for chunk in chunks]

    while len(column_texts) < PAGE_BODY_MAX_COLUMNS:
        column_texts.append("")
    return column_texts[:PAGE_BODY_MAX_COLUMNS]


def get_link_layout_limit(source: dict) -> int:
    ui = source.get("ui", {})
    layout = ui.get("layout", {}) if isinstance(ui, dict) else {}
    try:
        value = int(layout.get("maxLinks", DEFAULT_MAX_LINKS))
    except (TypeError, ValueError):
        value = DEFAULT_MAX_LINKS
    return max(1, min(5, value))


def get_active_links(source: dict) -> list[dict]:
    links = source.get("links", [])
    if not isinstance(links, list):
        return []

    active_links: list[dict] = []
    for link in links:
        if not isinstance(link, dict):
            continue
        if link.get("enabled", True) is False:
            continue
        if not str(link.get("url") or "").strip():
            continue
        active_links.append(link)

    max_links = get_link_layout_limit(source)
    if len(active_links) > max_links:
        raise ValueError(
            f"Too many active links for configured layout: links={len(active_links)}, allowed={max_links}. "
            "Reduce source links or lower enabled links."
        )

    return active_links


def get_enabled_links_count(source: dict) -> int:
    links = source.get("links", [])
    if not isinstance(links, list):
        return 0

    enabled_count = 0
    for link in links:
        if not isinstance(link, dict):
            continue
        if link.get("enabled", True) is False:
            continue
        enabled_count += 1

    max_links = get_link_layout_limit(source)
    return max(0, min(max_links, enabled_count))


def get_active_tabs(source: dict) -> list[dict]:
    tabs = source.get("tabs", [])
    if not isinstance(tabs, list):
        return []

    active_tabs: list[dict] = []
    for tab in tabs:
        if not isinstance(tab, dict):
            continue
        if tab.get("enabled", True) is False:
            continue
        active_tabs.append(tab)

    return active_tabs


def get_show_options_section(source: dict) -> bool:
    ui = source.get("ui", {}) if isinstance(source.get("ui", {}), dict) else {}
    return _safe_bool(ui.get("showOptionsSection", True), default=True)


def get_ui_feature_toggles(source: dict) -> dict[str, bool]:
    ui = source.get("ui", {}) if isinstance(source.get("ui", {}), dict) else {}
    links_enabled = get_enabled_links_count(source) > 0
    return {
        "showOptionsSection": _safe_bool(ui.get("showOptionsSection", True), default=True),
        "showAdminVersion": _safe_bool(ui.get("showAdminVersion", True), default=True),
        "showLinksSection": links_enabled,
        "showServerJoinSection": _safe_bool(ui.get("showServerJoinSection", True), default=True),
    }


def build_texts_payload(source: dict) -> dict:
    """Create a user-editable payload containing only text content fields."""
    labels = source.get("labels", {}) if isinstance(source.get("labels", {}), dict) else {}
    source_tabs = source.get("tabs", []) if isinstance(source.get("tabs", []), list) else []
    source_admin_tabs = source.get("adminTabs", []) if isinstance(source.get("adminTabs", []), list) else []
    if not source_admin_tabs:
        source_admin_tabs = copy.deepcopy(source_tabs)

    label_keys = [
        "headerTitle",
        "headerMotto",
        "headerTitleHV",
        "headerMottoHV",
    ]

    payload_headers: dict[str, str] = {}
    for key in label_keys:
        if key in labels:
            payload_headers[key] = str(labels.get(key) or "")

    payload_pages: list[dict[str, str | int]] = []
    for idx in range(1, DEFAULT_MAX_PAGES + 1):
        tab = source_tabs[idx - 1] if idx - 1 < len(source_tabs) and isinstance(source_tabs[idx - 1], dict) else {}
        body_text = str(tab.get("body") or "").replace("\r\n", "\n")
        body_text = body_text.replace("\\n", "\n")
        body_col2_text = str(tab.get("bodyColumn2") or "").replace("\r\n", "\n")
        body_col2_text = body_col2_text.replace("\\n", "\n")
        body_col3_text = str(tab.get("bodyColumn3") or "").replace("\r\n", "\n")
        body_col3_text = body_col3_text.replace("\\n", "\n")
        page_columns = tab.get("bodyColumns", 1)
        try:
            page_columns = int(str(page_columns).strip())
        except (ValueError, TypeError, AttributeError):
            page_columns = 1
        page_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, page_columns))

        row: dict[str, str | int | bool] = {
            "id": idx,
            "title": str(tab.get("title") or f"Page {idx}"),
            "columns": page_columns,
            "bodyLines": body_text.split("\n"),
            "bodyColumn2Lines": body_col2_text.split("\n") if body_col2_text else [],
            "bodyColumn3Lines": body_col3_text.split("\n") if body_col3_text else [],
            "autoSplitColumns": _safe_bool(tab.get("autoSplitColumns"), default=False),
            "bodyWidthPercent": _safe_body_width_percent(
                tab.get("bodyWidthPercent", DEFAULT_BODY_WIDTH_PERCENT),
                DEFAULT_BODY_WIDTH_PERCENT,
            ),
            "bodyAlign": _safe_align(tab.get("bodyAlign", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN),
            "bodyBoxCentered": _safe_bool(tab.get("bodyBoxCentered"), default=False),
            "bodyColumn2Align": _safe_align(tab.get("bodyColumn2Align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN),
            "bodyColumn2BoxCentered": _safe_bool(tab.get("bodyColumn2BoxCentered"), default=False),
            "bodyColumn3Align": _safe_align(tab.get("bodyColumn3Align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN),
            "bodyColumn3BoxCentered": _safe_bool(tab.get("bodyColumn3BoxCentered"), default=False),
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

    payload_admin_pages: list[dict[str, str | int]] = []
    for idx in range(1, DEFAULT_MAX_PAGES + 1):
        tab = source_admin_tabs[idx - 1] if idx - 1 < len(source_admin_tabs) and isinstance(source_admin_tabs[idx - 1], dict) else {}
        body_text = str(tab.get("body") or "").replace("\r\n", "\n")
        body_text = body_text.replace("\\n", "\n")
        body_col2_text = str(tab.get("bodyColumn2") or "").replace("\r\n", "\n")
        body_col2_text = body_col2_text.replace("\\n", "\n")
        body_col3_text = str(tab.get("bodyColumn3") or "").replace("\r\n", "\n")
        body_col3_text = body_col3_text.replace("\\n", "\n")
        page_columns = tab.get("bodyColumns", 1)
        try:
            page_columns = int(str(page_columns).strip())
        except (ValueError, TypeError, AttributeError):
            page_columns = 1
        page_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, page_columns))

        row: dict[str, str | int | bool] = {
            "id": idx,
            "title": str(tab.get("title") or tab.get("button") or f"Admin Page {idx}"),
            "columns": page_columns,
            "bodyLines": body_text.split("\n"),
            "bodyColumn2Lines": body_col2_text.split("\n") if body_col2_text else [],
            "bodyColumn3Lines": body_col3_text.split("\n") if body_col3_text else [],
            "autoSplitColumns": _safe_bool(tab.get("autoSplitColumns"), default=False),
            "bodyAlign": _safe_align(tab.get("bodyAlign", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN),
            "bodyBoxCentered": _safe_bool(tab.get("bodyBoxCentered"), default=False),
            "bodyColumn2Align": _safe_align(tab.get("bodyColumn2Align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN),
            "bodyColumn2BoxCentered": _safe_bool(tab.get("bodyColumn2BoxCentered"), default=False),
            "bodyColumn3Align": _safe_align(tab.get("bodyColumn3Align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN),
            "bodyColumn3BoxCentered": _safe_bool(tab.get("bodyColumn3BoxCentered"), default=False),
            "titleFontSize": _safe_font_size(
                tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
                DEFAULT_PAGE_TITLE_FONT_SIZE,
            ),
            "bodyFontSize": _safe_font_size(
                tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
                DEFAULT_PAGE_BODY_FONT_SIZE,
            ),
        }
        payload_admin_pages.append(row)

    source_links = source.get("links", []) if isinstance(source.get("links", []), list) else []
    payload_links: list[dict[str, str | int]] = []
    for idx in range(1, DEFAULT_MAX_LINKS + 1):
        source_link = source_links[idx - 1] if idx - 1 < len(source_links) and isinstance(source_links[idx - 1], dict) else {}
        label = str(source_link.get("label") or source_link.get("title") or f"Link {idx}")
        url = str(source_link.get("url") or "").strip()
        payload_links.append({"id": idx, "label": label, "url": url})

    enabled_links = get_enabled_links_count(source)
    enabled_pages = len(get_active_tabs(source))
    enabled_admin_pages = len([
        tab
        for tab in source_admin_tabs
        if isinstance(tab, dict) and tab.get("enabled", True) is not False
    ])
    enabled_admin_pages = max(1, min(DEFAULT_MAX_PAGES, enabled_admin_pages or enabled_pages or 1))
    link_source_mode, links_web_xml_base_url = get_link_source_settings(source)
    feature_toggles = get_ui_feature_toggles(source)
    ui = source.get("ui", {}) if isinstance(source.get("ui", {}), dict) else {}
    joining_title = str(labels.get("joiningTitle") or "Joining")
    joining_title_hv = joining_title
    joining_body = str(labels.get("joiningBody") or "")
    joining_title_font_size = _safe_font_size(
        ui.get("serverJoinTitleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
        DEFAULT_PAGE_TITLE_FONT_SIZE,
    )
    joining_body_font_size = _safe_font_size(
        ui.get("serverJoinBodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
        DEFAULT_PAGE_BODY_FONT_SIZE,
    )
    joining_body_align = _safe_align(ui.get("serverJoinBodyAlign", "center"), "center")

    source_themes = source.get("themes", {}) if isinstance(source.get("themes", {}), dict) else {}
    theme_color = source_themes.get("color", {}) if isinstance(source_themes.get("color", {}), dict) else {}

    color_aliases = {
        "headerBackgroundColor": ["headerBackgroundColor", "headerBackground"],
        "pageBackgroundColor": ["pageBackgroundColor", "pageBackground"],
        "linkBackgroundColor": ["linkBackgroundColor", "linkBackground"],
        "optionsBackgroundColor": ["optionsBackgroundColor", "optionsBackground"],
        "panelBorderColor": ["panelBorderColor", "panelBorder", "borderColor"],
        "headerTitleColor": ["headerTitleColor", "headerTitle"],
        "headerMottoColor": ["headerMottoColor", "headerMotto"],
        "pageTabButtonBackgroundColor": [
            "pageTabButtonBackgroundColor",
            "pageTabButtonBackground",
        ],
        "pageTabButtonBorderColor": [
            "pageTabButtonBorderColor",
            "pageTabButtonBorder",
            "sectionButtonBorderColor",
            "sectionButtonBorder",
        ],
        "pageTabButtonSelectedColor": [
            "pageTabButtonSelectedColor",
            "pageTabButtonSelected",
            "tabButtonSelectedColor",
            "tabButtonSelected",
        ],
        "pageTabButtonTextColor": [
            "pageTabButtonTextColor",
            "pageTabButtonText",
            "pageButtonTextColor",
            "pageButtonText",
        ],
        "pageTitleColor": ["pageTitleColor", "pageTitle"],
        "pageBodyColor": ["pageBodyColor", "pageBody"],
        "textBoldingColor": ["textBoldingColor", "textBolding"],
        "textHighlightColor": ["textHighlightColor", "textHighlight"],
        "linkButtonBackgroundColor": ["linkButtonBackgroundColor", "linkButtonBackground"],
        "linkButtonBorderColor": ["linkButtonBorderColor", "linkButtonBorder"],
        "linkButtonTextColor": ["linkButtonTextColor", "linkButtonText"],
        "linkButtonHoverColor": ["linkButtonHoverColor", "linkButtonHover"],
    }
    payload_colors: dict[str, str] = {}
    for out_key, aliases in color_aliases.items():
        for key in aliases:
            if key in theme_color:
                payload_colors[out_key] = str(theme_color.get(key) or "")
                break

    return {
        "_comment": "Edit text values here. Generator maps these into localized output automatically.",
        "colors": payload_colors,
        "headers": payload_headers,
        "pages": payload_pages,
        "adminPages": payload_admin_pages,
        "links": payload_links,
        "enabledLinks": enabled_links,
        "enabledPages": enabled_pages,
        "enabledAdminPages": enabled_admin_pages,
        "linksClientOrServer": link_source_mode,
        "linksXMLWebBaseUrl": links_web_xml_base_url,
        "showOptionsSection": feature_toggles["showOptionsSection"],
        "showAdminVersion": feature_toggles["showAdminVersion"],
        "showLinksSection": feature_toggles["showLinksSection"],
        "showServerJoinSection": feature_toggles["showServerJoinSection"],
        "serverJoin": {
            "showServerJoinSection": feature_toggles["showServerJoinSection"],
            "joiningTitle": joining_title,
            "joiningTitleHV": joining_title_hv,
            "joiningBody": joining_body,
            "joiningTitleFontSize": joining_title_font_size,
            "joiningBodyFontSize": joining_body_font_size,
            "joiningBodyAlign": joining_body_align,
        },
    }


def _build_texts_markdown(payload: dict) -> str:
    colors = payload.get("colors", {}) if isinstance(payload.get("colors", {}), dict) else {}
    headers = payload.get("headers", {}) if isinstance(payload.get("headers", {}), dict) else {}
    pages = payload.get("pages", []) if isinstance(payload.get("pages", []), list) else []
    admin_pages = payload.get("adminPages", []) if isinstance(payload.get("adminPages", []), list) else []
    if not admin_pages:
        admin_pages = copy.deepcopy(pages)
    links = payload.get("links", []) if isinstance(payload.get("links", []), list) else []
    enabled_pages = payload.get("enabledPages", len([row for row in pages if isinstance(row, dict)]))
    try:
        enabled_pages = int(str(enabled_pages).strip())
    except (ValueError, TypeError, AttributeError):
        enabled_pages = len([row for row in pages if isinstance(row, dict)])
    enabled_pages = max(1, min(DEFAULT_MAX_PAGES, enabled_pages))
    enabled_admin_pages = payload.get("enabledAdminPages", len([row for row in admin_pages if isinstance(row, dict)]))
    try:
        enabled_admin_pages = int(str(enabled_admin_pages).strip())
    except (ValueError, TypeError, AttributeError):
        enabled_admin_pages = len([row for row in admin_pages if isinstance(row, dict)])
    enabled_admin_pages = max(1, min(DEFAULT_MAX_PAGES, enabled_admin_pages))
    enabled_links = payload.get("enabledLinks", len([row for row in links if isinstance(row, dict) and str(row.get("url") or "").strip()]))
    try:
        enabled_links = int(str(enabled_links).strip())
    except (ValueError, TypeError, AttributeError):
        enabled_links = len([row for row in links if isinstance(row, dict) and str(row.get("url") or "").strip()])
    enabled_links = max(0, min(DEFAULT_MAX_LINKS, enabled_links))
    link_source_mode = _safe_link_source_mode(payload.get("linksClientOrServer", DEFAULT_LINK_SOURCE_MODE))
    links_web_xml_base_url = _normalize_web_base_url(payload.get("linksXMLWebBaseUrl", ""))
    show_options_section = _safe_bool(payload.get("showOptionsSection", True), default=True)
    show_admin_version = _safe_bool(payload.get("showAdminVersion", True), default=True)
    show_server_join_section = _safe_bool(payload.get("showServerJoinSection", True), default=True)
    server_join = payload.get("serverJoin", {}) if isinstance(payload.get("serverJoin", {}), dict) else {}
    joining_title = str(server_join.get("joiningTitle") or "Joining")
    joining_title_hv = joining_title
    joining_body = str(server_join.get("joiningBody") or "")
    joining_title_font_size = _safe_font_size(
        server_join.get("joiningTitleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
        DEFAULT_PAGE_TITLE_FONT_SIZE,
    )
    joining_body_font_size = _safe_font_size(
        server_join.get("joiningBodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
        DEFAULT_PAGE_BODY_FONT_SIZE,
    )
    joining_body_align = _safe_align(server_join.get("joiningBodyAlign", "center"), "center")

    lines: list[str] = [
        "// ===============================",
        "// START HERE (SIMPLE EDIT GUIDE)",
        "// ===============================",
        "// STEP 1: Edit this file.",
        "// STEP 2: Only change text after the :",
        "// STEP 3: Do not change section titles or separator lines.",
        "// STEP 4: Save this file.",
        "// STEP 5: Run _Generator\\3-GenerateESCWindow.bat",
        "// Need a fresh file? Run _Generator\\1-CreateNewConfigFile.bat",
        "//",
        "// OUTLINE",
        "// 1) Feature Toggles",
        "// 2) Color Scheme",
        "// 3) Headers",
        "// 4) Links",
        "// 5) Server Join",
        "// 6) Main Content",
        "// 7) Admin Content",
        "",
        "===============================================",
        "1) FEATURE TOGGLES",
        "===============================================",
        "# Toggles",
        "// Below are options you can set to true or false",
        "",
        f"showOptionsSection: {'true' if show_options_section else 'false'}",
        f"showAdminVersion: {'true' if show_admin_version else 'false'}",
        "",
        "// Options Section is small panel with additional player options",
        "// Admin Version provides a set of up to 8 pages that only Admins can see",
        "",
        "===============================================",
        "2) COLOR SCHEME",
        "===============================================",
        "# Colors",
        "// Color format is R,G,B where each number is 0-255.",
        "// Examples: 255,255,255 = white | 0,0,0 = black | 255,0,0 = red",
        "",
        "// Panel Backgrounds",
    ]

    for key in ["headerBackgroundColor", "pageBackgroundColor", "linkBackgroundColor", "optionsBackgroundColor"]:
        if key in colors:
            lines.append(f"{key}: {str(colors[key])}")

    lines.extend([
        "",
        "// Header Text Colors",
    ])
    for key in ["headerTitleColor", "headerMottoColor"]:
        if key in colors:
            lines.append(f"{key}: {str(colors[key])}")

    lines.extend([
        "",
        "// Page tab button colors (tab row at top of page content)",
    ])
    for key in ["pageTabButtonBackgroundColor", "pageTabButtonSelectedColor", "pageTabButtonTextColor"]:
        if key in colors:
            lines.append(f"{key}: {str(colors[key])}")

    lines.extend([
        "",
        "// Page title and body text colors",
    ])
    for key in ["pageTitleColor", "pageBodyColor"]:
        if key in colors:
            lines.append(f"{key}: {str(colors[key])}")

    lines.extend([
        "",
        "// Inline emphasis colors used in page body (**bold / ==highlight==)",
    ])
    for key in ["textBoldingColor", "textHighlightColor"]:
        if key in colors:
            lines.append(f"{key}: {str(colors[key])}")

    lines.extend([
        "",
        "// Link button colors (buttons inside LINKS box)",
    ])
    for key in ["linkButtonBackgroundColor", "linkButtonTextColor", "linkButtonHoverColor"]:
        if key in colors:
            lines.append(f"{key}: {str(colors[key])}")

    lines.extend([
        "",
        "",
        "===============================================",
        "3) HEADERS",
        "===============================================",
        "# Headers",
        "// Edit text after the : only.",
        "// Keep text fairly short so it fits nicely in game.",
    ])
    for key in ["headerTitle", "headerMotto"]:
        if key in headers:
            lines.append(f"{key}: {str(headers[key])}")

    lines.extend([
        "",
        "",
        "===============================================",
        "4) LINKS",
        "===============================================",
        "# Links",
        "// @enabledLinks controls how many link rows are used (1-5).",
        "// Link row format: number | Button Name | Full URL",
        "",
        f"@enabledLinks: {enabled_links}",
        f"@linksClientOrServer: {link_source_mode}",
        f"@linksXMLWebBaseUrl: {links_web_xml_base_url}",
        "",
        "// Advanced (optional):",
        "// - @linksClientOrServer: client or server",
        "// - client = uses local Links/<id>.xml files",
        "// - server = auto-builds from @linksXMLWebBaseUrl + /<id>.xml",
        "",
        "-----------------------------------------------",
    ])

    links_by_id: dict[int, tuple[str, str]] = {}
    for row in links:
        if not isinstance(row, dict):
            continue
        try:
            link_id = int(str(row.get("id")).strip())
        except (ValueError, TypeError, AttributeError):
            continue
        if link_id < 1 or link_id > DEFAULT_MAX_LINKS:
            continue
        links_by_id[link_id] = (str(row.get("label") or ""), str(row.get("url") or ""))

    for link_id in range(1, DEFAULT_MAX_LINKS + 1):
        label, url = links_by_id.get(link_id, ("", ""))
        lines.append(f"{link_id} | {label} | {url}")

    lines.extend([
        "",
        "",
        "===============================================",
        "5) SERVER JOIN",
        "===============================================",
        "# Server Join",
        f"showServerJoinSection: {'true' if show_server_join_section else 'false'}",
        f"joiningTitleFontSize: {joining_title_font_size}",
        f"joiningBodyFontSize: {joining_body_font_size}",
        f"joiningBodyAlign: {joining_body_align}",
        f"joiningTitle: {joining_title}",
        "",
        "<!-- Server Join Body starts below this line -->",
    ])
    for body_line in joining_body.split("\n") if joining_body else []:
        lines.append(str(body_line))
    lines.extend([
        "<!-- Server Join Body ends -->",
        "",
        "",
        "===============================================",
        "6) MAIN CONTENT",
        "===============================================",
        "# Pages",
        f"@enabledPages: {enabled_pages}",
        "",
        "// Max Pages is 8.",
        "// Body text color uses pageBodyColor from the Colors section above.",
        "// Bold color: wrap text with ** on both sides. Example: **Important rule**",
        "// Highlight color: wrap text with == on both sides. Example: ==Read this==",
        "// Preferred page fields: titleFontSize, bodyFontSize, titleLabel, bodyColumns (1-3).",
        "// Alignment fields per page:",
        "//   bodyAlign, body2Align, body3Align: left|center|right",
        "//   bodyBoxCentered, body2BoxCentered, body3BoxCentered: true|false",
    ])

    for row in pages:
        if not isinstance(row, dict):
            continue
        page_id = row.get("id")
        try:
            page_id = int(str(page_id).strip())
        except (ValueError, TypeError, AttributeError):
            continue
        title = str(row.get("title") or "")
        page_columns = row.get("columns", 1)
        try:
            page_columns = int(str(page_columns).strip())
        except (ValueError, TypeError, AttributeError):
            page_columns = 1
        page_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, page_columns))

        lines.extend([
            "",
            "-----------------------------------------------",
            f"PAGE {page_id}",
            "-----------------------------------------------",
            f"titleFontSize: {row.get('titleFontSize', DEFAULT_PAGE_TITLE_FONT_SIZE)}",
            f"bodyFontSize: {row.get('bodyFontSize', DEFAULT_PAGE_BODY_FONT_SIZE)}",
            f"titleLabel: {title}",
            f"bodyColumns: {page_columns}",
            f"autoSplitColumns: {'true' if _safe_bool(row.get('autoSplitColumns')) else 'false'}",
            f"bodyAlign: {_safe_align(row.get('bodyAlign', DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)}",
            f"bodyBoxCentered: {'true' if _safe_bool(row.get('bodyBoxCentered')) else 'false'}",
            f"body2Align: {_safe_align(row.get('bodyColumn2Align', DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)}",
            f"body2BoxCentered: {'true' if _safe_bool(row.get('bodyColumn2BoxCentered')) else 'false'}",
            f"body3Align: {_safe_align(row.get('bodyColumn3Align', DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)}",
            f"body3BoxCentered: {'true' if _safe_bool(row.get('bodyColumn3BoxCentered')) else 'false'}",
            "",
            "<!-- Body Column 1 starts below this line -->",
        ])

        body_lines = row.get("bodyLines")
        if isinstance(body_lines, list):
            for body_line in body_lines:
                lines.append(str(body_line))
        else:
            lines.append(str(row.get("body") or ""))

        lines.extend([
            "<!-- Body Column 1 ends -->",
            "<!-- Body Column 2, if Enabled, starts below this line -->",
        ])
        body_col2_lines = row.get("bodyColumn2Lines")
        if isinstance(body_col2_lines, list):
            for body_line in body_col2_lines:
                lines.append(str(body_line))

        lines.extend([
            "<!-- Body Column 2 ends -->",
            "<!-- Body Column 3, if Enabled, starts below this line -->",
        ])
        body_col3_lines = row.get("bodyColumn3Lines")
        if isinstance(body_col3_lines, list):
            for body_line in body_col3_lines:
                lines.append(str(body_line))
        lines.append("<!-- Body Column 3 ends -->")

    lines.extend([
        "",
        "",
        "===============================================",
        "7) ADMIN CONTENT",
        "===============================================",
        "# Admin Pages",
        f"@enabledAdminPages: {enabled_admin_pages}",
        "",
        "// Max of 8 pages",
    ])

    for row in admin_pages:
        if not isinstance(row, dict):
            continue
        page_id = row.get("id")
        try:
            page_id = int(str(page_id).strip())
        except (ValueError, TypeError, AttributeError):
            continue
        title = str(row.get("title") or f"Admin Page {page_id}")
        page_columns = row.get("columns", 1)
        try:
            page_columns = int(str(page_columns).strip())
        except (ValueError, TypeError, AttributeError):
            page_columns = 1
        page_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, page_columns))

        lines.extend([
            "",
            "-----------------------------------------------",
            f"ADMIN PAGE {page_id}",
            "-----------------------------------------------",
            f"titleFontSize: {row.get('titleFontSize', DEFAULT_PAGE_TITLE_FONT_SIZE)}",
            f"bodyFontSize: {row.get('bodyFontSize', DEFAULT_PAGE_BODY_FONT_SIZE)}",
            f"titleLabel: {title}",
            f"bodyColumns: {page_columns}",
            f"bodyAlign: {_safe_align(row.get('bodyAlign', DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)}",
            f"bodyBoxCentered: {'true' if _safe_bool(row.get('bodyBoxCentered')) else 'false'}",
            f"body2Align: {_safe_align(row.get('bodyColumn2Align', DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)}",
            f"body2BoxCentered: {'true' if _safe_bool(row.get('bodyColumn2BoxCentered')) else 'false'}",
            f"body3Align: {_safe_align(row.get('bodyColumn3Align', DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)}",
            f"body3BoxCentered: {'true' if _safe_bool(row.get('bodyColumn3BoxCentered')) else 'false'}",
            f"autoSplitColumns: {'true' if _safe_bool(row.get('autoSplitColumns')) else 'false'}",
            "",
            "<!-- Body Column 1 starts below this line -->",
        ])

        admin_body_lines = row.get("bodyLines")
        if isinstance(admin_body_lines, list):
            for body_line in admin_body_lines:
                lines.append(str(body_line))
        else:
            lines.append(str(row.get("body") or ""))

        lines.extend([
            "<!-- Body Column 1 ends -->",
            "<!-- Body Column 2, if Enabled, starts below this line -->",
        ])

        body_col2_lines = row.get("bodyColumn2Lines")
        if isinstance(body_col2_lines, list):
            for body_line in body_col2_lines:
                lines.append(str(body_line))

        lines.extend([
            "<!-- Body Column 2 ends -->",
            "<!-- Body Column 3, if Enabled, starts below this line -->",
        ])

        body_col3_lines = row.get("bodyColumn3Lines")
        if isinstance(body_col3_lines, list):
            for body_line in body_col3_lines:
                lines.append(str(body_line))
        lines.append("<!-- Body Column 3 ends -->")

    lines.append("")
    return "\n".join(lines)


def _parse_texts_markdown(text: str) -> dict:
    colors: dict[str, str] = {}
    headers: dict[str, str] = {}
    links: list[dict[str, str | int]] = []
    enabled_links: int | None = None
    link_source_mode: str | None = None
    links_web_xml_base_url: str | None = None
    show_options_section: bool | None = None
    show_admin_version: bool | None = None
    show_links_section: bool | None = None
    show_server_join_section: bool | None = None
    joining_title: str | None = None
    joining_title_hv: str | None = None
    joining_body: str | None = None
    joining_title_font_size: int | None = None
    joining_body_font_size: int | None = None
    joining_body_align: str | None = None
    in_server_join_body = False
    server_join_body_lines: list[str] = []
    enabled_pages: int | None = None
    pages: list[dict[str, str | int | bool | list[str]]] = []
    enabled_admin_pages: int | None = None
    admin_pages: list[dict[str, str | int | bool | list[str]]] = []
    current_admin_page: dict[str, str | int | bool | list[str]] | None = None
    in_admin_body = False
    current_admin_page_body_key = "bodyLines"

    section: str | None = None
    current_page: dict[str, str | int | bool | list[str]] | None = None
    in_comment_block = False
    pending_page_settings: dict[str, int | bool] = {}
    expecting_page_header = False
    current_page_body_key = "bodyLines"

    for raw_line in text.splitlines():
        line = raw_line.rstrip("\r")
        stripped = line.strip()

        if section == "serverjoin":
            if SERVER_JOIN_BODY_START_MARKER_RE.match(stripped):
                in_server_join_body = True
                server_join_body_lines = []
                continue
            if SERVER_JOIN_BODY_END_MARKER_RE.match(stripped):
                in_server_join_body = False
                joining_body = "\n".join(server_join_body_lines).rstrip("\n")
                continue
            if in_server_join_body:
                server_join_body_lines.append(line)
                continue

        if section == "pages" and current_page is not None:
            start_match = BODY_COLUMN_START_MARKER_RE.match(stripped)
            if start_match:
                col_num = int(start_match.group("col"))
                if col_num == 1:
                    current_page_body_key = "bodyLines"
                elif col_num == 2:
                    current_page_body_key = "bodyColumn2Lines"
                else:
                    current_page_body_key = "bodyColumn3Lines"
                if current_page_body_key not in current_page:
                    current_page[current_page_body_key] = []
                continue

        if section == "adminpages":
            start_match = BODY_COLUMN_START_MARKER_RE.match(stripped)
            if start_match and current_admin_page is not None:
                col_num = int(start_match.group("col"))
                if col_num == 1:
                    current_admin_page_body_key = "bodyLines"
                elif col_num == 2:
                    current_admin_page_body_key = "bodyColumn2Lines"
                else:
                    current_admin_page_body_key = "bodyColumn3Lines"
                if current_admin_page_body_key not in current_admin_page:
                    current_admin_page[current_admin_page_body_key] = []
                continue

            if ADMIN_BODY_START_MARKER_RE.match(stripped):
                in_admin_body = True
                current_admin_page_body_key = "bodyLines"
                if current_admin_page is not None and "bodyLines" not in current_admin_page:
                    current_admin_page["bodyLines"] = []
                continue
            if ADMIN_BODY_END_MARKER_RE.match(stripped):
                in_admin_body = False
                continue

            if in_admin_body and current_admin_page is not None:
                body_lines = current_admin_page.get(current_admin_page_body_key)
                if isinstance(body_lines, list):
                    body_lines.append(line)
                continue
            end_match = BODY_COLUMN_END_MARKER_RE.match(stripped)
            if end_match:
                current_admin_page_body_key = "bodyLines"
                continue

        if stripped.startswith("<!--"):
            in_comment_block = True
        if in_comment_block:
            if "-->" in stripped:
                in_comment_block = False
            continue

        if stripped.startswith("# "):
            if current_page is not None:
                _trim_page_columns_trailing_blanks(current_page)
                pages.append(current_page)
                current_page = None
            if current_admin_page is not None:
                _trim_page_columns_trailing_blanks(current_admin_page)
                body_lines = current_admin_page.get("bodyLines")
                current_admin_page["body"] = "\n".join(body_lines) if isinstance(body_lines, list) else ""
                body_col2_lines = current_admin_page.get("bodyColumn2Lines")
                current_admin_page["bodyColumn2"] = "\n".join(body_col2_lines) if isinstance(body_col2_lines, list) else ""
                body_col3_lines = current_admin_page.get("bodyColumn3Lines")
                current_admin_page["bodyColumn3"] = "\n".join(body_col3_lines) if isinstance(body_col3_lines, list) else ""
                admin_pages.append(current_admin_page)
                current_admin_page = None
            heading = stripped[2:].strip().lower()
            if heading == "colors":
                section = "colors"
            elif heading == "headers":
                section = "headers"
            elif heading == "links":
                section = "links"
            elif heading in {"server join", "serverjoin"}:
                section = "serverjoin"
            elif heading == "toggles":
                section = "toggles"
            elif heading == "pages":
                section = "pages"
            elif heading in {"admin pages", "adminpages"}:
                section = "adminpages"
            else:
                section = None
            continue

        if section == "toggles":
            if not stripped or stripped.startswith("//"):
                continue
            key, sep, value = line.partition(":")
            if not sep:
                continue
            token = key.strip().lower()
            if token == "showoptionssection":
                show_options_section = _safe_bool(value.strip(), default=True)
                continue
            if token == "showadminversion":
                show_admin_version = _safe_bool(value.strip(), default=True)
                continue
            if token == "showlinkssection":
                show_links_section = _safe_bool(value.strip(), default=True)
                continue
            if token == "showserverjoinsection":
                show_server_join_section = _safe_bool(value.strip(), default=True)
                continue
            continue

        if section == "serverjoin":
            if not stripped or stripped.startswith("//"):
                continue
            key, sep, value = line.partition(":")
            if not sep:
                continue
            token = key.strip().lower()
            parsed_value = value.strip()
            if token == "showserverjoinsection":
                show_server_join_section = _safe_bool(parsed_value, default=True)
                continue
            if token == "joiningtitle":
                joining_title = parsed_value
                continue
            if token == "joiningtitleHV":
                joining_title_hv = parsed_value
                continue
            if token == "joiningbody":
                joining_body = parsed_value
                continue
            if token == "joiningtitlefontsize":
                joining_title_font_size = _safe_font_size(parsed_value, DEFAULT_PAGE_TITLE_FONT_SIZE)
                continue
            if token == "joiningbodyfontsize":
                joining_body_font_size = _safe_font_size(parsed_value, DEFAULT_PAGE_BODY_FONT_SIZE)
                continue
            if token == "joiningbodyalign":
                joining_body_align = _safe_align(parsed_value, "center")
                continue
            continue

        if section == "pages" and (stripped.startswith("@enabledPages:") or stripped.startswith("enabledPages:")):
            _, _, value = stripped.partition(":")
            try:
                parsed = int(value.strip())
            except ValueError:
                parsed = 1
            enabled_pages = max(1, min(DEFAULT_MAX_PAGES, parsed))
            continue

        if section == "adminpages" and (stripped.startswith("@enabledAdminPages:") or stripped.startswith("enabledAdminPages:")):
            _, _, value = stripped.partition(":")
            try:
                parsed = int(value.strip())
            except ValueError:
                parsed = 1
            enabled_admin_pages = max(1, min(DEFAULT_MAX_PAGES, parsed))
            continue

        admin_page_banner_match = ADMIN_PAGE_BANNER_RE.match(stripped) if section == "adminpages" else None
        if section == "adminpages" and (stripped.startswith("## ") or admin_page_banner_match):
            if current_admin_page is not None:
                _trim_page_columns_trailing_blanks(current_admin_page)
                body_lines = current_admin_page.get("bodyLines")
                current_admin_page["body"] = "\n".join(body_lines) if isinstance(body_lines, list) else ""
                body_col2_lines = current_admin_page.get("bodyColumn2Lines")
                current_admin_page["bodyColumn2"] = "\n".join(body_col2_lines) if isinstance(body_col2_lines, list) else ""
                body_col3_lines = current_admin_page.get("bodyColumn3Lines")
                current_admin_page["bodyColumn3"] = "\n".join(body_col3_lines) if isinstance(body_col3_lines, list) else ""
                admin_pages.append(current_admin_page)
            if admin_page_banner_match:
                page_id = int(admin_page_banner_match.group("num"))
                page_title = f"Admin Page {page_id}"
            else:
                page_header = stripped[3:].strip()
                page_id_text, _, page_title = page_header.partition(":")
                page_title = page_title.strip()
                try:
                    page_id = int(page_id_text.strip())
                except ValueError:
                    page_id = len(admin_pages) + 1
            current_admin_page = {
                "id": page_id,
                "title": page_title or f"Admin Page {page_id}",
                "columns": 1,
                "bodyAlign": DEFAULT_BODY_ALIGN,
                "bodyBoxCentered": False,
                "bodyColumn2Align": DEFAULT_BODY_ALIGN,
                "bodyColumn2BoxCentered": False,
                "bodyColumn3Align": DEFAULT_BODY_ALIGN,
                "bodyColumn3BoxCentered": False,
                "autoSplitColumns": False,
                "body": "",
                "bodyLines": [],
                "bodyColumn2Lines": [],
                "bodyColumn3Lines": [],
            }
            current_admin_page_body_key = "bodyLines"
            in_admin_body = False
            continue

        if section == "adminpages" and current_admin_page is not None:
            if not stripped or stripped.startswith("//"):
                continue
            if SEPARATOR_LINE_RE.match(stripped):
                continue
            if stripped.lower().startswith("bodycolumns:"):
                _, _, value = stripped.partition(":")
                try:
                    parsed_columns = int(value.strip())
                except ValueError:
                    parsed_columns = 1
                current_admin_page["columns"] = max(1, min(PAGE_BODY_MAX_COLUMNS, parsed_columns))
                continue
            if stripped.lower().startswith("autosplitcolumns:"):
                _, _, value = stripped.partition(":")
                current_admin_page["autoSplitColumns"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("bodywidthpercent:"):
                # Width is auto-generated from layout logic for admin pages.
                continue
            if stripped.lower().startswith("bodyalign:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyAlign"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("bodyboxcentered:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyBoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("body2align:") or stripped.lower().startswith("bodycolumn2align:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyColumn2Align"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("body2boxcentered:") or stripped.lower().startswith("bodycolumn2boxcentered:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyColumn2BoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("body3align:") or stripped.lower().startswith("bodycolumn3align:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyColumn3Align"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("body3boxcentered:") or stripped.lower().startswith("bodycolumn3boxcentered:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyColumn3BoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("scrollablebody:"):
                # Scroll wrappers are intentionally disabled.
                continue
            if stripped.startswith("bodySourceUrl:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodySourceUrl"] = value.strip()
                continue
            if stripped.startswith("body2SourceUrl:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyColumn2SourceUrl"] = value.strip()
                continue
            if stripped.startswith("body3SourceUrl:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyColumn3SourceUrl"] = value.strip()
                continue
            if stripped.startswith("titleFontSize:"):
                _, _, value = stripped.partition(":")
                current_admin_page["titleFontSize"] = _safe_font_size(value.strip(), DEFAULT_PAGE_TITLE_FONT_SIZE)
                continue
            if stripped.startswith("bodyFontSize:"):
                _, _, value = stripped.partition(":")
                current_admin_page["bodyFontSize"] = _safe_font_size(value.strip(), DEFAULT_PAGE_BODY_FONT_SIZE)
                continue
            if stripped.startswith("titleLabel:"):
                _, _, value = stripped.partition(":")
                current_admin_page["title"] = value.strip()
                continue
            body_lines = current_admin_page.get(current_admin_page_body_key)
            if isinstance(body_lines, list):
                body_lines.append(line)
            continue

        page_banner_match = PAGE_BANNER_RE.match(stripped) if section == "pages" else None
        if section == "pages" and page_banner_match:
            if current_page is not None:
                _trim_page_columns_trailing_blanks(current_page)
                pages.append(current_page)
            page_id = int(page_banner_match.group("num"))
            current_page = {
                "id": page_id,
                "title": f"Page {page_id}",
                "columns": 1,
                "bodyWidthPercent": DEFAULT_BODY_WIDTH_PERCENT,
                "bodyAlign": DEFAULT_BODY_ALIGN,
                "bodyBoxCentered": False,
                "bodyColumn2Align": DEFAULT_BODY_ALIGN,
                "bodyColumn2BoxCentered": False,
                "bodyColumn3Align": DEFAULT_BODY_ALIGN,
                "bodyColumn3BoxCentered": False,
                "autoSplitColumns": False,
                "bodyLines": [],
            }
            current_page_body_key = "bodyLines"
            if "titleFontSize" in pending_page_settings:
                current_page["titleFontSize"] = pending_page_settings["titleFontSize"]
            if "bodyFontSize" in pending_page_settings:
                current_page["bodyFontSize"] = pending_page_settings["bodyFontSize"]
            if "autoSplitColumns" in pending_page_settings:
                current_page["autoSplitColumns"] = pending_page_settings["autoSplitColumns"]
            if "bodyWidthPercent" in pending_page_settings:
                current_page["bodyWidthPercent"] = pending_page_settings["bodyWidthPercent"]
            if "bodyAlign" in pending_page_settings:
                current_page["bodyAlign"] = pending_page_settings["bodyAlign"]
            if "bodyBoxCentered" in pending_page_settings:
                current_page["bodyBoxCentered"] = pending_page_settings["bodyBoxCentered"]
            if "bodyColumn2Align" in pending_page_settings:
                current_page["bodyColumn2Align"] = pending_page_settings["bodyColumn2Align"]
            if "bodyColumn2BoxCentered" in pending_page_settings:
                current_page["bodyColumn2BoxCentered"] = pending_page_settings["bodyColumn2BoxCentered"]
            if "bodyColumn3Align" in pending_page_settings:
                current_page["bodyColumn3Align"] = pending_page_settings["bodyColumn3Align"]
            if "bodyColumn3BoxCentered" in pending_page_settings:
                current_page["bodyColumn3BoxCentered"] = pending_page_settings["bodyColumn3BoxCentered"]
            pending_page_settings = {}
            expecting_page_header = False
            continue

        if section == "pages" and stripped.startswith("## "):
            page_header = stripped[3:].strip()
            page_id_text, _, page_title = page_header.partition(":")
            page_id_text = page_id_text.strip()
            page_title = page_title.strip()
            try:
                page_id = int(page_id_text)
            except ValueError:
                page_id = None

            if current_page is None:
                if page_id is None:
                    continue
                current_page = {
                    "id": page_id,
                    "title": page_title or f"Page {page_id}",
                    "columns": 1,
                    "bodyWidthPercent": DEFAULT_BODY_WIDTH_PERCENT,
                    "bodyAlign": DEFAULT_BODY_ALIGN,
                    "bodyBoxCentered": False,
                    "bodyColumn2Align": DEFAULT_BODY_ALIGN,
                    "bodyColumn2BoxCentered": False,
                    "bodyColumn3Align": DEFAULT_BODY_ALIGN,
                    "bodyColumn3BoxCentered": False,
                    "autoSplitColumns": False,
                    "bodyLines": [],
                }
                current_page_body_key = "bodyLines"
                if "titleFontSize" in pending_page_settings:
                    current_page["titleFontSize"] = pending_page_settings["titleFontSize"]
                if "bodyFontSize" in pending_page_settings:
                    current_page["bodyFontSize"] = pending_page_settings["bodyFontSize"]
                if "autoSplitColumns" in pending_page_settings:
                    current_page["autoSplitColumns"] = pending_page_settings["autoSplitColumns"]
                if "bodyWidthPercent" in pending_page_settings:
                    current_page["bodyWidthPercent"] = pending_page_settings["bodyWidthPercent"]
                if "bodyAlign" in pending_page_settings:
                    current_page["bodyAlign"] = pending_page_settings["bodyAlign"]
                if "bodyBoxCentered" in pending_page_settings:
                    current_page["bodyBoxCentered"] = pending_page_settings["bodyBoxCentered"]
                if "bodyColumn2Align" in pending_page_settings:
                    current_page["bodyColumn2Align"] = pending_page_settings["bodyColumn2Align"]
                if "bodyColumn2BoxCentered" in pending_page_settings:
                    current_page["bodyColumn2BoxCentered"] = pending_page_settings["bodyColumn2BoxCentered"]
                if "bodyColumn3Align" in pending_page_settings:
                    current_page["bodyColumn3Align"] = pending_page_settings["bodyColumn3Align"]
                if "bodyColumn3BoxCentered" in pending_page_settings:
                    current_page["bodyColumn3BoxCentered"] = pending_page_settings["bodyColumn3BoxCentered"]
                pending_page_settings = {}
                expecting_page_header = False
                continue

            if page_title:
                current_page["title"] = page_title
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

            if stripped.startswith("@enabledLinks:"):
                _, _, value = stripped.partition(":")
                try:
                    parsed = int(value.strip())
                except ValueError:
                    parsed = 0
                enabled_links = max(0, min(DEFAULT_MAX_LINKS, parsed))
                continue

            if stripped.lower().startswith("@linksclientorserver:"):
                _, _, value = stripped.partition(":")
                link_source_mode = _safe_link_source_mode(value, DEFAULT_LINK_SOURCE_MODE)
                continue

            if stripped.lower().startswith("@linksxmlwebbaseurl:"):
                _, _, value = stripped.partition(":")
                links_web_xml_base_url = _normalize_web_base_url(value)
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
            if stripped.lower().startswith("@columns:") or stripped.lower().startswith("bodycolumns:"):
                _, _, value = stripped.partition(":")
                try:
                    parsed_columns = int(value.strip())
                except ValueError:
                    parsed_columns = 1
                current_page["columns"] = max(1, min(PAGE_BODY_MAX_COLUMNS, parsed_columns))
                continue
            if stripped.lower().startswith("@autosplitcolumns:") or stripped.lower().startswith("autosplitcolumns:"):
                _, _, value = stripped.partition(":")
                parsed_bool = _safe_bool(value, default=False)
                if expecting_page_header:
                    pending_page_settings["autoSplitColumns"] = parsed_bool
                else:
                    current_page["autoSplitColumns"] = parsed_bool
                continue
            if stripped.lower().startswith("@bodywidthpercent:") or stripped.lower().startswith("bodywidthpercent:"):
                _, _, value = stripped.partition(":")
                parsed_percent = _safe_body_width_percent(value, DEFAULT_BODY_WIDTH_PERCENT)
                if expecting_page_header:
                    pending_page_settings["bodyWidthPercent"] = parsed_percent
                else:
                    current_page["bodyWidthPercent"] = parsed_percent
                continue
            if stripped.lower().startswith("@bodyalign:") or stripped.lower().startswith("bodyalign:"):
                _, _, value = stripped.partition(":")
                parsed_align = _safe_align(value, DEFAULT_BODY_ALIGN)
                if expecting_page_header:
                    pending_page_settings["bodyAlign"] = parsed_align
                else:
                    current_page["bodyAlign"] = parsed_align
                continue
            if stripped.lower().startswith("@bodyboxcentered:") or stripped.lower().startswith("bodyboxcentered:"):
                _, _, value = stripped.partition(":")
                parsed_bool = _safe_bool(value, default=False)
                if expecting_page_header:
                    pending_page_settings["bodyBoxCentered"] = parsed_bool
                else:
                    current_page["bodyBoxCentered"] = parsed_bool
                continue
            if stripped.lower().startswith("@body2align:") or stripped.lower().startswith("body2align:") or stripped.lower().startswith("@bodycolumn2align:") or stripped.lower().startswith("bodycolumn2align:"):
                _, _, value = stripped.partition(":")
                parsed_align = _safe_align(value, DEFAULT_BODY_ALIGN)
                if expecting_page_header:
                    pending_page_settings["bodyColumn2Align"] = parsed_align
                else:
                    current_page["bodyColumn2Align"] = parsed_align
                continue
            if stripped.lower().startswith("@body2boxcentered:") or stripped.lower().startswith("body2boxcentered:") or stripped.lower().startswith("@bodycolumn2boxcentered:") or stripped.lower().startswith("bodycolumn2boxcentered:"):
                _, _, value = stripped.partition(":")
                parsed_bool = _safe_bool(value, default=False)
                if expecting_page_header:
                    pending_page_settings["bodyColumn2BoxCentered"] = parsed_bool
                else:
                    current_page["bodyColumn2BoxCentered"] = parsed_bool
                continue
            if stripped.lower().startswith("@body3align:") or stripped.lower().startswith("body3align:") or stripped.lower().startswith("@bodycolumn3align:") or stripped.lower().startswith("bodycolumn3align:"):
                _, _, value = stripped.partition(":")
                parsed_align = _safe_align(value, DEFAULT_BODY_ALIGN)
                if expecting_page_header:
                    pending_page_settings["bodyColumn3Align"] = parsed_align
                else:
                    current_page["bodyColumn3Align"] = parsed_align
                continue
            if stripped.lower().startswith("@body3boxcentered:") or stripped.lower().startswith("body3boxcentered:") or stripped.lower().startswith("@bodycolumn3boxcentered:") or stripped.lower().startswith("bodycolumn3boxcentered:"):
                _, _, value = stripped.partition(":")
                parsed_bool = _safe_bool(value, default=False)
                if expecting_page_header:
                    pending_page_settings["bodyColumn3BoxCentered"] = parsed_bool
                else:
                    current_page["bodyColumn3BoxCentered"] = parsed_bool
                continue
            if stripped.lower().startswith("@scrollablebody:") or stripped.lower().startswith("scrollablebody:"):
                # Scroll wrappers are intentionally disabled.
                continue
            if stripped.lower() == "@body1" or stripped.lower() == "@column1":
                current_page_body_key = "bodyLines"
                if "bodyLines" not in current_page:
                    current_page["bodyLines"] = []
                continue
            if stripped.lower() == "@body2" or stripped.lower() == "@column2":
                current_page_body_key = "bodyColumn2Lines"
                if "bodyColumn2Lines" not in current_page:
                    current_page["bodyColumn2Lines"] = []
                continue
            if stripped.lower() == "@body3" or stripped.lower() == "@column3":
                current_page_body_key = "bodyColumn3Lines"
                if "bodyColumn3Lines" not in current_page:
                    current_page["bodyColumn3Lines"] = []
                continue
            if PAGE_BANNER_RE.match(stripped):
                expecting_page_header = True
                continue
            if SEPARATOR_LINE_RE.match(stripped):
                # Decorative separator lines are ignored within a page block.
                continue
            if stripped.startswith("@bodySourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodySourceUrl"] = value.strip()
                continue
            if stripped.startswith("bodySourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodySourceUrl"] = value.strip()
                continue
            if stripped.startswith("@body2SourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodyColumn2SourceUrl"] = value.strip()
                continue
            if stripped.startswith("body2SourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodyColumn2SourceUrl"] = value.strip()
                continue
            if stripped.startswith("@body3SourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodyColumn3SourceUrl"] = value.strip()
                continue
            if stripped.startswith("body3SourceUrl:"):
                _, _, value = stripped.partition(":")
                if expecting_page_header:
                    continue
                current_page["bodyColumn3SourceUrl"] = value.strip()
                continue
            if stripped.startswith("@titleFontSize:"):
                _, _, value = stripped.partition(":")
                parsed_size = _safe_font_size(value.strip(), DEFAULT_PAGE_TITLE_FONT_SIZE)
                if expecting_page_header:
                    pending_page_settings["titleFontSize"] = parsed_size
                else:
                    current_page["titleFontSize"] = parsed_size
                continue
            if stripped.startswith("titleFontSize:"):
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
            if stripped.startswith("bodyFontSize:"):
                _, _, value = stripped.partition(":")
                parsed_size = _safe_font_size(value.strip(), DEFAULT_PAGE_BODY_FONT_SIZE)
                if expecting_page_header:
                    pending_page_settings["bodyFontSize"] = parsed_size
                else:
                    current_page["bodyFontSize"] = parsed_size
                continue
            if stripped.startswith("titleLabel:"):
                _, _, value = stripped.partition(":")
                current_page["title"] = value.strip()
                continue
            body_lines = current_page.get("bodyLines")
            if current_page_body_key != "bodyLines":
                body_lines = current_page.get(current_page_body_key)
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
            if stripped.lower().startswith("@autosplitcolumns:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["autoSplitColumns"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("@bodywidthpercent:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyWidthPercent"] = _safe_body_width_percent(value, DEFAULT_BODY_WIDTH_PERCENT)
                continue
            if stripped.lower().startswith("@bodyalign:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyAlign"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("@bodyboxcentered:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyBoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("@body2align:") or stripped.lower().startswith("@bodycolumn2align:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn2Align"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("@body2boxcentered:") or stripped.lower().startswith("@bodycolumn2boxcentered:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn2BoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("@body3align:") or stripped.lower().startswith("@bodycolumn3align:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn3Align"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("@body3boxcentered:") or stripped.lower().startswith("@bodycolumn3boxcentered:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn3BoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("@scrollablebody:"):
                # Scroll wrappers are intentionally disabled.
                continue
            if stripped.lower().startswith("autosplitcolumns:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["autoSplitColumns"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("bodywidthpercent:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyWidthPercent"] = _safe_body_width_percent(value, DEFAULT_BODY_WIDTH_PERCENT)
                continue
            if stripped.lower().startswith("bodyalign:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyAlign"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("bodyboxcentered:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyBoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("body2align:") or stripped.lower().startswith("bodycolumn2align:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn2Align"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("body2boxcentered:") or stripped.lower().startswith("bodycolumn2boxcentered:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn2BoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("body3align:") or stripped.lower().startswith("bodycolumn3align:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn3Align"] = _safe_align(value, DEFAULT_BODY_ALIGN)
                continue
            if stripped.lower().startswith("body3boxcentered:") or stripped.lower().startswith("bodycolumn3boxcentered:"):
                _, _, value = stripped.partition(":")
                pending_page_settings["bodyColumn3BoxCentered"] = _safe_bool(value, default=False)
                continue
            if stripped.lower().startswith("scrollablebody:"):
                # Scroll wrappers are intentionally disabled.
                continue
            if stripped.startswith("//") or BODY_BOUNDARY_MARKER_RE.match(stripped):
                continue
            if PAGE_BANNER_RE.match(stripped) or SEPARATOR_LINE_RE.match(stripped):
                expecting_page_header = True
                continue

    if current_page is not None:
        _trim_page_columns_trailing_blanks(current_page)
        pages.append(current_page)
    if current_admin_page is not None:
        _trim_page_columns_trailing_blanks(current_admin_page)
        body_lines = current_admin_page.get("bodyLines")
        current_admin_page["body"] = "\n".join(body_lines) if isinstance(body_lines, list) else ""
        body_col2_lines = current_admin_page.get("bodyColumn2Lines")
        current_admin_page["bodyColumn2"] = "\n".join(body_col2_lines) if isinstance(body_col2_lines, list) else ""
        body_col3_lines = current_admin_page.get("bodyColumn3Lines")
        current_admin_page["bodyColumn3"] = "\n".join(body_col3_lines) if isinstance(body_col3_lines, list) else ""
        admin_pages.append(current_admin_page)

    return {
        "colors": colors,
        "headers": headers,
        "pages": pages,
        "links": links,
        "enabledLinks": enabled_links,
        "linksClientOrServer": link_source_mode,
        "linksXMLWebBaseUrl": links_web_xml_base_url,
        "showOptionsSection": show_options_section,
        "showAdminVersion": show_admin_version,
        "showLinksSection": show_links_section,
        "showServerJoinSection": show_server_join_section,
        "serverJoin": {
            "showServerJoinSection": show_server_join_section,
            "joiningTitle": joining_title,
            "joiningTitleHV": joining_title_hv,
            "joiningBody": joining_body,
            "joiningTitleFontSize": joining_title_font_size,
            "joiningBodyFontSize": joining_body_font_size,
            "joiningBodyAlign": joining_body_align,
        },
        "enabledPages": enabled_pages,
        "adminPages": admin_pages,
        "enabledAdminPages": enabled_admin_pages,
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
                "linkBackgroundColor",
                "optionsBackgroundColor",
                "panelBorderColor",
                "borderColor",
                "headerTitleColor",
                "headerMottoColor",
                "pageTabButtonBackgroundColor",
                "pageTabButtonBorderColor",
                "sectionButtonBorderColor",
                "pageTabButtonSelectedColor",
                "pageTabButtonTextColor",
                "tabButtonSelectedColor",
                "pageButtonTextColor",
                "pageTitleColor",
                "pageBodyColor",
                "textBoldingColor",
                "textHighlightColor",
                "linkButtonBackgroundColor",
                "linkButtonBorderColor",
                "linkButtonTextColor",
                "linkButtonHoverColor",
            }:
                theme_color[key] = str(value)
                applied += 1

    headers_overrides = payload.get("headers", {})
    if headers_overrides and not isinstance(headers_overrides, dict):
        raise ValueError(f"Texts file 'headers' must be an object: {texts_path}")

    labels = updated.setdefault("labels", {})
    for key, value in headers_overrides.items() if isinstance(headers_overrides, dict) else []:
        if key in {"optionsTitle", "optionsTitleHV", "linkLabel", "linkLabelHV"}:
            # Options section title is feature-owned, not part of user text overrides.
            continue
        labels[key] = str(value)
        applied += 1

    pages_overrides_raw = payload.get("pages")
    admin_pages_overrides_raw = payload.get("adminPages")
    enabled_pages_raw = payload.get("enabledPages")
    enabled_admin_pages_raw = payload.get("enabledAdminPages")
    if pages_overrides_raw is not None and not isinstance(pages_overrides_raw, list):
        raise ValueError(f"Texts file 'pages' must be a list: {texts_path}")
    if admin_pages_overrides_raw is not None and not isinstance(admin_pages_overrides_raw, list):
        raise ValueError(f"Texts file 'adminPages' must be a list: {texts_path}")

    existing_tabs = updated.get("tabs", []) if isinstance(updated.get("tabs", []), list) else []
    if isinstance(pages_overrides_raw, list):
        if not pages_overrides_raw:
            raise ValueError("Texts file must include at least one page in 'pages'.")

        if enabled_pages_raw is None:
            enabled_page_count = len([
                tab
                for tab in existing_tabs
                if isinstance(tab, dict) and tab.get("enabled", True) is not False
            ])
            enabled_page_count = max(1, min(DEFAULT_MAX_PAGES, enabled_page_count or 1))
        else:
            try:
                enabled_page_count = int(str(enabled_pages_raw).strip())
            except (ValueError, TypeError, AttributeError):
                enabled_page_count = 1
            enabled_page_count = max(1, min(DEFAULT_MAX_PAGES, enabled_page_count))

        tab_layout = get_tab_layout(updated)
        allowed_tabs = min(DEFAULT_MAX_PAGES, tab_layout["maxTabs"])
        if enabled_page_count > allowed_tabs:
            raise ValueError(
                f"Too many enabled pages for current tab layout: enabledPages={enabled_page_count}, allowed={allowed_tabs}. "
                "Lower @enabledPages or adjust ui.layout settings in source JSON."
            )

        rows_by_pos: dict[int, dict] = {}
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

            if 0 <= target_idx < DEFAULT_MAX_PAGES:
                rows_by_pos[target_idx] = row

        new_tabs: list[dict] = []
        for slot in range(DEFAULT_MAX_PAGES):
            row = rows_by_pos.get(slot, {})
            source_tab: dict = {}
            if 0 <= slot < len(existing_tabs) and isinstance(existing_tabs[slot], dict):
                source_tab = existing_tabs[slot]

            page_num = slot + 1
            title_value = str(row.get("title") or source_tab.get("title") or source_tab.get("button") or f"Page {page_num}")
            resolved_body = _resolve_page_column_text(row, "bodyLines", "body", "bodySourceUrl")
            body_value = resolved_body if resolved_body is not None else str(source_tab.get("body") or "")
            resolved_body_col2 = _resolve_page_column_text(row, "bodyColumn2Lines", "bodyColumn2", "bodyColumn2SourceUrl")
            body_col2_value = (
                resolved_body_col2 if resolved_body_col2 is not None else str(source_tab.get("bodyColumn2") or "")
            )
            resolved_body_col3 = _resolve_page_column_text(row, "bodyColumn3Lines", "bodyColumn3", "bodyColumn3SourceUrl")
            body_col3_value = (
                resolved_body_col3 if resolved_body_col3 is not None else str(source_tab.get("bodyColumn3") or "")
            )

            page_columns_raw = row.get("columns", source_tab.get("bodyColumns", 1))
            try:
                page_columns = int(str(page_columns_raw).strip())
            except (ValueError, TypeError, AttributeError):
                page_columns = 1
            page_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, page_columns))

            body_font_size = _safe_font_size(
                row.get("bodyFontSize", source_tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE)),
                DEFAULT_PAGE_BODY_FONT_SIZE,
            )
            title_font_size = _safe_font_size(
                row.get("titleFontSize", source_tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE)),
                DEFAULT_PAGE_TITLE_FONT_SIZE,
            )
            auto_split_columns = _safe_bool(
                row.get("autoSplitColumns", source_tab.get("autoSplitColumns", False)),
                default=False,
            )
            body_width_percent = _safe_body_width_percent(
                row.get("bodyWidthPercent", source_tab.get("bodyWidthPercent", DEFAULT_BODY_WIDTH_PERCENT)),
                DEFAULT_BODY_WIDTH_PERCENT,
            )
            body_align = _safe_align(
                row.get("bodyAlign", source_tab.get("bodyAlign", DEFAULT_BODY_ALIGN)),
                DEFAULT_BODY_ALIGN,
            )
            body_box_centered = _safe_bool(
                row.get("bodyBoxCentered", source_tab.get("bodyBoxCentered", False)),
                default=False,
            )
            body_col2_align = _safe_align(
                row.get("bodyColumn2Align", source_tab.get("bodyColumn2Align", DEFAULT_BODY_ALIGN)),
                DEFAULT_BODY_ALIGN,
            )
            body_col2_box_centered = _safe_bool(
                row.get("bodyColumn2BoxCentered", source_tab.get("bodyColumn2BoxCentered", False)),
                default=False,
            )
            body_col3_align = _safe_align(
                row.get("bodyColumn3Align", source_tab.get("bodyColumn3Align", DEFAULT_BODY_ALIGN)),
                DEFAULT_BODY_ALIGN,
            )
            body_col3_box_centered = _safe_bool(
                row.get("bodyColumn3BoxCentered", source_tab.get("bodyColumn3BoxCentered", False)),
                default=False,
            )

            if auto_split_columns and page_columns > 1:
                split_columns = _split_body_text_by_fit(body_value, page_columns, body_font_size)
                body_value = split_columns[0]
                body_col2_value = split_columns[1] if page_columns >= 2 else ""
                body_col3_value = split_columns[2] if page_columns >= 3 else ""

            if page_columns < 2:
                body_col2_value = ""
            if page_columns < 3:
                body_col3_value = ""

            new_tab = {
                "id": str(source_tab.get("id") or f"page_{page_num}"),
                "enabled": page_num <= enabled_page_count,
                "button": title_value,
                "title": title_value,
                "body": body_value,
                "bodyColumns": page_columns,
                "bodyColumn2": body_col2_value,
                "bodyColumn3": body_col3_value,
                "titleFontSize": title_font_size,
                "bodyFontSize": body_font_size,
                "autoSplitColumns": auto_split_columns,
                "bodyWidthPercent": body_width_percent,
                "bodyAlign": body_align,
                "bodyBoxCentered": body_box_centered,
                "bodyColumn2Align": body_col2_align,
                "bodyColumn2BoxCentered": body_col2_box_centered,
                "bodyColumn3Align": body_col3_align,
                "bodyColumn3BoxCentered": body_col3_box_centered,
            }
            if row.get("bodySourceUrl"):
                new_tab["bodySourceUrl"] = str(row.get("bodySourceUrl"))
            if row.get("bodyColumn2SourceUrl"):
                new_tab["bodyColumn2SourceUrl"] = str(row.get("bodyColumn2SourceUrl"))
            if row.get("bodyColumn3SourceUrl"):
                new_tab["bodyColumn3SourceUrl"] = str(row.get("bodyColumn3SourceUrl"))
            new_tabs.append(new_tab)
            applied += 10

        updated["tabs"] = new_tabs

    existing_admin_tabs = updated.get("adminTabs", []) if isinstance(updated.get("adminTabs", []), list) else []
    if isinstance(admin_pages_overrides_raw, list):
        if enabled_admin_pages_raw is None:
            enabled_admin_count = len([
                tab
                for tab in existing_admin_tabs
                if isinstance(tab, dict) and tab.get("enabled", True) is not False
            ])
            if enabled_admin_count <= 0:
                enabled_admin_count = len([
                    tab
                    for tab in updated.get("tabs", [])
                    if isinstance(tab, dict) and tab.get("enabled", True) is not False
                ])
            enabled_admin_count = max(1, min(DEFAULT_MAX_PAGES, enabled_admin_count or 1))
        else:
            try:
                enabled_admin_count = int(str(enabled_admin_pages_raw).strip())
            except (ValueError, TypeError, AttributeError):
                enabled_admin_count = 1
            enabled_admin_count = max(1, min(DEFAULT_MAX_PAGES, enabled_admin_count))

        rows_by_pos: dict[int, dict] = {}
        for i, row in enumerate(admin_pages_overrides_raw):
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
            if 0 <= target_idx < DEFAULT_MAX_PAGES:
                rows_by_pos[target_idx] = row

        new_admin_tabs: list[dict] = []
        player_tabs = updated.get("tabs", []) if isinstance(updated.get("tabs", []), list) else []
        for slot in range(DEFAULT_MAX_PAGES):
            row = rows_by_pos.get(slot, {})
            source_tab: dict = {}
            if 0 <= slot < len(existing_admin_tabs) and isinstance(existing_admin_tabs[slot], dict):
                source_tab = existing_admin_tabs[slot]
            elif 0 <= slot < len(player_tabs) and isinstance(player_tabs[slot], dict):
                source_tab = copy.deepcopy(player_tabs[slot])

            page_num = slot + 1
            title_value = str(row.get("title") or source_tab.get("title") or source_tab.get("button") or f"Admin Page {page_num}")
            resolved_admin_body = _resolve_page_column_text(row, "bodyLines", "body", "bodySourceUrl")
            body_value = resolved_admin_body if resolved_admin_body is not None else str(source_tab.get("body") or "")
            resolved_admin_body_col2 = _resolve_page_column_text(row, "bodyColumn2Lines", "bodyColumn2", "bodyColumn2SourceUrl")
            body_col2_value = (
                resolved_admin_body_col2 if resolved_admin_body_col2 is not None else str(source_tab.get("bodyColumn2") or "")
            )
            resolved_admin_body_col3 = _resolve_page_column_text(row, "bodyColumn3Lines", "bodyColumn3", "bodyColumn3SourceUrl")
            body_col3_value = (
                resolved_admin_body_col3 if resolved_admin_body_col3 is not None else str(source_tab.get("bodyColumn3") or "")
            )

            page_columns_raw = row.get("columns", source_tab.get("bodyColumns", 1))
            try:
                page_columns = int(str(page_columns_raw).strip())
            except (ValueError, TypeError, AttributeError):
                page_columns = 1
            page_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, page_columns))

            body_font_size = _safe_font_size(
                row.get("bodyFontSize", source_tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE)),
                DEFAULT_PAGE_BODY_FONT_SIZE,
            )
            title_font_size = _safe_font_size(
                row.get("titleFontSize", source_tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE)),
                DEFAULT_PAGE_TITLE_FONT_SIZE,
            )
            auto_split_columns = _safe_bool(
                row.get("autoSplitColumns", source_tab.get("autoSplitColumns", False)),
                default=False,
            )
            body_align = _safe_align(
                row.get("bodyAlign", source_tab.get("bodyAlign", DEFAULT_BODY_ALIGN)),
                DEFAULT_BODY_ALIGN,
            )
            body_box_centered = _safe_bool(
                row.get("bodyBoxCentered", source_tab.get("bodyBoxCentered", False)),
                default=False,
            )
            body_col2_align = _safe_align(
                row.get("bodyColumn2Align", source_tab.get("bodyColumn2Align", DEFAULT_BODY_ALIGN)),
                DEFAULT_BODY_ALIGN,
            )
            body_col2_box_centered = _safe_bool(
                row.get("bodyColumn2BoxCentered", source_tab.get("bodyColumn2BoxCentered", False)),
                default=False,
            )
            body_col3_align = _safe_align(
                row.get("bodyColumn3Align", source_tab.get("bodyColumn3Align", DEFAULT_BODY_ALIGN)),
                DEFAULT_BODY_ALIGN,
            )
            body_col3_box_centered = _safe_bool(
                row.get("bodyColumn3BoxCentered", source_tab.get("bodyColumn3BoxCentered", False)),
                default=False,
            )

            if auto_split_columns and page_columns > 1:
                split_columns = _split_body_text_by_fit(body_value, page_columns, body_font_size)
                body_value = split_columns[0]
                body_col2_value = split_columns[1] if page_columns >= 2 else ""
                body_col3_value = split_columns[2] if page_columns >= 3 else ""

            if page_columns < 2:
                body_col2_value = ""
            if page_columns < 3:
                body_col3_value = ""

            new_admin_tab = copy.deepcopy(source_tab) if isinstance(source_tab, dict) else {}
            new_admin_tab.update(
                {
                    "id": str(new_admin_tab.get("id") or f"admin_page_{page_num}"),
                    "enabled": page_num <= enabled_admin_count,
                    "button": title_value,
                    "title": title_value,
                    "body": body_value,
                    "bodyColumns": page_columns,
                    "bodyColumn2": body_col2_value,
                    "bodyColumn3": body_col3_value,
                    "titleFontSize": title_font_size,
                    "bodyFontSize": body_font_size,
                    "autoSplitColumns": auto_split_columns,
                    "bodyAlign": body_align,
                    "bodyBoxCentered": body_box_centered,
                    "bodyColumn2Align": body_col2_align,
                    "bodyColumn2BoxCentered": body_col2_box_centered,
                    "bodyColumn3Align": body_col3_align,
                    "bodyColumn3BoxCentered": body_col3_box_centered,
                }
            )
            new_admin_tab.pop("bodyWidthPercent", None)
            if row.get("bodySourceUrl"):
                new_admin_tab["bodySourceUrl"] = str(row.get("bodySourceUrl"))
            if row.get("bodyColumn2SourceUrl"):
                new_admin_tab["bodyColumn2SourceUrl"] = str(row.get("bodyColumn2SourceUrl"))
            if row.get("bodyColumn3SourceUrl"):
                new_admin_tab["bodyColumn3SourceUrl"] = str(row.get("bodyColumn3SourceUrl"))
            new_admin_tabs.append(new_admin_tab)
            applied += 10

        updated["adminTabs"] = new_admin_tabs

    links_overrides = payload.get("links", [])
    if links_overrides and not isinstance(links_overrides, list):
        raise ValueError(f"Texts file 'links' must be a list: {texts_path}")

    enabled_links_raw = payload.get("enabledLinks")
    link_source_mode_raw = payload.get("linksClientOrServer")
    links_web_xml_base_url_raw = payload.get("linksXMLWebBaseUrl")
    show_options_section_raw = payload.get("showOptionsSection")
    show_admin_version_raw = payload.get("showAdminVersion")
    show_server_join_section_raw = payload.get("showServerJoinSection")
    server_join_overrides = payload.get("serverJoin", {}) if isinstance(payload.get("serverJoin", {}), dict) else {}

    meta = updated.setdefault("meta", {}) if isinstance(updated.get("meta", {}), dict) else {}
    if link_source_mode_raw is not None:
        mode = _safe_link_source_mode(link_source_mode_raw, DEFAULT_LINK_SOURCE_MODE)
        meta["linksClientOrServer"] = mode
        applied += 1
    if links_web_xml_base_url_raw is not None:
        base_url = _normalize_web_base_url(links_web_xml_base_url_raw)
        meta["linksXMLWebBaseUrl"] = base_url
        applied += 1
    if show_options_section_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["showOptionsSection"] = _safe_bool(show_options_section_raw, default=True)
        applied += 1
    if show_admin_version_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["showAdminVersion"] = _safe_bool(show_admin_version_raw, default=True)
        applied += 1
    # showLinksSection is intentionally ignored for behavior.
    # Links visibility/generation is driven by @enabledLinks (0 disables, 1-5 enables).
    if show_server_join_section_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["showServerJoinSection"] = _safe_bool(show_server_join_section_raw, default=True)
        applied += 1

    server_join_toggle_raw = server_join_overrides.get("showServerJoinSection")
    if server_join_toggle_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["showServerJoinSection"] = _safe_bool(server_join_toggle_raw, default=True)
        applied += 1

    joining_title_raw = server_join_overrides.get("joiningTitle")
    if joining_title_raw is not None and str(joining_title_raw).strip() != "":
        labels["joiningTitle"] = str(joining_title_raw)
        applied += 1

    # HV joining title intentionally mirrors full-color joiningTitle.

    joining_body_raw = server_join_overrides.get("joiningBody")
    if joining_body_raw is not None:
        labels["joiningBody"] = str(joining_body_raw)
        applied += 1

    joining_title_font_size_raw = server_join_overrides.get("joiningTitleFontSize")
    if joining_title_font_size_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["serverJoinTitleFontSize"] = _safe_font_size(joining_title_font_size_raw, DEFAULT_PAGE_TITLE_FONT_SIZE)
        applied += 1

    joining_body_font_size_raw = server_join_overrides.get("joiningBodyFontSize")
    if joining_body_font_size_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["serverJoinBodyFontSize"] = _safe_font_size(joining_body_font_size_raw, DEFAULT_PAGE_BODY_FONT_SIZE)
        applied += 1

    joining_body_align_raw = server_join_overrides.get("joiningBodyAlign")
    if joining_body_align_raw is not None:
        ui = updated.setdefault("ui", {}) if isinstance(updated.get("ui", {}), dict) else {}
        ui["serverJoinBodyAlign"] = _safe_align(joining_body_align_raw, "center")
        applied += 1

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

        if enabled_links_raw is None:
            enabled_count = min(
                max_links,
                len([
                    link
                    for link in active_existing_links[:max_links]
                    if isinstance(link, dict) and str(link.get("url") or "").strip()
                ]),
            )
        else:
            try:
                enabled_count = int(str(enabled_links_raw).strip())
            except (ValueError, TypeError, AttributeError):
                enabled_count = 0
            enabled_count = max(0, min(max_links, enabled_count))

        rows_by_pos: dict[int, dict] = {}
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
            if 0 <= target_pos < max_links:
                rows_by_pos[target_pos] = row

        new_links: list[dict] = []
        existing_all_links = [link for link in existing_links if isinstance(link, dict)]
        for slot in range(max_links):
            row = rows_by_pos.get(slot, {})
            source_link: dict = existing_all_links[slot] if slot < len(existing_all_links) else {}

            link_num = slot + 1
            label = str(
                row.get("label")
                or source_link.get("label")
                or source_link.get("title")
                or f"Link {link_num}"
            )
            url = str(row.get("url") or source_link.get("url") or "").strip()
            stable_id = str(source_link.get("id") or slugify_token(label) or f"link_{link_num}")
            new_links.append(
                {
                    "id": stable_id,
                    "enabled": link_num <= enabled_count,
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
    tabs = get_active_tabs(source)
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
        body_columns_raw = tab.get("bodyColumns", 1)
        try:
            body_columns = int(str(body_columns_raw).strip())
        except (ValueError, TypeError, AttributeError):
            body_columns = 1
        body_columns = max(1, min(PAGE_BODY_MAX_COLUMNS, body_columns))
        body_column_2 = str(tab.get("bodyColumn2") or "")
        body_column_3 = str(tab.get("bodyColumn3") or "")
        title_font_size = _safe_font_size(
            tab.get("titleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
            DEFAULT_PAGE_TITLE_FONT_SIZE,
        )
        body_font_size = _safe_font_size(
            tab.get("bodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
            DEFAULT_PAGE_BODY_FONT_SIZE,
        )

        button_hv = str(tab.get("buttonHV") or button)
        title_hv = str(tab.get("titleHV") or title)
        body_hv = str(tab.get("bodyHV") or tab.get("body") or "")
        body_column_2_hv = str(tab.get("bodyColumn2HV") or tab.get("bodyColumn2") or "")
        body_column_3_hv = str(tab.get("bodyColumn3HV") or tab.get("bodyColumn3") or "")
        scrollable_body = False
        body_width_percent = _safe_body_width_percent(
            tab.get("bodyWidthPercent", DEFAULT_BODY_WIDTH_PERCENT),
            DEFAULT_BODY_WIDTH_PERCENT,
        )
        body_align = _safe_align(tab.get("bodyAlign", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)
        body_box_centered = _safe_bool(tab.get("bodyBoxCentered"), default=False)
        body_column_2_align = _safe_align(tab.get("bodyColumn2Align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)
        body_column_2_box_centered = _safe_bool(tab.get("bodyColumn2BoxCentered"), default=False)
        body_column_3_align = _safe_align(tab.get("bodyColumn3Align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)
        body_column_3_box_centered = _safe_bool(tab.get("bodyColumn3BoxCentered"), default=False)

        models.append(
            {
                "idx": idx,
                "id": tab_id,
                "button": button,
                "title": title,
                "body": body,
                "body_columns": body_columns,
                "body_column_2": body_column_2,
                "body_column_3": body_column_3,
                "title_font_size": title_font_size,
                "body_font_size": body_font_size,
                "button_hv": button_hv,
                "title_hv": title_hv,
                "body_hv": body_hv,
                "body_column_2_hv": body_column_2_hv,
                "body_column_3_hv": body_column_3_hv,
                "scrollable_body": scrollable_body,
                "body_width_percent": body_width_percent,
                "body_align": body_align,
                "body_box_centered": body_box_centered,
                "body_column_2_align": body_column_2_align,
                "body_column_2_box_centered": body_column_2_box_centered,
                "body_column_3_align": body_column_3_align,
                "body_column_3_box_centered": body_column_3_box_centered,
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
    # Keep HV background in a practical mid-range so it is not blown-out or crushed.
    grey = max(32, min(224, luminance))
    return f"{grey}, {grey}, {grey}"


def derive_auto_high_visibility_colors(theme_color: dict[str, str]) -> dict[str, str]:
    hv = dict(DEFAULT_AUTO_HIGH_VISIBILITY_COLORS)

    hv["headerBackground"] = _grayscale_from_color(
        theme_color.get("headerBackground", hv["headerBackground"]),
        fallback=hv["headerBackground"],
    )
    hv["pageBackground"] = _grayscale_from_color(
        theme_color.get("pageBackground", hv["pageBackground"]),
        fallback=hv["pageBackground"],
    )
    hv["linkBackground"] = hv["pageBackground"]
    hv["optionsBackground"] = hv["pageBackground"]

    header_text = _bw_rgb_for_readability(hv["headerBackground"], fallback="255, 255, 255")
    body_text = _bw_rgb_for_readability(hv["pageBackground"], fallback="255, 255, 255")

    hv["headerTitle"] = header_text
    hv["headerMotto"] = header_text
    hv["pageTabButtonText"] = body_text
    hv["pageTitle"] = body_text
    hv["pageBody"] = body_text
    hv["textBolding"] = body_text
    hv["textHighlight"] = "0, 0, 0" if body_text == "255, 255, 255" else "255, 255, 255"

    if body_text == "255, 255, 255":
        hv["pageTabButtonBackground"] = "55, 55, 55"
        hv["pageTabButtonBorder"] = "255, 255, 255"
        hv["pageTabButtonSelected"] = "10, 10, 10"
    else:
        hv["pageTabButtonBackground"] = "205, 205, 205"
        hv["pageTabButtonBorder"] = "0, 0, 0"
        hv["pageTabButtonSelected"] = "245, 245, 245"

    # Keep selected tab fill on the same readable side of luminance as the fixed tab label color.
    hv["linkButtonBackground"] = hv["pageBackground"]
    hv["linkButtonBorder"] = hv["panelBorder"]
    hv["linkButtonText"] = _bw_rgb_for_readability(hv["linkButtonBackground"], fallback="255, 255, 255")
    hv["linkButtonHover"] = (
        "120, 120, 120" if hv["linkButtonText"] == "255, 255, 255" else "190, 190, 190"
    )
    return hv


def get_theme_colors(
    source: dict,
    theme_name: str,
    use_hv_dev_overrides: bool = False,
) -> dict[str, str]:
    if theme_name == "highVisibility":
        # High Visibility defaults to auto-derived values, but if dev.highVisibilityPreview
        # is present we treat it as the authoritative standards palette.
        derived = derive_auto_high_visibility_colors(get_theme_colors(source, "color"))

        dev = source.get("dev", {})
        preview = dev.get("highVisibilityPreview", {}) if isinstance(dev, dict) else {}
        if not isinstance(preview, dict):
            return derived

        # Keep pure auto-derived colors when no preview overrides are provided.
        if not preview and not use_hv_dev_overrides:
            return derived

        aliases = {
            "headerBackground": ["headerBackground", "headerBackgroundColor"],
            "pageBackground": ["pageBackground", "pageBackgroundColor"],
            "linkBackground": ["linkBackground", "linkBackgroundColor"],
            "optionsBackground": ["optionsBackground", "optionsBackgroundColor"],
            "panelBorder": ["panelBorder", "panelBorderColor", "borderColor"],
            "headerTitle": ["headerTitle", "headerTitleColor"],
            "headerMotto": ["headerMotto", "headerMottoColor"],
            "pageTabButtonBackground": [
                "pageTabButtonBackground",
                "pageTabButtonBackgroundColor",
            ],
            "pageTabButtonBorder": [
                "pageTabButtonBorder",
                "pageTabButtonBorderColor",
                "sectionButtonBorder",
                "sectionButtonBorderColor",
            ],
            "pageTabButtonSelected": [
                "pageTabButtonSelected",
                "pageTabButtonSelectedColor",
                "tabButtonSelected",
                "tabButtonSelectedColor",
            ],
            "pageTabButtonText": [
                "pageTabButtonText",
                "pageTabButtonTextColor",
                "pageButtonText",
                "pageButtonTextColor",
            ],
            "pageTitle": ["pageTitle", "pageTitleColor"],
            "pageBody": ["pageBody", "pageBodyColor"],
            "textBolding": ["textBolding", "textBoldingColor"],
            "textHighlight": ["textHighlight", "textHighlightColor"],
            "linkButtonBackground": ["linkButtonBackground", "linkButtonBackgroundColor"],
            "linkButtonBorder": ["linkButtonBorder", "linkButtonBorderColor"],
            "linkButtonText": ["linkButtonText", "linkButtonTextColor"],
            "linkButtonHover": ["linkButtonHover", "linkButtonHoverColor"],
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
        "linkBackground": ["linkBackground", "linkBackgroundColor"],
        "optionsBackground": ["optionsBackground", "optionsBackgroundColor"],
        "panelBorder": ["panelBorder", "panelBorderColor", "borderColor"],
        "headerTitle": ["headerTitle", "headerTitleColor"],
        "headerMotto": ["headerMotto", "headerMottoColor"],
        "pageTabButtonBackground": [
            "pageTabButtonBackground",
            "pageTabButtonBackgroundColor",
        ],
        "pageTabButtonBorder": [
            "pageTabButtonBorder",
            "pageTabButtonBorderColor",
            "sectionButtonBorder",
            "sectionButtonBorderColor",
        ],
        "pageTabButtonSelected": [
            "pageTabButtonSelected",
            "pageTabButtonSelectedColor",
            "tabButtonSelected",
            "tabButtonSelectedColor",
        ],
        "pageTabButtonText": [
            "pageTabButtonText",
            "pageTabButtonTextColor",
            "pageButtonText",
            "pageButtonTextColor",
        ],
        "pageTitle": ["pageTitle", "pageTitleColor"],
        "pageBody": ["pageBody", "pageBodyColor"],
        "textBolding": ["textBolding", "textBoldingColor"],
        "textHighlight": ["textHighlight", "textHighlightColor"],
        "linkButtonBackground": ["linkButtonBackground", "linkButtonBackgroundColor"],
        "linkButtonBorder": ["linkButtonBorder", "linkButtonBorderColor"],
        "linkButtonText": ["linkButtonText", "linkButtonTextColor"],
        "linkButtonHover": ["linkButtonHover", "linkButtonHoverColor"],
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


def _get_tab_body_columns(tab: dict, is_hv: bool) -> list[str]:
    base = str(tab.get("body_hv") if is_hv else tab.get("body") or "")
    col2 = str(tab.get("body_column_2_hv") if is_hv else tab.get("body_column_2") or "")
    col3 = str(tab.get("body_column_3_hv") if is_hv else tab.get("body_column_3") or "")
    columns_raw = tab.get("body_columns", 1)
    try:
        columns_count = int(str(columns_raw).strip())
    except (ValueError, TypeError, AttributeError):
        columns_count = 1
    columns_count = max(1, min(PAGE_BODY_MAX_COLUMNS, columns_count))

    return [base, col2, col3][:columns_count]


def _get_body_column_layout(column_count: int) -> list[tuple[str, str]]:
    count = max(1, min(PAGE_BODY_MAX_COLUMNS, column_count))

    pos_parts = [p.strip() for p in PAGE_BODY_POS.split(",", 1)]
    start_x = int(pos_parts[0]) if pos_parts else 26
    pos_y = pos_parts[1] if len(pos_parts) > 1 else "-144"

    total_gap = PAGE_BODY_COLUMN_GAP * (count - 1)
    available_width = max(1, PAGE_BODY_WIDTH - total_gap)
    base_width = available_width // count
    remainder = available_width - (base_width * count)

    layout: list[tuple[str, str]] = []
    cursor_x = start_x
    for idx in range(count):
        width = base_width + (1 if idx < remainder else 0)
        layout.append((f"{cursor_x},{pos_y}", str(width)))
        cursor_x += width + PAGE_BODY_COLUMN_GAP

    return layout


def _get_column_align(tab: dict, column_idx: int) -> str:
    if column_idx <= 1:
        return _safe_align(tab.get("body_align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)
    if column_idx == 2:
        return _safe_align(tab.get("body_column_2_align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)
    return _safe_align(tab.get("body_column_3_align", DEFAULT_BODY_ALIGN), DEFAULT_BODY_ALIGN)


def _get_column_box_centered(tab: dict, column_idx: int) -> bool:
    if column_idx <= 1:
        return _safe_bool(tab.get("body_box_centered"), default=False)
    if column_idx == 2:
        return _safe_bool(tab.get("body_column_2_box_centered"), default=False)
    return _safe_bool(tab.get("body_column_3_box_centered"), default=False)


def build_localization_rows(source: dict, use_hv_dev_overrides: bool = False) -> list[list[str]]:
    labels = source["labels"]
    tab_models = build_tab_models(source)
    theme_color = get_theme_colors(source, "color")
    theme_hv = get_theme_colors(source, "highVisibility", use_hv_dev_overrides=use_hv_dev_overrides)

    header_title = labels.get("headerTitle", "")
    header_motto = labels.get("headerMotto", "")
    options_title = labels.get("optionsTitle", "Options")
    joining_title = labels.get("joiningTitle", "Joining")
    joining_body = labels.get("joiningBody", "")
    joining_title_hv = labels.get("joiningTitleHV", joining_title)
    mode_color_label = labels.get("modeColorTab", "Full Color")
    mode_hv_label = labels.get("modeHighVisibilityTab", "High Visibility")
    admin_player_view_label = labels.get("adminPlayerViewTab", "Player View")
    admin_admin_view_label = labels.get("adminAdminViewTab", "Admin View")
    header_title_hv = header_title
    header_motto_hv = header_motto
    options_title_hv = options_title

    active_links = get_active_links(source)
    screamer_title = labels.get("optionsScreamerTitle", "Screamer Alert")

    # Join body supports the same inline styling tokens as page bodies.
    joining_body_color = normalize_text(
        _apply_inline_body_styles(
            joining_body,
            theme_color["textBolding"],
            theme_color["textHighlight"],
        )
    )
    joining_body_hv = normalize_text(
        _apply_inline_body_styles(
            joining_body,
            theme_hv["textBolding"],
            theme_hv["textHighlight"],
        )
    )

    rows: list[list[str]] = [
        ["Key", "Context", "English"],
        ["windowESC_mode_color_tab", "UI", mode_color_label],
        ["windowESC_mode_highvisibility_tab", "UI", mode_hv_label],
        [
            "windowESC_options_screamer_tooltip_off",
            "UI",
            "Hides the visual screamer alert.",
        ],
        [
            "windowESC_options_screamer_tooltip_on",
            "UI",
            "Shows the visual screamer alert when screamers and screamer-spawned zombies are within 120m.",
        ],
        [
            "windowESC_options_screamer_tooltip_on_num",
            "UI",
            "Shows the visual screamer alert plus the current number of screamers and screamer-spawned zombies within 120m.",
        ],
        ["windowESCAdmin_player_view_tab", "UI", admin_player_view_label],
        ["windowESCAdmin_admin_view_tab", "UI", admin_admin_view_label],
        ["windowESCAdmin_mode_color_tab", "UI", mode_color_label],
        ["windowESCAdmin_mode_highvisibility_tab", "UI", mode_hv_label],
    ]

    color_rows: list[list[str]] = [
        ["windowESC_color_header_title", "UI", header_title],
        ["windowESC_color_header_motto", "UI", header_motto],
        ["windowESC_color_options_title", "UI", options_title],
        ["windowESC_color_join_title", "UI", joining_title],
        ["windowESC_color_join_body", "UI", joining_body_color],
    ]
    high_rows: list[list[str]] = [
        ["windowESC_highvisibility_header_title", "UI", header_title_hv],
        ["windowESC_highvisibility_header_motto", "UI", header_motto_hv],
        ["windowESC_highvisibility_options_title", "UI", options_title_hv],
        ["windowESC_highvisibility_join_title", "UI", joining_title_hv],
        ["windowESC_highvisibility_join_body", "UI", joining_body_hv],
        ["windowESC_join_Leave", "UI", "Leave\\nServer"],
        ["windowESC_join_Spawn", "UI", "Join\\nServer"],
    ]

    # Screamer option labels are generated with theme-driven colors so they track config changes.
    color_rows.extend(
        [
            ["windowESC_color_options_screamer_title", "UI", screamer_title],
            [
                "windowESC_color_options_screamer_off",
                "UI",
                _colorize_text("Off", theme_color["pageTabButtonText"]),
            ],
            [
                "windowESC_color_options_screamer_on",
                "UI",
                _colorize_text("On", theme_color["pageTabButtonText"]),
            ],
            [
                "windowESC_color_options_screamer_on_num",
                "UI",
                _colorize_text("On + #", theme_color["pageTabButtonText"]),
            ],
        ]
    )
    high_rows.extend(
        [
            ["windowESC_highvisibility_options_screamer_title", "UI", screamer_title],
            [
                "windowESC_highvisibility_options_screamer_off",
                "UI",
                _colorize_text("Off", theme_hv["pageTabButtonText"]),
            ],
            [
                "windowESC_highvisibility_options_screamer_on",
                "UI",
                _colorize_text("On", theme_hv["pageTabButtonText"]),
            ],
            [
                "windowESC_highvisibility_options_screamer_on_num",
                "UI",
                _colorize_text("On + #", theme_hv["pageTabButtonText"]),
            ],
        ]
    )

    if active_links:
        for link_idx, link in enumerate(active_links, start=1):
            fallback_label = str(link.get("title") or f"Link {link_idx}")
            link_label = str(link.get("label") or link.get("title") or fallback_label)
            color_rows.append(
                [
                    f"windowESC_color_link_{link_idx}_label",
                    "UI",
                    _colorize_text(link_label, theme_color["linkButtonText"]),
                ]
            )
            high_rows.append(
                [
                    f"windowESC_highvisibility_link_{link_idx}_label",
                    "UI",
                    _colorize_text(link_label, theme_hv["linkButtonText"]),
                ]
            )
    else:
        fallback_label = labels.get("linkLabel", "")
        color_rows.append(
            [
                "windowESC_color_link_1_label",
                "UI",
                _colorize_text(fallback_label, theme_color["linkButtonText"]),
            ]
        )
        high_rows.append(
            [
                "windowESC_highvisibility_link_1_label",
                "UI",
                _colorize_text(fallback_label, theme_hv["linkButtonText"]),
            ]
        )

    def _append_page_rows(tab_list: list[dict], key_prefix: str, page_token: str = "page") -> None:
        for tab in tab_list:
            idx = tab["idx"]
            body_columns_color = [
                normalize_text(
                    _apply_inline_body_styles(
                        column_text,
                        theme_color["textBolding"],
                        theme_color["textHighlight"],
                    )
                )
                for column_text in _get_tab_body_columns(tab, is_hv=False)
            ]
            body_columns_hv = [
                normalize_text(
                    _apply_inline_body_styles(
                        column_text,
                        theme_hv["textBolding"],
                        theme_hv["textHighlight"],
                    )
                )
                for column_text in _get_tab_body_columns(tab, is_hv=True)
            ]

            color_rows.append(
                [
                    f"{key_prefix}_color_{page_token}_{idx}_button",
                    "UI",
                    _colorize_text(tab["button"], theme_color["pageTabButtonText"]),
                ]
            )
            color_rows.append([f"{key_prefix}_color_{page_token}_{idx}_title", "UI", tab["title"]])
            color_rows.append([f"{key_prefix}_color_{page_token}_{idx}_body", "UI", body_columns_color[0]])
            if len(body_columns_color) > 1:
                for column_idx, column_text in enumerate(body_columns_color, start=1):
                    color_rows.append(
                        [
                            f"{key_prefix}_color_{page_token}_{idx}_body_column_{column_idx}",
                            "UI",
                            column_text,
                        ]
                    )

            high_rows.append(
                [
                    f"{key_prefix}_highvisibility_{page_token}_{idx}_button",
                    "UI",
                    _colorize_text(tab["button"], theme_hv["pageTabButtonText"]),
                ]
            )
            high_rows.append([f"{key_prefix}_highvisibility_{page_token}_{idx}_title", "UI", tab["title"]])
            high_rows.append([f"{key_prefix}_highvisibility_{page_token}_{idx}_body", "UI", body_columns_hv[0]])
            if len(body_columns_hv) > 1:
                for column_idx, column_text in enumerate(body_columns_hv, start=1):
                    high_rows.append(
                        [
                            f"{key_prefix}_highvisibility_{page_token}_{idx}_body_column_{column_idx}",
                            "UI",
                            column_text,
                        ]
                    )

    _append_page_rows(tab_models, "windowESC", "page")

    admin_tabs_source = source.get("adminTabs", source.get("tabs", []))
    if not isinstance(admin_tabs_source, list):
        admin_tabs_source = source.get("tabs", []) if isinstance(source.get("tabs", []), list) else []
    admin_source = dict(source)
    admin_source["tabs"] = admin_tabs_source
    admin_tab_models = build_tab_models(admin_source)
    _append_page_rows(admin_tab_models, "windowESCAdmin", "adminPage")

    rows.extend(sorted(color_rows, key=lambda r: str(r[0] if len(r) > 0 else "").lower()))
    rows.extend(sorted(high_rows, key=lambda r: str(r[0] if len(r) > 0 else "").lower()))

    return rows


def build_tabs_ui_snippet(source: dict, use_hv_dev_overrides: bool = False) -> str:
    tab_models = build_tab_models(source)
    tab_layout = get_tab_layout(source)
    validate_tab_count(len(tab_models), tab_layout)
    theme_color = get_theme_colors(source, "color")
    theme_hv = get_theme_colors(source, "highVisibility", use_hv_dev_overrides=use_hv_dev_overrides)

    tab_count = max(1, len(tab_models))
    cell_width = tab_layout["buttonCellWidth"]
    total_width = tab_count * cell_width
    start_x = max(
        tab_layout["contentPadding"],
        (tab_layout["contentWidth"] - total_width) // 2,
    ) + TAB_BUTTON_X_NUDGE

    def build_pages(is_hv: bool) -> str:
        pages: list[str] = []
        theme = theme_hv if is_hv else theme_color
        for tab in tab_models:
            idx = tab["idx"]
            mode_prefix = "windowESC_highvisibility" if is_hv else "windowESC_color"
            title_key = f"{mode_prefix}_page_{idx}_title"
            body_key = f"{mode_prefix}_page_{idx}_body"
            button_key = f"{mode_prefix}_page_{idx}_button"
            page_name = f"page_{idx}_hv" if is_hv else f"page_{idx}"
            title_name = f"page_{idx}_hv_title" if is_hv else f"page_{idx}_title"
            body_name = f"page_{idx}_hv_body" if is_hv else f"page_{idx}_body"
            scrollable_body = False
            body_columns = _get_tab_body_columns(tab, is_hv=is_hv)
            body_layout = _get_body_column_layout(len(body_columns))
            single_body_text = body_columns[0] if body_columns else ""
            single_body_pos, single_body_width = _single_body_layout(tab, single_body_text)
            single_body_align = _get_column_align(tab, 1)
            pages.extend(
                [
                    f'\t\t<rect name="{page_name}" controller="TabSelectorTab" tab_key="{button_key}">',
                    f'\t\t\t<label name="{title_name}" depth="10" pos="{PAGE_TITLE_POS}" justify="center" effect="Outline8" effect_color="0,0,0,255" effect_distance="1,1" color="{_theme_xml_color(theme, "pageTitle", is_hv)}" font_size="{tab["title_font_size"]}" text_key="{title_key}" foregroundlayer="true"/>',
                ]
            )
            if len(body_columns) <= 1:
                if scrollable_body:
                    pages.extend(
                        [
                            f'\t\t\t<panel name="{body_name}_scroll" depth="10" pos="{single_body_pos}" width="{single_body_width}" height="{PAGE_BODY_HEIGHT}" on_scroll="true" disableautobackground="true" createuipanel="true" clipping="SoftClip" clippingsoftness="4,4" clippingsize="{single_body_width},{PAGE_BODY_HEIGHT}" clippingcenter="{single_body_width // 2},-{PAGE_BODY_HEIGHT // 2}">',
                            _build_body_label_xml(
                                indent="\t\t\t\t",
                                name=body_name,
                                depth=11,
                                pos="0,0",
                                width=single_body_width,
                                height=PAGE_BODY_SCROLL_CONTENT_HEIGHT,
                                justify=single_body_align,
                                color=_theme_xml_color(theme, "pageBody", is_hv),
                                font_size=tab["body_font_size"],
                                text_key=body_key,
                            ),
                            "\t\t\t</panel>",
                        ]
                    )
                else:
                    pages.append(
                        _build_body_label_xml(
                            indent="\t\t\t",
                            name=body_name,
                            depth=10,
                            pos=single_body_pos,
                            width=single_body_width,
                            height=PAGE_BODY_HEIGHT,
                            justify=single_body_align,
                            color=_theme_xml_color(theme, "pageBody", is_hv),
                            font_size=tab["body_font_size"],
                            text_key=body_key,
                        )
                    )
            else:
                for column_idx, (pos_value, width_value) in enumerate(body_layout, start=1):
                    body_key_column = f"{mode_prefix}_page_{idx}_body_column_{column_idx}"
                    body_name_column = f"{body_name}_column_{column_idx}"
                    column_align = _get_column_align(tab, column_idx)
                    column_box_centered = _get_column_box_centered(tab, column_idx)
                    column_text = body_columns[column_idx - 1] if (column_idx - 1) < len(body_columns) else ""
                    body_col_pos, body_col_width = _resolve_body_block_layout(
                        pos_value,
                        int(str(width_value)),
                        column_text,
                        _safe_font_size(tab.get("body_font_size"), DEFAULT_PAGE_BODY_FONT_SIZE),
                        column_box_centered,
                    )
                    if scrollable_body:
                        pages.extend(
                            [
                                f'\t\t\t<panel name="{body_name_column}_scroll" depth="10" pos="{body_col_pos}" width="{body_col_width}" height="{PAGE_BODY_HEIGHT}" on_scroll="true" disableautobackground="true" createuipanel="true" clipping="SoftClip" clippingsoftness="4,4" clippingsize="{body_col_width},{PAGE_BODY_HEIGHT}" clippingcenter="{body_col_width // 2},-{PAGE_BODY_HEIGHT // 2}">',
                                _build_body_label_xml(
                                    indent="\t\t\t\t",
                                    name=body_name_column,
                                    depth=11,
                                    pos="0,0",
                                    width=body_col_width,
                                    height=PAGE_BODY_SCROLL_CONTENT_HEIGHT,
                                    justify=column_align,
                                    color=_theme_xml_color(theme, "pageBody", is_hv),
                                    font_size=tab["body_font_size"],
                                    text_key=body_key_column,
                                ),
                                "\t\t\t</panel>",
                            ]
                        )
                    else:
                        pages.append(
                            _build_body_label_xml(
                                indent="\t\t\t",
                                name=body_name_column,
                                depth=10,
                                pos=body_col_pos,
                                width=body_col_width,
                                height=PAGE_BODY_HEIGHT,
                                justify=column_align,
                                color=_theme_xml_color(theme, "pageBody", is_hv),
                                font_size=tab["body_font_size"],
                                text_key=body_key_column,
                            )
                        )
            pages.append("\t\t</rect>")
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
        f'\t\t\t<simplebutton name="tabButton" depth="8" pos="{TAB_BUTTON_INNER_POS}" width="{button_width}" height="{tab_layout["buttonHeight"]}" font_size="20" sprite="ui_game_header_fill" bordercolor="{_theme_xml_color(theme_color, "pageTabButtonBorder", False)}" defaultcolor="{_theme_xml_color(theme_color, "pageTabButtonBackground", False)}" selectedsprite="{TAB_BUTTON_SELECTED_SPRITE}" selectedcolor="{_theme_xml_color(theme_color, "pageTabButtonSelected", False)}" foregroundlayer="false" caption="{{tab_name_localized}}"/>',
        "\t\t</rect>",
        "\t</grid>",
        "</rect>",
        "",
        "<!-- Color: tabsContents -->",
        "<rect name=\"tabsContents\">",
        build_pages(is_hv=False),
        "</rect>",
        "",
        "<!-- High Visibility: tabsHeader -->",
        "<rect name=\"tabsHeader\">",
        f'\t<grid name="tabButtons" pos="{start_x},{tab_layout["buttonPosY"]}" depth="3" rows="1" cols="{tab_count}" cell_width="{tab_layout["buttonCellWidth"]}" cell_height="{tab_layout["buttonHeight"]}" repeat_content="true" arrangement="horizontal">',
        "\t\t<rect controller=\"TabSelectorButton\">",
        f'\t\t\t<simplebutton name="tabButton" depth="8" pos="{TAB_BUTTON_INNER_POS}" width="{button_width}" height="{tab_layout["buttonHeight"]}" font_size="20" sprite="ui_game_header_fill" bordercolor="{_theme_xml_color(theme_hv, "pageTabButtonBorder", True)}" defaultcolor="{_theme_xml_color(theme_hv, "pageTabButtonBackground", True)}" selectedsprite="{TAB_BUTTON_SELECTED_SPRITE}" selectedcolor="{_theme_xml_color(theme_hv, "pageTabButtonSelected", True)}" foregroundlayer="false" caption="{{tab_name_localized}}"/>',
        "\t\t</rect>",
        "\t</grid>",
        "</rect>",
        "",
        "<!-- High Visibility: tabsContents -->",
        "<rect name=\"tabsContents\">",
        build_pages(is_hv=True),
        "</rect>",
        "</generatedTabs>",
    ]
    return "\n".join(lines) + "\n"


def _build_tab_buttons_grid_xml(source: dict, is_hv: bool, use_hv_dev_overrides: bool = False) -> str:
    tab_models = build_tab_models(source)
    tab_layout = get_tab_layout(source)
    validate_tab_count(len(tab_models), tab_layout)
    theme = get_theme_colors(
        source,
        "highVisibility" if is_hv else "color",
        use_hv_dev_overrides=use_hv_dev_overrides,
    )

    tab_count = max(1, len(tab_models))
    cell_width = tab_layout["buttonCellWidth"]
    total_width = tab_count * cell_width
    start_x = max(
        tab_layout["contentPadding"],
        (tab_layout["contentWidth"] - total_width) // 2,
    ) + TAB_BUTTON_X_NUDGE
    button_width = max(1, tab_layout["buttonCellWidth"] - 4)

    lines = [
        (
            f'<grid name="tabButtons" pos="{start_x},{tab_layout["buttonPosY"]}" depth="3" rows="1" '
            f'cols="{tab_count}" cell_width="{tab_layout["buttonCellWidth"]}" '
            f'cell_height="{tab_layout["buttonHeight"]}" repeat_content="true" arrangement="horizontal">'
        ),
        '\t<rect controller="TabSelectorButton">',
        (
            f'\t\t<simplebutton name="tabButton" depth="8" pos="{TAB_BUTTON_INNER_POS}" '
            f'width="{button_width}" height="{tab_layout["buttonHeight"]}" font_size="20" '
            f'sprite="ui_game_header_fill" bordercolor="{_theme_xml_color(theme, "pageTabButtonBorder", is_hv)}" '
            f'defaultcolor="{_theme_xml_color(theme, "pageTabButtonBackground", is_hv)}" '
            f'selectedsprite="{TAB_BUTTON_SELECTED_SPRITE}" '
            f'selectedcolor="{_theme_xml_color(theme, "pageTabButtonSelected", is_hv)}" '
            'foregroundlayer="false" caption="{tab_name_localized}"/>'
        ),
        "\t</rect>",
        "</grid>",
    ]
    return "\n".join(lines)


def _build_tabs_contents_xml(
    source: dict,
    is_hv: bool,
    name_prefix: str = "",
    key_prefix: str = "windowESC",
    page_token: str = "page",
    use_hv_dev_overrides: bool = False,
) -> str:
    tab_models = build_tab_models(source)
    theme = get_theme_colors(
        source,
        "highVisibility" if is_hv else "color",
        use_hv_dev_overrides=use_hv_dev_overrides,
    )

    lines = ['<rect name="tabsContents">']
    for tab in tab_models:
        idx = tab["idx"]
        mode_prefix = f"{key_prefix}_highvisibility" if is_hv else f"{key_prefix}_color"
        title_key = f"{mode_prefix}_{page_token}_{idx}_title"
        body_key = f"{mode_prefix}_{page_token}_{idx}_body"
        button_key = f"{mode_prefix}_{page_token}_{idx}_button"

        page_name = f"{name_prefix}{page_token}_{idx}_hv" if is_hv else f"{name_prefix}{page_token}_{idx}"
        title_name = f"{name_prefix}{page_token}_{idx}_hv_title" if is_hv else f"{name_prefix}{page_token}_{idx}_title"
        body_name = f"{name_prefix}{page_token}_{idx}_hv_body" if is_hv else f"{name_prefix}{page_token}_{idx}_body"
        scrollable_body = False
        body_columns = _get_tab_body_columns(tab, is_hv=is_hv)
        body_layout = _get_body_column_layout(len(body_columns))
        single_body_text = body_columns[0] if body_columns else ""
        single_body_pos, single_body_width = _single_body_layout(tab, single_body_text)
        single_body_align = _get_column_align(tab, 1)

        lines.extend(
            [
                f'\t<rect name="{page_name}" controller="TabSelectorTab" tab_key="{button_key}">',
                (
                    f'\t\t<label name="{title_name}" depth="10" pos="{PAGE_TITLE_POS}" justify="center" '
                    'effect="Outline8" effect_color="0,0,0,255" effect_distance="1,1" '
                    f'color="{_theme_xml_color(theme, "pageTitle", is_hv)}" font_size="{tab["title_font_size"]}" '
                    f'text_key="{title_key}" foregroundlayer="true"/>'
                ),
            ]
        )

        if len(body_columns) <= 1:
            if scrollable_body:
                lines.extend(
                    [
                        (
                            f'\t\t<panel name="{body_name}_scroll" depth="10" pos="{single_body_pos}" width="{single_body_width}" '
                            f'height="{PAGE_BODY_HEIGHT}" on_scroll="true" disableautobackground="true" createuipanel="true" clipping="SoftClip" clippingsoftness="4,4" clippingsize="{single_body_width},{PAGE_BODY_HEIGHT}" clippingcenter="{single_body_width // 2},-{PAGE_BODY_HEIGHT // 2}">'
                        ),
                        _build_body_label_xml(
                            indent="\t\t\t",
                            name=body_name,
                            depth=11,
                            pos="0,0",
                            width=single_body_width,
                            height=PAGE_BODY_SCROLL_CONTENT_HEIGHT,
                            justify=single_body_align,
                            color=_theme_xml_color(theme, "pageBody", is_hv),
                            font_size=tab["body_font_size"],
                            text_key=body_key,
                        ),
                        "\t\t</panel>",
                    ]
                )
            else:
                lines.append(
                    _build_body_label_xml(
                        indent="\t\t",
                        name=body_name,
                        depth=10,
                        pos=single_body_pos,
                        width=single_body_width,
                        height=PAGE_BODY_HEIGHT,
                        justify=single_body_align,
                        color=_theme_xml_color(theme, "pageBody", is_hv),
                        font_size=tab["body_font_size"],
                        text_key=body_key,
                    )
                )
        else:
            for column_idx, (pos_value, width_value) in enumerate(body_layout, start=1):
                body_key_column = f"{mode_prefix}_{page_token}_{idx}_body_column_{column_idx}"
                body_name_column = f"{body_name}_column_{column_idx}"
                column_align = _get_column_align(tab, column_idx)
                column_box_centered = _get_column_box_centered(tab, column_idx)
                column_text = body_columns[column_idx - 1] if (column_idx - 1) < len(body_columns) else ""
                body_col_pos, body_col_width = _resolve_body_block_layout(
                    pos_value,
                    int(str(width_value)),
                    column_text,
                    _safe_font_size(tab.get("body_font_size"), DEFAULT_PAGE_BODY_FONT_SIZE),
                    column_box_centered,
                )
                if scrollable_body:
                    lines.extend(
                        [
                            (
                                f'\t\t<panel name="{body_name_column}_scroll" depth="10" pos="{body_col_pos}" width="{body_col_width}" '
                                f'height="{PAGE_BODY_HEIGHT}" on_scroll="true" disableautobackground="true" createuipanel="true" clipping="SoftClip" clippingsoftness="4,4" clippingsize="{body_col_width},{PAGE_BODY_HEIGHT}" clippingcenter="{body_col_width // 2},-{PAGE_BODY_HEIGHT // 2}">'
                            ),
                            _build_body_label_xml(
                                indent="\t\t\t",
                                name=body_name_column,
                                depth=11,
                                pos="0,0",
                                width=body_col_width,
                                height=PAGE_BODY_SCROLL_CONTENT_HEIGHT,
                                justify=column_align,
                                color=_theme_xml_color(theme, "pageBody", is_hv),
                                font_size=tab["body_font_size"],
                                text_key=body_key_column,
                            ),
                            "\t\t</panel>",
                        ]
                    )
                else:
                    lines.append(
                        _build_body_label_xml(
                            indent="\t\t",
                            name=body_name_column,
                            depth=10,
                            pos=body_col_pos,
                            width=body_col_width,
                            height=PAGE_BODY_HEIGHT,
                            justify=column_align,
                            color=_theme_xml_color(theme, "pageBody", is_hv),
                            font_size=tab["body_font_size"],
                            text_key=body_key_column,
                        )
                    )

        lines.append("\t</rect>")

    lines.append("</rect>")
    return "\n".join(lines)


def update_windows_tab_layout(
    windows_file: Path,
    source: dict,
    use_hv_dev_overrides: bool = False,
) -> dict[str, int]:
    """Replace tabsHeader tabButtons and tabsContents page blocks with generated dynamic layout."""
    if not windows_file.exists():
        raise FileNotFoundError(f"Windows file not found: {windows_file}")

    text = windows_file.read_text(encoding="utf-8")
    updates: dict[str, int] = {}

    def _is_in_highvisibility_block(xml_text: str, pos: int) -> bool:
        color_idx = xml_text.rfind('<rect name="Color"', 0, pos)
        hv_idx = xml_text.rfind('<rect name="HighVisibility"', 0, pos)
        if color_idx == -1 and hv_idx == -1:
            return False
        return hv_idx > color_idx

    # Update nested tabs sections only inside BODY blocks to avoid touching outer selectors.
    body_pattern = re.compile(
        r'(?P<indent>^[ \t]*)<rect name="body"(?P<attrs>[^>]*)>\s*\n(?P<inner>.*?)(?=^\1</rect>\s*$)^\1</rect>',
        re.MULTILINE | re.DOTALL,
    )
    tabs_pattern = re.compile(
        r'(?P<indent>^[ \t]*)<rect name="tabs" controller="TabSelector" select_tab_contents_on_open="true">\s*\n'
        r'(?P<body>.*?)(?=^\1</rect>\s*$)^\1</rect>',
        re.MULTILINE | re.DOTALL,
    )

    section_updates = 0

    def _replace_body(match: re.Match[str]) -> str:
        nonlocal section_updates
        body_inner = match.group("inner")
        body_indent = match.group("indent")
        body_attrs = match.group("attrs")
        is_hv = _is_in_highvisibility_block(text, match.start())

        def _replace_tabs(tabs_match: re.Match[str]) -> str:
            nonlocal section_updates
            tabs_body = tabs_match.group("body")

            is_admin_tabs = 'tab_key="windowESCAdmin_' in tabs_body
            has_modern_keys = ('tab_key="windowESC_' in tabs_body) or is_admin_tabs
            if not has_modern_keys:
                return tabs_match.group(0)

            if 'name="join_page_1' in tabs_body:
                name_prefix = "join_"
            elif is_admin_tabs:
                name_prefix = "admin"
            else:
                name_prefix = ""
            key_prefix = "windowESCAdmin" if is_admin_tabs else "windowESC"
            page_token = "adminPage" if is_admin_tabs else "page"
            indent = tabs_match.group("indent")

            tabs_header_block = "\n".join(
                [
                    '<rect name="tabsHeader">',
                    _indent_block(
                        _build_tab_buttons_grid_xml(
                            source,
                            is_hv=is_hv,
                            use_hv_dev_overrides=use_hv_dev_overrides,
                        ),
                        "\t",
                    ),
                    "</rect>",
                ]
            )
            tabs_contents_block = _build_tabs_contents_xml(
                source,
                is_hv=is_hv,
                name_prefix=name_prefix,
                key_prefix=key_prefix,
                page_token=page_token,
                use_hv_dev_overrides=use_hv_dev_overrides,
            )

            replacement = _indent_block(
                "\n".join(
                    [
                        '<rect name="tabs" controller="TabSelector" select_tab_contents_on_open="true">',
                        _indent_block(tabs_header_block, "\t"),
                        _indent_block(tabs_contents_block, "\t"),
                        "</rect>",
                    ]
                ),
                indent,
            )

            if tabs_match.group(0) == replacement:
                return tabs_match.group(0)

            section_updates += 1
            return replacement

        new_inner = tabs_pattern.sub(_replace_tabs, body_inner)
        if new_inner == body_inner:
            return match.group(0)
        return f"{body_indent}<rect name=\"body\"{body_attrs}>\n{new_inner}{body_indent}</rect>"

    text = body_pattern.sub(_replace_body, text)

    if section_updates > 0:
        updates["tabButtons"] = section_updates
        updates["tabsContents"] = section_updates

    if updates:
        windows_file.write_text(text, encoding="utf-8")

    return updates


def _esc_localization_order_key(key: str) -> tuple[int, str]:
    token = str(key or "")
    if token == "windowESC_mode_color_tab":
        return (0, token)
    if token == "windowESC_mode_highvisibility_tab":
        return (1, token)
    if token.startswith("windowESC_color_"):
        return (2, token)
    if token.startswith("windowESC_highvisibility_"):
        return (3, token)
    if token == "windowESCAdmin_player_view_tab":
        return (4, token)
    if token == "windowESCAdmin_admin_view_tab":
        return (5, token)
    if token == "windowESCAdmin_mode_color_tab":
        return (6, token)
    if token == "windowESCAdmin_mode_highvisibility_tab":
        return (7, token)
    if token.startswith("windowESCAdmin_color_"):
        return (8, token)
    if token.startswith("windowESCAdmin_highvisibility_"):
        return (9, token)
    if token.startswith("windowESC_"):
        return (10, token)
    return (11, token)


def _localization_fixed_translations() -> dict[str, dict[str, str]]:
    return {
        "windowESC_options_screamer_tooltip_off": {
            "german": "Blendet die visuelle Schreier-Warnung aus.",
            "spanish": "Oculta la alerta visual de chilladora.",
            "french": "Masque l'alerte visuelle de hurleuse.",
            "italian": "Nasconde l'avviso visivo strillatrice.",
            "japanese": "視覚的なスクリーマー警報を非表示にします。",
            "koreana": "시각적 스크리머 경보를 숨깁니다.",
            "polish": "Ukrywa wizualny alarm krzykaczki.",
            "brazilian": "Oculta o alerta visual de gritadora.",
            "russian": "Скрывает визуальное предупреждение о крикуне.",
            "turkish": "Gorsel ciglikci alarmini gizler.",
            "schinese": "隐藏尖叫者视觉警报。",
            "tchinese": "隱藏尖叫者視覺警報。",
        },
        "windowESC_options_screamer_tooltip_on": {
            "german": "Zeigt die visuelle Schreier-Warnung, wenn sich Schreier oder von Schreiern gespawnte Zombies innerhalb von 120 m befinden.",
            "spanish": "Muestra la alerta visual de chilladora cuando hay chilladoras o zombis generados por chilladoras dentro de 120 m.",
            "french": "Affiche l'alerte visuelle de hurleuse lorsque des hurleuses ou des zombies invoques par des hurleuses sont dans un rayon de 120 m.",
            "italian": "Mostra l'avviso visivo strillatrice quando strillatrici o zombie generati dalle strillatrici sono entro 120 m.",
            "japanese": "スクリーマーまたはスクリーマーがスポーンさせたゾンビが120m以内にいるとき、視覚的なスクリーマー警報を表示します。",
            "koreana": "스크리머 또는 스크리머가 생성한 좀비가 120m 이내에 있을 때 시각적 스크리머 경보를 표시합니다.",
            "polish": "Pokazuje wizualny alarm krzykaczki, gdy krzykaczki lub zombie zrespione przez krzykaczki sa w zasiegu 120 m.",
            "brazilian": "Mostra o alerta visual de gritadora quando gritadoras ou zumbis gerados por gritadoras estao dentro de 120 m.",
            "russian": "Показывает визуальное предупреждение о крикуне, когда крикуны или зомби, призванные крикунами, находятся в радиусе 120 м.",
            "turkish": "Ciglikcilar veya ciglikcilarin cagdirdigi zombiler 120 m icindeyken gorsel ciglikci alarmini gosterir.",
            "schinese": "当120米内有尖叫者或由尖叫者召唤的丧尸时，显示尖叫者视觉警报。",
            "tchinese": "當120米內有尖叫者或由尖叫者召喚的喪屍時，顯示尖叫者視覺警報。",
        },
        "windowESC_options_screamer_tooltip_on_num": {
            "german": "Zeigt die visuelle Schreier-Warnung plus die aktuelle Anzahl von Schreiern und von Schreiern gespawnten Zombies innerhalb von 120 m.",
            "spanish": "Muestra la alerta visual de chilladora mas la cantidad actual de chilladoras y zombis generados por chilladoras dentro de 120 m.",
            "french": "Affiche l'alerte visuelle de hurleuse plus le nombre actuel de hurleuses et de zombies invoques par des hurleuses dans un rayon de 120 m.",
            "italian": "Mostra l'avviso visivo strillatrice piu il numero attuale di strillatrici e zombie generati dalle strillatrici entro 120 m.",
            "japanese": "視覚的なスクリーマー警報に加えて、120m以内にいるスクリーマーとスクリーマーがスポーンさせたゾンビの現在数を表示します。",
            "koreana": "시각적 스크리머 경보와 함께 120m 이내의 스크리머 및 스크리머가 생성한 좀비의 현재 수를 표시합니다.",
            "polish": "Pokazuje wizualny alarm krzykaczki plus biezaca liczbe krzykaczek i zombie zrespionych przez krzykaczki w zasiegu 120 m.",
            "brazilian": "Mostra o alerta visual de gritadora mais a quantidade atual de gritadoras e zumbis gerados por gritadoras dentro de 120 m.",
            "russian": "Показывает визуальное предупреждение о крикуне плюс текущее количество крикунов и зомби, призванных крикунами, в радиусе 120 м.",
            "turkish": "Gorsel ciglikci alarmini ve 120 m icindeki ciglikcilar ile ciglikcilarin cagdirdigi zombilerin guncel sayisini gosterir.",
            "schinese": "显示尖叫者视觉警报，并显示120米内尖叫者及由尖叫者召唤的丧尸当前数量。",
            "tchinese": "顯示尖叫者視覺警報，並顯示120米內尖叫者及由尖叫者召喚的喪屍目前數量。",
        },
        "windowESC_mode_color_tab": {
            "german": "Vollfarbe",
            "spanish": "Color completo",
            "french": "Couleur complete",
            "italian": "Colore completo",
            "japanese": "フルカラー",
            "koreana": "풀 컬러",
            "polish": "Pelny kolor",
            "brazilian": "Cor completa",
            "russian": "Polnyy tsvet",
            "turkish": "Tam Renk",
            "schinese": "全彩色",
            "tchinese": "全彩色",
        },
        "windowESC_mode_highvisibility_tab": {
            "german": "Hoher Kontrast",
            "spanish": "Alto contraste",
            "french": "Contraste eleve",
            "italian": "Alto contrasto",
            "japanese": "ハイコントラスト",
            "koreana": "고대비",
            "polish": "Wysoki kontrast",
            "brazilian": "Alto contraste",
            "russian": "Vysokaya kontrastnost",
            "turkish": "Yuksek Kontrast",
            "schinese": "高对比度",
            "tchinese": "高對比度",
        },
        "windowESC_join_Leave": {
            "german": "Server\\nverlassen",
            "spanish": "Salir del\\nservidor",
            "french": "Quitter le\\nserveur",
            "italian": "Esci dal\\nserver",
            "japanese": "サーバーを\\n退出",
            "koreana": "서버에서\\n나가기",
            "polish": "Opuść\\nserwer",
            "brazilian": "Sair do\\nservidor",
            "russian": "Покинуть\\nсервер",
            "turkish": "Sunucudan\\nAyrıl",
            "schinese": "离开\\n服务器",
            "tchinese": "離開\\n伺服器",
        },
        "windowESC_join_Spawn": {
            "german": "Server\\nbeitreten",
            "spanish": "Unirse al\\nservidor",
            "french": "Rejoindre le\\nserveur",
            "italian": "Entra nel\\nserver",
            "japanese": "サーバーに\\n参加",
            "koreana": "서버에\\n참가",
            "polish": "Dołącz do\\nserwera",
            "brazilian": "Entrar no\\nservidor",
            "russian": "Войти на\\nсервер",
            "turkish": "Sunucuya\\nKatıl",
            "schinese": "加入\\n服务器",
            "tchinese": "加入\\n伺服器",
        },
    }


def _write_localization_full_csv(rows: list[list[str]], out_file: Path) -> Path:
    out_file.parent.mkdir(parents=True, exist_ok=True)

    full_header = [
        "Key",
        "File",
        "Type",
        "UsedInMainMenu",
        "NoTranslate",
        "english",
        "Context / Alternate Text",
        "german",
        "spanish",
        "french",
        "italian",
        "japanese",
        "koreana",
        "polish",
        "brazilian",
        "russian",
        "turkish",
        "schinese",
        "tchinese",
    ]

    fixed_translations = _localization_fixed_translations()
    full_header_lookup = {str(name).strip().lower(): idx for idx, name in enumerate(full_header)}
    english_col = full_header_lookup.get("english", 5)

    ordered_rows = rows
    if rows:
        header = rows[0]
        body = rows[1:]
        body_sorted = sorted(body, key=lambda r: _esc_localization_order_key(str(r[0] if len(r) > 0 else "")))
        ordered_rows = [header, *body_sorted]

    output_rows: list[list[str]] = []
    for idx, row in enumerate(ordered_rows):
        if idx == 0:
            continue

        key = str(row[0] if len(row) > 0 else "").replace("\n", " ")
        context = str(row[1] if len(row) > 1 else "").replace("\n", " ")
        english = str(row[2] if len(row) > 2 else "")

        full_row = [""] * len(full_header)
        full_row[0] = key
        full_row[2] = "UI"
        full_row[5] = english
        full_row[6] = context

        if key in fixed_translations:
            lang_map = fixed_translations[key]
            for col_name, translated in lang_map.items():
                if col_name in full_header:
                    full_row[full_header.index(col_name)] = translated

        _apply_theme_color_to_translated_columns(full_row, full_header_lookup, english_col)

        output_rows.append(full_row)

    _write_localization_csv_with_language_quotes(out_file, full_header, output_rows)

    return out_file


def write_localization_target(rows: list[list[str]], target_file: Path) -> Path:
    return _write_localization_full_csv(rows, target_file)


def _is_esc_generated_managed_key(key: str) -> bool:
    """Keys managed by the current ESC generator."""
    if key.startswith("windowESC_"):
        return True
    if key.startswith("windowESCAdmin_"):
        return True
    return False


def _apply_theme_color_to_translation(value: str, color_prefix: str) -> str:
    text = str(value)
    if not text:
        return text
    if HAS_ANY_COLOR_TAG_RE.search(text):
        return text
    return f"{color_prefix}{text}[-]"


def _apply_theme_color_to_translated_columns(
    row: list[str],
    header_lookup: dict[str, int],
    english_col: int,
) -> None:
    if english_col >= len(row):
        return

    english_text = str(row[english_col])
    match = FULL_COLOR_WRAPPER_RE.match(english_text)
    if not match:
        return

    color_prefix = match.group(1)
    for lang_col_name in LOCALIZATION_LANGUAGE_COLUMNS:
        lang_idx = header_lookup.get(lang_col_name)
        if lang_idx is None or lang_idx >= len(row):
            continue
        if not row[lang_idx]:
            continue
        row[lang_idx] = _apply_theme_color_to_translation(str(row[lang_idx]), color_prefix)


def _write_localization_csv_with_language_quotes(file_path: Path, header: list[str], rows: list[list[str]]) -> None:
    """Write localization CSV with english + language columns always wrapped in double quotes."""
    quoted_columns = {
        "english",
        "german",
        "spanish",
        "french",
        "italian",
        "japanese",
        "koreana",
        "polish",
        "brazilian",
        "russian",
        "turkish",
        "schinese",
        "tchinese",
    }
    quote_indices = {
        idx
        for idx, name in enumerate(header)
        if str(name).strip().lower() in quoted_columns
    }

    def _encode_cell(value: str, force_quote: bool) -> str:
        text = str(value)
        text = text.replace('"', '""')
        if force_quote:
            return f'"{text}"'
        return text

    with file_path.open("w", encoding="utf-8", newline="") as f:
        header_line = ",".join(_encode_cell(col, False) for col in header)
        f.write(header_line + "\n")
        for row in rows:
            padded = list(row[: len(header)])
            if len(padded) < len(header):
                padded.extend([""] * (len(header) - len(padded)))
            line = ",".join(
                _encode_cell(padded[idx], idx in quote_indices)
                for idx in range(len(header))
            )
            f.write(line + "\n")


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
        existing_rows = [[
            "Key",
            "File",
            "Type",
            "UsedInMainMenu",
            "NoTranslate",
            "english",
            "Context / Alternate Text",
            "german",
            "spanish",
            "french",
            "italian",
            "japanese",
            "koreana",
            "polish",
            "brazilian",
            "russian",
            "turkish",
            "schinese",
            "tchinese",
        ]]

    header = existing_rows[0]
    generated_body = rows[1:]

    header_lookup = {str(name).strip().lower(): idx for idx, name in enumerate(header)}
    key_col = header_lookup.get("key", 0)
    context_col = header_lookup.get("context / alternate text", 1)
    english_col = header_lookup.get("english", 2)
    type_col = header_lookup.get("type")

    has_full_localization_header = (
        "key" in header_lookup
        and "english" in header_lookup
        and "context / alternate text" in header_lookup
        and "type" in header_lookup
    )

    fixed_translations = _localization_fixed_translations()

    def _build_target_row(
        key: str,
        context: str,
        english: str,
        existing_row: list[str] | None = None,
    ) -> list[str]:
        if not has_full_localization_header:
            return [key, context, english]

        if existing_row is not None:
            row = list(existing_row[: len(header)])
            if len(row) < len(header):
                row.extend([""] * (len(header) - len(row)))
        else:
            row = [""] * len(header)
        row[key_col] = key
        row[context_col] = context
        row[english_col] = english
        if type_col is not None:
            row[type_col] = "UI"

        translations = fixed_translations.get(key)
        if translations:
            for lang_key, text_value in translations.items():
                lang_col = header_lookup.get(lang_key)
                if lang_col is not None:
                    row[lang_col] = text_value

        _apply_theme_color_to_translated_columns(row, header_lookup, english_col)

        return row

    index_by_key: dict[str, int] = {}
    for i in range(1, len(existing_rows)):
        row = existing_rows[i]
        if len(row) > key_col and row[key_col]:
            index_by_key[row[key_col]] = i

    updated = 0
    added = 0
    removed = 0

    for grow in generated_body:
        key = grow[0] if len(grow) > 0 else ""
        if not key:
            continue

        new_context = grow[1] if len(grow) > 1 else ""
        new_english = grow[2] if len(grow) > 2 else ""
        existing_row_for_key = existing_rows[index_by_key[key]] if key in index_by_key else None
        desired_row = _build_target_row(key, new_context, new_english, existing_row_for_key)

        if key in index_by_key:
            idx = index_by_key[key]
            row = existing_rows[idx]
            while len(row) < len(desired_row):
                row.append("")
            current_trimmed = row[: len(desired_row)]
            if current_trimmed != desired_row:
                existing_rows[idx] = desired_row
                updated += 1
        else:
            existing_rows.append(desired_row)
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

    sorted_body_rows = sorted(
        existing_rows[1:],
        key=lambda row: _esc_localization_order_key(str(row[key_col] if len(row) > key_col else "")),
    )
    existing_rows = [existing_rows[0]] + sorted_body_rows

    header_row = existing_rows[0] if existing_rows else []
    body_rows = existing_rows[1:] if len(existing_rows) > 1 else []
    _write_localization_csv_with_language_quotes(target_file, header_row, body_rows)

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


def empty_news_xml_content() -> str:
    return (
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        "<news>\n"
        "</news>\n"
    )


def write_news_files(source: dict, links_dir: Path, links_enabled: bool = True) -> list[Path]:
    created: list[Path] = []
    links_dir.mkdir(parents=True, exist_ok=True)
    expected_names: set[str] = set()

    def _cleanup_stale_link_xml() -> None:
        expected_names_lower = {name.lower() for name in expected_names}
        for existing_file in links_dir.glob("*.xml"):
            if existing_file.name.lower() in expected_names_lower:
                continue
            try:
                existing_file.unlink()
            except OSError:
                # Non-fatal cleanup miss; generation output still remains valid.
                continue

    def _cleanup_all_link_files() -> None:
        for existing_path in links_dir.iterdir():
            try:
                if existing_path.is_dir():
                    shutil.rmtree(existing_path)
                else:
                    existing_path.unlink()
            except OSError:
                # Non-fatal cleanup miss; generation output still remains valid.
                continue

    if not links_enabled:
        _cleanup_all_link_files()
        return created

    now_utc = dt.datetime.now(dt.timezone.utc).strftime("%Y-%m-%d %H:%M:%SZ")

    for link in get_active_links(source):
        link_id = (link.get("id") or "link").strip()
        title = (link.get("title") or "Open Link").strip()
        url = (link.get("url") or "").strip()
        if not url:
            continue

        safe_id = "".join(ch for ch in link_id if ch.isalnum() or ch in ("-", "_")) or "link"
        out_name = f"{safe_id}.xml"
        expected_names.add(out_name)
        out_file = links_dir / out_name
        out_file.write_text(news_xml_content(title, url, now_utc), encoding="utf-8")
        created.append(out_file)

    _cleanup_stale_link_xml()

    return created


def _safe_link_id(link_id: str) -> str:
    return "".join(ch for ch in link_id if ch.isalnum() or ch in ("-", "_")) or "link"


def resolve_mod_folder_name(source: dict, config_path: Path) -> str:
    """Resolve the current outer mod folder name from disk.

    This keeps folder renames safe by preferring the actual project root that contains
    Config/ and Links/ over any stale metadata value stored in the source JSON.
    """
    project_root = config_path.parent
    if project_root.name.lower() == "code" and project_root.parent.name.lower() == "_generator":
        project_root = project_root.parent.parent
    elif project_root.name.lower() == "_generator":
        project_root = project_root.parent

    folder_name = project_root.name.strip()
    if folder_name:
        return folder_name

    meta = source.get("meta", {}) if isinstance(source.get("meta", {}), dict) else {}
    explicit = str(meta.get("modFolder") or "").strip()
    if explicit:
        return explicit

    return str(meta.get("name") or "AGF-4Modders-ESCWindowPlus").strip()


def build_local_news_source_map(source: dict, mod_folder_name: str) -> dict[str, str]:
    source_map: dict[str, str] = {}
    mode, web_base_url = get_link_source_settings(source)
    for link in get_active_links(source):
        link_id = (link.get("id") or "").strip()
        if not link_id:
            continue
        source_map[link_id] = _resolve_link_source_xml_url(link, mode, web_base_url)
    return source_map


def _indent_block(text: str, indent: str) -> str:
    return "\n".join(f"{indent}{line}" if line else line for line in text.splitlines())


def _dedent_block(text: str) -> str:
    lines = text.splitlines()
    non_empty = [len(re.match(r"[ \t]*", line).group(0)) for line in lines if line.strip()]
    if not non_empty:
        return text.strip("\n")
    trim = min(non_empty)
    return "\n".join(line[trim:] if len(line) >= trim else "" for line in lines).strip("\n")


def _load_options_inner_template() -> str:
    return _dedent_block(DEFAULT_OPTIONS_INNER_TEMPLATE)


def _build_options_inner_xml(is_hv: bool) -> str:
    inner = _load_options_inner_template()
    if not is_hv:
        return inner

    replacements = {
        "[optionsBackgroundColor]": "[optionsBackgroundColorHV]",
        "[panelBorderColor]": "[panelBorderColorHV]",
        "[textBoldingColor]": "[textBoldingColorHV]",
        "[pageBodyColor]": "[pageBodyColorHV]",
        "[pageTabButtonBorderColor]": "[pageTabButtonBorderColorHV]",
        "[pageTabButtonBackgroundColor]": "[pageTabButtonBackgroundColorHV]",
        "[pageTabButtonSelectedColor]": "[pageTabButtonSelectedColorHV]",
        'text_key="windowESC_color_options_title"': 'text_key="windowESC_highvisibility_options_title"',
    }
    updated = inner
    for old, new in replacements.items():
        updated = updated.replace(old, new)

    sprite_pattern = re.compile(
        r'(<sprite\b[^>]*\bname="(?:optionsBackground|optionsBorder)"[^>]*?)(\s*/?>)',
        re.IGNORECASE,
    )

    def _normalize_sprite_opacity(match: re.Match[str]) -> str:
        attrs = match.group(1)
        close = match.group(2)
        if re.search(r'\bglobalopacity\s*=\s*"', attrs, re.IGNORECASE):
            attrs = re.sub(
                r'\bglobalopacity\s*=\s*"[^"]*"',
                'globalopacity="false"',
                attrs,
                flags=re.IGNORECASE,
            )
            return f"{attrs}{close}"
        return f'{attrs} globalopacity="false"{close}'

    return sprite_pattern.sub(_normalize_sprite_opacity, updated)


def _build_links_grid_xml(
    source: dict,
    is_hv: bool,
    use_hv_dev_overrides: bool = False,
) -> str:
    active_links = get_active_links(source)
    link_count = len(active_links)
    theme = get_theme_colors(
        source,
        "highVisibility" if is_hv else "color",
        use_hv_dev_overrides=use_hv_dev_overrides,
    )

    max_links = DEFAULT_LINK_LAYOUT["maxLinks"]
    cell_width = DEFAULT_LINK_LAYOUT["cellWidth"]
    button_width = DEFAULT_LINK_LAYOUT["buttonWidth"]
    button_height = DEFAULT_LINK_LAYOUT["buttonHeight"]
    button_pos_y = DEFAULT_LINK_LAYOUT["buttonPosY"]
    button_depth = DEFAULT_LINK_LAYOUT["buttonDepth"]
    container_width = DEFAULT_LINK_LAYOUT["containerWidth"]
    max_pos_x = DEFAULT_LINK_LAYOUT["posXForMaxLinks"]

    if link_count > 0 and link_count < max_links:
        pos_x = max(0, (container_width - (link_count * cell_width)) // 2)
    else:
        pos_x = max_pos_x

    grid_name = "linkButtons_hv" if is_hv else "linkButtons"
    caption_prefix = "windowESC_highvisibility" if is_hv else "windowESC_color"

    lines: list[str] = [
        (
            f'<grid name="{grid_name}" pos="{pos_x},{button_pos_y}" depth="{button_depth}" rows="1" '
            f'cols="{max_links}" cell_width="{cell_width}" cell_height="{button_height}" arrangement="horizontal">'
        )
    ]

    mode, web_base_url = get_link_source_settings(source)

    for idx, link in enumerate(active_links, start=1):
        source_path = _resolve_link_source_xml_url(link, mode, web_base_url)
        caption_key = f"{caption_prefix}_link_{idx}_label"
        lines.append(f'\t<rect controller="NewsWindow" sources="{source_path}">')
        lines.append(
            '\t\t<simplebutton name="btnLink" '
            f'width="{button_width}" height="{button_height}" font_size="24" '
            f'sprite="menu_empty3px" bordercolor="{_theme_xml_color(theme, "linkButtonBorder", is_hv)}" '
            f'defaultcolor="{_theme_xml_color(theme, "linkButtonBackground", is_hv)}" hovercolor="{_theme_xml_color(theme, "linkButtonHover", is_hv)}" '
            f'selectedsprite="menu_empty3px" selectedcolor="{_theme_xml_color(theme, "pageTabButtonSelected", is_hv)}" '
            f'foregroundlayer="false" caption_key="{caption_key}" '
            'enabled="{has_link}" gamepad_selectable="{has_news}"/>'
        )
        lines.append("\t</rect>")

    lines.append("</grid>")
    return "\n".join(lines)


def build_links_ui_snippet(source: dict, use_hv_dev_overrides: bool = False) -> str:
    active_links = get_active_links(source)
    lines = [
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
        "<generatedLinks>",
        "<!-- Generated links layout snippet -->",
        f"<!-- active_links={len(active_links)}, max_links={DEFAULT_LINK_LAYOUT['maxLinks']} -->",
        "",
        "<!-- Color: replace grid name=\"linkButtons\" -->",
        _build_links_grid_xml(source, is_hv=False, use_hv_dev_overrides=use_hv_dev_overrides),
        "",
        "<!-- High Visibility: replace grid name=\"linkButtons_hv\" -->",
        _build_links_grid_xml(source, is_hv=True, use_hv_dev_overrides=use_hv_dev_overrides),
        "</generatedLinks>",
    ]
    return "\n".join(lines) + "\n"


def update_windows_link_grids(
    windows_file: Path,
    source: dict,
    use_hv_dev_overrides: bool = False,
) -> dict[str, int]:
    """Replace active linkButtons/linkButtons_hv grid blocks with generated centered layout."""
    if not windows_file.exists():
        raise FileNotFoundError(f"Windows file not found: {windows_file}")

    text = windows_file.read_text(encoding="utf-8")
    changed_by_grid: dict[str, int] = {}
    theme_color = get_theme_colors(source, "color")
    theme_hv = get_theme_colors(
        source,
        "highVisibility",
        use_hv_dev_overrides=use_hv_dev_overrides,
    )

    def _is_in_highvisibility_block(xml_text: str, pos: int) -> bool:
        # Choose the most recent tab-section marker before this position.
        # This is robust even after colors are normalized to numeric RGB values.
        color_idx = xml_text.rfind('<rect name="Color"', 0, pos)
        hv_idx = xml_text.rfind('<rect name="HighVisibility"', 0, pos)
        if color_idx == -1 and hv_idx == -1:
            return False
        return hv_idx > color_idx

    def _strip_xml_comments(value: str) -> str:
        return re.sub(r"<!--.*?-->", "", value, flags=re.DOTALL)

    def _section_has_active_grid(xml_text: str, grid_name: str, is_hv: bool) -> bool:
        section_marker = '<rect name="HighVisibility"' if is_hv else '<rect name="Color"'
        start = xml_text.find(section_marker)
        if start == -1:
            return False

        if is_hv:
            end = len(xml_text)
        else:
            next_marker = xml_text.find('<rect name="HighVisibility"', start + len(section_marker))
            end = next_marker if next_marker != -1 else len(xml_text)

        section_text = _strip_xml_comments(xml_text[start:end])
        return f'name="{grid_name}"' in section_text

    def _insert_grid_when_missing(xml_text: str, grid_name: str, is_hv: bool, block: str) -> tuple[str, int]:
        """Insert generated link grid in the section links panel when the grid no longer exists."""
        section_marker = '<rect name="HighVisibility"' if is_hv else '<rect name="Color"'
        section_start = xml_text.find(section_marker)
        if section_start == -1:
            return xml_text, 0

        links_open = re.search(
            r'(?m)^(?P<indent>[ \t]*)<rect name="links"[^>]*>',
            xml_text[section_start:],
        )
        if not links_open:
            return xml_text, 0
        links_start = section_start + links_open.start()
        links_indent = links_open.group("indent")
        links_open_end = section_start + links_open.end()

        close_re = re.compile(rf'(?m)^{re.escape(links_indent)}</rect>\s*$')
        close_match = close_re.search(xml_text, links_open_end)
        if not close_match:
            return xml_text, 0
        links_end = close_match.end()

        links_block = xml_text[links_start:links_end]
        if f'name="{grid_name}"' in _strip_xml_comments(links_block):
            return xml_text, 0

        border_match = re.search(r'(?m)^(?P<indent>[ \t]*)<sprite name="linksBorder"[^>]*/>\s*$', links_block)
        if border_match:
            child_indent = f"{border_match.group('indent')}\t"
            indented_block = _indent_block(block, child_indent)
            insert_at = border_match.end()
            new_links_block = f"{links_block[:insert_at]}\n\n{indented_block}{links_block[insert_at:]}"
        else:
            closing_indent_match = re.search(r"\n([ \t]*)</rect>\s*$", links_block)
            if not closing_indent_match:
                return xml_text, 0
            child_indent = f"{closing_indent_match.group(1)}\t"
            indented_block = _indent_block(block, child_indent)
            new_links_block = re.sub(
                r"\n([ \t]*)</rect>\s*$",
                lambda m: f"\n\n{indented_block}\n{m.group(1)}</rect>",
                links_block,
                count=1,
            )
        new_text = f"{xml_text[:links_start]}{new_links_block}{xml_text[links_end:]}"
        return new_text, 1

    for grid_name, is_hv in (("linkButtons", False), ("linkButtons_hv", True)):
        block = _build_links_grid_xml(
            source,
            is_hv=is_hv,
            use_hv_dev_overrides=use_hv_dev_overrides,
        )
        pattern = re.compile(
            rf'(?P<indent>^[ \t]*)<grid name="{re.escape(grid_name)}"[^>]*>.*?</grid>',
            re.MULTILINE | re.DOTALL,
        )

        def _replace(match: re.Match[str]) -> str:
            indent = match.group("indent")
            return _indent_block(block, indent)

        text, count = pattern.subn(_replace, text)
        if count > 0:
            changed_by_grid[grid_name] = count
        if not _section_has_active_grid(text, grid_name, is_hv):
            text, inserted = _insert_grid_when_missing(text, grid_name, is_hv, block)
            if inserted > 0:
                changed_by_grid[grid_name] = changed_by_grid.get(grid_name, 0) + inserted

    # Also keep surrounding panel backgrounds in sync with theme colors.
    sprite_patterns = {
        "headerBackground": (
            "headerBackground",
            re.compile(r'(<sprite name="headerBackground"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "headerBorder": (
            "panelBorder",
            re.compile(r'(<sprite name="headerBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "bodyBg": (
            "pageBackground",
            re.compile(r'(<sprite name="bodyBg"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "bodyBorder": (
            "panelBorder",
            re.compile(r'(<sprite name="bodyBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "linksBackground": (
            "linkBackground",
            re.compile(r'(<sprite name="linksBackground"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "linksBorder": (
            "panelBorder",
            re.compile(r'(<sprite name="linksBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "optionsBackground": (
            "optionsBackground",
            re.compile(r'(<sprite name="optionsBackground"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "optionsBorder": (
            "panelBorder",
            re.compile(r'(<sprite name="optionsBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
    }

    for sprite_name, (theme_key, pattern) in sprite_patterns.items():
        updates = 0

        def _replace_sprite(match: re.Match[str]) -> str:
            nonlocal updates
            current = match.group(2).strip()
            use_hv = _is_in_highvisibility_block(text, match.start())
            replacement = _theme_xml_color(theme_hv if use_hv else theme_color, theme_key, use_hv)
            if current == replacement:
                return match.group(0)
            updates += 1
            return f"{match.group(1)}{replacement}{match.group(3)}"

        text = pattern.sub(_replace_sprite, text)
        if updates > 0:
            changed_by_grid[sprite_name] = updates

    # Keep key labels synced to theme colors while leaving localization text plain.
    label_tag_pattern = re.compile(r"<label\b[^>]*>")
    label_updates = 0

    def _replace_label_tag(match: re.Match[str]) -> str:
        nonlocal label_updates
        tag = match.group(0)

        key_match = re.search(r'\btext_key="([^"]+)"', tag)
        if not key_match:
            return tag
        text_key = key_match.group(1)

        theme_key: str | None = None
        if re.match(r"windowESC(?:Admin)?_(?:color|highvisibility)_header_title$", text_key):
            theme_key = "headerTitle"
        elif re.match(r"windowESC(?:Admin)?_(?:color|highvisibility)_header_motto$", text_key):
            theme_key = "headerMotto"
        elif re.match(r"windowESC(?:Admin)?_(?:color|highvisibility)_(?:options|join)_title$", text_key):
            theme_key = "textBolding"
        elif re.match(r"windowESC(?:Admin)?_(?:color|highvisibility)_(?:page|adminPage)_\d+_title$", text_key):
            theme_key = "pageTitle"
        elif re.match(r"windowESC(?:Admin)?_(?:color|highvisibility)_(?:page|adminPage)_\d+_body(?:_column_\d+)?$", text_key):
            theme_key = "pageBody"
        elif re.match(r"windowESC(?:Admin)?_(?:color|highvisibility)_join_body$", text_key):
            theme_key = "pageBody"

        if theme_key is None:
            return tag

        use_hv = "_highvisibility_" in text_key
        replacement = _theme_xml_color(theme_hv if use_hv else theme_color, theme_key, use_hv)

        color_match = re.search(r'\bcolor="([^"]*)"', tag)
        if color_match:
            current = color_match.group(1).strip()
            if current == replacement:
                return tag
            label_updates += 1
            return re.sub(r'(\bcolor=")([^"]*)(")', rf'\g<1>{replacement}\g<3>', tag, count=1)

        label_updates += 1
        return tag.replace("/>", f' color="{replacement}"/>')

    text = label_tag_pattern.sub(_replace_label_tag, text)
    if label_updates > 0:
        changed_by_grid["textLabels"] = label_updates

    # Keep button outlines and panel borders synced even when a section is not fully rebuilt.
    border_patterns = {
        "tabButtonBorders": (
            "pageTabButtonBorder",
            re.compile(r'(<simplebutton name="tabButton"[^>]*\bbordercolor=")([^"]+)(")'),
        ),
        "linkButtonBorders": (
            "linkButtonBorder",
            re.compile(r'(<simplebutton name="btnLink"[^>]*\bbordercolor=")([^"]+)(")'),
        ),
        "headerBorderSprites": (
            "panelBorder",
            re.compile(r'(<sprite name="headerBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "bodyBorderSprites": (
            "panelBorder",
            re.compile(r'(<sprite name="bodyBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "linksBorderSprites": (
            "panelBorder",
            re.compile(r'(<sprite name="linksBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
        "optionsBorderSprites": (
            "panelBorder",
            re.compile(r'(<sprite name="optionsBorder"[^>]*\bcolor=")([^"]+)(")'),
        ),
    }

    for metric_key, (theme_key, pattern) in border_patterns.items():
        updates = 0

        def _replace_border(match: re.Match[str]) -> str:
            nonlocal updates
            current = match.group(2).strip()
            use_hv = _is_in_highvisibility_block(text, match.start())
            replacement = _theme_xml_color(theme_hv if use_hv else theme_color, theme_key, use_hv)
            if current == replacement:
                return match.group(0)
            updates += 1
            return f"{match.group(1)}{replacement}{match.group(3)}"

        text = pattern.sub(_replace_border, text)
        if updates > 0:
            changed_by_grid[metric_key] = updates

    if changed_by_grid:
        windows_file.write_text(text, encoding="utf-8")

    return changed_by_grid


def update_windows_options_visibility(windows_file: Path, source: dict) -> dict[str, int]:
    """Toggle visibility of user-selected UI sections in windows.xml."""
    if not windows_file.exists():
        raise FileNotFoundError(f"Windows file not found: {windows_file}")

    text = windows_file.read_text(encoding="utf-8")
    toggles = get_ui_feature_toggles(source)
    updates_by_key: dict[str, int] = {}
    theme_color = get_theme_colors(source, "color")
    theme_hv = get_theme_colors(source, "highVisibility")

    def _normalize_rgb(value: str, fallback: str) -> str:
        rgb = str(value or fallback).strip()
        if not rgb:
            rgb = fallback
        return re.sub(r"\s+", "", rgb)

    def _set_visible_attrs_in_fragment(
        xml_text: str,
        tag_name: str,
        element_name: str,
        enabled: bool,
    ) -> tuple[str, int]:
        desired = "true" if enabled else "false"
        updates = 0
        pattern = re.compile(
            rf'(?P<prefix><{tag_name}\b)(?P<attrs>[^>]*)(?P<suffix>>)',
            re.IGNORECASE | re.MULTILINE,
        )

        def _replace(match: re.Match[str]) -> str:
            nonlocal updates
            attrs = match.group("attrs")
            attrs_original = attrs
            # Normalize malformed self-closing attrs like '/ visible="true"' -> ' visible="true" /'.
            attrs = re.sub(r'/\s*(visible="[^"]*")', r' \1 /', attrs, flags=re.IGNORECASE)
            name_match = re.search(r'\bname="([^"]*)"', attrs, re.IGNORECASE)
            if not name_match or name_match.group(1).strip() != element_name:
                return match.group(0)
            current_match = re.search(r'\bvisible="([^"]*)"', attrs)
            if current_match:
                current_visible = current_match.group(1).strip().lower()
                if current_visible == desired:
                    if attrs != attrs_original:
                        updates += 1
                        return f"{match.group('prefix')}{attrs}{match.group('suffix')}"
                    return match.group(0)
                new_attrs = re.sub(r'\bvisible="[^"]*"', f'visible="{desired}"', attrs, count=1)
                updates += 1
                return f"{match.group('prefix')}{new_attrs}{match.group('suffix')}"

            updates += 1
            if re.search(r'/\s*$', attrs):
                attrs_no_slash = re.sub(r'/\s*$', '', attrs)
                return f"{match.group('prefix')}{attrs_no_slash} visible=\"{desired}\" /{match.group('suffix')}"

            return f"{match.group('prefix')}{attrs} visible=\"{desired}\"{match.group('suffix')}"

        updated_xml = pattern.sub(_replace, xml_text)
        return updated_xml, updates

    def _set_visible_attrs(
        xml_text: str,
        metric_key: str,
        tag_name: str,
        element_name: str,
        enabled: bool,
        window_name: str | None = None,
    ) -> str:
        if not window_name:
            updated_xml, updates = _set_visible_attrs_in_fragment(xml_text, tag_name, element_name, enabled)
            if updates > 0:
                updates_by_key[metric_key] = updates
            return updated_xml

        updates_total = 0
        window_pattern = re.compile(
            rf'(?P<window><window\b[^>]*\bname="{re.escape(window_name)}"[^>]*>.*?</window>)',
            re.IGNORECASE | re.DOTALL,
        )

        def _replace_window(match: re.Match[str]) -> str:
            nonlocal updates_total
            block = match.group("window")
            updated_block, updates = _set_visible_attrs_in_fragment(block, tag_name, element_name, enabled)
            updates_total += updates
            return updated_block

        updated_xml = window_pattern.sub(_replace_window, xml_text)
        if updates_total > 0:
            updates_by_key[metric_key] = updates_total
        return updated_xml

    def _remove_named_rect_blocks(xml_text: str, section_name: str) -> tuple[str, int]:
        tag_pattern = re.compile(r'<(/?)rect\b([^>]*)>', re.IGNORECASE)
        name_pattern = re.compile(r'\bname="([^"]+)"', re.IGNORECASE)

        stack: list[tuple[str, int]] = []
        spans: list[tuple[int, int]] = []

        for match in tag_pattern.finditer(xml_text):
            is_close = match.group(1) == "/"
            tag_text = match.group(0)

            if not is_close:
                attrs = match.group(2)
                name_match = name_pattern.search(attrs)
                rect_name = name_match.group(1).strip() if name_match else ""
                if tag_text.rstrip().endswith("/>"):
                    if rect_name == section_name:
                        spans.append((match.start(), match.end()))
                    continue
                stack.append((rect_name, match.start()))
                continue

            if not stack:
                continue

            rect_name, start_idx = stack.pop()
            if rect_name == section_name:
                remove_start = start_idx
                comment_match = re.search(
                    rf'(^|\n)[ \t]*<!--\s*{re.escape(section_name.upper())}\s*-->[ \t]*\r?\n[ \t]*$',
                    xml_text[:start_idx],
                )
                if comment_match:
                    remove_start = comment_match.start()
                spans.append((remove_start, match.end()))

        if not spans:
            return xml_text, 0

        updated = xml_text
        for start_idx, end_idx in sorted(spans, reverse=True):
            updated = updated[:start_idx] + updated[end_idx:]
        return updated, len(spans)

    def _ensure_links_rect_blocks(xml_text: str) -> tuple[str, int]:
        panel_tag_pattern = re.compile(r'<(/?)panel\b([^>]*)>', re.IGNORECASE)
        name_pattern = re.compile(r'\bname="([^"]+)"', re.IGNORECASE)

        stack: list[tuple[str, int]] = []
        spans: list[tuple[int, int]] = []

        for match in panel_tag_pattern.finditer(xml_text):
            is_close = match.group(1) == "/"
            tag_text = match.group(0)

            if not is_close:
                attrs = match.group(2)
                name_match = name_pattern.search(attrs)
                panel_name = name_match.group(1).strip() if name_match else ""
                if tag_text.rstrip().endswith("/>"):
                    continue
                stack.append((panel_name, match.start()))
                continue

            if not stack:
                continue

            panel_name, start_idx = stack.pop()
            if panel_name == "core":
                spans.append((start_idx, match.end()))

        if not spans:
            return xml_text, 0

        updated = xml_text
        additions = 0

        def _is_hv_panel(full_text: str, pos: int) -> bool:
            color_idx = full_text.rfind('<rect name="Color"', 0, pos)
            hv_idx = full_text.rfind('<rect name="HighVisibility"', 0, pos)
            if color_idx == -1 and hv_idx == -1:
                return False
            return hv_idx > color_idx

        for start_idx, end_idx in sorted(spans, reverse=True):
            block = updated[start_idx:end_idx]
            if 'name="links"' in block:
                continue

            panel_line_match = re.search(r'(?m)^(?P<indent>[ \t]*)<panel\b', block)
            panel_indent = panel_line_match.group("indent") if panel_line_match else ""
            child_indent = panel_indent + "\t"
            grid_indent = child_indent + "\t"

            is_hv = _is_hv_panel(updated, start_idx)
            if is_hv:
                bg_color = _theme_xml_color(theme_hv, "linkBackground", True)
                border_color = _theme_xml_color(theme_hv, "panelBorder", True)
                rect_open = f'{child_indent}<rect name="links" pos="0,-860" width="1100" height="60" visible="true">'
                bg_line = (
                    f'{grid_indent}<sprite name="linksBackground" depth="2" pos="5,-5" sprite="ui_game_header_fill" '
                    f'color="{bg_color}" width="1090" height="50" type="sliced" globalopacity="false"/>'
                )
                border_line = (
                    f'{grid_indent}<sprite name="linksBorder" depth="1" sprite="ui_game_header_fill" '
                    f'color="{border_color}" width="1100" height="60" type="sliced" globalopacity="false"/>'
                )
            else:
                bg_color = _theme_xml_color(theme_color, "linkBackground", False)
                border_color = _theme_xml_color(theme_color, "panelBorder", False)
                rect_open = (
                    f'{child_indent}<rect name="links" pos="0,-860" width="1100" height="60" '
                    'globalopacity="true" globalopacitymod="1.75" visible="true">'
                )
                bg_line = (
                    f'{grid_indent}<sprite name="linksBackground" depth="2" pos="5,-5" sprite="ui_game_header_fill" '
                    f'color="{bg_color}" width="1090" height="50" type="sliced"/>'
                )
                border_line = (
                    f'{grid_indent}<sprite name="linksBorder" depth="1" sprite="ui_game_header_fill" '
                    f'color="{border_color}" width="1100" height="60" type="sliced"/>'
                )

            generated_grid = _build_links_grid_xml(source, is_hv=is_hv, use_hv_dev_overrides=False)
            links_block = "\n".join(
                [
                    f"{child_indent}<!-- LINKS -->",
                    rect_open,
                    bg_line,
                    border_line,
                    _indent_block(generated_grid, grid_indent),
                    f"{child_indent}</rect>",
                    "",
                ]
            )

            options_match = re.search(r'(?m)^[ \t]*<rect name="options"\b', block)
            if options_match:
                insert_at = options_match.start()
            else:
                panel_close_match = re.search(r'(?m)^[ \t]*</panel>\s*$', block)
                if not panel_close_match:
                    continue
                insert_at = panel_close_match.start()

            new_block = block[:insert_at] + links_block + block[insert_at:]
            updated = updated[:start_idx] + new_block + updated[end_idx:]
            additions += 1

        return updated, additions

    updated_text = text

    # Main ESC windows: apply user options toggle only to base/admin windows.
    updated_text = _set_visible_attrs(
        updated_text,
        "optionsVisibilityMain",
        "rect",
        "options",
        toggles["showOptionsSection"],
        window_name="windowESC",
    )
    updated_text = _set_visible_attrs(
        updated_text,
        "optionsVisibilityAdmin",
        "rect",
        "options",
        toggles["showOptionsSection"],
        window_name="windowESCAdmin",
    )
    updated_text = _set_visible_attrs(
        updated_text,
        "linksVisibility",
        "rect",
        "links",
        toggles["showLinksSection"],
    )
    if toggles["showLinksSection"]:
        updated_text, links_added = _ensure_links_rect_blocks(updated_text)
        if links_added > 0:
            updates_by_key["linksAdded"] = links_added
    if not toggles["showLinksSection"]:
        updated_text, links_removed = _remove_named_rect_blocks(updated_text, "links")
        if links_removed > 0:
            updates_by_key["linksRemoved"] = links_removed
    updated_text = _set_visible_attrs(
        updated_text,
        "adminVersionVisibility",
        "window",
        "windowESCAdmin",
        toggles["showAdminVersion"],
    )

    # Server join visibility is generation-driven (present when enabled, absent when disabled)
    # and is handled by sync_windows_copied_variants().

    if updates_by_key:
        windows_file.write_text(updated_text, encoding="utf-8")

    return updates_by_key


def sync_windows_copied_variants(
    windows_file: Path,
    source: dict | None = None,
    include_admin: bool = True,
    include_server_join: bool = True,
) -> dict[str, int]:
    """Sync server/admin window variants as copies of windowESC.

        - serverjoinrulesdialog: copy of windowESC with ServerJoinRulesDialog controller,
            with dedicated serverJoinActions panel and includes btnLeave/btnSpawn.
    - windowESCAdmin: direct copy of windowESC with window name override.
    """
    if not windows_file.exists():
        raise FileNotFoundError(f"Windows file not found: {windows_file}")

    text = windows_file.read_text(encoding="utf-8")
    text = re.sub(
        r'^\s*<!--\s*centered-ish; keep your "down a bit" so it doesn\'t live in the compass lane\s*-->\s*\r?\n?',
        "",
        text,
        flags=re.IGNORECASE | re.MULTILINE,
    )
    original_text = text
    updates_by_key: dict[str, int] = {}

    windowesc_pattern = re.compile(
        r'(?mi)^[ \t]*<window\b[^>]*\bname="windowESC"[^>]*>.*?^[ \t]*</window>',
        re.IGNORECASE | re.DOTALL,
    )
    source_match = windowesc_pattern.search(text)
    if not source_match:
        return updates_by_key

    source_window = source_match.group(0)

    def _set_window_name(block: str, new_name: str) -> str:
        return re.sub(r'\bname="[^"]*"', f'name="{new_name}"', block, count=1)

    def _ensure_window_attr(block: str, attr_name: str, attr_value: str) -> str:
        open_tag_match = re.search(r'<window\b[^>]*>', block, re.IGNORECASE)
        if not open_tag_match:
            return block
        open_tag = open_tag_match.group(0)
        if re.search(rf'\b{re.escape(attr_name)}="[^"]*"', open_tag, re.IGNORECASE):
            new_open_tag = re.sub(
                rf'\b{re.escape(attr_name)}="[^"]*"',
                f'{attr_name}="{attr_value}"',
                open_tag,
                count=1,
                flags=re.IGNORECASE,
            )
        else:
            new_open_tag = open_tag[:-1] + f' {attr_name}="{attr_value}">'
        return block.replace(open_tag, new_open_tag, 1)

    def _ensure_tag_attr(tag_text: str, attr_name: str, attr_value: str) -> str:
        if re.search(rf'\b{re.escape(attr_name)}="[^"]*"', tag_text, re.IGNORECASE):
            return re.sub(
                rf'\b{re.escape(attr_name)}="[^"]*"',
                f'{attr_name}="{attr_value}"',
                tag_text,
                count=1,
                flags=re.IGNORECASE,
            )
        return tag_text[:-1] + f' {attr_name}="{attr_value}">'

    def _normalize_options_geometry(block: str) -> str:
        updated = re.sub(
            r'(?is)<rect\b[^>]*\bname="options"[^>]*>',
            lambda m: _ensure_tag_attr(
                _ensure_tag_attr(m.group(0), "pos", "-330,-300"),
                "height",
                "550",
            ),
            block,
        )
        updated = re.sub(
            r'(?is)<sprite\b[^>]*\bname="optionsBackground"[^>]*>',
            lambda m: _ensure_tag_attr(m.group(0), "height", "540"),
            updated,
        )
        updated = re.sub(
            r'(?is)<sprite\b[^>]*\bname="optionsBorder"[^>]*>',
            lambda m: _ensure_tag_attr(m.group(0), "height", "550"),
            updated,
        )
        return updated

    source_window = _normalize_options_geometry(source_window)
    text = text[: source_match.start()] + source_window + text[source_match.end() :]

    source_ui = source.get("ui", {}) if isinstance(source, dict) and isinstance(source.get("ui", {}), dict) else {}
    theme_color = get_theme_colors(source, "color") if isinstance(source, dict) else dict(DEFAULT_THEME_COLORS)
    theme_hv = get_theme_colors(source, "highVisibility") if isinstance(source, dict) else dict(DEFAULT_AUTO_HIGH_VISIBILITY_COLORS)

    # Day/time label uses regular text color for label and highlight color for value.
    color_font_tag = _localization_color_value(str(theme_color.get("pageBody", DEFAULT_THEME_COLORS["pageBody"])))
    color_highlight_tag = _localization_color_value(str(theme_color.get("textHighlight", DEFAULT_THEME_COLORS["textHighlight"])))
    hv_font_tag = _localization_color_value(str(theme_hv.get("pageBody", DEFAULT_AUTO_HIGH_VISIBILITY_COLORS["pageBody"])))
    hv_highlight_tag = _localization_color_value(str(theme_hv.get("textHighlight", DEFAULT_AUTO_HIGH_VISIBILITY_COLORS["textHighlight"])))

    server_join_title_font_size = _safe_font_size(
        source_ui.get("serverJoinTitleFontSize", DEFAULT_PAGE_TITLE_FONT_SIZE),
        DEFAULT_PAGE_TITLE_FONT_SIZE,
    )
    server_join_body_font_size = _safe_font_size(
        source_ui.get("serverJoinBodyFontSize", DEFAULT_PAGE_BODY_FONT_SIZE),
        DEFAULT_PAGE_BODY_FONT_SIZE,
    )
    server_join_body_align = _safe_align(source_ui.get("serverJoinBodyAlign", "center"), "center")

    def _set_section_visible(block: str, tag_name: str, section_name: str, enabled: bool) -> str:
        desired = "true" if enabled else "false"
        pattern = re.compile(
            rf'(?P<prefix><{tag_name}\b)(?P<attrs>[^>]*)(?P<suffix>>)',
            re.IGNORECASE | re.MULTILINE,
        )

        def _replace(match: re.Match[str]) -> str:
            attrs = match.group("attrs")
            name_match = re.search(r'\bname="([^"]*)"', attrs, re.IGNORECASE)
            if not name_match or name_match.group(1).strip() != section_name:
                return match.group(0)

            current = re.search(r'\bvisible="([^"]*)"', attrs, re.IGNORECASE)
            if current:
                new_attrs = re.sub(r'\bvisible="[^"]*"', f'visible="{desired}"', attrs, count=1, flags=re.IGNORECASE)
            else:
                if re.search(r'/\s*$', attrs):
                    attrs_no_slash = re.sub(r'/\s*$', '', attrs)
                    new_attrs = f"{attrs_no_slash} visible=\"{desired}\" /"
                else:
                    new_attrs = f"{attrs} visible=\"{desired}\""
            return f"{match.group('prefix')}{new_attrs}{match.group('suffix')}"

        return pattern.sub(_replace, block)

    def _place_server_join_buttons_in_options(block: str) -> str:
        updated = re.sub(
            r'\n[ \t]*<simplebutton\b[^>]*\bname="(?:btnLeave|btnSpawn)"[^>]*/>\s*',
            "\n",
            block,
            flags=re.IGNORECASE,
        )
        window_open = re.search(r'<window\b[^>]*\bname="serverjoinrulesdialog"[^>]*>', updated, re.IGNORECASE)
        if not window_open:
            return updated

        insertion = (
            "\n\t\t<!-- JOIN - BUTTONS -->"
            "\n\t\t\t<simplebutton name=\"btnLeave\" depth=\"30\" pos=\"575,-295\" width=\"140\" height=\"80\" caption_key=\"windowESC_join_Leave\"/>"
            "\n\t\t\t<simplebutton name=\"btnSpawn\" depth=\"30\" pos=\"725,-295\" width=\"140\" height=\"80\" caption_key=\"windowESC_join_Spawn\"/>"
        )
        insert_pos = window_open.end()
        return updated[:insert_pos] + insertion + updated[insert_pos:]

    def _remove_named_rect_block(block: str, rect_name: str) -> str:
        escaped = re.escape(rect_name)
        pattern = re.compile(
            rf'(?is)\n[ \t]*<rect\b[^>]*\bname="{escaped}"[^>]*>.*?\n[ \t]*</rect>\s*'
        )
        return pattern.sub("\n", block)

    def _convert_options_to_server_join_actions(block: str) -> str:
        updated = block
        updated = re.sub(
            r'\n[ \t]*<!--\s*SERVER JOIN ACTIONS\s*-->\s*\n?',
            "\n",
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(
            r'\n[ \t]*<rect\b[^>]*\bname="serverJoinActions"[^>]*>.*?\n[ \t]*</rect>\s*',
            "\n",
            updated,
            flags=re.IGNORECASE | re.DOTALL,
        )
        updated = re.sub(
            r'\n[ \t]*<!--\s*JOINING\s*-->\s*\n?',
            "\n",
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(
            r'\n[ \t]*<rect\b[^>]*\bname="joining"[^>]*>.*?\n[ \t]*</rect>\s*',
            "\n",
            updated,
            flags=re.IGNORECASE | re.DOTALL,
        )

        def _build_joining_block(indent: str, is_hv: bool) -> str:
            title_key = "windowESC_highvisibility_join_title" if is_hv else "windowESC_color_join_title"
            body_key = "windowESC_highvisibility_join_body" if is_hv else "windowESC_color_join_body"
            active_theme = theme_hv if is_hv else theme_color
            title_color = _theme_xml_color(active_theme, "textBolding", is_hv)
            body_color = _theme_xml_color(active_theme, "pageBody", is_hv)
            joining_bg_color = _theme_xml_color(active_theme, "pageBackground", is_hv)
            joining_border_color = _theme_xml_color(active_theme, "panelBorder", is_hv)
            if is_hv:
                joining_rect_attrs = 'name="joining" pos="1110,-300" width="320" height="550" globalopacity="false" visible="true"'
            else:
                joining_rect_attrs = 'name="joining" pos="1110,-300" width="320" height="550" globalopacity="true" globalopacitymod="1.75" visible="true"'

            return "\n".join(
                [
                    f"{indent}<!-- JOINING -->",
                    f"{indent}<rect {joining_rect_attrs}>",
                    f"{indent}\t<sprite name=\"joiningBackground\" depth=\"2\" pos=\"5,-5\" sprite=\"ui_game_header_fill\" color=\"{joining_bg_color}\" width=\"310\" height=\"540\" type=\"sliced\"/>",
                    f"{indent}\t<sprite name=\"joiningBorder\" depth=\"1\" sprite=\"ui_game_header_fill\" color=\"{joining_border_color}\" width=\"320\" height=\"550\" type=\"sliced\"/>",
                    f"{indent}\t<label name=\"joining_title\" depth=\"10\" pos=\"160,-40\" height=\"40\" width=\"180\" justify=\"center\" pivot=\"center\" effect=\"Outline8\" effect_color=\"0,0,0,255\" effect_distance=\"1,1\" color=\"{title_color}\" font_size=\"{server_join_title_font_size}\" text_key=\"{title_key}\" foregroundlayer=\"true\"/>",
                    f"{indent}\t<label name=\"join_body\" depth=\"10\" pos=\"160,-243\" height=\"335\" width=\"290\" justify=\"{server_join_body_align}\" pivot=\"center\" effect=\"Outline8\" effect_color=\"0,0,0,255\" effect_distance=\"1,1\" color=\"{body_color}\" font_size=\"{server_join_body_font_size}\" text_key=\"{body_key}\" foregroundlayer=\"true\"/>",
                    f"{indent}</rect>",
                ]
            )

        def _replace_mode_options_with_joining(window_block: str, mode_name: str, is_hv: bool) -> str:
            mode_range = _extract_first_named_rect_block(window_block, mode_name)
            if not mode_range:
                return window_block

            mode_start, mode_end, mode_block = mode_range
            options_range = _extract_first_named_rect_block(mode_block, "options")
            if not options_range:
                return window_block

            options_start, options_end, options_block = options_range
            options_indent_match = re.search(r'(?m)^(?P<indent>[ \t]*)<rect\b[^>]*\bname="options"', options_block)
            options_indent = options_indent_match.group("indent") if options_indent_match else ""

            replace_start = options_start
            comment_match = re.search(r'(?m)^[ \t]*<!--\s*OPTIONS[^>]*-->\s*$', mode_block[:options_start])
            if comment_match:
                line_start = mode_block.rfind("\n", 0, comment_match.start())
                replace_start = 0 if line_start == -1 else line_start + 1

            joining_block = _build_joining_block(options_indent, is_hv)
            replaced_mode_block = mode_block[:replace_start] + joining_block + mode_block[options_end:]
            return window_block[:mode_start] + replaced_mode_block + window_block[mode_end:]

        updated = _replace_mode_options_with_joining(updated, "HighVisibility", True)
        updated = _replace_mode_options_with_joining(updated, "Color", False)

        for mode_name in ("HighVisibility", "Color"):
            mode_range = _extract_first_named_rect_block(updated, mode_name)
            if not mode_range:
                continue

            mode_start, mode_end, mode_block = mode_range
            while True:
                options_range = _extract_first_named_rect_block(mode_block, "options")
                if not options_range:
                    break

                options_start, options_end, _ = options_range
                comment_match = re.search(r'(?m)^[ \t]*<!--\s*OPTIONS[^\n]*-->\s*$', mode_block[:options_start])
                remove_start = options_start
                if comment_match:
                    line_start = mode_block.rfind("\n", 0, comment_match.start())
                    remove_start = 0 if line_start == -1 else line_start + 1
                mode_block = mode_block[:remove_start] + mode_block[options_end:]

            updated = updated[:mode_start] + mode_block + updated[mode_end:]

        updated = re.sub(
            r'(?mi)^\s*<!--\s*OPTIONS\s*</rect>\s*$',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*OPTIONS\s*-->\s*$',
            "",
            updated,
        )
        return updated

    def _strip_mode_comment(block: str) -> str:
        return block.replace(
            "<!-- Section that allows switching between Full Color mode and High Visibility mode -->\n",
            "",
        )

    def _force_globalopacity_false(block: str) -> str:
        updated = re.sub(r'\bglobalopacity="true"', 'globalopacity="false"', block, flags=re.IGNORECASE)

        # Make Admin Full Color deterministic by ensuring these panel sprites always
        # explicitly opt out of global fade, even when the source tag omitted it.
        sprite_names = (
            "headerBackground",
            "headerBorder",
            "bodyBg",
            "bodyBorder",
            "optionsBackground",
            "optionsBorder",
            "linksBackground",
            "linksBorder",
        )
        sprite_pattern = re.compile(
            rf'(<sprite\b[^>]*\bname="(?:{"|".join(sprite_names)})"[^>]*?)(\s*/?>)',
            re.IGNORECASE,
        )

        def _normalize_sprite_opacity(match: re.Match[str]) -> str:
            attrs = match.group(1)
            close = match.group(2)
            if re.search(r'\bglobalopacity\s*=\s*"', attrs, re.IGNORECASE):
                attrs = re.sub(
                    r'\bglobalopacity\s*=\s*"[^"]*"',
                    'globalopacity="false"',
                    attrs,
                    flags=re.IGNORECASE,
                )
                return f"{attrs}{close}"
            return f'{attrs} globalopacity="false"{close}'

        return sprite_pattern.sub(_normalize_sprite_opacity, updated)

    def _retarget_comment_context(block: str, target_context: str) -> str:
        # Keep comment text intact, but retarget trailing context marker (e.g. "- MAIN" -> "- ADMIN").
        return re.sub(
            r'<!--\s*([^<>]*)\s*-\s*(MAIN|ADMIN|JOIN)\s*-->',
            lambda m: f"<!-- {m.group(1).strip()} - {target_context} -->",
            block,
            flags=re.IGNORECASE,
        )

    def _ensure_blank_line_before_header_comments(block: str) -> str:
        return re.sub(
            r'(?m)(?<!\n)\n([ \t]*<!--[^\n]*-->)',
            r'\n\n\1',
            block,
        )

    def _ensure_single_top_buttons_title(block: str, title: str) -> str:
        updated = re.sub(
            rf'(?mi)^\s*<!--\s*{re.escape(title)}\s*-->\s*\n',
            "",
            block,
        )
        updated = re.sub(
            r'(?m)^([ \t]*)<rect\s+name="tabsHeader">',
            rf'\1<!-- {title} -->\n\1<rect name="tabsHeader">',
            updated,
            count=1,
        )
        return updated

    def _annotate_admin_view_comment_titles(block: str) -> str:
        """Ensure admin comment titles include PLAYER VIEW / ADMIN VIEW suffix where applicable."""
        lines = block.splitlines()
        current_view: str | None = None
        out_lines: list[str] = []

        for line in lines:
            if re.search(r'<rect\b[^>]*\bname="PlayerView"', line, flags=re.IGNORECASE):
                current_view = "PLAYER VIEW"
            elif re.search(r'<rect\b[^>]*\bname="AdminView"', line, flags=re.IGNORECASE):
                current_view = "ADMIN VIEW"

            if current_view and re.search(r'<!--\s*(?:HEADER|BODY|PAGES|OPTIONS|LINKS)\s*-\s*[^>]*-\s*ADMIN(?:\s*-->)', line, flags=re.IGNORECASE):
                if not re.search(r'-\s*(?:PLAYER\s+VIEW|ADMIN\s+VIEW)\s*-->', line, flags=re.IGNORECASE):
                    line = re.sub(
                        r'-\s*ADMIN\s*-->',
                        f'- ADMIN - {current_view} -->',
                        line,
                        flags=re.IGNORECASE,
                    )

            out_lines.append(line)

        return "\n".join(out_lines)

    def _normalize_admin_mode_titles(block: str) -> str:
        """Ensure top-level admin mode title comments exist exactly once and without view suffixes."""
        updated = block
        updated = re.sub(
            r'(?mi)^\s*<!--\s*FULL\s+COLOR\s*-\s*ADMIN(?:\s*-\s*(?:PLAYER\s+VIEW|ADMIN\s+VIEW))?\s*-->\s*\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*HIGH\s+VISIBILITY\s*-\s*ADMIN(?:\s*-\s*(?:PLAYER\s+VIEW|ADMIN\s+VIEW))?\s*-->\s*\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?m)^([ \t]*)<rect\s+name="Color"\s+controller="TabSelectorTab"\s+tab_key="windowESCAdmin_mode_color_tab">',
            r'\1<!-- FULL COLOR - ADMIN -->\n\1<rect name="Color" controller="TabSelectorTab" tab_key="windowESCAdmin_mode_color_tab">',
            updated,
            count=1,
        )
        updated = re.sub(
            r'(?m)^([ \t]*)<rect\s+name="HighVisibility"\s+controller="TabSelectorTab"\s+tab_key="windowESCAdmin_mode_highvisibility_tab">',
            r'\1<!-- HIGH VISIBILITY - ADMIN -->\n\1<rect name="HighVisibility" controller="TabSelectorTab" tab_key="windowESCAdmin_mode_highvisibility_tab">',
            updated,
            count=1,
        )
        return updated

    def _ensure_pages_headers(block: str, target_context: str) -> str:
        # Normalize stale page headers first to avoid duplicate insertions.
        updated = re.sub(
            r'(?mi)^\s*<!--\s*PAGES\s*-\s*(?:FULL\s+COLOR|HIGH\s+VISIBILITY)\s*-\s*(?:MAIN|ADMIN|JOIN)(?:\s*-\s*(?:PLAYER\s+VIEW|ADMIN\s+VIEW))?\s*-->\s*\n',
            "",
            block,
        )
        pattern = re.compile(
            r'(?ms)(?P<tabsheader>^[ \t]*<rect\b[^>]*\bname="tabsHeader"[^>]*>.*?^[ \t]*</rect>\s*\n)'
            r'(?P<indent>[ \t]*)<rect\b[^>]*\bname="tabsContents"[^>]*>\s*\n'
            r'(?P<firstpage>[ \t]*<rect\b[^>]*\bname="(?:page_|adminadminPage_)[^\"]*"[^>]*>)'
        )

        def _replace(match: re.Match[str]) -> str:
            tabs_header = match.group("tabsheader")
            indent = match.group("indent")
            first_page_line = match.group("firstpage")
            mode_name = "HIGH VISIBILITY" if re.search(r'_hv\b|highvisibility', first_page_line, re.IGNORECASE) else "FULL COLOR"
            view_suffix = ""
            if target_context.upper() == "ADMIN":
                if "adminadminpage_" in first_page_line.lower():
                    view_suffix = " - ADMIN VIEW"
                else:
                    view_suffix = " - PLAYER VIEW"
            pages_comment = f"{indent}<!-- PAGES - {mode_name} - {target_context}{view_suffix} -->\n"
            return f"{tabs_header}{pages_comment}{indent}<rect name=\"tabsContents\">\n{first_page_line}"

        return pattern.sub(_replace, updated)

    def _extract_first_named_rect_block(block: str, rect_name: str) -> tuple[int, int, str] | None:
        open_match = re.search(
            rf'<rect\b[^>]*\bname="{re.escape(rect_name)}"[^>]*>',
            block,
            re.IGNORECASE,
        )
        if not open_match:
            return None

        start = open_match.start()
        scan_start = open_match.start()
        depth = 0
        for tag in re.finditer(r'</?rect\b[^>]*>', block[scan_start:], re.IGNORECASE):
            tag_text = tag.group(0)
            is_close = tag_text.startswith("</")
            is_self_closing = tag_text.endswith("/>")
            if is_close:
                depth -= 1
                if depth == 0:
                    end = scan_start + tag.end()
                    return start, end, block[start:end]
            elif not is_self_closing:
                depth += 1

        return None

    def _replace_options_block_with_template(mode_block: str, is_hv: bool) -> str:
        def _ensure_options_sprite_globalopacity_false(block: str) -> str:
            sprite_pattern = re.compile(
                r'(<sprite\b[^>]*\bname="(?:optionsBackground|optionsBorder)"[^>]*?)(\s*/?>)',
                re.IGNORECASE,
            )

            def _normalize_sprite_opacity(match: re.Match[str]) -> str:
                attrs = match.group(1)
                close = match.group(2)
                if re.search(r'\bglobalopacity\s*=\s*"', attrs, re.IGNORECASE):
                    attrs = re.sub(
                        r'\bglobalopacity\s*=\s*"[^"]*"',
                        'globalopacity="false"',
                        attrs,
                        flags=re.IGNORECASE,
                    )
                    return f"{attrs}{close}"
                return f'{attrs} globalopacity="false"{close}'

            return sprite_pattern.sub(_normalize_sprite_opacity, block)

        options_range = _extract_first_named_rect_block(mode_block, "options")
        if not options_range:
            return mode_block

        start, end, options_block = options_range
        open_match = re.match(
            r'(?s)(?P<open><rect\b[^>]*\bname="options"[^>]*>)(?P<body>.*)(?P<close>[ \t]*</rect>)\s*$',
            options_block,
            re.IGNORECASE,
        )
        if not open_match:
            return mode_block

        rect_indent_match = re.search(r'(?m)^(?P<indent>[ \t]*)<rect\b[^>]*\bname="options"', options_block)
        rect_indent = rect_indent_match.group("indent") if rect_indent_match else ""
        inner_indent = rect_indent + "\t"
        new_block = (
            f'{open_match.group("open")}\n'
            f'{_indent_block(_build_options_inner_xml(is_hv), inner_indent)}\n'
            f'{rect_indent}</rect>'
        )
        if is_hv:
            new_block = _ensure_options_sprite_globalopacity_false(new_block)
        return mode_block[:start] + new_block + mode_block[end:]

    def _replace_source_options_sections(block: str) -> str:
        updated = block
        for rect_name, is_hv in (("HighVisibility", True), ("Color", False)):
            mode_range = _extract_first_named_rect_block(updated, rect_name)
            if not mode_range:
                continue
            start, end, mode_block = mode_range
            replaced_mode_block = _replace_options_block_with_template(mode_block, is_hv=is_hv)
            updated = updated[:start] + replaced_mode_block + updated[end:]
        return updated

    def _replace_mode_key_prefixes(block: str, key_prefix: str) -> str:
        updated = block
        updated = re.sub(
            r'\bwindowESC_mode_color_tab\b',
            f"{key_prefix}_mode_color_tab",
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(
            r'\bwindowESC_mode_highvisibility_tab\b',
            f"{key_prefix}_mode_highvisibility_tab",
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(r'\bwindowESC_color_', f"{key_prefix}_color_", updated, flags=re.IGNORECASE)
        updated = re.sub(
            r'\bwindowESC_highvisibility_',
            f"{key_prefix}_highvisibility_",
            updated,
            flags=re.IGNORECASE,
        )
        return updated

    def _replace_admin_page_token(block: str) -> str:
        return re.sub(r'page_(\d+)', r'adminPage_\1', block, flags=re.IGNORECASE)

    def _restore_admin_shared_keys(block: str) -> str:
        updated = block
        updated = re.sub(
            r'\bwindowESCAdmin_color_options_title\b',
            'windowESC_color_options_title',
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(
            r'\bwindowESCAdmin_highvisibility_options_title\b',
            'windowESC_highvisibility_options_title',
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(
            r'\bwindowESCAdmin_color_link_(\d+)_label\b',
            r'windowESC_color_link_\1_label',
            updated,
            flags=re.IGNORECASE,
        )
        updated = re.sub(
            r'\bwindowESCAdmin_highvisibility_link_(\d+)_label\b',
            r'windowESC_highvisibility_link_\1_label',
            updated,
            flags=re.IGNORECASE,
        )
        return updated

    def _build_admin_root_tabs(existing_tabs_block: str) -> str:
        inner_match = re.match(
            r'(?is)^\s*<rect\b[^>]*\bname="tabs"[^>]*>\s*(.*?)\s*</rect>\s*$',
            existing_tabs_block,
        )
        if not inner_match:
            return existing_tabs_block

        player_tabs_inner = inner_match.group(1)
        admin_tabs_inner = _replace_mode_key_prefixes(player_tabs_inner, "windowESCAdmin")
        admin_tabs_inner = _replace_admin_page_token(admin_tabs_inner)
        admin_tabs_inner = _restore_admin_shared_keys(admin_tabs_inner)
        admin_tabs_inner = _force_globalopacity_false(admin_tabs_inner)

        player_color_range = _extract_first_named_rect_block(player_tabs_inner, "Color")
        player_hv_range = _extract_first_named_rect_block(player_tabs_inner, "HighVisibility")
        admin_color_range = _extract_first_named_rect_block(admin_tabs_inner, "Color")
        admin_hv_range = _extract_first_named_rect_block(admin_tabs_inner, "HighVisibility")
        if not player_color_range or not player_hv_range or not admin_color_range or not admin_hv_range:
            return existing_tabs_block

        player_color_block = player_color_range[2]
        player_hv_block = player_hv_range[2]
        admin_color_block = admin_color_range[2]
        admin_hv_block = admin_hv_range[2]

        def _unwrap_mode_block(mode_block: str) -> str:
            inner_match = re.match(
                r'(?is)^\s*<rect\b[^>]*>\s*(.*?)\s*</rect>\s*$',
                mode_block,
            )
            return inner_match.group(1) if inner_match else mode_block

        player_color_inner = _unwrap_mode_block(player_color_block)
        player_hv_inner = _unwrap_mode_block(player_hv_block)
        admin_color_inner = _unwrap_mode_block(admin_color_block)
        admin_hv_inner = _unwrap_mode_block(admin_hv_block)

        def _replace_admin_inner_tabs(mode_inner_block: str, is_hv: bool) -> str:
            if not isinstance(source, dict):
                return mode_inner_block

            admin_tabs_source = source.get("adminTabs", source.get("tabs", []))
            if not isinstance(admin_tabs_source, list):
                admin_tabs_source = source.get("tabs", []) if isinstance(source.get("tabs", []), list) else []

            admin_source = copy.deepcopy(source)
            admin_source["tabs"] = admin_tabs_source

            tabs_header_block = "\n".join(
                [
                    '<rect name="tabsHeader">',
                    _indent_block(
                        _build_tab_buttons_grid_xml(
                            admin_source,
                            is_hv=is_hv,
                        ),
                        "\t",
                    ),
                    '</rect>',
                ]
            )
            tabs_contents_block = _build_tabs_contents_xml(
                admin_source,
                is_hv=is_hv,
                name_prefix="admin",
                key_prefix="windowESCAdmin",
                page_token="adminPage",
            )

            replacement_tabs = "\n".join(
                [
                    '<rect name="tabs" controller="TabSelector" select_tab_contents_on_open="true">',
                    _indent_block(tabs_header_block, "\t"),
                    _indent_block(tabs_contents_block, "\t"),
                    '</rect>',
                ]
            )

            tabs_pattern = re.compile(
                r'(?P<indent>^[ \t]*)<rect name="tabs" controller="TabSelector" select_tab_contents_on_open="true">\s*\n'
                r'(?P<body>.*?)(?=^\1</rect>\s*$)^\1</rect>',
                re.MULTILINE | re.DOTALL,
            )

            def _replace_first_tabs(match: re.Match[str]) -> str:
                indent = match.group("indent")
                return _indent_block(replacement_tabs, indent)

            return tabs_pattern.sub(_replace_first_tabs, mode_inner_block, count=1)

        admin_color_inner = _replace_admin_inner_tabs(admin_color_inner, is_hv=False)
        admin_hv_inner = _replace_admin_inner_tabs(admin_hv_inner, is_hv=True)

        def _build_view_tabs(player_mode_block: str, admin_mode_block: str, mode_label: str, is_hv: bool) -> str:
            player_mode_block = _retarget_comment_context(player_mode_block, "ADMIN")
            admin_mode_block = _retarget_comment_context(admin_mode_block, "ADMIN")
            player_mode_block = player_mode_block.replace(" - MAIN -->", " - ADMIN -->")
            admin_mode_block = admin_mode_block.replace(" - MAIN -->", " - ADMIN -->")
            return "\n".join(
                [
                    '<rect name="tabs" controller="TabSelector" select_tab_contents_on_open="true">',
                    f'\t<!-- BUTTONS for PLAYER VIEW and ADMIN VIEW - {mode_label} -->',
                    '\t<rect name="tabsHeader">',
                    '\t\t<grid name="tabButtons" pos="355,438" depth="12" rows="2" cols="1" cell_width="170" cell_height="46" repeat_content="true" arrangement="horizontal">',
                    '\t\t\t<rect controller="TabSelectorButton">',
                    f'\t\t\t\t{_large_tab_button_xml(is_hv=is_hv)}',
                    '\t\t\t</rect>',
                    '\t\t</grid>',
                    '\t</rect>',
                    '\t<rect name="tabsContents">',
                    f'\t\t<!-- PLAYER VIEW - {mode_label} -->',
                    '\t\t<rect name="PlayerView" controller="TabSelectorTab" tab_key="windowESCAdmin_player_view_tab">',
                    _indent_block(player_mode_block, "\t\t\t"),
                    '\t\t</rect>',
                    f'\t\t<!-- ADMIN VIEW - {mode_label} -->',
                    '\t\t<rect name="AdminView" controller="TabSelectorTab" tab_key="windowESCAdmin_admin_view_tab">',
                    _indent_block(admin_mode_block, "\t\t\t"),
                    '\t\t</rect>',
                    '\t</rect>',
                    '</rect>',
                ]
            )

        color_view_tabs_block = _build_view_tabs(player_color_inner, admin_color_inner, "FULL COLOR", is_hv=False)
        hv_view_tabs_block = _build_view_tabs(player_hv_inner, admin_hv_inner, "HIGH VISIBILITY", is_hv=True)

        return "\n".join(
            [
                '<rect name="tabs" controller="TabSelector" select_tab_contents_on_open="true">',
                '\t<rect name="tabsHeader">',
                '\t\t<grid name="tabButtons" pos="-525,438" depth="12" rows="2" cols="1" cell_width="170" cell_height="46" repeat_content="true" arrangement="horizontal">',
                '\t\t\t<rect controller="TabSelectorButton">',
                f'\t\t\t\t{_large_tab_button_xml(is_hv=False)}',
                "\t\t\t</rect>",
                "\t\t</grid>",
                "\t</rect>",
                "\t<rect name=\"tabsContents\">",
                '\t\t<!-- FULL COLOR - ADMIN -->',
                '\t\t<rect name="Color" controller="TabSelectorTab" tab_key="windowESCAdmin_mode_color_tab">',
                _indent_block(color_view_tabs_block, "\t\t\t"),
                "\t\t</rect>",
                '\t\t<!-- HIGH VISIBILITY - ADMIN -->',
                '\t\t<rect name="HighVisibility" controller="TabSelectorTab" tab_key="windowESCAdmin_mode_highvisibility_tab">',
                _indent_block(hv_view_tabs_block, "\t\t\t"),
                "\t\t</rect>",
                "\t</rect>",
                "</rect>",
            ]
        )

    def _transform_admin_window_tabs(block: str) -> str:
        tabs_range = _extract_first_named_rect_block(block, "tabs")
        if not tabs_range:
            return block
        start, end, tabs_block = tabs_range
        replacement = _build_admin_root_tabs(tabs_block)
        return block[:start] + replacement + block[end:]

    def _normalize_large_mode_selector(block: str, is_hv: bool = False) -> str:
        pattern = re.compile(
            r'<simplebutton\s+name="tabButton"\s+depth="12"\s+pos="0,0"\s+width="170"\s+height="36"\s+font_size="30"[^>]*/>',
            re.IGNORECASE,
        )
        return pattern.sub(_large_tab_button_xml(is_hv=is_hv), block, count=1)

    def _normalize_windows_section_comments(full_text: str) -> str:
        updated = full_text
        updated = re.sub(
            r'(?mi)^\s*<!--\s*The ESC Menu used while in game\s*-->\s*\r?\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*Admin Section\s*-->\s*\r?\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*Server Join Rules Section\s*-->\s*\r?\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*Vanilla buttons preserved, relocated\s*-->\s*\r?\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*(?:WINDOW ESC SECTION|SERVER JOIN SECTION|ADMIN WINDOW ESC SECTION)\s*-->\s*\r?\n',
            "",
            updated,
        )
        updated = re.sub(
            r'(?mi)^\s*<!--\s*(?:WINDOW ESC - MAIN|WINDOW ESC - SERVER JOINING|WINDOW ESC - ADMIN)\s*-->\s*\r?\n',
            "",
            updated,
        )
        updated = re.sub(r'(?mi)^\s*<!--\s*HIGH VISIBILITY MAIN WINDOW\s*-->\s*\r?\n', "", updated)
        updated = re.sub(r'(?mi)^\s*<!--\s*OPTIONS\s*-->\s*\r?\n', "", updated)
        updated = re.sub(r'<!--\s*The Main Window\s*-->', "<!-- MAIN WINDOW -->", updated, flags=re.IGNORECASE)
        updated = re.sub(r'<!--\s*Full Color Main Window\s*-->', "<!-- FULL COLOR MAIN WINDOW -->", updated, flags=re.IGNORECASE)
        updated = re.sub(r'<!--\s*High Visibility:\s*tabsHeader\s*-->', "<!-- HIGH VISIBILITY: TABS HEADER -->", updated, flags=re.IGNORECASE)
        updated = re.sub(r'<!--\s*High Visibility:\s*tabsContents\s*-->', "<!-- HIGH VISIBILITY: TABS CONTENTS -->", updated, flags=re.IGNORECASE)
        # Do not inject generic mode/section comments; mode-specific comments already exist in layout blocks.
        updated = re.sub(
            r'(?is)(<append\s+xpath="/windows"\s*>\s*\n\s*<window\b[^>]*\bname="windowESC"[^>]*>)',
            r'\t<!-- WINDOW ESC - MAIN -->\n\1',
            updated,
            count=1,
        )
        updated = re.sub(
            r'(?is)(<remove\s+xpath="/windows/window\[@name=[\'\"]serverjoinrulesdialog[\'\"]\]"\s*/>\s*\n\s*<append\s+xpath="/windows"\s*>\s*\n\s*<window\b[^>]*\bname="serverjoinrulesdialog"[^>]*>)',
            r'\t<!-- WINDOW ESC - SERVER JOINING -->\n\1',
            updated,
            count=1,
        )
        updated = re.sub(
            r'(?is)(<append\s+xpath="/windows"\s*>\s*\n\s*<window\b[^>]*\bname="windowESCAdmin"[^>]*>)',
            r'\t<!-- WINDOW ESC - ADMIN -->\n\1',
            updated,
            count=1,
        )
        return updated

    def _reorder_named_window_append_blocks(full_text: str) -> str:
        append_pattern = re.compile(
            r'(?is)<append\s+xpath="/windows"\s*>[\s\S]*?</append>\s*'
        )

        window_names = ("windowESC", "serverjoinrulesdialog", "windowESCAdmin")
        targets: dict[str, str] = {}
        for window_name in window_names:
            window_pattern = re.compile(
                rf'(?mi)^[ \t]*<window\b[^>]*\bname="{re.escape(window_name)}"[^>]*>.*?^[ \t]*</window>'
            )
            window_match = window_pattern.search(full_text)
            if window_match:
                targets[window_name] = window_match.group(0).strip() + "\n"

        if not targets:
            return full_text

        updated = full_text
        spans: list[tuple[int, int]] = []
        for match in append_pattern.finditer(full_text):
            block = match.group(0)
            if any(re.search(rf'<window\b[^>]*\bname="{re.escape(name)}"', block, re.IGNORECASE) for name in window_names):
                spans.append((match.start(), match.end()))

        for start_idx, end_idx in sorted(spans, reverse=True):
            updated = updated[:start_idx] + updated[end_idx:]

        ordered_blocks: list[tuple[str, str, bool]] = [
            ("windowESC", "WINDOW ESC - MAIN", True),
            ("serverjoinrulesdialog", "WINDOW ESC - SERVER JOINING", include_server_join),
            ("windowESCAdmin", "WINDOW ESC - ADMIN", include_admin),
        ]

        assembled = ""
        for window_name, section_title, should_include in ordered_blocks:
            if not should_include:
                continue
            block = targets.get(window_name)
            if not block:
                continue
            assembled += f"\n\t<!-- {section_title} -->\n\t<append xpath=\"/windows\">\n{block}\t</append>\n"

        if not assembled:
            return updated

        insert_before = re.search(r'(?i)</AGF-ESCWindowPlus>\s*$', updated)
        if insert_before:
            return updated[: insert_before.start()] + assembled + updated[insert_before.start() :]
        return updated + assembled

    source_window = _replace_source_options_sections(source_window)
    source_window = _ensure_pages_headers(source_window, "MAIN")
    source_window = _ensure_single_top_buttons_title(source_window, "BUTTONS for FULL COLOR and HIGH VISIBILITY")
    source_window = _normalize_large_mode_selector(source_window, is_hv=False)
    source_window = _ensure_blank_line_before_header_comments(source_window)
    text = re.sub(
        r'(?mi)^[ \t]*<window\b[^>]*\bname="windowESC"[^>]*>.*?^[ \t]*</window>',
        source_window,
        text,
        count=1,
        flags=re.IGNORECASE | re.DOTALL,
    )

    server_block = _set_window_name(source_window, "serverjoinrulesdialog")
    server_block = _ensure_window_attr(server_block, "depth", "400")
    server_block = _ensure_window_attr(server_block, "controller", "ServerJoinRulesDialog")
    server_block = _ensure_window_attr(server_block, "globalopacity", "false")
    server_block = _strip_mode_comment(server_block)
    server_block = _set_section_visible(server_block, "rect", "options", True)
    server_block = _convert_options_to_server_join_actions(server_block)
    server_block = _place_server_join_buttons_in_options(server_block)
    server_block = _retarget_comment_context(server_block, "JOIN")
    server_block = re.sub(
        r'<!--\s*OPTIONS\s*-\s*(FULL\s+COLOR|HIGH\s+VISIBILITY)\s*-\s*JOIN\s*-->',
        lambda m: f"<!-- JOIN - {m.group(1).upper()} - JOIN -->",
        server_block,
        flags=re.IGNORECASE,
    )
    server_block = re.sub(
        r'<!--\s*JOINING\s*-->',
        "",
        server_block,
        flags=re.IGNORECASE,
    )
    server_block = _ensure_pages_headers(server_block, "JOIN")
    server_block = _ensure_single_top_buttons_title(server_block, "BUTTONS for FULL COLOR and HIGH VISIBILITY")
    server_block = _ensure_blank_line_before_header_comments(server_block)

    admin_block = _set_window_name(source_window, "windowESCAdmin")
    admin_block = _ensure_window_attr(admin_block, "depth", "40")
    admin_block = _ensure_window_attr(admin_block, "globalopacity", "false")
    admin_block = _strip_mode_comment(admin_block)
    admin_block = _transform_admin_window_tabs(admin_block)
    admin_block = _force_globalopacity_false(admin_block)
    admin_block = _retarget_comment_context(admin_block, "ADMIN")
    admin_block = _ensure_pages_headers(admin_block, "ADMIN")
    admin_block = _annotate_admin_view_comment_titles(admin_block)
    admin_block = _normalize_admin_mode_titles(admin_block)
    admin_block = _ensure_single_top_buttons_title(admin_block, "BUTTONS for FULL COLOR and HIGH VISIBILITY")
    admin_block = _ensure_blank_line_before_header_comments(admin_block)

    server_pattern = re.compile(
        r'(?mi)^[ \t]*<window\b[^>]*\bname="serverjoinrulesdialog"[^>]*>.*?^[ \t]*</window>',
        re.IGNORECASE | re.DOTALL,
    )
    admin_pattern = re.compile(
        r'(?mi)^[ \t]*<window\b[^>]*\bname="windowESCAdmin"[^>]*>.*?^[ \t]*</window>',
        re.IGNORECASE | re.DOTALL,
    )

    server_replaced = 0
    server_removed = 0
    if include_server_join:
        text, removed_windows = server_pattern.subn("", text)
        server_removed += removed_windows
        text = re.sub(
            r'\n[ \t]*<remove\s+xpath="/windows/window\[@name=[\'\"]serverjoinrulesdialog[\'\"]\]"\s*/>\s*\n?',
            "\n",
            text,
            flags=re.IGNORECASE,
        )
        remove_line = '\n\t<remove xpath="/windows/window[@name=\'serverjoinrulesdialog\']"/>\n'
        append_block = (
            "\n\t<!-- SERVER JOIN SECTION -->\n"
            f"{remove_line}"
            f"\t<append xpath=\"/windows\">\n{server_block}\n\t</append>\n"
        )
        insert_before = re.search(r'(?i)</AGF-ESCWindowPlus>\s*$', text)
        if insert_before:
            text = text[: insert_before.start()] + append_block + text[insert_before.start() :]
        else:
            text += append_block
        server_replaced = 1
    else:
        text, removed_windows = server_pattern.subn("", text)
        server_removed += removed_windows
        text, removed_xpath = re.subn(
            r'\n[ \t]*<remove\s+xpath="/windows/window\[@name=[\'\"]serverjoinrulesdialog[\'\"]\]"\s*/>\s*\n?',
            "\n",
            text,
            flags=re.IGNORECASE,
        )
        server_removed += removed_xpath
        text = re.sub(
            r'\n[ \t]*<!--\s*Server Join Rules Section\s*-->[ \t]*\n',
            "\n",
            text,
            flags=re.IGNORECASE,
        )
        text = re.sub(
            r'\n[ \t]*<!--\s*Vanilla buttons preserved, relocated\s*-->[ \t]*\n',
            "\n",
            text,
            flags=re.IGNORECASE,
        )
        text = re.sub(
            r'\n[ \t]*<append\s+xpath="/windows"\s*>\s*</append>\s*\n',
            "\n",
            text,
            flags=re.IGNORECASE,
        )

    admin_replaced = 0
    admin_removed = 0
    if include_admin:
        text, removed_admin = admin_pattern.subn("", text)
        admin_removed += removed_admin
        append_block = f"\n\t<!-- ADMIN WINDOW ESC SECTION -->\n\t<append xpath=\"/windows\">\n{admin_block}\n\t</append>\n"
        insert_before = re.search(r'(?i)</AGF-ESCWindowPlus>\s*$', text)
        if insert_before:
            text = text[: insert_before.start()] + append_block + text[insert_before.start() :]
        else:
            text += append_block
        admin_replaced = 1
    else:
        text, admin_removed = admin_pattern.subn("", text)
        # Cleanup trailing admin-only comment line if present.
        text = re.sub(
            r'\n[ \t]*<!--\s*(?:If Enabled, this is the ESC menu version for Admins|Admin Section)\s*-->[ \t]*\n',
            "\n",
            text,
            flags=re.IGNORECASE,
        )

    text = re.sub(
        r'\n[ \t]*<append\s+xpath="/windows"\s*>\s*</append>\s*\n',
        "\n",
        text,
        flags=re.IGNORECASE,
    )

    text = _normalize_windows_section_comments(text)

    # Safety normalization: keep MAIN page header comments present in both mode variants.
    text = re.sub(
        r'(?ms)(<!--\s*BODY\s*-\s*FULL\s+COLOR\s*-\s*MAIN\s*-->.*?<rect\b[^>]*\bname="tabsHeader"[^>]*>.*?</rect>\s*\n)([ \t]*)<rect\b[^>]*\bname="tabsContents"[^>]*>\s*\n([ \t]*<rect\b[^>]*\bname="page_1"[^>]*>)',
        r'\1\2<!-- PAGES - FULL COLOR - MAIN -->\n\2<rect name="tabsContents">\n\3',
        text,
        count=1,
        flags=re.IGNORECASE,
    )
    text = re.sub(
        r'(?ms)(<!--\s*BODY\s*-\s*HIGH\s+VISIBILITY\s*-\s*MAIN\s*-->.*?<rect\b[^>]*\bname="tabsHeader"[^>]*>.*?</rect>\s*\n)([ \t]*)<rect\b[^>]*\bname="tabsContents"[^>]*>\s*\n([ \t]*<rect\b[^>]*\bname="page_1_hv"[^>]*>)',
        r'\1\2<!-- PAGES - HIGH VISIBILITY - MAIN -->\n\2<rect name="tabsContents">\n\3',
        text,
        count=1,
        flags=re.IGNORECASE,
    )

    # Final safety pass: keep all trailing context comment suffixes inside admin window as ADMIN.
    text = re.sub(
        r'(?is)(<window\b[^>]*\bname="windowESCAdmin"[^>]*>)(.*?)(</window>)',
        lambda m: m.group(1) + m.group(2).replace(" - MAIN -->", " - ADMIN -->") + m.group(3),
        text,
        count=1,
    )

    if server_replaced > 0:
        updates_by_key["serverJoinWindowSync"] = server_replaced
    if server_removed > 0:
        updates_by_key["serverJoinWindowRemoved"] = server_removed
    if admin_replaced > 0:
        updates_by_key["adminWindowSync"] = admin_replaced
    if admin_removed > 0:
        updates_by_key["adminWindowRemoved"] = admin_removed
    if text != original_text:
        updates_by_key["windowSectionOrderAndComments"] = 1

    if updates_by_key:
        windows_file.write_text(text, encoding="utf-8")

    return updates_by_key


def update_xui_admin_window_binding(xui_file: Path, enabled: bool) -> int:
    """Ensure xui.xml includes admin window registration only when enabled."""
    if not xui_file.exists():
        raise FileNotFoundError(f"XUi file not found: {xui_file}")

    text = xui_file.read_text(encoding="utf-8")
    updates = 0

    block_pattern = re.compile(
        r'(?mi)^[ \t]*(?P<open><append\s+xpath="/xui/ruleset/window_group\[@name=[\'\"]ingameDebugMenu[\'\"]\]"\s*>)(?P<body>.*?)(?P<close>^[ \t]*</append>)',
        re.IGNORECASE | re.DOTALL,
    )
    admin_line_pattern = re.compile(r'^[ \t]*<window\s+name="windowESCAdmin"\s*/>\s*\r?\n?', re.IGNORECASE | re.MULTILINE)
    active_admin_line_pattern = re.compile(r'(?mi)^[ \t]*<window\s+name="windowESCAdmin"\s*/>\s*$')

    def _is_empty_append_body(body_text: str) -> bool:
        stripped = re.sub(r'<!--.*?-->', '', body_text, flags=re.DOTALL)
        stripped = re.sub(r'\s+', '', stripped)
        return stripped == ""

    text_without_comments = re.sub(r'<!--.*?-->', '', text, flags=re.DOTALL)

    block_match = block_pattern.search(text_without_comments)
    if block_match:
        body = block_match.group("body")
        has_admin = bool(admin_line_pattern.search(body))
        should_remove_empty_block = (not enabled) and _is_empty_append_body(body)

        if enabled and not has_admin:
            indent = "\n\t\t"
            if "\n" in body:
                indent_match = re.search(r'\n([ \t]*)<', body)
                if indent_match:
                    indent = f"\n{indent_match.group(1)}"
            body = body.rstrip() + f"{indent}<window name=\"windowESCAdmin\"/>\n\t"
            updates += 1
        elif (not enabled) and has_admin:
            body, removed = admin_line_pattern.subn("", body)
            updates += removed

        if updates > 0 or should_remove_empty_block:
            real_block_match = block_pattern.search(text)
            if real_block_match:
                if should_remove_empty_block or ((not enabled) and _is_empty_append_body(body)):
                    text = text[: real_block_match.start()] + text[real_block_match.end() :]
                    if should_remove_empty_block and updates == 0:
                        updates += 1
                else:
                    text = (
                        text[: real_block_match.start()]
                        + real_block_match.group("open")
                        + body
                        + real_block_match.group("close")
                        + text[real_block_match.end() :]
                    )
    elif enabled and not active_admin_line_pattern.search(text_without_comments):
        block = (
            "\n\t<append xpath=\"/xui/ruleset/window_group[@name='ingameDebugMenu']\">\n"
            "\t\t<window name=\"windowESCAdmin\"/>\n"
            "\t</append>\n"
        )
        close_root = re.search(r'</(?:configs|AGF-ESCWindowPlus)>\s*$', text, re.IGNORECASE)
        if close_root:
            text = text[: close_root.start()] + block + text[close_root.start() :]
        else:
            text = text.rstrip() + block
        updates += 1

    if updates > 0:
        xui_file.write_text(text, encoding="utf-8")

    return updates


def update_windows_news_sources(
    windows_file: Path,
    local_sources_by_id: dict[str, str],
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

        # Normalize any existing local mapping for this link id to the latest mod name/path.
        local_pattern = re.compile(
            r'(sources=")(@[^\"]+:Links/'
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


def _pretty_print_xml_text(xml_text: str) -> str:
    """Pretty print XML while preserving comments and removing blank spacer lines."""
    stripped = xml_text.strip()
    if not stripped:
        return xml_text

    dom = minidom.parseString(stripped.encode("utf-8"))
    pretty = dom.toprettyxml(indent="   ", newl="\n")

    lines = [line for line in pretty.splitlines() if line.strip()]
    with_header_spacing: list[str] = []
    for line in lines:
        if re.match(r'^\s*<!--[^>]*-->', line, flags=re.IGNORECASE):
            if with_header_spacing and with_header_spacing[-1] != "":
                with_header_spacing.append("")
        with_header_spacing.append(line)
    lines = with_header_spacing
    if lines and lines[0].startswith("<?xml"):
        lines = lines[1:]

    if not lines:
        return ""
    return "\n".join(lines).rstrip() + "\n"


def pretty_print_xml_file(xml_file: Path) -> int:
    """Apply pretty-print formatting to an XML file. Returns 1 when file changed."""
    if not xml_file.exists():
        return 0

    original = xml_file.read_text(encoding="utf-8")
    try:
        formatted = _pretty_print_xml_text(original)
    except Exception:
        # If parsing fails, leave file untouched instead of risking destructive rewrites.
        return 0

    if formatted != original:
        xml_file.write_text(formatted, encoding="utf-8")
        return 1
    return 0


def normalize_admin_window_comment_context(windows_file: Path) -> int:
    """Force trailing comment context in windowESCAdmin block to ADMIN."""
    if not windows_file.exists():
        return 0

    original = windows_file.read_text(encoding="utf-8")
    def _normalize_admin_block(match: re.Match[str]) -> str:
        body = match.group(2)
        body = body.replace(" - MAIN -->", " - ADMIN -->")
        body = re.sub(
            r'<!--\s*HIGH\s+VISIBILITY\s+MAIN\s+WINDOW\s*-->',
            "<!-- HIGH VISIBILITY - ADMIN -->",
            body,
            flags=re.IGNORECASE,
        )
        return match.group(1) + body + match.group(3)

    updated = re.sub(
        r'(?is)(<window\b[^>]*\bname="windowESCAdmin"[^>]*>)(.*?)(</window>)',
        _normalize_admin_block,
        original,
        count=1,
    )
    if updated != original:
        windows_file.write_text(updated, encoding="utf-8")
        return 1
    return 0


def main() -> None:
    args = parse_args()
    global TEMP_BODY_LABEL_BG_ENABLED

    if args.easy:
        args.merge_localization = True
        args.no_merge_localization = False
        args.update_windows_sources = True
        args.windows_update_all_links = True
        args.update_windows_links_layout = True
        args.use_hv_dev_overrides = True

    TEMP_BODY_LABEL_BG_ENABLED = args.dev_body_label_bg

    config_path = Path(args.config)
    links_dir = Path(args.links_dir)
    texts_path = Path(args.texts_file)

    source = load_source(config_path)

    if args.easy and not texts_path.exists() and not args.init_texts_file:
        write_texts_file(texts_path, source)
        print(f"Created texts file: {texts_path}")
        print("Easy mode paused so you can edit your text file.")
        print("Next run: re-run the same command with --easy after editing text.")
        return

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

    rows = build_localization_rows(source, use_hv_dev_overrides=args.use_hv_dev_overrides)

    merge_target: Path | None = None
    merge_result: tuple[int, int, int] | None = None
    localization_target = Path(args.localization_file)

    if args.no_merge_localization:
        localization_path = localization_target
    elif args.merge_localization:
        merge_target = Path(args.localization_file)
        merge_result = merge_localization(
            rows,
            merge_target,
        )
        localization_path = merge_target
    else:
        localization_path = write_localization_target(rows, localization_target)

    feature_toggles = get_ui_feature_toggles(source)
    news_paths = write_news_files(source, links_dir, links_enabled=feature_toggles["showLinksSection"])

    windows_file: Path | None = None
    xui_file: Path | None = None
    windows_update_by_id: dict[str, int] = {}
    windows_tab_updates: dict[str, int] = {}
    windows_grid_updates: dict[str, int] = {}
    windows_copy_updates: dict[str, int] = {}
    windows_options_updates: dict[str, int] = {}
    windows_admin_comment_updates = 0
    xui_admin_updates = 0
    windows_pretty_updates = 0
    xui_pretty_updates = 0
    if args.update_windows_sources or args.update_windows_links_layout:
        windows_file = Path(args.windows_file)
        xui_file = Path(args.xui_file)

    if args.update_windows_sources and windows_file is not None:
        local_sources_by_id = build_local_news_source_map(source, mod_folder_name)
        if args.windows_update_all_links:
            link_ids = sorted(local_sources_by_id.keys())
        else:
            link_ids = [args.windows_link_id]

        windows_update_by_id = update_windows_news_sources(
            windows_file,
            local_sources_by_id,
            link_ids,
        )

    if args.update_windows_links_layout and windows_file is not None:
        windows_tab_updates = update_windows_tab_layout(
            windows_file,
            source,
            use_hv_dev_overrides=args.use_hv_dev_overrides,
        )
        windows_grid_updates = update_windows_link_grids(
            windows_file,
            source,
            use_hv_dev_overrides=args.use_hv_dev_overrides,
        )

    if windows_file is not None and (args.update_windows_links_layout or args.update_windows_sources):
        windows_copy_updates = sync_windows_copied_variants(
            windows_file,
            source=source,
            include_admin=feature_toggles["showAdminVersion"],
            include_server_join=feature_toggles["showServerJoinSection"],
        )

    if windows_file is not None and (args.update_windows_links_layout or args.update_windows_sources):
        windows_options_updates = update_windows_options_visibility(windows_file, source)
        if xui_file is not None:
            xui_admin_updates = update_xui_admin_window_binding(
                xui_file,
                feature_toggles["showAdminVersion"],
            )
        windows_admin_comment_updates = normalize_admin_window_comment_context(windows_file)

    if windows_file is not None and (args.update_windows_links_layout or args.update_windows_sources):
        windows_pretty_updates = pretty_print_xml_file(windows_file)
        if xui_file is not None:
            xui_pretty_updates = pretty_print_xml_file(xui_file)

    print(f"Updated localization: {localization_path}")
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
        total_grid_updates = (
            sum(windows_tab_updates.values())
            + sum(windows_grid_updates.values())
            + sum(windows_copy_updates.values())
            + sum(windows_options_updates.values())
        )
        print(
            "Updated windows changes: "
            f"sources={total_windows_updates}, grids={total_grid_updates} in {windows_file}"
        )
        if windows_update_by_id:
            details = ", ".join(f"{k}={v}" for k, v in sorted(windows_update_by_id.items()))
            print(f"Windows source updates by link: {details}")
        if windows_tab_updates:
            details = ", ".join(f"{k}={v}" for k, v in sorted(windows_tab_updates.items()))
            print(f"Windows tab updates: {details}")
        if windows_grid_updates:
            details = ", ".join(f"{k}={v}" for k, v in sorted(windows_grid_updates.items()))
            print(f"Windows grid updates: {details}")
        if windows_copy_updates:
            details = ", ".join(f"{k}={v}" for k, v in sorted(windows_copy_updates.items()))
            print(f"Windows copy updates: {details}")
        if windows_options_updates:
            details = ", ".join(f"{k}={v}" for k, v in sorted(windows_options_updates.items()))
            print(f"Windows options updates: {details}")
        if xui_file is not None and xui_admin_updates > 0:
            print(f"XUi admin registration updates: {xui_admin_updates} in {xui_file}")
        if windows_pretty_updates > 0:
            print(f"XML pretty print applied: {windows_file}")
        if xui_file is not None and xui_pretty_updates > 0:
            print(f"XML pretty print applied: {xui_file}")
if __name__ == "__main__":
    main()

