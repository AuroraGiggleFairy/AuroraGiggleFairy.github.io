"""
Turn a 7D2D Player.log into a short markdown record.

Usage:
  py SCRIPT-LogReader.py "C:\\path\\Player.log"
"""
from __future__ import annotations

import argparse
import io
import re
import shutil
import urllib.error
import urllib.request
import zipfile
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path


HERE = Path(__file__).resolve().parent
KNOWLEDGE_PATH = HERE / "Knowledge.md"
RECORDS_DIR = HERE / "Records"
RAW_DIR = RECORDS_DIR / "raw"
INDEX_PATH = RECORDS_DIR / "_index.md"

TS_RE = re.compile(r"^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})\s+([\d.]+)\s+(INF|WRN|ERR|EXC)\s+(.*)$")
WHO_RE = re.compile(r"[\\/]Users[\\/]([^\\/]+)[\\/]AppData", re.I)
LOADED_MOD_RE = re.compile(r"Loaded Mod:\s+(.+?)\s+\((.*)\)\s*$")
FOLDER_RE = re.compile(r"Trying to load from folder:\s+'([^']+)'")
SOURCE_RE = re.compile(r"Start loading from:\s+'([^']+)'")
INIT_RE = re.compile(r"Initializing mod\s+(.+)\s*$")
GMSG_RE = re.compile(r"GMSG: Player '([^']+)'")
VERSION_RE = re.compile(r"Version:\s+(V\s+[^\s]+(?:\s+\([^)]+\))?).*Build:\s+(.+)$")
WORLD_RE = re.compile(r"createWorld:\s+(.+?),\s+.+?\(src:[^)]+\),\s+(.+?),\s+GameMode")
EXC_RE = re.compile(r"^(\w+Exception(?:`\d+)?:.*)$")
KBLOCK_RE = re.compile(r"^---\s*$", re.M)
CUSTOM_MOD_RE = re.compile(r"^\[([^\]]+)\]\s+(.*)$")
LOOT_OPEN_RE = re.compile(
    r"Loot (?:OpenLooting|openContainer):\s+name=(.+?),\s+lootList=([^,]+).*"
    r"backendSlots=(\d+),\s+visibleSlots=(\d+)",
    re.I,
)
COLOR_TAG_RE = re.compile(r"\[[^\]]*\]")

LOOT_EXC_NEEDLES = (
    "XUiC_Loot",
    "XUiC_ContainerStandardControls",
    "TEFeatureStorage",
    "LootWindow",
    "LootContainer",
)


@dataclass
class Knowledge:
    id: str
    match: str
    kind: str
    severity: str
    meaning: str
    cause: str = ""
    suspects: str = ""
    confidence: str = ""
    fixed_mod: str = ""
    fixed_in: str = ""
    fix_note: str = ""
    area: str = ""
    needs_mods: str = ""


@dataclass
class ModTry:
    folder: str
    source: str
    name: str = ""
    version: str = ""
    status: str = "unknown"
    note: str = ""
    has_dll: bool = False


@dataclass
class LootOpen:
    source: str
    name: str
    loot_list: str
    backend: int
    visible: int


@dataclass
class Issue:
    kind: str
    text: str
    first_ts: str = ""
    count: int = 1
    kid: str = ""
    severity: str = "unknown"
    meaning: str = ""
    cause: str = ""
    suspects: str = ""
    confidence: str = ""
    fixed_mod: str = ""
    fixed_in: str = ""
    fix_note: str = ""
    area: str = ""
    action: str = ""
    context: str = ""
    slots_note: str = ""


@dataclass
class Parsed:
    who: str = "unknown"
    ingame: str = ""
    first_ts: str = ""
    last_ts: str = ""
    version: str = ""
    build: str = ""
    eac: str = "unknown"
    session: str = ""
    world: str = ""
    save: str = ""
    sources: list[str] = field(default_factory=list)
    mods: list[ModTry] = field(default_factory=list)
    issues: list[Issue] = field(default_factory=list)
    source_log: str = ""


def read_text(path: Path) -> str:
    data = path.read_bytes()
    for enc in ("utf-8-sig", "utf-16", "utf-16-le", "cp1252"):
        try:
            return data.decode(enc)
        except UnicodeDecodeError:
            continue
    return data.decode("utf-8", errors="replace")


def load_knowledge() -> list[Knowledge]:
    if not KNOWLEDGE_PATH.is_file():
        return []
    text = KNOWLEDGE_PATH.read_text(encoding="utf-8")
    parts = KBLOCK_RE.split(text)
    out: list[Knowledge] = []
    for part in parts:
        fields = {}
        for line in part.splitlines():
            if ":" not in line:
                continue
            key, val = line.split(":", 1)
            key = key.strip().lower()
            if key in {
                "id",
                "match",
                "kind",
                "severity",
                "meaning",
                "cause",
                "suspects",
                "confidence",
                "fixed_mod",
                "fixed_in",
                "fix_note",
                "area",
                "needs_mods",
            }:
                fields[key] = val.strip()
        if {"id", "match", "meaning"} <= fields.keys():
            out.append(
                Knowledge(
                    id=fields["id"],
                    match=fields["match"],
                    kind=fields.get("kind", "ANY"),
                    severity=fields.get("severity", "look"),
                    meaning=fields["meaning"],
                    cause=fields.get("cause", ""),
                    suspects=fields.get("suspects", ""),
                    confidence=fields.get("confidence", ""),
                    fixed_mod=fields.get("fixed_mod", ""),
                    fixed_in=fields.get("fixed_in", ""),
                    fix_note=fields.get("fix_note", ""),
                    area=fields.get("area", ""),
                    needs_mods=fields.get("needs_mods", ""),
                )
            )
    return out


def loaded_has(mods: list[ModTry], needle: str) -> bool:
    key = compact_alnum(needle)
    if not key:
        return False
    return any(
        key in compact_alnum(f"{mod.name} {mod.folder}")
        for mod in mods
        if mod.status == "loaded"
    )


def mods_satisfy(needs: str, mods: list[ModTry]) -> bool:
    if not needs:
        return True
    return all(
        loaded_has(mods, part.strip())
        for part in needs.split(",")
        if part.strip()
    )


def annotate(
    kind: str,
    text: str,
    catalog: list[Knowledge],
    mods: list[ModTry] | None = None,
) -> Knowledge | None:
    mods = mods or []
    for item in catalog:
        if item.kind not in {"ANY", kind}:
            continue
        if item.needs_mods and not mods_satisfy(item.needs_mods, mods):
            continue
        if item.match.lower() in text.lower():
            return item
    if kind == "EXC" or re.match(r"^\w+Exception", text):
        return Knowledge(
            id="",
            match="",
            kind="EXC",
            severity="error",
            meaning="The game hit an exception. This can break a feature or the session.",
            cause="Unknown red error. The stack below is the crash site.",
            confidence="Low until this pattern is added to Knowledge.md.",
        )
    if "XML patch for" in text:
        return Knowledge(
            id="",
            match="",
            kind="WRN",
            severity="conflict",
            meaning="An XML patch missed.",
        )
    return None


def clean_path(path: str) -> str:
    text = path.replace("/", "\\")
    while True:
        nxt = re.sub(r"[^\\]+\\\.\.\\", "", text, count=1)
        if nxt == text:
            return text
        text = nxt


def guess_who(text: str, sources: list[str]) -> str:
    for src in sources:
        m = WHO_RE.search(src)
        if m:
            return m.group(1)
    m = WHO_RE.search(text)
    return m.group(1) if m else "unknown"


