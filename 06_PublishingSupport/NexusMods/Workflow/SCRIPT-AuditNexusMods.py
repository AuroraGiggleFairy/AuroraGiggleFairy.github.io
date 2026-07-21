"""Audit all release source mods against Nexus to discover which are published.

Outputs a markdown report showing:
- Mod name + local version
- Whether found on Nexus
- Nexus ID + Nexus version
- Version comparison (local vs nexus)
- Config status
"""
import getpass, json, os, re, sys, urllib.error, urllib.request, xml.etree.ElementTree as ET
from typing import Dict, List, Optional, Tuple

sys.dont_write_bytecode = True  # never leave a __pycache__ behind

# ── Paths ────────────────────────────────────────────────────────────────
NEXUS_WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
VS_CODE_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(NEXUS_WORKFLOW_DIR)))
RELEASE_SOURCE_DIR = os.path.join(VS_CODE_ROOT, "03_ReleaseSource")
CONFIG_PATH = os.path.join(NEXUS_WORKFLOW_DIR, "nexusmods-config.json")

# ── API setup ────────────────────────────────────────────────────────────
API_KEY_ENV_VAR = "AGF_NEXUSMODS_API_KEY"
HEADERS = {
    "accept": "application/json",
    "apikey": "",
    "Application-Name": "AGF-NexusMods-Automation",
    "Application-Version": "0.1.0",
    "User-Agent": "AGF-NexusMods-Automation/0.1.0",
}
GAME_DOMAIN = "7daystodie"
API_V1_BASE = "https://api.nexusmods.com/v1"
API_V3_BASE = "https://api.nexusmods.com/v3"

AGF_AUTHOR_NAMES = ("AuroraGiggleFairy", "auroragigglefairy", "AuroraGiggleFairyAGF", "GiggleFairy")


# ── Helper functions ─────────────────────────────────────────────────────

def get_base_mod_name(name: str) -> str:
    """Strip version suffix from folder name to get base mod name."""
    return re.sub(r"-v\d+(?:\.\d+)*$", "", name)


def parse_version_from_folder(folder: str) -> str:
    m = re.search(r"-v(\d+(?:\.\d+)+)$", folder)
    return m.group(1) if m else "?"


def parse_local_version(folder_path: str, folder_name: str) -> str:
    """Read the authoritative version from ModInfo.xml, with folder-name fallback."""
    modinfo_path = os.path.join(folder_path, "ModInfo.xml")
    try:
        root = ET.parse(modinfo_path).getroot()
        for child in root:
            if child.tag.lower() == "version":
                version = child.attrib.get("value", "").strip()
                if version:
                    return version
    except (OSError, ET.ParseError):
        pass
    return parse_version_from_folder(folder_name)


def parse_version_parts(version: str) -> Tuple[int, int, int]:
    nums = [int(p) for p in re.findall(r"\d+", version)]
    while len(nums) < 3:
        nums.append(0)
    return nums[0], nums[1], nums[2]


def compare_versions(left: str, right: str) -> int:
    lp = parse_version_parts(left)
    rp = parse_version_parts(right)
    if lp < rp: return -1
    if lp > rp: return 1
    return 0


def request_json(url: str, method: str = "GET", body: dict = None):
    payload = None if body is None else json.dumps(body).encode("utf-8")
    hdrs = dict(HEADERS)
    if payload is not None:
        hdrs["content-type"] = "application/json"
    req = urllib.request.Request(url, headers=hdrs, method=method, data=payload)
    with urllib.request.urlopen(req, timeout=30) as resp:
        raw = resp.read()
    return json.loads(raw.decode("utf-8")) if raw else {}


def safe_int(val) -> int:
    try: return int(val)
    except: return 0


def author_matches(result: dict) -> bool:
    for field in ("author", "username", "uploader", "uploaded_by", "author_name", "uploaded_by_name"):
        raw = result.get(field, None)
        if raw is not None:
            s = str(raw).strip()
            if s and any(a.lower() in s.lower() for a in AGF_AUTHOR_NAMES):
                return True
    return False


def fetch_known_mod_info(nexus_mod_id: int) -> dict:
    """Fetch detailed info for a known mod ID."""
    result = {}
    # Try v3
    try:
        data = request_json(f"{API_V3_BASE}/games/{GAME_DOMAIN}/mods/{nexus_mod_id}")
        if isinstance(data, dict) and "data" in data:
            result.update(data["data"])
        elif isinstance(data, dict):
            result.update(data)
    except:
        pass
    # Try v1 for version info
    try:
        data = request_json(f"{API_V1_BASE}/games/{GAME_DOMAIN}/mods/{nexus_mod_id}.json")
        if isinstance(data, dict):
            result.update(data)
    except:
        pass
    return result


