"""
SCRIPT-GeneratePublishChecklist.py

Scans 02_ActiveBuild and cross-references:
  - 00_Images/01_ImageWorkflow/PrimaryImageSources (screenshots)
  - 7DaysToDieMods/Workflow/7dtdmods-config.json (7dtdmods status)
  - NexusMods/Workflow/nexusmods-config.json & Nexus-Status.json (Nexus status)
  - Prior Publish-Checklist.md (for historical live version baseline)

Outputs a comprehensive, up-to-date Publish-Checklist.md for all 90 ActiveBuild mods.
"""

from __future__ import annotations

import datetime as dt
import json
import os
import re
import xml.etree.ElementTree as ET
from pathlib import Path

REPO_ROOT = Path(r"C:\GitHub\7D2D-Mods")
ACTIVE_BUILD_DIR = REPO_ROOT / "02_ActiveBuild"
IMAGE_SRC_DIR = REPO_ROOT / "00_Images" / "01_ImageWorkflow" / "PrimaryImageSources"
CHECKLIST_PATH = REPO_ROOT / "06_PublishingSupport" / "Publish-Checklist.md"

NEXUS_CONFIG_PATH = REPO_ROOT / "06_PublishingSupport" / "NexusMods" / "Workflow" / "nexusmods-config.json"
NEXUS_STATUS_PATH = REPO_ROOT / "06_PublishingSupport" / "NexusMods" / "Workflow" / "Nexus-Status.json"
SEVEN_CONFIG_PATH = REPO_ROOT / "06_PublishingSupport" / "7DaysToDieMods" / "Workflow" / "7dtdmods-config.json"

SPECIAL_MAP = {
    "1hudplus": "hudplus1main",
    "2bmcounter": "hudplusbmcounter",
    "2purplebook": "hudpluspurplebook",
    "2removeenteringpopup": "hudplusremoveenteringpopup",
    "2visualentitytracker": "hudplusvisualentitytracker",
    "2weekday": "hudplusweekday",
    "3visualentitytrackeraddon": "visualentitytrackeraddon",
    "2showstabilityoption": "showstabilityoption",
}


def clean_key(s: str) -> str:
    s = s.lower()
    s = re.sub(r"^(?:agf-|zzzagf-|zzzzzagf-)?", "", s)
    s = re.sub(r"^(?:v3-)?", "", s)
    s = re.sub(r"^(?:server-|client-|both-|admin-|compat-|vp-|vps-|noeac-|requested-|4modders-)?", "", s)
    s = re.sub(r"^[0-9]+", "", s)
    s = s.replace("-", "").replace("_", "")
    return s


def find_matching_entry(base: str, catalog: dict) -> tuple[str, dict] | None:
    if base in catalog and isinstance(catalog[base], dict):
        return base, catalog[base]
    ck = clean_key(base)
    core_alias = None
    for k_alias, v_alias in SPECIAL_MAP.items():
        if k_alias in base.lower():
            core_alias = v_alias
            break

    best_match = None
    for k, v in catalog.items():
        if not isinstance(v, dict):
            continue
        cnk = clean_key(k)
        if ck == cnk or (core_alias and core_alias == cnk):
            if v.get("site_mod_id") or v.get("nexus_mod_id"):
                return k, v
            if best_match is None:
                best_match = (k, v)
    return best_match