def normalize_issue(text: str) -> str:
    t = re.sub(r"C:[\\/][^\s\]]+", "<path>", text)
    t = re.sub(r"\b\d+\.\d+s\b", "<n>s", t)
    t = re.sub(r":\s+\d+\s+ms\b", ": <n> ms", t)
    t = re.sub(r"CacheFilename=\[[^\]]+\]", "CacheFilename=<file>", t)
    t = re.sub(r"line \d+ at pos \d+", "line <n> at pos <n>", t)
    return t.strip()


XUI_MISMATCH_NOTE = "xui mismatches (see log for specifics)"
XUI_MISMATCH_MIN = 20


def is_xui_mismatch(text: str) -> bool:
    head = text.splitlines()[0] if text else ""
    if "[XUi]" not in head:
        return False
    return (
        "Can not parse input" in head
        or "Exception parsing result of binding" in head
        or "Failed initializing window group" in head
    )


def xui_mismatch_total(issues: list[Issue]) -> int:
    return sum(issue.count for issue in issues if is_xui_mismatch(issue.text))


def mod_has_xui(mod: ModTry) -> bool:
    if mod.status != "loaded" or not mod.source or not mod.folder:
        return False
    config = Path(mod.source) / mod.folder / "Config"
    if not config.is_dir():
        return False
    for xml in config.rglob("*.xml"):
        if "xui" in xml.as_posix().lower():
            return True
    return False


RECT_VISIBLE_FALSE = re.compile(
    r"<rect\b[^>]*\bvisible\s*=\s*[\"']false[\"']",
    re.I,
)


def visible_false_rect_mods(mods: list[ModTry]) -> list[str]:
    """Loaded mods whose XUi sets a literal visible=false on a rect."""
    found: list[str] = []
    for mod in mods:
        if mod.status != "loaded" or not mod.source or not mod.folder:
            continue
        config = Path(mod.source) / mod.folder / "Config"
        if not config.is_dir():
            continue
        names: list[str] = []
        files: list[str] = []
        for xml in config.rglob("*.xml"):
            if "xui" not in xml.as_posix().lower():
                continue
            try:
                text = xml.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            for match in RECT_VISIBLE_FALSE.finditer(text):
                name_m = re.search(r"\bname\s*=\s*[\"']([^\"']+)", match.group(0))
                names.append(name_m.group(1) if name_m else "unnamed rect")
                if xml.name not in files:
                    files.append(xml.name)
        if not names:
            continue
        label = mod.name or mod.folder
        ver = f" {mod.version}" if mod.version else ""
        shown = ", ".join(names[:4])
        where = ", ".join(files)
        found.append(f"{label}{ver} — rect {shown} in {where}")
    return found


def is_xui_cascade_error(issue: Issue) -> bool:
    blob = issue.text
    return any(
        needle in blob
        for needle in ("XUi", "ParsingMethodCache", "BindingInfo", "XUiFromXml")
    )


def issue_kind(issue: Issue) -> str:
    if is_xui_mismatch(issue.text) or is_xui_cascade_error(issue):
        return "XUi"
    head = issue.text.splitlines()[0] if issue.text else ""
    if "Xbox" in head or "XBL" in head or "HResult" in head:
        return "Xbox Live"
    if "EOS" in head:
        return "EOS"
    title = re.sub(r"^\[[^\]]+\]\s*", "", head.split(":", 1)[0]).strip()
    return title or "Other"


def later_issues(first: Issue, issues: list[Issue]) -> list[Issue]:
    first_ts = first.first_ts or ""
    out = []
    for issue in issues:
        if issue is first or issue.severity not in {"error", "unknown"}:
            continue
        ts = issue.first_ts or ""
        if ts and first_ts and ts < first_ts:
            continue
        out.append(issue)
    out.sort(key=lambda issue: issue.first_ts or "9999")
    return out


def later_kind_lines(issues: list[Issue]) -> list[str]:
    seen: list[str] = []
    for issue in issues:
        kind = issue_kind(issue)
        if kind not in seen:
            seen.append(kind)
    return [
        f"- Later {kind} errors may be resolved after fixing the first error."
        for kind in seen
    ]


def xui_mismatch_mods(mods: list[ModTry]) -> list[str]:
    names = []
    for mod in mods:
        if not mod_has_xui(mod):
            continue
        label = mod.name or mod.folder
        if mod.version:
            label = f"{label} ({mod.version})"
        names.append(label)
    return names


def compact_alnum(text: str) -> str:
    return re.sub(r"[^a-z0-9]+", "", text.lower())


def parse_loot_open(line: str) -> LootOpen | None:
    cm = CUSTOM_MOD_RE.match(line)
    if not cm:
        return None
    loot = LOOT_OPEN_RE.search(cm.group(2))
    if not loot:
        return None
    name = COLOR_TAG_RE.sub("", loot.group(1)).strip()
    return LootOpen(
        source=cm.group(1),
        name=name,
        loot_list=loot.group(2).strip(),
        backend=int(loot.group(3)),
        visible=int(loot.group(4)),
    )


def is_loot_exception(text: str) -> bool:
    return any(needle in text for needle in LOOT_EXC_NEEDLES)


ACTION_HINTS = (
    (
        ("XUiC_ContainerStandardControls:Sort()", "OnClick"),
        "Clicked Sort on a loot/storage container",
    ),
    (
        ("XUiC_ContainerStandardControls:Sort()", "Pressed"),
        "Clicked Sort on a loot/storage container",
    ),
    (("XUiC_BagContainer", "btnSort_OnPress"), "Clicked Sort on the backpack"),
)


def guess_action(text: str) -> str:
    for needles, label in ACTION_HINTS:
        if all(needle in text for needle in needles):
            return label
    return ""


def specialize_action(action: str, loot: LootOpen | None) -> str:
    if not action:
        return ""
    if loot and loot.name and "Sort" in action and "backpack" not in action.lower():
        return f"Clicked Sort on {loot.name}"
    return action


def loot_context_line(loot: LootOpen) -> str:
    return (
        f"Last opened container: {loot.name} "
        f"(lootList={loot.loot_list}, {loot.backend} backend slots, {loot.visible} visible) "
        f"logged by [{loot.source}]"
    )


def merge_suspects(existing: str, extra: list[str]) -> str:
    found = [part.strip() for part in existing.split(",") if part.strip()]
    seen = {compact_alnum(part) for part in found}
    for frag in extra:
        key = compact_alnum(frag)
        if not key or key in seen:
            continue
        found.append(frag)
        seen.add(key)
    return ", ".join(found)