def search_nexus(query: str) -> list:
    """Search Nexus v1 API by name, return results filtered to AGF author."""
    import urllib.parse as urlparse
    encoded = urlparse.quote(query)
    url = f"{API_V1_BASE}/games/{GAME_DOMAIN}/mods/search.json?name={encoded}"
    try:
        data = request_json(url)
        results = data if isinstance(data, list) else (data.get("data", []) if isinstance(data, dict) and "data" in data else [])
        if not isinstance(results, list):
            return []
        # Filter to AGF author
        return [r for r in results if isinstance(r, dict) and author_matches(r)]
    except urllib.error.HTTPError as ex:
        if ex.code in (404, 501):
            return []
        print(f"  [SEARCH ERROR] HTTP {ex.code} for '{query}'")
        return []
    except Exception as ex:
        print(f"  [SEARCH ERROR] {ex} for '{query}'")
        return []


def score_match(result_name: str, mod_name: str) -> float:
    """Score how well a Nexus result name matches our mod name."""
    rn = result_name.lower().strip()
    mn = mod_name.lower().strip()
    
    # Exact match
    if rn == mn:
        return 1.0
    
    # Remove prefix for comparison
    mod_base = mn
    for prefix in ("zzzagf-", "agf-"):
        if mn.startswith(prefix):
            mod_base = mn[len(prefix):]
            break
    
    mod_base_lower = mod_base.lower()
    
    # Result contains our base name
    if mod_base_lower in rn:
        len_ratio = min(len(mod_base_lower), len(rn)) / max(len(mod_base_lower), len(rn))
        return 0.7 + (0.2 * len_ratio)
    
    # Word overlap
    mod_words = set(re.findall(r"[a-z0-9]+", mod_base_lower))
    result_words = set(re.findall(r"[a-z0-9]+", rn))
    skip = {"plus", "v", "agf", "zzzagf", "mod", "the", "a", "an", "and", "or", "of", "for", "to", "in"}
    mod_words -= skip
    result_words -= skip
    if mod_words and result_words:
        overlap = mod_words & result_words
        recall = len(overlap) / len(mod_words) if mod_words else 0
        precision = len(overlap) / len(result_words) if result_words else 0
        if recall > 0 and precision > 0:
            f1 = 2 * (precision * recall) / (precision + recall)
            return f1 * 0.8
    return 0.0


def generate_search_keywords(name: str) -> List[str]:
    """Generate search keywords from a base mod name."""
    n = name.strip()
    for prefix in ("zzzAGF-", "AGF-"):
        if n.startswith(prefix):
            n = n[len(prefix):]
            break
    parts = [p for p in re.split(r"[-_/\\]+", n) if p]
    keywords = []
    full_display = " ".join(parts)
    if full_display:
        keywords.append(full_display)
    keywords.append(name)
    skip_words = {"plus", "v", "mod", "agf", "zzzagf"}
    significant = [p for p in parts if p.lower() not in skip_words and len(p) > 1]
    if significant:
        keywords.append(" ".join(significant))
    if len(parts) >= 2:
        keywords.append(" ".join(parts[:2]))
    if len(parts) >= 3:
        keywords.append(" ".join(parts[:3]))
    return keywords


# ── Main audit ───────────────────────────────────────────────────────────

