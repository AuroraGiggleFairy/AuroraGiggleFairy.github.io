"""Discover ReleaseSource mods and refresh Publish-Checklist.md.

Sources:
- 03_ReleaseSource / ModInfo.xml (version, DisplayName BETA)
- PrimaryImageSources/{base}_01.* (screenshot yes/no)
- Nexus-Status.json (from RUN-Nexus-Status.bat) when present
- 7dtdmods-config.json site_mod_id + optional public API version

Usage:
  python SCRIPT-RefreshPublishChecklist.py
  python SCRIPT-RefreshPublishChecklist.py --fetch-7dtdmods
"""
from __future__ import annotations

import argparse
import datetime as dt
import json
import re
import urllib.request
import xml.etree.ElementTree as ET
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parent.parent  # 06_PublishingSupport -> repo? HERE=06_PublishingSupport, parents[0]=repo
# Actually: HERE = .../06_PublishingSupport ; REPO = parents[0] wait
# Path: repo/06_PublishingSupport/SCRIPT-...
REPO = HERE.parent
RELEASE = REPO / "03_ReleaseSource"
PRIMARY = REPO / "00_Images" / "01_ImageWorkflow" / "PrimaryImageSources"
CHECKLIST = HERE / "Publish-Checklist.md"
NEXUS_STATUS = HERE / "NexusMods" / "Workflow" / "Nexus-Status.json"
DTD_CONFIG = HERE / "7DaysToDieMods" / "Workflow" / "7dtdmods-config.json"
API_7D = "https://api.7daystodiemods.com/v1/mods"

AGF_PREFIXES = ("zzzAGF-", "AGF-")


def strip_prefix(base: str) -> str:
    for p in AGF_PREFIXES:
        if base.startswith(p):
            return base[len(p) :]
    return base


def get_base_mod_name(folder: str) -> str:
    return re.sub(r"-v\d+(?:\.\d+)*$", "", folder)


def parse_modinfo(folder: Path) -> tuple[str, str]:
    """Return (version, display_name)."""
    path = folder / "ModInfo.xml"
    version = "?"
    display = ""
    if not path.is_file():
        m = re.search(r"-v(\d+(?:\.\d+)+)$", folder.name)
        return (m.group(1) if m else "?", "")
    try:
        root = ET.parse(path).getroot()
    except ET.ParseError:
        return version, display
    for child in root:
        tag = child.tag.lower()
        val = (child.attrib.get("value") or "").strip()
        if tag == "version" and val:
            version = val
        elif tag == "displayname" and val:
            display = val
        elif tag == "name" and not display and val:
            display = val
    return version, display


def has_screenshot(base: str) -> bool:
    if not PRIMARY.is_dir():
        return False
    for ext in (".png", ".jpg", ".jpeg", ".webp"):
        if (PRIMARY / f"{base}_01{ext}").is_file():
            return True
        if (PRIMARY / f"{base}{ext}").is_file():
            return True
    return False


def is_beta(display_name: str) -> bool:
    return "BETA" in (display_name or "").upper()


def compare_versions(left: str, right: str) -> int:
    def parts(v: str) -> tuple[int, ...]:
        nums = [int(x) for x in re.findall(r"\d+", v or "")]
        while len(nums) < 3:
            nums.append(0)
        return tuple(nums[:4])

    a, b = parts(left), parts(right)
    return (a > b) - (a < b)


def load_nexus_status() -> dict[str, dict]:
    if not NEXUS_STATUS.is_file():
        return {}
    data = json.loads(NEXUS_STATUS.read_text(encoding="utf-8"))
    out: dict[str, dict] = {}
    for row in data.get("rows") or []:
        base = row.get("base_name") or ""
        if base:
            out[base] = row
    # Also flatten buckets if rows missing
    if not out:
        for bucket, items in (data.get("buckets") or {}).items():
            for row in items or []:
                base = row.get("base_name") or ""
                if base:
                    row = dict(row)
                    row.setdefault("status", {
                        "needs_update": "Needs Update",
                        "matches": "Matches",
                        "first_upload_needed": "First Upload Needed",
                    }.get(bucket, bucket))
                    out[base] = row
    return out


def nexus_cell(base: str, nexus: dict[str, dict], local_ver: str) -> str:
    row = nexus.get(base)
    if not row:
        return "**upload**"
    status = str(row.get("status") or "")
    live = str(row.get("live_version") or row.get("nexus_version") or "-")
    if status == "Needs Update":
        return f"**update** {live}"
    if status == "Matches":
        return "done"
    if status == "First Upload Needed":
        return "**upload**"
    # Fallback compare
    if live in ("", "-", "?", "None"):
        return "**upload**"
    cmp = compare_versions(local_ver, live)
    if cmp > 0:
        return f"**update** {live}"
    if cmp == 0:
        return "done"
    return f"**check** nexus {live}"


def load_7dtd_config() -> dict:
    if not DTD_CONFIG.is_file():
        return {}
    return json.loads(DTD_CONFIG.read_text(encoding="utf-8"))


def fetch_7dtd_version(mod_id: str) -> str | None:
    try:
        req = urllib.request.Request(
            f"{API_7D}/{mod_id}",
            headers={"User-Agent": "AGF-PublishChecklist/0.1"},
        )
        with urllib.request.urlopen(req, timeout=20) as resp:
            data = json.loads(resp.read().decode("utf-8"))
        cv = data.get("current_version") or {}
        return str(cv.get("version") or "") or None
    except Exception:
        return None