def parse_log(path: Path, catalog: list[Knowledge]) -> Parsed:
    text = read_text(path)
    lines = text.splitlines()
    p = Parsed(source_log=str(path))
    current: ModTry | None = None
    pending_exc: list[str] = []
    pending_exc_ts = ""
    last_loot: LootOpen | None = None
    issue_map: dict[tuple[str, str], Issue] = {}

    def flush_exc() -> None:
        nonlocal pending_exc, pending_exc_ts
        if not pending_exc:
            return
        add_issue("EXC", "\n".join(pending_exc), pending_exc_ts)
        pending_exc = []
        pending_exc_ts = ""

    def add_issue(kind: str, text: str, ts: str) -> None:
        key = (kind, normalize_issue(text))
        if key in issue_map:
            issue_map[key].count += 1
            return
        item = annotate(kind, text, catalog, p.mods)
        issue = Issue(
            kind=kind,
            text=text,
            first_ts=ts,
            kid=item.id if item else "",
            severity=item.severity if item else "unknown",
            meaning=item.meaning if item else "",
            cause=item.cause if item else "",
            suspects=item.suspects if item else "",
            confidence=item.confidence if item else "",
            fixed_mod=item.fixed_mod if item else "",
            fixed_in=item.fixed_in if item else "",
            fix_note=item.fix_note if item else "",
            area=item.area if item else "",
        )
        if kind == "EXC":
            use_loot = last_loot if last_loot and is_loot_exception(text) else None
            issue.action = specialize_action(guess_action(text), use_loot)
            if use_loot:
                issue.context = loot_context_line(use_loot)
                issue.slots_note = (
                    f"Container had {use_loot.backend} backend slots and "
                    f"{use_loot.visible} visible slots"
                )
                issue.suspects = merge_suspects(
                    issue.suspects,
                    [use_loot.source, use_loot.name, use_loot.loot_list],
                )
        issue_map[key] = issue

    for raw in lines:
        line = raw.rstrip("\r")
        ts_m = TS_RE.match(line)
        if ts_m:
            flush_exc()
            ts, _elapsed, level, msg = ts_m.groups()
            if not p.first_ts:
                p.first_ts = ts
            p.last_ts = ts

            if msg.startswith("Version:") and not p.version:
                vm = VERSION_RE.search(msg)
                if vm:
                    p.version = vm.group(1).strip()
                    p.build = vm.group(2).strip()

            if "Not started with EAC" in msg or "EACEnabled = False" in msg:
                p.eac = "off"
            elif "AntiCheat enabled, mod skipped" in msg or "ClientAntiCheatEnabled" in msg:
                p.eac = "on"

            if "Starting offline server" in msg:
                p.session = "Singleplayer"
            elif "StartAsClient" in msg:
                p.session = "Joining Client"
            elif "Starting server protocols" in msg or "LiteNetLib server started" in msg:
                p.session = "Hosting"

            sm = SOURCE_RE.search(msg)
            if sm:
                src = clean_path(sm.group(1))
                if src not in p.sources:
                    p.sources.append(src)
                current = None
                continue

            fm = FOLDER_RE.search(msg)
            if fm:
                current = ModTry(
                    folder=fm.group(1),
                    source=p.sources[-1] if p.sources else "",
                )
                p.mods.append(current)
                continue

            if current is not None:
                if "Loaded assembly" in msg:
                    current.has_dll = True
                lm = LOADED_MOD_RE.search(msg)
                if lm:
                    current.name = lm.group(1)
                    current.version = lm.group(2)
                    current.status = "loaded"
                    continue
                if "already loaded, ignoring" in msg:
                    current.status = "ignored-duplicate"
                    nm = re.search(r"same name \(([^)]+)\)", msg)
                    current.note = nm.group(1) if nm else "duplicate name"
                    continue
                if "does not contain a ModInfo.xml" in msg:
                    current.status = "ignored-no-modinfo"
                    current.note = "no ModInfo.xml"
                    continue
                if "AntiCheat enabled, mod skipped" in msg:
                    current.status = "skipped-eac"
                    current.note = "SkipWithAntiCheat"
                    continue
                if "AntiCheat needs to be disabled" in msg:
                    current.status = "skipped-eac-dll"
                    current.note = "DLL blocked by EAC"
                    continue
                if "Failed loading DLL" in msg or "Failed loading mod from folder" in msg:
                    current.status = "failed"
                    current.note = msg.strip()
                    continue
                if "in legacy format" in msg or "Could not parse" in msg:
                    current.status = "failed"
                    current.note = msg.strip()
                    continue

            im = INIT_RE.search(msg)
            if im:
                name = im.group(1).strip()
                for mod in p.mods:
                    if mod.name == name and not mod.note.startswith("inited"):
                        if mod.status == "loaded":
                            mod.note = "dll-init"
                        break

            wm = WORLD_RE.search(msg)
            if wm and not p.world:
                p.world = wm.group(1).strip()
                p.save = wm.group(2).strip()

            gm = GMSG_RE.search(msg)
            if gm and not p.ingame:
                p.ingame = gm.group(1)

            if "Player registered:" in msg:
                nm = re.search(r"name=([^,\]]+)", msg)
                if nm:
                    p.ingame = nm.group(1).strip()

            if level == "EXC":
                pending_exc = [msg]
                pending_exc_ts = ts
                continue

            if level in {"WRN", "ERR"}:
                add_issue(level, msg, ts)
            continue

        if EXC_RE.match(line) or line.startswith("NullReferenceException"):
            flush_exc()
            pending_exc = [line]
            pending_exc_ts = p.last_ts
            continue
        if pending_exc and (
            line.startswith("  at ") or line.startswith("   at ") or line.startswith("\tat ")
        ):
            pending_exc.append(line)
            continue
        if pending_exc and not line.strip():
            flush_exc()
            continue

        if line.startswith("The referenced script on this Behaviour"):
            add_issue("UNITY", line, p.last_ts)
        elif "is not readable, so Texture2D" in line:
            add_issue("UNITY", line, p.last_ts)
        elif line.startswith("UnloadAsset can only be used"):
            add_issue("UNITY", line, p.last_ts)
        elif line.startswith("Addressables.Release was called"):
            add_issue("UNITY", line, p.last_ts)
        elif line.startswith("Cannot Prepare a disabled VideoPlayer"):
            add_issue("UNITY", line, p.last_ts)
        elif line.startswith("Fallback handler could not load library"):
            add_issue("UNITY", line, p.last_ts)
        else:
            loot = parse_loot_open(line)
            if loot:
                last_loot = loot

    flush_exc()
    p.who = guess_who(text, p.sources)
    p.issues = list(issue_map.values())

    gp_eac = re.search(r"GamePref\.EACEnabled\s*=\s*(True|False)", text)
    if gp_eac:
        p.eac = "off" if gp_eac.group(1) == "False" else "on"
    gw = re.search(r"GamePref\.GameWorld\s*=\s*(.+)", text)
    gn = re.search(r"GamePref\.GameName\s*=\s*(.+)", text)
    if gw:
        p.world = gw.group(1).strip()
    if gn:
        p.save = gn.group(1).strip()
    return p


def parse_ts(ts: str) -> datetime | None:
    if not ts:
        return None
    try:
        return datetime.strptime(ts, "%Y-%m-%dT%H:%M:%S")
    except ValueError:
        return None


def written_day(dt: datetime) -> str:
    return f"{dt.strftime('%B')} {dt.day}, {dt.year}"


def written_when(first_ts: str, last_ts: str) -> str:
    start = parse_ts(first_ts)
    end = parse_ts(last_ts)
    if not start:
        return "unknown"
    start_clock = start.strftime("%H:%M")
    if not end:
        return f"{written_day(start)}, {start_clock}"
    end_clock = end.strftime("%H:%M")
    if start.date() == end.date():
        if start_clock == end_clock:
            return f"{written_day(start)}, {start_clock}"
        return f"{written_day(start)}, {start_clock}-{end_clock}"
    return f"{written_day(start)}, {start_clock} - {written_day(end)}, {end_clock}"


def nice_ts(ts: str) -> str:
    dt = parse_ts(ts)
    if not dt:
        return "unknown"
    return f"{written_day(dt)}, {dt.strftime('%H:%M')}"


def short_game(version: str) -> str:
    if not version:
        return "unknown"
    return version.replace("V ", "", 1).strip()


def short_head(text: str, limit: int = 90) -> str:
    head = text.splitlines()[0].strip()
    if len(head) > limit:
        return head[: limit - 3] + "..."
    return head


def file_stem(p: Parsed) -> str:
    who = re.sub(r"[^A-Za-z0-9._-]+", "_", p.who or "unknown")
    if p.first_ts:
        stamp = p.first_ts.replace("T", "_").replace(":", "")
    else:
        stamp = datetime.now().strftime("%Y-%m-%d_%H%M%S")
    return f"{who}_{stamp}"


