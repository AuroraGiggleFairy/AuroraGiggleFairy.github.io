"""Sync Publishing Matrix Data into mod-publishing-matrix.canvas.tsx.

Reads Publish-State.json, formats rows, and injects only the RAW data array
into the canvas file without modifying the React component code.
"""
from __future__ import annotations

import json
import os
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parent
STATE_FILE = HERE / "Publish-State.json"
CHECKLIST_FILE = HERE / "Publish-Checklist.md"
CANVAS_FILE = Path(os.environ.get("USERPROFILE", "")) / ".cursor/projects/c-GitHub-7D2D-Mods/canvases/mod-publishing-matrix.canvas.tsx"
DELTA_SCRIPT = HERE / "SCRIPT-PublishDelta.py"
RAW_PREFIX = "const RAW: ModRow[] = "


def load_json(path: Path) -> dict:
    if path.is_file():
        try:
            with open(path, "r", encoding="utf-8") as f:
                return json.load(f)
        except Exception:
            return {}
    return {}


def get_beta_mods() -> set[str]:
    betas = set()
    active_dir = REPO_ROOT / "02_ActiveBuild"
    if active_dir.is_dir():
        for d in active_dir.iterdir():
            modinfo = d / "ModInfo.xml"
            if modinfo.is_file():
                try:
                    text = modinfo.read_text(encoding="utf-8")
                    if "BETA" in text:
                        base = re.sub(r"-v\d+.*$", "", d.name)
                        betas.add(base)
                except Exception:
                    pass
    return betas


def get_screenshot_mods(state_mods: list[str]) -> set[str]:
    """Detect mods that have a slot 01 source screenshot in PrimaryImageSources."""
    src_dir = REPO_ROOT / "00_Images" / "01_ImageWorkflow" / "PrimaryImageSources"
    if not src_dir.is_dir():
        return set()

    allowed_ext = {".png", ".jpg", ".jpeg", ".webp"}
    img_files = [f for f in src_dir.iterdir() if f.is_file() and f.suffix.lower() in allowed_ext]

    def get_token(name: str) -> str:
        m = re.match(r"^(?:0|z+)?AGF-V\d+-(?:SERVER|CLIENT|BOTH|ADMIN|COMPAT|TBD)-(.+)$", name)
        return m.group(1) if m else name

    def clean(s: str) -> str:
        s = s.lower()
        s = re.sub(r"^(?:0|z+)?agf-(?:v\d+-)?(?:server-|client-|both-|admin-|compat-|tbd-)?", "", s)
        s = re.sub(r"_[0-9]+.*$", "", s)
        s = s.replace("-", "").replace("_", "")
        return s

    matched = set()
    for mod in state_mods:
        token = get_token(mod)
        c_mod = clean(mod)
        for f in img_files:
            stem = f.stem
            # Direct exact candidate matches
            if stem in {f"{token}_01", token, f"{mod}_01", mod}:
                matched.add(mod)
                break
            # Normalized slug match (e.g. 2showstabilityoption)
            c_stem = clean(stem)
            if c_mod and c_stem == c_mod:
                matched.add(mod)
                break

    return matched


def build_rows() -> list[dict]:
    state = load_json(STATE_FILE)
    betas = get_beta_mods()
    screenshots = get_screenshot_mods(list(state.keys()))

    rows = []
    for mod, data in sorted(state.items()):
        d7 = data.get("7dtd", {})
        dnx = data.get("nexus", {})
        
        mid_7dtd = d7.get("mod_id", "")
        url_7dtd = f"https://7daystodiemods.com/mods/{mid_7dtd}" if mid_7dtd else ""

        mid_nx = dnx.get("mod_id", 0)
        url_nx = f"https://www.nexusmods.com/7daystodie/mods/{mid_nx}" if mid_nx else ""

        # Clean status strings
        st_7dtd = d7.get("status", "unknown").replace("**", "")
        st_nx = dnx.get("status", "unknown").replace("**", "")

        rows.append({
            "mod": mod,
            "ver": data.get("version", "1.0.0"),
            "mode": data.get("mode", "auto"),
            "status7dtd": st_7dtd,
            "url7dtd": url_7dtd,
            "statusNexus": st_nx,
            "urlNexus": url_nx,
            "delta": data.get("delta_summary", "Up to Date"),
            "screenshot": mod in screenshots,
            "isBeta": mod in betas,
        })
    return rows


def inject_canvas(rows: list[dict]) -> bool:
    if not CANVAS_FILE.is_file():
        print(f"Canvas file not found: {CANVAS_FILE}")
        return False

    text = CANVAS_FILE.read_text(encoding="utf-8")
    start = text.find(RAW_PREFIX)
    if start < 0:
        print("Canvas missing RAW_PREFIX")
        return False

    arr_at = start + len(RAW_PREFIX)
    if text[arr_at] != "[":
        print("Canvas RAW is not an array")
        return False

    _, end_rel = json.JSONDecoder().raw_decode(text[arr_at:])
    end = arr_at + end_rel
    if text[end : end + 1] != ";":
        print("Canvas RAW not followed by semicolon")
        return False

    payload = RAW_PREFIX + json.dumps(rows, ensure_ascii=False, indent=2) + ";"
    new_text = text[:start] + payload + text[end + 1 :]
    CANVAS_FILE.write_text(new_text, encoding="utf-8")
    print(f"Injected {len(rows)} mod rows into {CANVAS_FILE.name}")
    return True


def main():
    import subprocess
    # 1. Update delta state first
    subprocess.run([sys.executable, str(DELTA_SCRIPT)], check=True)

    # 2. Build rows
    rows = build_rows()

    # 3. Save data JSON
    data_out = HERE / "publish-matrix.data.json"
    with open(data_out, "w", encoding="utf-8") as f:
        json.dump(rows, f, indent=2, ensure_ascii=False)
    print(f"Saved {data_out.name}")

    # 4. Inject into canvas
    inject_canvas(rows)


if __name__ == "__main__":
    main()