def main():
    api_key = os.getenv(API_KEY_ENV_VAR, "").strip()
    if not api_key:
        print("Nexus API key is not configured.")
        print("Paste it below for this check only; it will not be displayed or saved.")
        try:
            api_key = getpass.getpass("Nexus API key: ").strip()
        except (EOFError, KeyboardInterrupt):
            print("\nVersion check cancelled.")
            return 1
    if not api_key:
        print("No API key entered; version check cancelled.")
        return 1
    HEADERS["apikey"] = api_key

    # Load config
    config = json.load(open(CONFIG_PATH, "r")) if os.path.isfile(CONFIG_PATH) else {"mods": {}}
    config_mods = config.get("mods", {})
    if not isinstance(config_mods, dict):
        config_mods = {}
    
    # Scan all release source mods
    mods: List[dict] = []
    for entry in sorted(os.listdir(RELEASE_SOURCE_DIR)):
        fp = os.path.join(RELEASE_SOURCE_DIR, entry)
        if not os.path.isdir(fp):
            continue
        if not entry.startswith(("AGF-", "zzzAGF-")):
            continue
        
        base_name = get_base_mod_name(entry)
        local_version = parse_local_version(fp, entry)
        
        # Check config
        config_entry = config_mods.get(base_name, {})
        if not isinstance(config_entry, dict):
            config_entry = {}
        nexus_mod_id = safe_int(config_entry.get("nexus_mod_id", 0))
        
        mods.append({
            "folder": entry,
            "base_name": base_name,
            "local_version": local_version,
            "configured": nexus_mod_id > 0,
            "nexus_mod_id": nexus_mod_id,
            "config_entry": config_entry,
        })
    
    total = len(mods)
    print(f"\n{'='*100}")
    print(f"  NEXUS AUDIT: Scanning {total} AGF release source mods against Nexus")
    print(f"{'='*100}")
    
    # ── Phase 1: Fetch live data for already-configured mods ──
    print(f"\n── Phase 1: Fetching live data for {sum(1 for m in mods if m['configured'])} configured mods ──\n")
    for m in mods:
        if not m["configured"]:
            continue
        nid = m["nexus_mod_id"]
        print(f"  Fetching mod_id={nid} ({m['base_name']})...", end=" ")
        try:
            live = fetch_known_mod_info(nid)
            live_version = str(live.get("version", live.get("mod_version", ""))).strip() or "?"
            live_name = str(live.get("name", "")).strip() or "?"
            m["live_version"] = live_version
            m["live_name"] = live_name
            m["live_id"] = str(live.get("id", live.get("mod_id", nid))).strip()
            print(f"OK — '{live_name}' v{live_version}")
        except Exception as ex:
            m["live_version"] = "ERROR"
            m["live_name"] = "ERROR"
            print(f"FAILED: {ex}")
    
    # ── Phase 2: Broad search for unconfigured mods ──
    unconfigured = [m for m in mods if not m["configured"]]
    print(f"\n── Phase 2: Searching Nexus for {len(unconfigured)} unconfigured mods ──\n")
    
    # Broad search first
    broad_results = []
    for q in ("AGF", "AuroraGiggleFairy"):
        print(f"  Broad search for '{q}'...", end=" ")
        try:
            results = search_nexus(q)
            broad_results.extend(results)
            print(f"{len(results)} results")
        except Exception as ex:
            print(f"ERROR: {ex}")
    
    # Deduplicate broad results
    seen_ids: set = set()
    unique_broad: List[dict] = []
    for r in broad_results:
        rid = safe_int(r.get("mod_id", r.get("id", 0)))
        if rid > 0 and rid not in seen_ids:
            seen_ids.add(rid)
            unique_broad.append(r)
    
    print(f"  Total unique broad results: {len(unique_broad)}")
    
    # Try to match broad results to our mods
    broad_matched: Dict[str, dict] = {}
    for r in unique_broad:
        rname = str(r.get("name", "")).strip()
        rid = safe_int(r.get("mod_id", r.get("id", 0)))
        rversion = str(r.get("version", r.get("mod_version", ""))).strip()
        for m in unconfigured:
            if m["base_name"] in broad_matched:
                continue
            score = score_match(rname, m["base_name"])
            if score >= 0.7:
                broad_matched[m["base_name"]] = {
                    "nexus_mod_id": rid,
                    "live_name": rname,
                    "live_version": rversion,
                    "score": score,
                }
                print(f"  [BROAD MATCH] {m['base_name']} -> '{rname}' (id={rid}, score={score:.2f})")
    
    # For mods not matched by broad search, do granular search
    still_needed = [m for m in unconfigured if m["base_name"] not in broad_matched]
    for m in still_needed:
        keywords = generate_search_keywords(m["base_name"])
        best_score = 0.0
        best_result = None
        for kw in keywords:
            results = search_nexus(kw)
            for r in results:
                if not isinstance(r, dict):
                    continue
                score = score_match(str(r.get("name", "")).strip(), m["base_name"])
                if score > best_score:
                    best_score = score
                    best_result = r
            if best_score >= 0.8:
                break
            # Sleep briefly to avoid rate limiting
            import time
            time.sleep(0.5)
        
        if best_result and best_score >= 0.5:
            rname = str(best_result.get("name", "")).strip()
            rid = safe_int(best_result.get("mod_id", best_result.get("id", 0)))
            rversion = str(best_result.get("version", best_result.get("mod_version", ""))).strip()
            broad_matched[m["base_name"]] = {
                "nexus_mod_id": rid,
                "live_name": rname,
                "live_version": rversion,
                "score": best_score,
            }
            label = "STRONG" if best_score >= 0.8 else "POSSIBLE"
            print(f"  [{label}] {m['base_name']} -> '{rname}' (id={rid}, v{rversion}, score={best_score:.2f})")
        else:
            print(f"  [NOT FOUND] {m['base_name']} (no results matching AGF author)")
    
    # Apply broad matches to mod entries
    for m in unconfigured:
        if m["base_name"] in broad_matched:
            m["nexus_mod_id"] = broad_matched[m["base_name"]]["nexus_mod_id"]
            m["live_version"] = broad_matched[m["base_name"]]["live_version"]
            m["live_name"] = broad_matched[m["base_name"]]["live_name"]
            m["found"] = True
            m["score"] = broad_matched[m["base_name"]]["score"]
        else:
            m["found"] = False
            m["live_version"] = "—"
            m["live_name"] = "—"
    
    # ── Phase 3: Generate report ──
    print(f"\n{'='*100}")
    print(f"  AUDIT REPORT — {total} AGF Release Source Mods vs Nexus")
    print(f"{'='*100}")
    
    print(f"\n{'Mod Name':<50} {'Local Ver':<12} {'On Nexus?':<12} {'Nexus ID':<12} {'Nexus Ver':<14} {'Status'}")
    print("-"*120)
    
    configured_count = 0
    found_count = 0
    not_found_count = 0
    needs_update_count = 0
    out_of_date_count = 0
    in_sync_count = 0
    
    for m in mods:
        base_name = m["base_name"]
        local_ver = m["local_version"]
        
        if m["configured"]:
            configured_count += 1
            on_nexus = "✅ YES"
            nexus_id = str(m["nexus_mod_id"])
            nexus_ver = m.get("live_version", "?")
            
            if nexus_ver and nexus_ver != "?" and nexus_ver != "ERROR":
                cmp = compare_versions(local_ver, nexus_ver)
                if cmp > 0:
                    status = "⬆ LOCAL NEWER (ready to publish)"
                    needs_update_count += 1
                elif cmp < 0:
                    status = "⬇ NEXUS NEWER (pull from Nexus)"
                    out_of_date_count += 1
                else:
                    status = "✅ IN SYNC"
                    in_sync_count += 1
            else:
                status = "⚠ FETCH ERROR"
                out_of_date_count += 1
        
        elif m.get("found"):
            found_count += 1
            on_nexus = "✓ YES"
            nexus_id = str(m["nexus_mod_id"])
            nexus_ver = str(m.get("live_version", "?"))
            score = m.get("score", 0)
            score_label = "✓" if score >= 0.8 else "⚠"
            status = f"{score_label} NOT IN CONFIG (score={score:.2f})"
            needs_update_count += 1
        
        else:
            not_found_count += 1
            on_nexus = "❌ NO"
            nexus_id = "—"
            nexus_ver = "—"
            status = "NOT ON NEXUS"
        
        print(f"{base_name:<50} {local_ver:<12} {on_nexus:<12} {nexus_id:<12} {nexus_ver:<14} {status}")
    
    print("-"*120)
    print(f"\n  SUMMARY:")
    print(f"    Total release source mods:     {total}")
    print(f"    Already configured in config:  {configured_count}")
    print(f"    Found on Nexus (unconfigured): {found_count}")
    print(f"    NOT found on Nexus:            {not_found_count}")
    print(f"    Local newer than Nexus (ready): {needs_update_count}")
    print(f"    In sync with Nexus:             {in_sync_count}")
    print(f"    Needs attention:                {out_of_date_count}")
    print(f"\n  Total on Nexus (configured + discovered): {configured_count + found_count}")
    print(f"  Total NOT on Nexus: {not_found_count}")
    
    # ── Detailed tables ──
    print(f"\n{'='*100}")
    print(f"  DETAIL: Mods found on Nexus but NOT in config.json")
    print(f"{'='*100}\n")
    for m in mods:
        if not m["configured"] and m.get("found"):
            score = m.get("score", 0)
            rid = m["nexus_mod_id"]
            rname = m.get("live_name", "?")
            rver = m.get("live_version", "?")
            print(f"  {m['base_name']:<45} → Nexus mod_id={rid:<6} | '{rname}' | v{rver} | score={score:.2f}")
            print(f"    Recommended config:")
            print(f'    "{m["base_name"]}": {{')
            print(f'      "intent": "update",')
            print(f'      "nexus_mod_id": {rid},')
            print(f'      "page_url": "https://www.nexusmods.com/{GAME_DOMAIN}/mods/{rid}"')
            print(f'    }}')
            print()
    
    # ── Version comparison ──
    print(f"\n{'='*100}")
    print(f"  DETAIL: Version comparison for configured mods")
    print(f"{'='*100}\n")
    for m in mods:
        if not m["configured"]:
            continue
        lv = m["local_version"]
        nv = m.get("live_version", "?")
        nid = m["nexus_mod_id"]
        print(f"  {m['base_name']:<45} local={lv:<10} nexus={nv:<10} (id={nid})")
    
    print(f"\n{'='*100}")
    print(f"  MODS NOT ON NEXUS:")
    print(f"{'='*100}\n")
    for m in mods:
        if not m.get("found") and not m["configured"]:
            print(f"  • {m['base_name']} v{m['local_version']}")
    
    print()
    return 0


if __name__ == "__main__":
    sys.exit(main())