LABEL_COLOR = "skyblue"
ISSUE_COLOR = "crimson"
WARN_COLOR = "gold"
RECOMMENDED_MODS = r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods"
DISCORD_RULE = "--------------------------------------------------------------------"
APP_DATA_RE = re.compile(r"[A-Za-z]:\\Users\\[^\\]+\\AppData", re.I)


def public_path(path: str) -> str:
    if not path:
        return path
    text = path.replace("/", "\\")
    return APP_DATA_RE.sub(r"...\\AppData", text)


def where(mod: ModTry) -> str:
    return public_path(mod.source) if mod.source else "unknown Mods folder"


def color_label(name: str) -> str:
    return f'<span style="color:{LABEL_COLOR}"><strong>{name}:</strong></span>'


def color_issue(text: str) -> str:
    return f'<span style="color:{ISSUE_COLOR}"><strong>{text}</strong></span>'


def color_warn(text: str) -> str:
    return f'<span style="color:{WARN_COLOR}"><strong>{text}</strong></span>'


REPO_ROOT = HERE.parents[1]
ACTIVEBUILD_DIR = REPO_ROOT / "02_ActiveBuild"
DOWNLOAD_ZIP_BASE = (
    "https://github.com/AuroraGiggleFairy/AuroraGiggleFairy.github.io/raw/main/04_DownloadZips"
)

# Old ModInfo <Name> → current ActiveBuild name (zip / latest lookup).
# BEGIN NAME MAP
AGF_NAME_ALIASES = {
    "0AGF-LawnTractorPatchGuard": "AGF-LawnTractorPatchGuard",
    "AGF-4Modders-ESCWindowPlus": "AGF-ESCWindowPlus",
    "AGF-4Modders-Fix4DestroyBiomeBadge": "AGF-Fix4DestroyBiomeBadge",
    "AGF-4Modders-Fix4PerkPageReset": "AGF-Fix4PerkPageReset",
    "AGF-HUDPlus-1Main": "AGF-1HUDPlus",
    "AGF-HUDPlus-BMCounter": "AGF-2BMCounter",
    "AGF-HUDPlus-PurpleBook": "AGF-2PurpleBook",
    "AGF-HUDPlus-RemoveEnteringPopUp": "AGF-2RemoveEnteringPopUp",
    "AGF-HUDPlus-VisualEntityTracker": "AGF-2VisualEntityTracker",
    "AGF-HUDPlus-Weekday": "AGF-2Weekday",
    "AGF-NoEAC-AudioOptionsPlus": "AGF-AudioOptionsPlus",
    "AGF-NoEAC-AutoRun": "AGF-AutoRun",
    "AGF-NoEAC-CombatGlitchMitigations": "AGF-CombatGlitchMitigations",
    "AGF-NoEAC-ConsoleOpacityMod": "AGF-ConsoleOpacityMod",
    "AGF-NoEAC-CosmeticLockIcon": "AGF-CosmeticLockIcon",
    "AGF-NoEAC-EnhancedAGF": "AGF-EnhancedAGF",
    "AGF-NoEAC-FuelAutoShutOff": "AGF-FuelAutoShutOff",
    "AGF-NoEAC-GlobalStormTracker": "AGF-GlobalStormTracker",
    "AGF-NoEAC-GyroFlightModes": "AGF-GyroFlightModes",
    "AGF-NoEAC-HideDLCCosmetics": "AGF-HideDLCCosmetics",
    "AGF-NoEAC-MapPlus": "AGF-MapPlus",
    "AGF-NoEAC-ModSync": "AGF-ModSync",
    "AGF-NoEAC-MultiLookStorage": "AGF-MultiLookStorage",
    "AGF-NoEAC-OpenAllButton": "AGF-OpenAllButton",
    "AGF-NoEAC-PartyGroupPlus": "AGF-PartyGroupPlus",
    "AGF-NoEAC-QuartermasterCrafting": "AGF-QuartermasterCrafting",
    "AGF-NoEAC-ScreamerAlert": "AGF-ScreamerAlert",
    "AGF-NoEAC-SmeltTimerOption": "AGF-SmeltTimerOption",
    "AGF-NoEAC-StorageLaptop": "AGF-StorageLaptop",
    "AGF-NoEAC-Toolbelt12Slots": "AGF-Toolbelt12Slots",
    "AGF-NoEAC-VisualEntityTrackerAddon": "AGF-3VisualEntityTrackerAddon",
    "AGF-Requested-AnimalTrackerAlwaysOn": "AGF-AnimalTrackerAlwaysOn",
    "AGF-Requested-SmallerInteractionPrompt": "AGF-SmallerInteractionPrompt",
    "AGF-Requested-TinyBuffsPopUp": "AGF-TinyBuffsPopUp",
    "AGF-VP-AdminModdingSupport": "AGF-AdminModdingSupport",
    "AGF-VP-AlternativeRecipes": "AGF-AlternativeRecipes",
    "AGF-VP-AmmoDisassembly": "AGF-AmmoDisassembly",
    "AGF-VP-ArcheryFeathersChange": "AGF-ArcheryFeathersChange",
    "AGF-VP-AutomobilesRespawn": "AGF-AutomobilesRespawn",
    "AGF-VP-BedrollPlus": "AGF-BedrollPlus",
    "AGF-VP-BetterEggChance": "AGF-BetterEggChance",
    "AGF-VP-BreakItGetIt": "AGF-BreakItGetIt",
    "AGF-VP-CraftSewingKits": "AGF-CraftSewingKits",
    "AGF-VP-CraftStackEngBattCells": "AGF-CraftStackEngBattCells",
    "AGF-VP-CraftVitamins": "AGF-CraftVitamins",
    "AGF-VP-DecorationBlock": "AGF-DecorationBlock",
    "AGF-VP-DoorsPlus": "AGF-DoorsPlus",
    "AGF-VP-DrinkableAcid": "AGF-DrinkableAcid",
    "AGF-VP-DyesPlus": "AGF-DyesPlus",
    "AGF-VP-FloraHarvester": "AGF-FloraHarvester",
    "AGF-VP-FuelBurnPlus": "AGF-FuelBurnPlus",
    "AGF-VP-LargerStorageOption": "AGF-LargerStorageOption",
    "AGF-VP-MasterTool": "AGF-MasterTool",
    "AGF-VP-MaxLevel500": "AGF-MaxLevel500",
    "AGF-VP-MiningPlus": "AGF-MiningPlus",
    "AGF-VP-Mod988": "AGF-Mod988",
    "AGF-VP-ModSlotsPlus": "AGF-ModSlotsPlus",
    "AGF-VP-PaintbrushPlus": "AGF-PaintbrushPlus",
    "AGF-VP-PickupLanternsPlus": "AGF-PickupLanternsPlus",
    "AGF-VP-PlayerResetQuests": "AGF-PlayerResetQuests",
    "AGF-VP-RebundleBundles": "AGF-RebundleBundles",
    "AGF-VP-RecipeRottingFlesh": "AGF-RecipeRottingFlesh",
    "AGF-VP-RestorePowerAnyTime": "AGF-RestorePowerAnyTime",
    "AGF-VP-ScrapBatts4Acid": "AGF-ScrapBatts4Acid",
    "AGF-VP-ScrapEquipmentFaster": "AGF-ScrapEquipmentFaster",
    "AGF-VP-SimplifiedStacks": "AGF-SimplifiedStacks",
    "AGF-VP-SmeltingPlus": "AGF-SmeltingPlus",
    "AGF-VP-StayLongerAnimalCorpse": "AGF-StayLongerAnimalCorpse",
    "AGF-VP-StayLongerPlayerBackpack": "AGF-StayLongerPlayerBackpack",
    "AGF-VP-TacticalRiflePlus": "AGF-TacticalRiflePlus",
    "AGF-VP-VehiclePerformance": "AGF-VehiclePerformance",
    "AGF-VP-VehicleStoragePlus": "AGF-VehicleStoragePlus",
    "AGF-VP-VehiclesExtraSeating": "AGF-VehiclesExtraSeating",
    "AGF-VP-ZombieCorpseLeaveQuicker": "AGF-ZombieCorpseLeaveQuicker",
    "AGF-VP-zHelpfulRenames": "AGF-HelpfulRenames",
    "AGF-VPS-FuelAutoShutOff": "AGF-FuelAutoShutOff",
    "AGF-VPS-GlobalStormTracker": "AGF-GlobalStormTracker",
    "AGF-VPS-HonkOpensYourDoors": "AGF-HonkOpensYourDoors",
    "AGF-VPS-LootTimerHolds": "AGF-LootTimerHolds",
    "AGF-VPS-ScreamerAlert": "AGF-ScreamerAlert",
    "AGF-VPS-SortingCart": "AGF-SortingCart",
    "AGF-VPS-VisualEntityTrackerAddon": "AGF-3VisualEntityTrackerAddon",
    "zzzAGF-Requested-LootStaysOnEmpty": "AGF-LootStaysOnEmpty",
    "zzzzAGF-LawnTractorV3Fix": "AGF-LawnTractorV3Fix",
}