def guess_7dtd_slug(base: str, display: str) -> str:
    """Best-effort public slug from base name (matches common AGF listing pattern)."""
    # e.g. AGF-VP-FuelBurnPlus -> agf-v3-vp-fuelburnplus
    core = base
    for p in AGF_PREFIXES:
        if core.startswith(p):
            core = core[len(p) :]
            break
    parts = ["agf", "v3"] + [p.lower() for p in core.split("-") if p]
    return "-".join(parts)


def resolve_7dtd_id(base: str, entry: dict, *, fetch: bool) -> tuple[str | None, str | None]:
    """Return (mod_id, live_version)."""
    mid = entry.get("site_mod_id")
    if mid and not fetch:
        return str(mid), None
    if mid and fetch:
        return str(mid), fetch_7dtd_version(str(mid))
    if not fetch:
        return None, None
    # Try slug lookup
    slug = entry.get("site_slug") or guess_7dtd_slug(base, "")
    try:
        req = urllib.request.Request(
            f"{API_7D}/{slug}",
            headers={"User-Agent": "AGF-PublishChecklist/0.1"},
        )
        with urllib.request.urlopen(req, timeout=15) as resp:
            data = json.loads(resp.read().decode("utf-8"))
        mod_id = str(data.get("id") or data.get("mod_id") or "")
        cv = data.get("current_version") or {}
        ver = str(cv.get("version") or "") or None
        return (mod_id or None), ver
    except Exception:
        return None, None


def dtd_cell(
    base: str,
    cfg: dict,
    local_ver: str,
    *,
    fetch: bool,
    screenshot: bool,
    beta: bool,
) -> str:
    if beta:
        return "beta"
    entry = (cfg.get("mods") or {}).get(base) or {}
    mid, live = resolve_7dtd_id(base, entry, fetch=fetch)
    if not mid:
        return "**upload**" if screenshot else "**upload**"
    # Persist discovered id into memory only this run (caller may save)
    entry_ids = cfg.setdefault("_discovered_7dtd", {})
    entry_ids[base] = mid
    if not fetch:
        return "done"
    if not live:
        live = fetch_7dtd_version(str(mid))
    if not live:
        return "done"
    cmp = compare_versions(local_ver, live)
    if cmp > 0:
        return f"**update** {live}"
    if cmp < 0:
        return f"**check** site {live}"
    return "done"


def discover_mods() -> list[dict]:
    mods = []
    if not RELEASE.is_dir():
        raise SystemExit(f"Missing ReleaseSource: {RELEASE}")
    for folder in sorted(RELEASE.iterdir(), key=lambda p: p.name.lower()):
        if not folder.is_dir():
            continue
        name = folder.name
        if not (name.startswith("AGF-") or name.startswith("zzzAGF-")):
            continue
        base = get_base_mod_name(name)
        ver, display = parse_modinfo(folder)
        mods.append(
            {
                "base": base,
                "short": strip_prefix(base),
                "folder": name,
                "version": ver,
                "display": display,
                "screenshot": has_screenshot(base),
                "beta": is_beta(display),
            }
        )
    return mods


def write_checklist(mods: list[dict], nexus: dict[str, dict], cfg: dict, *, fetch_7d: bool) -> None:
    today = dt.date.today().isoformat()
    lines = [
        "# Publish checklist",
        "",
        f"{today}. Tick the site column when that site is done.",
        "",
        "Screenshot = yes if a real capture exists in `00_Images/01_ImageWorkflow/PrimaryImageSources/` "
        "(not the generated `02_ImagesFinal/*_01.png` banner).",
        "",
        "BETA = ModInfo DisplayName contains `BETA` — skip auto upload/update unless named explicitly.",
        "",
        "| Mod | Ver | Screenshot | BETA | 7dtdmods | Nexus |",
        "|---|---|---|---|---|---|",
    ]
    for m in mods:
        shot = "yes" if m["screenshot"] else "no"
        beta = "yes" if m["beta"] else "no"
        dtd = dtd_cell(
            m["base"],
            cfg,
            m["version"],
            fetch=fetch_7d,
            screenshot=m["screenshot"],
            beta=m["beta"],
        )
        nex = nexus_cell(m["base"], nexus, m["version"])
        if m["beta"] and nex not in ("done",):
            # Keep nexus status but flag beta in column; auto queue skips these
            pass
        lines.append(
            f"| {m['short']} | {m['version']} | {shot} | {beta} | {dtd} | {nex} |"
        )
    lines.append("")
    CHECKLIST.write_text("\n".join(lines), encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser(description="Refresh Publish-Checklist.md from ReleaseSource")
    ap.add_argument(
        "--fetch-7dtdmods",
        action="store_true",
        help="Hit public API for version compare when site_mod_id is known (slower)",
    )
    args = ap.parse_args()

    mods = discover_mods()
    nexus = load_nexus_status()
    cfg = load_7dtd_config()
    write_checklist(mods, nexus, cfg, fetch_7d=args.fetch_7dtdmods)

    shot_yes = sum(1 for m in mods if m["screenshot"])
    betas = sum(1 for m in mods if m["beta"])
    print(f"Wrote {CHECKLIST}")
    print(f"  mods: {len(mods)}")
    print(f"  screenshot yes: {shot_yes}")
    print(f"  BETA: {betas}")
    print(f"  nexus status rows loaded: {len(nexus)}")
    if not nexus:
        print("  NOTE: Nexus-Status.json missing — run RUN-Nexus-Status.bat first for Nexus column.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
