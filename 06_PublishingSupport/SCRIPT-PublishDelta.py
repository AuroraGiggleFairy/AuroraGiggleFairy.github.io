"""Delta Fingerprinting and State Engine for Mod Publishing.

Tracks hashes of:
  - Description body (HTML / BBCode)
  - One-line summary
  - Title / DisplayName
  - Primary image
  - Release zip

Determines for each mod whether:
  - Files/changelog need update (version bumped or zip changed)
  - Description body needs update (readme / features / scope changed)
  - Summary / title need update
  - Media needs update (primary graphic asset changed)
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parent
ACTIVE_BUILD = REPO_ROOT / "02_ActiveBuild"
ZIPS_DIR = REPO_ROOT / "04_DownloadZips"
IMAGES_DIR = REPO_ROOT / "00_Images" / "02_ImagesFinal"
HELP_7DTD = HERE / "7DaysToDieMods" / "PublishHelp"
HELP_NEXUS = HERE / "NexusMods" / "PublishHelp"
CONFIG_7DTD = HERE / "7DaysToDieMods" / "Workflow" / "7dtdmods-config.json"
CONFIG_NEXUS = HERE / "NexusMods" / "Workflow" / "nexusmods-config.json"
STATE_FILE = HERE / "Publish-State.json"
CHECKLIST_FILE = HERE / "Publish-Checklist.md"
CANVAS_DATA_FILE = Path(os.environ.get("USERPROFILE", "")) / ".cursor/projects/c-GitHub-7D2D-Mods/canvases/mod-publishing-matrix.canvas.data.json"


def load_canvas_modes() -> dict[str, str]:
    """Read user-selected modes (Auto vs Skip) directly from the Canvas data sidecar."""
    if CANVAS_DATA_FILE.is_file():
        try:
            with open(CANVAS_DATA_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
            return data.get("mod-automation", {})
        except Exception:
            return {}
    return {}


def file_sha256(path: Path) -> str:
    if not path.is_file():
        return ""
    h = hashlib.sha256()
    with open(path, "rb") as f:
        while chunk := f.read(65536):
            h.update(chunk)
    return h.hexdigest()[:16]


def text_sha256(text: str) -> str:
    if not text:
        return ""
    # Normalize whitespace/line endings before hashing
    norm = re.sub(r"\r\n|\r", "\n", text).strip()
    return hashlib.sha256(norm.encode("utf-8")).hexdigest()[:16]


def load_json(path: Path) -> dict:
    if path.is_file():
        try:
            with open(path, "r", encoding="utf-8") as f:
                return json.load(f)
        except Exception:
            return {}
    return {}


def save_json(path: Path, data: dict) -> None:
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
        f.write("\n")


def extract_7dtd_packet_data(md_path: Path) -> dict:
    if not md_path.is_file():
        return {}
    text = md_path.read_text(encoding="utf-8")
    
    title_m = re.search(r"## 1\) Title.*?```(?:text)?\r?\n(.*?)```", text, re.S)
    summary_m = re.search(r"## 2\) Summary.*?```(?:text)?\r?\n(.*?)```", text, re.S)
    desc_m = re.search(r"## 4\) Description.*?```(?:text)?\r?\n(.*?)```\r?\n\r?\n---\r?\n\r?\n## 5\)", text, re.S)
    
    return {
        "title": title_m.group(1).strip() if title_m else "",
        "summary": summary_m.group(1).strip() if summary_m else "",
        "description": desc_m.group(1).strip() if desc_m else "",
    }


def extract_nexus_packet_data(md_path: Path) -> dict:
    if not md_path.is_file():
        return {}
    text = md_path.read_text(encoding="utf-8")
    
    # Nexus format: first page info has title, version, summary
    fences = re.findall(r"```text\r?\n(.*?)```", text, re.S)
    title = fences[0].strip() if len(fences) > 0 else ""
    summary = fences[2].strip() if len(fences) > 2 else ""
    
    desc_m = re.search(r"## 1b\) Full Description.*?```text\r?\n(.*?)```", text, re.S)
    desc = desc_m.group(1).strip() if desc_m else ""
    
    return {
        "title": title,
        "summary": summary,
        "description": desc,
    }


def build_publish_state(dry_run: bool = False) -> dict:
    current_state = load_json(STATE_FILE)
    cfg_7dtd = load_json(CONFIG_7DTD).get("mods", {})
    cfg_nexus = load_json(CONFIG_NEXUS).get("mods", {})
    
    # Read active build directories to get all current mod versions
    active_mods: dict[str, str] = {}
    if ACTIVE_BUILD.is_dir():
        for d in ACTIVE_BUILD.iterdir():
            if d.is_dir():
                m = re.search(r"^(.*?)-v(\d+\.\d+\.\d+.*)$", d.name)
                if m:
                    active_mods[m.group(1)] = m.group(2)
                else:
                    active_mods[d.name] = "1.0.0"

    # Also parse checklist for site statuses
    checklist_status = {}
    if CHECKLIST_FILE.is_file():
        for line in CHECKLIST_FILE.read_text(encoding="utf-8").splitlines():
            if "|" in line and not line.strip().startswith("| Mod") and not line.strip().startswith("|---"):
                parts = [p.strip() for p in line.split("|")]
                if len(parts) >= 7:
                    mod = parts[1]
                    status_7dtd = parts[5]
                    status_nexus = parts[6]
                    checklist_status[mod] = {
                        "7dtd": status_7dtd,
                        "nexus": status_nexus,
                    }

    updated_state = {}
    canvas_modes = load_canvas_modes()

    for mod, ver in sorted(active_mods.items()):
        existing = current_state.get(mod, {})
        user_mode = canvas_modes.get(mod, existing.get("mode", "auto"))  # auto | skip
        
        # 7dtd packet
        p_7dtd = extract_7dtd_packet_data(HELP_7DTD / f"{mod}.md")
        desc_hash_7dtd = text_sha256(p_7dtd.get("description", ""))
        summary_hash_7dtd = text_sha256(p_7dtd.get("summary", ""))
        title_7dtd = p_7dtd.get("title", "")
        
        # Nexus packet
        p_nexus = extract_nexus_packet_data(HELP_NEXUS / f"{mod}.md")
        desc_hash_nexus = text_sha256(p_nexus.get("description", ""))
        summary_hash_nexus = text_sha256(p_nexus.get("summary", ""))
        title_nexus = p_nexus.get("title", "")

        # Zip & Image
        zip_path = ZIPS_DIR / f"{mod}.zip"
        zip_hash = file_sha256(zip_path)
        
        img_candidates = list(IMAGES_DIR.glob(f"{mod}_01.png"))
        img_hash = file_sha256(img_candidates[0]) if img_candidates else ""

        # Site config records
        c_7dtd = cfg_7dtd.get(mod, {})
        mid_7dtd = c_7dtd.get("site_mod_id", "")
        last_tag_7dtd = c_7dtd.get("last_release_tag", "")

        c_nexus = cfg_nexus.get(mod, {})
        mid_nexus = c_nexus.get("nexus_mod_id", 0)

        chk = checklist_status.get(mod, {})
        st_7dtd = chk.get("7dtd", "unknown")
        st_nexus = chk.get("nexus", "unknown")

        # Prior recorded hashes
        prev_7dtd = existing.get("7dtd", {})
        prev_nexus = existing.get("nexus", {})

        # Compute delta flags
        needs_zip_7dtd = (ver != last_tag_7dtd) or ("update" in st_7dtd) or ("upload" in st_7dtd)
        pub_title_7dtd = c_7dtd.get("last_published_title") or prev_7dtd.get("published_title", "")
        needs_title_7dtd = bool(
            (pub_title_7dtd and title_7dtd != pub_title_7dtd)
            or (title_7dtd and prev_7dtd.get("title") and title_7dtd != prev_7dtd.get("title"))
            or (prev_7dtd.get("needs_title", False) and pub_title_7dtd != title_7dtd)
        )
        needs_desc_7dtd = bool(desc_hash_7dtd and prev_7dtd.get("desc_hash") and desc_hash_7dtd != prev_7dtd.get("desc_hash")) or needs_title_7dtd
        needs_media_7dtd = bool(img_hash and prev_7dtd.get("image_hash") and img_hash != prev_7dtd.get("image_hash"))

        needs_zip_nexus = ("update" in st_nexus) or ("upload" in st_nexus)
        pub_title_nexus = c_nexus.get("last_published_title") or prev_nexus.get("published_title", "")
        needs_title_nexus = bool(
            (pub_title_nexus and title_nexus != pub_title_nexus)
            or (title_nexus and prev_nexus.get("title") and title_nexus != prev_nexus.get("title"))
            or (prev_nexus.get("needs_title", False) and pub_title_nexus != title_nexus)
        )
        needs_desc_nexus = bool(desc_hash_nexus and prev_nexus.get("desc_hash") and desc_hash_nexus != prev_nexus.get("desc_hash")) or needs_title_nexus

        # Determine summary delta flag for display
        delta_badges = []
        if "upload" in st_7dtd or "upload" in st_nexus:
            delta_badges.append("New Upload")
        elif needs_zip_7dtd or needs_zip_nexus:
            delta_badges.append("New Zip")
        if needs_title_7dtd or needs_title_nexus:
            delta_badges.append("Title Changed")
        elif needs_desc_7dtd or needs_desc_nexus:
            delta_badges.append("Desc Changed")
        if needs_media_7dtd:
            delta_badges.append("Image Changed")
        if not delta_badges:
            delta_badges.append("Up to Date")

        updated_state[mod] = {
            "version": ver,
            "mode": user_mode,
            "delta_summary": ", ".join(delta_badges),
            "7dtd": {
                "mod_id": mid_7dtd,
                "status": st_7dtd,
                "last_tag": last_tag_7dtd,
                "title": title_7dtd,
                "published_title": pub_title_7dtd or (title_7dtd if not needs_title_7dtd else ""),
                "desc_hash": desc_hash_7dtd or prev_7dtd.get("desc_hash", ""),
                "summary_hash": summary_hash_7dtd or prev_7dtd.get("summary_hash", ""),
                "image_hash": img_hash or prev_7dtd.get("image_hash", ""),
                "needs_zip": needs_zip_7dtd,
                "needs_title": needs_title_7dtd,
                "needs_desc": needs_desc_7dtd,
                "needs_media": needs_media_7dtd,
            },
            "nexus": {
                "mod_id": mid_nexus,
                "status": st_nexus,
                "title": title_nexus,
                "published_title": pub_title_nexus or (title_nexus if not needs_title_nexus else ""),
                "desc_hash": desc_hash_nexus or prev_nexus.get("desc_hash", ""),
                "summary_hash": summary_hash_nexus or prev_nexus.get("summary_hash", ""),
                "needs_zip": needs_zip_nexus,
                "needs_title": needs_title_nexus,
                "needs_desc": needs_desc_nexus,
            },
            "zip_hash": zip_hash or existing.get("zip_hash", ""),
        }

    if not dry_run:
        save_json(STATE_FILE, updated_state)
    return updated_state


def main():
    parser = argparse.ArgumentParser(description="Audit and compute publishing deltas")
    parser.add_argument("--dry-run", action="store_true", help="Print delta summary without saving state")
    parser.add_argument("--mod", help="Audit only a specific mod")
    args = parser.parse_args()

    state = build_publish_state(dry_run=args.dry_run)
    print(f"Computed publishing deltas for {len(state)} mods.")
    
    needs_action = []
    for mod, data in state.items():
        if args.mod and args.mod.lower() not in mod.lower():
            continue
        badges = data["delta_summary"]
        if badges != "Up to Date":
            needs_action.append((mod, data["version"], badges, data["7dtd"]["status"], data["nexus"]["status"]))

    print(f"\nMods requiring action ({len(needs_action)}):")
    for m, v, b, s7, sn in needs_action:
        print(f"  • {m} (v{v}) [{b}] -> 7dtd: {s7} | Nexus: {sn}")


if __name__ == "__main__":
    main()