# Old pack Name → the COMPAT mod Names that replaced that one pack.
AGF_REPLACED_BY: dict[str, list[str]] = {
    "zzzAGF-Special-Compatibilities": ["AGF-COMPAT-0SCore", "AGF-COMPAT-Companions", "AGF-COMPAT-Dewtas18SlotToolbelt", "AGF-COMPAT-DishongTowerChallenge", "AGF-COMPAT-GBZ15SlotToolbelt", "AGF-COMPAT-OakravenAmmoPress", "AGF-COMPAT-OutbackRoadies", "AGF-COMPAT-QuickStack", "AGF-COMPAT-WMM12SlotToolbelt"],
    "zzzAGF-Special-LocalizationPatches": ["AGF-COMPAT-BDubVehicles", "AGF-COMPAT-GSVanillaCookBook", "AGF-COMPAT-IZYWeapons"],
    "zzzAGF-Special-NoEACCompatibilities": ["AGF-COMPAT-DoomSurvival", "AGF-COMPAT-POIScourgeLite"],
}
# END NAME MAP


def agf_canonical_name(name: str) -> str:
    seen: set[str] = set()
    while name in AGF_NAME_ALIASES and name not in seen:
        seen.add(name)
        name = AGF_NAME_ALIASES[name]
    return name


def agf_download_url(canonical_name: str) -> str:
    return f"{DOWNLOAD_ZIP_BASE}/{canonical_name}.zip"


def load_agf_latest() -> dict[str, str]:
    latest: dict[str, str] = {}
    if not ACTIVEBUILD_DIR.is_dir():
        return latest
    for info in ACTIVEBUILD_DIR.glob("*/ModInfo.xml"):
        text = info.read_text(encoding="utf-8", errors="replace")
        name_m = re.search(r'<Name\s+value="([^"]+)"', text)
        ver_m = re.search(r'<Version\s+value="([^"]+)"', text)
        if not name_m or not ver_m:
            continue
        name = name_m.group(1)
        if name.startswith("AGF") or name.startswith("zzzAGF"):
            latest[name] = ver_m.group(1)
    return latest


def zip_modinfo_version(data: bytes) -> str:
    with zipfile.ZipFile(io.BytesIO(data)) as zf:
        infos = [
            name
            for name in zf.namelist()
            if name.replace("\\", "/").rstrip("/").endswith("ModInfo.xml")
        ]
        if not infos:
            return ""
        infos.sort(key=lambda name: name.count("/"))
        text = zf.read(infos[0]).decode("utf-8", errors="replace")
    ver_m = re.search(r'<Version\s+value="([^"]+)"', text)
    return ver_m.group(1) if ver_m else ""


def load_uploaded_versions(canons: list[str]) -> dict[str, str]:
    uploaded: dict[str, str] = {}
    for canon in canons:
        url = agf_download_url(canon)
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "AGF-LogReader"})
            with urllib.request.urlopen(req, timeout=20) as resp:
                data = resp.read()
        except (urllib.error.URLError, TimeoutError, OSError):
            continue
        ver = zip_modinfo_version(data)
        if ver:
            uploaded[canon] = ver
    return uploaded


def split_agf_stale(
    stale_repo: list[tuple[ModTry, str, str]],
    uploaded: dict[str, str],
) -> tuple[list[tuple[ModTry, str, str]], list[tuple[ModTry, str, str, str]]]:
    """Download-ready vs repo-only (not on GitHub yet)."""
    downloadable: list[tuple[ModTry, str, str]] = []
    unpublished: list[tuple[ModTry, str, str, str]] = []
    for mod, repo_ver, canon in stale_repo:
        have = version_tuple(mod.version) or version_tuple(mod.folder)
        up = uploaded.get(canon, "")
        up_ver = version_tuple(up)
        if have and up_ver and have < up_ver:
            downloadable.append((mod, up, canon))
        else:
            unpublished.append((mod, repo_ver, canon, up))
    return downloadable, unpublished


def ignored_has_latest(mods: list[ModTry], name: str, latest_ver: str) -> bool:
    want = version_tuple(latest_ver)
    if not want:
        return False
    canon = agf_canonical_name(name)
    for other in mods:
        if other.status != "ignored-duplicate":
            continue
        other_name = other.note or other.name
        if other_name != name and agf_canonical_name(other_name or "") != canon:
            continue
        other_ver = version_tuple(other.version) or version_tuple(other.folder)
        if other_ver and other_ver >= want:
            return True
    return False


def replaced_packs(mods: list[ModTry]) -> list[tuple[ModTry, list[str]]]:
    out = []
    for mod in mods:
        if mod.status != "loaded" or not mod.name:
            continue
        kids = AGF_REPLACED_BY.get(mod.name)
        if kids:
            out.append((mod, kids))
    return out


def outdated_agf(mods: list[ModTry], latest: dict[str, str]) -> list[tuple[ModTry, str, str]]:
    """Return (loaded_mod, latest_version, canonical_ActiveBuild_name)."""
    out = []
    for mod in mods:
        if mod.status != "loaded" or not mod.name:
            continue
        canon = agf_canonical_name(mod.name)
        cur = latest.get(canon)
        if not cur:
            continue
        have = version_tuple(mod.version) or version_tuple(mod.folder)
        want = version_tuple(cur)
        if have and want and have < want:
            if ignored_has_latest(mods, mod.name, cur):
                continue
            out.append((mod, cur, canon))
    return out