def main() -> None:
    nexus_cfg = {}
    if NEXUS_CONFIG_PATH.exists():
        with open(NEXUS_CONFIG_PATH, "r", encoding="utf-8") as f:
            nexus_cfg = json.load(f).get("mods", {})

    nexus_status_map = {}
    if NEXUS_STATUS_PATH.exists():
        with open(NEXUS_STATUS_PATH, "r", encoding="utf-8") as f:
            for item in json.load(f).get("mods", []):
                nexus_status_map[item.get("base_name", "")] = item

    seven_cfg = {}
    if SEVEN_CONFIG_PATH.exists():
        with open(SEVEN_CONFIG_PATH, "r", encoding="utf-8") as f:
            seven_cfg = json.load(f).get("mods", {})

    old_checklist = {}
    if CHECKLIST_PATH.exists():
        with open(CHECKLIST_PATH, "r", encoding="utf-8") as f:
            for line in f:
                if "|" in line and not line.strip().startswith("| Mod") and not line.strip().startswith("|---") and not line.strip().startswith("**Summary"):
                    parts = [p.strip() for p in line.split("|")]
                    if len(parts) >= 7:
                        old_checklist[parts[1]] = {
                            "ver": parts[2],
                            "screenshot": parts[3],
                            "beta": parts[4],
                            "7dtdmods": parts[5],
                            "nexus": parts[6],
                        }

    img_files = list(IMAGE_SRC_DIR.glob("*")) if IMAGE_SRC_DIR.exists() else []

    mods_data = []

    for entry in sorted(os.listdir(ACTIVE_BUILD_DIR)):
        folder_path = ACTIVE_BUILD_DIR / entry
        if not folder_path.is_dir():
            continue

        base_name = re.sub(r"-v\d+(?:\.\d+)*$", "", entry)
        match = re.search(r"-v(\d+(?:\.\d+)+)$", entry)
        folder_ver = match.group(1) if match else "?"

        modinfo = folder_path / "ModInfo.xml"
        ver = folder_ver
        disp_name = ""
        if modinfo.exists():
            try:
                root = ET.parse(modinfo).getroot()
                v_el = root.find("Version")
                if v_el is not None and v_el.get("value"):
                    ver = v_el.get("value")
                d_el = root.find("DisplayName")
                if d_el is not None and d_el.get("value"):
                    disp_name = d_el.get("value")
            except Exception:
                pass

        # Screenshot check
        ck = clean_key(base_name)
        has_screenshot = False
        for img in img_files:
            c_img = clean_key(img.stem)
            if ck and (ck in c_img or c_img in ck):
                has_screenshot = True
                break

        is_beta = "BETA" in disp_name.upper()

        # Lookups
        seven_match = find_matching_entry(base_name, seven_cfg)
        nexus_match = find_matching_entry(base_name, nexus_cfg)

        old_match = None
        for k_old, v_old in old_checklist.items():
            if clean_key(k_old) == ck:
                old_match = v_old
                break

        # ── 7dtdmods status ──
        s_status = "**upload**"
        if seven_match:
            sk, sv = seven_match
            site_id = sv.get("site_mod_id")
            last_tag = sv.get("last_release_tag")
            if site_id:
                if last_tag == ver:
                    s_status = "done"
                elif last_tag:
                    s_status = f"**update** {last_tag}"
                elif old_match and old_match.get("7dtdmods") == "done" and old_match.get("ver") != ver:
                    s_status = f"**update** {old_match['ver']}"
                else:
                    s_status = "done"
        elif old_match:
            if old_match.get("7dtdmods") == "done":
                if old_match.get("ver") == ver:
                    s_status = "done"
                else:
                    s_status = f"**update** {old_match['ver']}"
            elif "update" in old_match.get("7dtdmods", ""):
                s_status = old_match["7dtdmods"]

        # ── Nexus status ──
        n_status = "**upload**"
        if base_name in ("AGF-V3-SERVER-POISwitchesUnlockDoors", "AGF-V3-ADMIN-Fix4PerkPageReset", "zzzAGF-V3-CLIENT-LawnTractorV3Fix"):
            n_status = "done"
        elif nexus_match:
            nk, nv = nexus_match
            mid = nv.get("nexus_mod_id", 0)
            if mid in (13090, 13091, 13093):
                n_status = "done"
            elif mid and int(mid) > 0:
                st = nexus_status_map.get(nk) or nexus_status_map.get(base_name)
                live_v = st.get("live_version") if st else None
                if live_v and live_v != "?":
                    if live_v == ver:
                        n_status = "done"
                    else:
                        n_status = f"**update** {live_v}"
                elif old_match and old_match.get("nexus") == "done" and old_match.get("ver") != ver:
                    n_status = f"**update** {old_match['ver']}"
                elif old_match and "update" in old_match.get("nexus", ""):
                    n_status = old_match["nexus"]
                else:
                    n_status = "done"
        elif old_match:
            if old_match.get("nexus") == "done":
                if old_match.get("ver") == ver:
                    n_status = "done"
                else:
                    n_status = f"**update** {old_match['ver']}"
            elif "update" in old_match.get("nexus", ""):
                n_status = old_match["nexus"]

        # ── Special cases ──
        if base_name == "0AGF-V3-CLIENT-LawnTractorPatchGuard":
            s_status = "packaged"
            n_status = "packaged"
        elif base_name == "AGF-V3-ADMIN-Fix4DestroyBiomeBadge":
            s_status = "skip"
            n_status = "skip"
        elif "COMPAT" in base_name or base_name.startswith("zzzzzAGF-V3-COMPAT"):
            s_status = "skip"
            n_status = "skip"

        mods_data.append({
            "mod": base_name,
            "ver": ver,
            "screenshot": "yes" if has_screenshot else "no",
            "beta": "yes" if is_beta else "no",
            "7dtdmods": s_status,
            "nexus": n_status,
        })

    # Summary
    n_done = sum(1 for m in mods_data if m["nexus"] == "done")
    n_update = sum(1 for m in mods_data if "update" in m["nexus"])
    n_upload = sum(1 for m in mods_data if "upload" in m["nexus"])

    s_done = sum(1 for m in mods_data if m["7dtdmods"] == "done")
    s_update = sum(1 for m in mods_data if "update" in m["7dtdmods"])
    s_upload = sum(1 for m in mods_data if "upload" in m["7dtdmods"])

    today = dt.date.today().strftime("%Y-%m-%d")

    lines = [
        "# Publish checklist",
        "",
        f"{today}. Tick the site column when that site is done.",
        "",
        "Screenshot = yes if a real capture exists in `00_Images/01_ImageWorkflow/PrimaryImageSources/` (not the generated `02_ImagesFinal/*_01.png` banner).",
        "",
        "BETA = ModInfo DisplayName contains `BETA` — skip auto upload/update unless named explicitly.",
        "",
        f"**Summary:** Total Mods: **{len(mods_data)}** | Nexus: **{n_done}** done, **{n_update}** update, **{n_upload}** upload | 7dtdmods: **{s_done}** done, **{s_update}** update, **{s_upload}** upload",
        "",
        "| Mod | Ver | Screenshot | BETA | 7dtdmods | Nexus |",
        "|---|---|---|---|---|---|",
    ]

    for m in mods_data:
        lines.append(f"| {m['mod']} | {m['ver']} | {m['screenshot']} | {m['beta']} | {m['7dtdmods']} | {m['nexus']} |")

    lines.append("")

    with open(CHECKLIST_PATH, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    print(f"Successfully generated {CHECKLIST_PATH}")
    print(f"Total mods: {len(mods_data)}")
    print(f"  Nexus: {n_done} done, {n_update} needs update, {n_upload} first upload needed")
    print(f"  7dtdmods: {s_done} done, {s_update} needs update, {s_upload} first upload needed")

    # Sync Publishing Matrix Canvas
    sync_matrix = REPO_ROOT / "06_PublishingSupport" / "SCRIPT-SyncPublishingMatrix.py"
    if sync_matrix.is_file():
        import subprocess
        import sys
        try:
            subprocess.run([sys.executable, str(sync_matrix)], check=True)
        except Exception as e:
            print(f"Note: Matrix canvas sync skipped: {e}")


if __name__ == "__main__":
    main()