def explain_error(issue: Issue, mods: list[ModTry], latest: dict[str, str] | None = None) -> str:
    loaded = [m for m in mods if m.status == "loaded"]
    latest = latest or {}
    out = []
    if issue.cause:
        out.append(issue.cause)
    elif issue.meaning:
        out.append(issue.meaning)
    else:
        out.append("Unknown red error. Add this pattern to Knowledge.md.")
    if issue.action:
        out.append("")
        out.append(f"Player action: {issue.action}.")
    if issue.context:
        out.append(f"{issue.context}.")
    if issue.fixed_mod and issue.fixed_in:
        loaded_fix = next((m for m in loaded if m.name == issue.fixed_mod), None)
        out.append("")
        note = issue.fix_note or f"Fixed in {issue.fixed_mod} {issue.fixed_in}."
        if loaded_fix:
            have = version_tuple(loaded_fix.version) or version_tuple(loaded_fix.folder)
            want = version_tuple(issue.fixed_in)
            if have and want and have < want:
                out.append(
                    f"Known AGF fix: {issue.fixed_mod} {issue.fixed_in} — {note} "
                    f"This log loaded {loaded_fix.version}."
                )
            else:
                out.append(
                    f"Known AGF fix exists in {issue.fixed_mod} {issue.fixed_in} — {note} "
                    f"This log already loaded {loaded_fix.version}."
                )
        else:
            out.append(f"Known AGF fix: {issue.fixed_mod} {issue.fixed_in} — {note}")
        pub = latest.get(agf_canonical_name(issue.fixed_mod)) or latest.get(issue.fixed_mod)
        if pub:
            canon = agf_canonical_name(issue.fixed_mod)
            label = canon if canon != issue.fixed_mod else issue.fixed_mod
            out.append(f"Current AGF release of {label}: {pub}")
    if issue.confidence:
        out.append("")
        out.append(f"Confidence: {issue.confidence}")
    out.append("")
    out.append("Possible mod sources:")
    hits: list[str] = []
    frags = [s.strip() for s in issue.suspects.split(",") if s.strip()]
    stack = issue.text.lower()
    seen = set()
    for mod in loaded:
        blob = compact_alnum(f"{mod.name} {mod.folder}")
        matched = [
            frag for frag in frags if compact_alnum(frag) and compact_alnum(frag) in blob
        ]
        if not matched and mod.name and mod.name.lower() in stack:
            matched = [mod.name]
        if not matched:
            continue
        key = mod.folder or mod.name
        if key in seen:
            continue
        seen.add(key)
        label = f"{mod.folder} ({mod.name} {mod.version})".strip()
        conf = "high" if issue.fixed_mod and mod.name == issue.fixed_mod else "medium"
        hits.append(f"- {label} — matched '{matched[0]}' — {conf}")
    if hits:
        out.extend(hits)
    else:
        out.append("- No loaded mod name appears in this stack.")
        if "Challenges." in issue.text or "QuestJournal" in issue.text:
            out.append("- Stack is vanilla 7D2D challenge/quest code.")
    if "XUiView.set_IsVisible" in issue.text:
        found = visible_false_rect_mods(loaded)
        out.append("")
        if found:
            out.append("Quick search, loaded mods that set visible=\"false\" on a rect:")
            out.extend(f"- {line}" for line in found)
        else:
            out.append("Quick search: no loaded mod sets visible=\"false\" on a rect.")
    return "\n".join(out)


def find_winner(mods: list[ModTry], dup: ModTry) -> ModTry | None:
    name = dup.note or dup.name
    for mod in mods:
        if mod.status == "loaded" and mod.name == name:
            return mod
    return None


def status_label(mod: ModTry, winner: ModTry | None = None) -> str:
    if mod.status == "loaded":
        return ""
    if mod.status == "ignored-duplicate":
        extra_loc = where(mod)
        if winner:
            kept_loc = where(winner)
            kind = duplicate_kind(mod, winner)
            loc = (
                f"Both in {kept_loc}"
                if kept_loc.lower() == extra_loc.lower()
                else f"Loaded from: {kept_loc}\nThis copy: {extra_loc}"
            )
            return f"{kind}\n{loc}"
        return f"Duplicate of {mod.note}\nThis copy: {extra_loc}"
    if mod.status == "ignored-no-modinfo":
        return "Not a mod — no ModInfo.xml"
    if mod.status == "skipped-eac":
        return "Skipped — EAC skip flag"
    if mod.status == "skipped-eac-dll":
        return "Skipped — DLL blocked by EAC"
    if mod.status == "failed":
        return f"Failed — {mod.note}" if mod.note else "Failed"
    return "Unknown — tried, no Loaded Mod line"


FOLDER_VER_RE = re.compile(r"-v?\d+(?:\.\d+)+$", re.I)


def discord_title(mod: ModTry, winner: ModTry | None) -> str:
    if winner and winner.name:
        return winner.name
    if mod.name:
        return mod.name
    if mod.status == "ignored-duplicate" and mod.note:
        return mod.note
    stripped = FOLDER_VER_RE.sub("", mod.folder)
    return stripped or mod.folder


def version_tuple(text: str) -> tuple[int, ...]:
    m = re.search(r"(\d+)(?:\.\d+){1,}", text or "")
    if not m:
        return ()
    return tuple(int(part) for part in m.group(0).split("."))


def version_label(mod: ModTry) -> str:
    ver = (mod.version or "").strip()
    if ver and ver.lower() not in {"<unknown version>", "unknown"}:
        return ver
    parts = version_tuple(mod.folder)
    return ".".join(str(p) for p in parts)


def version_pair(ignored: ModTry, winner: ModTry) -> tuple[str, str]:
    return version_label(winner), version_label(ignored)


def is_multi_version(ignored: ModTry, winner: ModTry) -> bool:
    win_ver = version_tuple(winner.version) or version_tuple(winner.folder)
    skip_ver = version_tuple(ignored.version) or version_tuple(ignored.folder)
    return bool(win_ver and skip_ver and win_ver != skip_ver)


def duplicate_kind(ignored: ModTry, winner: ModTry) -> str:
    if is_multi_version(ignored, winner):
        kept, extra = version_pair(ignored, winner)
        return f"Multiple versions installed ({kept} and {extra})"
    return "Duplicate. Same mod is in two folders"


def older_copy(ignored: ModTry, winner: ModTry) -> ModTry:
    win_ver = version_tuple(winner.version) or version_tuple(winner.folder)
    skip_ver = version_tuple(ignored.version) or version_tuple(ignored.folder)
    if win_ver and skip_ver:
        if win_ver < skip_ver:
            return winner
        if skip_ver < win_ver:
            return ignored
    return winner


def discord_cleanup_lines(mod: ModTry, winner: ModTry | None) -> list[str]:
    details: list[str] = []
    if mod.status == "ignored-duplicate":
        remove = older_copy(mod, winner) if winner else mod
        if winner and is_multi_version(mod, winner):
            kept, extra = version_pair(mod, winner)
            details.append(f"Multiple versions installed ({kept} and {extra}).")
        else:
            details.append("Duplicate. Same mod is in two folders.")
        details.append(f"Delete `{remove.folder}` from `{where(remove)}`")
    elif mod.status == "ignored-no-modinfo":
        details.append("Isn't loading. No ModInfo.xml")
        details.append("Check the original download for ModInfo.xml and put it back, or remove this folder")
    elif mod.status == "skipped-eac":
        details.append("Isn't loading. Easy Anti-Cheat is on")
    elif mod.status == "skipped-eac-dll":
        details.append("Isn't loading. This is a DLL mod and Easy Anti-Cheat is on")
    elif mod.status == "failed":
        details.append("Isn't loading. It broke while the game tried to load it")
    else:
        details.append("Isn't loading")
    out = [f"- **{discord_title(mod, winner)}**"]
    out.extend(f"  - {line}" for line in details)
    return out


AREA_HINTS = (
    ("Challenges.", "Challenges"),
    ("ChallengeJournal", "Challenges"),
    ("QuestJournal", "Quests"),
    ("XUi", "UI"),
    ("ChunkManager", "World / chunks"),
    ("VehicleManager", "Vehicles"),
    ("Inventory", "Inventory"),
    ("PlayerMoveController", "Player movement"),
)


def guess_area(issue: Issue) -> str:
    if issue.area:
        return issue.area
    blob = issue.text
    for needle, label in AREA_HINTS:
        if needle in blob:
            return label
    return ""


def discord_update_lines(mod: ModTry, latest: str, canon: str) -> list[str]:
    line = f"  - {mod.version} loaded, latest {latest}"
    if canon != mod.name:
        line += f" (now `{canon}`)"
    url = agf_download_url(canon)
    return [
        f"- **{mod.name}**",
        line,
        f"  - [Download Latest](<{url}>)",
    ]


def discord_replaced_lines(mod: ModTry, kids: list[str]) -> list[str]:
    out = [
        f"- **{mod.name}**",
        "  - This pack was split into separate COMPAT mods. Remove it.",
    ]
    out.extend(f"  - `{kid}`" for kid in kids)
    return out


def discord_major_error_lines(issue: Issue, mods: list[ModTry]) -> list[str]:
    area = guess_area(issue)
    title = issue.text.splitlines()[0].split(":", 1)[0].strip() or "Red error"
    if area:
        out = [f"- {title} error is about the {area}."]
    else:
        out = [f"- {title} error."]
    if issue.action:
        act = issue.action
        out.append(f"- Player {act[0].lower() + act[1:]}.")
    if issue.kid == "loot-sort-echo-sortingcart":
        out.append("- Conflict between Echo Adaptive Backpack and Sorting Cart.")
        if issue.slots_note:
            out.append(f"- {issue.slots_note}.")
        return out
    frames = [ln.strip() for ln in issue.text.splitlines() if ln.strip().startswith("at ")]
    if frames:
        out.append(f"- {frames[0]}")
    found = (
        visible_false_rect_mods(mods)
        if "XUiView.set_IsVisible" in issue.text
        else []
    )
    if found:
        out.append("- Possible mods that set visible=\"false\" on a rect:")
        out.extend(f"- {line}" for line in found)
    if not issue.fixed_mod:
        if issue.kid or found:
            return out
        out.append("- Will have to do a deeper search.")
        return out
    out.append(f"- Known issue with {issue.fixed_mod} to cause that error.")
    loaded_fix = next((m for m in mods if m.status == "loaded" and m.name == issue.fixed_mod), None)
    if loaded_fix:
        have = version_tuple(loaded_fix.version) or version_tuple(loaded_fix.folder)
        want = version_tuple(issue.fixed_in)
        if have and want and have < want:
            canon = agf_canonical_name(issue.fixed_mod)
            if canon != issue.fixed_mod:
                out.append(
                    f"- Possible Fix: **Replace with `{canon}` {issue.fixed_in}**"
                )
            else:
                out.append(
                    f"- Possible Fix: **Remove {issue.fixed_mod} {loaded_fix.version}**"
                )
            return out
    out.append("- Will have to do a deeper search.")
    return out


def loaded_map(p: Parsed) -> dict[str, str]:
    return {(m.name or m.folder): (m.version or "") for m in p.mods if m.status == "loaded"}


def find_previous_same_save(current: Parsed, catalog: list[Knowledge]) -> Parsed | None:
    if not current.world or not current.save or not RAW_DIR.is_dir():
        return None
    current_raw = RAW_DIR / f"{file_stem(current)}.log"
    best: Parsed | None = None
    for raw in RAW_DIR.glob("*.log"):
        if current_raw.exists() and raw.resolve() == current_raw.resolve():
            continue
        try:
            prev = parse_log(raw, catalog)
        except Exception:
            continue
        if prev.world.strip().lower() != current.world.strip().lower():
            continue
        if prev.save.strip().lower() != current.save.strip().lower():
            continue
        if not prev.first_ts or prev.first_ts >= (current.first_ts or ""):
            continue
        if best is None or prev.first_ts > best.first_ts:
            best = prev
    return best


def compare_loadouts(prev: Parsed, current: Parsed) -> tuple[list[str], list[str], list[str]]:
    old = loaded_map(prev)
    new = loaded_map(current)
    added = [f"{name} {new[name]}".strip() for name in sorted(set(new) - set(old))]
    removed = [f"{name} {old[name]}".strip() for name in sorted(set(old) - set(new))]
    changed = [
        f"{name} {old[name]} → {new[name]}"
        for name in sorted(set(old) & set(new))
        if old[name] != new[name]
    ]
    return added, removed, changed


def write_record(p: Parsed, catalog: list[Knowledge] | None = None) -> Path:
    RECORDS_DIR.mkdir(parents=True, exist_ok=True)
    RAW_DIR.mkdir(parents=True, exist_ok=True)
    stem = file_stem(p)
    dest = RECORDS_DIR / f"{stem}.md"

    src = Path(p.source_log)
    if src.is_file() and src.stat().st_size <= 20 * 1024 * 1024:
        raw_dest = RAW_DIR / f"{dest.stem}.log"
        shutil.copy2(src, raw_dest)

    loaded = [m for m in p.mods if m.status == "loaded"]
    bad = [m for m in p.mods if m.status != "loaded"]
    by_sev: dict[str, list[Issue]] = {}
    for issue in p.issues:
        by_sev.setdefault(issue.severity, []).append(issue)
    latest = load_agf_latest()
    stale_repo = outdated_agf(p.mods, latest)
    uploaded = load_uploaded_versions(sorted({canon for _, _, canon in stale_repo}))
    stale, unpublished = split_agf_stale(stale_repo, uploaded)
    replaced = replaced_packs(p.mods)
    catalog = catalog or load_knowledge()
    previous = find_previous_same_save(p, catalog)
    added: list[str] = []
    removed: list[str] = []
    changed: list[str] = []
    if previous:
        added, removed, changed = compare_loadouts(previous, p)

    when = written_when(p.first_ts, p.last_ts)

    lines = [
        f"# {p.who}",
        "",
        f"- {color_label('When')} {when}",
        f"- {color_label('Player')} {p.ingame or p.who}",
        f"- {color_label('Game')} {short_game(p.version)}",
        f"- {color_label('EAC')} {p.eac}",
        f"- {color_label('Play')} {p.session or '?'}",
        f"- {color_label('World')} {p.world or '?'}",
        f"- {color_label('Save')} {p.save or '?'}",
        "",
        "## Mod folders",
    ]
    if p.sources:
        for src in p.sources:
            lines.append(f"- {public_path(src)}")
    else:
        lines.append("- none found")

    def add_log_block(text: str) -> None:
        lines.append("")
        lines.append("```")
        lines.append(text.rstrip("\n"))
        lines.append("```")

    def name_then_block(name: str, problem: str, prefix: str = "- ", paint=color_issue) -> None:
        lines.append(f"{prefix}{paint(name)}")
        add_log_block(problem)

    lines += ["", "## Did not load"]
    if bad:
        for mod in bad:
            winner = find_winner(p.mods, mod)
            name_then_block(mod.folder, status_label(mod, winner))
    else:
        lines.append("- none")

    lines += ["", "## AGF versions"]
    if stale or unpublished or replaced:
        for mod, cur, canon in stale:
            rename = f" → `{canon}`" if canon != mod.name else ""
            lines.append(
                f"- {color_warn(mod.name)} {mod.version} — uploaded {cur}{rename}"
            )
            lines.append(f"  - [Download Latest]({agf_download_url(canon)})")
        for mod, repo_ver, canon, up in unpublished:
            rename = f" → `{canon}`" if canon != mod.name else ""
            up_note = f"uploaded {up}" if up else "no zip on GitHub"
            lines.append(
                f"- {color_warn(mod.name)} {mod.version} — {up_note}, "
                f"repo {repo_ver}{rename} (not uploaded)"
            )
        for mod, kids in replaced:
            lines.append(
                f"- {color_warn(mod.name)} {mod.version} — split into COMPAT mods. Remove this pack."
            )
            for kid in kids:
                kid_ver = latest.get(kid, "")
                ver_note = f" {kid_ver}" if kid_ver else ""
                lines.append(f"  - `{kid}`{ver_note}")
    elif any(agf_canonical_name(m.name or "") in latest for m in loaded):
        lines.append("- All loaded AGF mods match ActiveBuild")
    else:
        lines.append("- no AGF mods to compare")

    lines += ["", f"## Mods ({len(loaded)} loaded)"]
    for i, mod in enumerate(p.mods, 1):
        winner = find_winner(p.mods, mod)
        extra = status_label(mod, winner)
        name = mod.name or mod.folder
        ver = f" ({mod.version})" if mod.version else ""
        if extra:
            name_then_block(name, extra, prefix=f"{i}. ")
        else:
            lines.append(f"{i}. {name}{ver}")

    errors = sorted(by_sev.get("error", []), key=lambda i: i.first_ts or "9999")
    first_error = errors[0] if errors else None
    later = later_issues(first_error, p.issues) if first_error else []
    later_lines = later_kind_lines(later)
    later_ids = {id(issue) for issue in later}
    lines += ["", "## Errors"]
    if errors:
        issue = errors[0]
        title = issue.text.splitlines()[0].split(":", 1)[0].strip() or "Red error"
        lines.append(f"- {color_issue(title)}")
        add_log_block(issue.text)
        add_log_block(explain_error(issue, p.mods, latest))
        lines.extend(later_lines)
    else:
        lines.append("- none")

    conflicts = by_sev.get("conflict", [])
    lines += ["", "## Mod Conflict"]
    if conflicts:
        for issue in conflicts:
            cm = re.search(r'from mod "([^"]+)"', issue.text)
            name = cm.group(1) if cm else "Unknown mod"
            name_then_block(name, issue.text, paint=color_warn)
    else:
        lines.append("- none")

    unknown = by_sev.get("unknown", [])
    xui_total = xui_mismatch_total(p.issues)
    xui_significant = xui_total >= XUI_MISMATCH_MIN
    if xui_significant:
        unknown = [issue for issue in unknown if not is_xui_mismatch(issue.text)]
    if first_error:
        unknown = [issue for issue in unknown if id(issue) not in later_ids]
    if xui_significant and not first_error:
        lines += ["", "## XUi"]
        named = xui_mismatch_mods(p.mods)
        if named:
            lines.append(f"- {', '.join(named)}")
        lines.append(f"- {XUI_MISMATCH_NOTE}")
    if unknown:
        lines += ["", "## New patterns"]
        for issue in unknown:
            meaning = issue.meaning or "NEW — add to Knowledge.md"
            name_then_block("Unknown", f"{meaning}\n\n{issue.text}")

    if previous:
        lines += ["", "## Since last log"]
        lines.append(f"- Last log: {written_when(previous.first_ts, previous.last_ts)}")
        if added:
            lines.append(f"- Added: {', '.join(added)}")
        if removed:
            lines.append(f"- Removed: {', '.join(removed)}")
        if changed:
            lines.append(f"- Version changed: {', '.join(changed)}")
        if not added and not removed and not changed:
            lines.append("- Same loaded mods as last log")

    lines += ["", "## Discord (copy this)"]
    lines.append("")
    lines.append("```")
    session = f" ({p.session})" if p.session else ""
    lines.append(
        f"# You are playing 7d2d version {short_game(p.version)} with {len(loaded)} mods with EAC {p.eac or 'unknown'}{session}."
    )

    def discord_section(title: str) -> None:
        lines.append(DISCORD_RULE)
        lines.append(title)

    if len(p.sources) > 1:
        steam_mods = RECOMMENDED_MODS
        extras = [
            public_path(src)
            for src in p.sources
            if public_path(src).replace("/", "\\").lower() != steam_mods.lower()
        ]
        froms = extras or ["the other Mods folder"]
        loc_word = "two locations" if len(p.sources) == 2 else "multiple locations"
        discord_section("## Recommendation")
        lines.append(f"- You have mods in {loc_word}.")
        lines.append(f"  - Move mods from: `{froms[0]}`")
        for extra in froms[1:]:
            lines.append(f"    - `{extra}`")
        lines.append(f"  - to `{steam_mods}`")
    if bad or stale or replaced:
        discord_section("## Clean-Up")
        for mod in bad:
            winner = find_winner(p.mods, mod)
            lines.append("")
            lines.extend(discord_cleanup_lines(mod, winner))
        for mod, cur, canon in stale:
            lines.append("")
            lines.extend(discord_update_lines(mod, cur, canon))
        for mod, kids in replaced:
            lines.append("")
            lines.extend(discord_replaced_lines(mod, kids))
    if xui_significant and not first_error:
        discord_section("## XUi")
        named = xui_mismatch_mods(p.mods)
        if named:
            lines.append(f"- {', '.join(named)}")
        lines.append(f"- {XUI_MISMATCH_NOTE}")
    if previous:
        discord_section("## Since last log")
        lines.append(f"- Last log: {written_when(previous.first_ts, previous.last_ts)}")
        if added:
            lines.append(f"- Added: {', '.join(added)}")
        if removed:
            lines.append(f"- Removed: {', '.join(removed)}")
        if changed:
            lines.append(f"- Version changed: {', '.join(changed)}")
        if not added and not removed and not changed:
            lines.append("- Same loaded mods as last log")
    if errors:
        discord_section("## Major Error")
        lines.extend(discord_major_error_lines(errors[0], p.mods))
        lines.extend(later_lines)
    lines.append(DISCORD_RULE)
    lines.append("```")

    lines += [
        "",
        "## Source",
        f"- `{Path(p.source_log).name}`",
        "",
    ]
    dest.write_text("\n".join(lines), encoding="utf-8")
    return dest


def rebuild_index() -> None:
    rows = []
    for path in sorted(RECORDS_DIR.glob("*.md")):
        if path.name.startswith("_"):
            continue
        text = path.read_text(encoding="utf-8")
        title = text.splitlines()[0].lstrip("# ").strip() if text else path.stem
        who = "unknown"
        game = ""
        for line in text.splitlines():
            if "Player:" in line:
                who = re.sub(r"<[^>]+>", "", line.split("Player:")[-1]).strip().strip("*").strip()
            if "Game:" in line:
                game = re.sub(r"<[^>]+>", "", line.split("Game:")[-1]).strip().strip("*").strip()
        if who == "unknown" and title:
            who = title.split("—")[0].strip()
        rows.append(f"| {title} | {who} | {game} | [{path.name}]({path.name}) |")
    body = [
        "# Log records",
        "",
        "| Log | Who | Game | File |",
        "|---|---|---|---|",
    ]
    body.extend(rows or ["| (none) | | | |"])
    body.append("")
    INDEX_PATH.write_text("\n".join(body), encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser(description="Summarize a 7D2D Player.log into a short MD record.")
    ap.add_argument("log", help="Path to Player.log / output_log")
    args = ap.parse_args()
    log_path = Path(args.log).expanduser().resolve()
    if not log_path.is_file():
        print(f"File not found: {log_path}")
        return 1
    catalog = load_knowledge()
    parsed = parse_log(log_path, catalog)
    dest = write_record(parsed, catalog)
    rebuild_index()
    loaded = sum(1 for m in parsed.mods if m.status == "loaded")
    bad = sum(1 for m in parsed.mods if m.status != "loaded")
    xui_significant = xui_mismatch_total(parsed.issues) >= XUI_MISMATCH_MIN
    unknown = sum(
        1
        for i in parsed.issues
        if i.severity == "unknown" and not (xui_significant and is_xui_mismatch(i.text))
    )
    print(f"Who: {parsed.who}")
    print(f"When: {nice_ts(parsed.first_ts)}")
    print(f"Game: {parsed.version}")
    print(f"Mods: {loaded} loaded, {bad} did not load")
    print(f"New patterns: {unknown}")
    print(f"Wrote: {dest}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
