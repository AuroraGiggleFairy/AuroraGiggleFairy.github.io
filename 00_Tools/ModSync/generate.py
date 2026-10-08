# ModSync Generator.
#
# Reads ADMIN-CONFIG.txt and writes ONE self-contained player .bat into Output\.
# The .bat carries the ModSync mod inside it as base64 text, so the admin has a
# single file to hand out and the player has nothing to keep together.
#
# Layout of the generated file:
#
#   @echo off ... config ... run the embedded PowerShell ... exit /b
#   ::B64::<mod folder as a zip, in base64>      (cmd never reaches these)
#   ### AGF-POWERSHELL ###
#   <the installer script>
from pathlib import Path
import base64
import io
import re
import sys
import zipfile

ROOT = Path(__file__).resolve().parent
ENGINE = ROOT / "engine"
OUT_DIR = ROOT / "Output"
CONFIG_PATH = ROOT / "ADMIN-CONFIG.txt"
REPO_ROOT = ROOT.parent.parent

MARKER = "### AGF-POWERSHELL ###"
B64_PREFIX = "::B64::"
B64_WIDTH = 240

# Server-only or per-server files. Never shipped to players.
PAYLOAD_SKIP = {"modsync-server.txt", "modsync-join.txt"}


def read_config(path: Path) -> dict:
    if not path.is_file():
        raise SystemExit("Missing ADMIN-CONFIG.txt")
    data = {}
    # utf-8-sig: Notepad and PowerShell both like to leave a BOM on the first line,
    # which would otherwise hide whichever setting happens to be at the top.
    for raw in path.read_text(encoding="utf-8-sig").splitlines():
        line = raw.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, val = line.split("=", 1)
        data[key.strip().upper()] = val.strip().strip('"')
    return data


def safe_name(name: str, what: str) -> str:
    cleaned = "".join("_" if ch in '<>:"/\\|?*' else ch for ch in name.strip()).strip()
    cleaned = cleaned.rstrip(".")
    if not cleaned:
        raise SystemExit(f"{what} is empty in ADMIN-CONFIG.txt")
    return cleaned


def find_mod_source(cfg: dict) -> Path:
    raw = cfg.get("MOD_SOURCE", "").strip()
    if raw:
        candidate = Path(raw)
        if not candidate.is_absolute():
            candidate = (ROOT / candidate).resolve()
        if not candidate.is_dir():
            raise SystemExit(f"MOD_SOURCE is not a folder: {candidate}")
        return candidate

    beside = [
        p for p in ROOT.iterdir()
        if p.is_dir() and (p / "ModSync.dll").is_file()
    ]
    if beside:
        beside.sort(key=lambda p: p.name, reverse=True)
        return beside[0]

    draft = REPO_ROOT / "01_Draft"
    matches = []
    if draft.is_dir():
        matches = sorted(
            (
                p for p in draft.iterdir()
                if p.is_dir() and "ModSync" in p.name and (p / "ModSync.dll").is_file()
            ),
            key=lambda p: p.name,
            reverse=True,
        )
    if not matches:
        raise SystemExit(
            "Could not find the ModSync mod. "
            "Build DLL_NoEAC-ModSync, or set MOD_SOURCE in ADMIN-CONFIG.txt."
        )
    return matches[0]


def pack_mod(mod_dir: Path) -> str:
    """Zips the mod folder contents (not the folder itself) and returns base64."""
    if not (mod_dir / "ModSync.dll").is_file():
        raise SystemExit(
            f"No ModSync.dll in {mod_dir}. Build DLL_NoEAC-ModSync first."
        )

    buffer = io.BytesIO()
    count = 0
    with zipfile.ZipFile(buffer, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        for item in sorted(mod_dir.rglob("*")):
            if not item.is_file():
                continue
            if item.name.lower() in PAYLOAD_SKIP:
                continue
            zf.write(item, item.relative_to(mod_dir).as_posix())
            count += 1

    if count == 0:
        raise SystemExit(f"Nothing to pack from {mod_dir}")

    print(f"  Packed {count} file(s) from {mod_dir.name}")
    return base64.b64encode(buffer.getvalue()).decode("ascii")


def check_config(cfg: dict) -> dict:
    """Everything the admin must get right, checked before any packing work."""
    server_name = cfg.get("SERVER_NAME", "").strip()
    if not server_name or re.search(r"SERVERNAME", server_name, re.I):
        raise SystemExit("Set SERVER_NAME in ADMIN-CONFIG.txt to your server's name.")

    ip = cfg.get("SERVER_IP", "").strip()
    if not ip:
        raise SystemExit("Set SERVER_IP in ADMIN-CONFIG.txt.")

    port_text = cfg.get("SERVER_PORT", "").strip()
    if not port_text.isdigit() or not (1 <= int(port_text) <= 65535):
        raise SystemExit("Set SERVER_PORT in ADMIN-CONFIG.txt to the server's port number.")

    copy_folder = cfg.get("COPY_FOLDER_NAME", "").strip()
    if not copy_folder:
        copy_folder = f"7 Days to Die - {server_name}"

    stem = cfg.get("CLIENT_BAT_NAME", "").strip()
    if not stem:
        stem = "ModSync." + re.sub(r"[^A-Za-z0-9]", "", server_name)
    if stem.lower().endswith(".bat"):
        stem = stem[:-4]
    if re.search(r"SERVERNAME", stem, re.I):
        raise SystemExit("Change CLIENT_BAT_NAME in ADMIN-CONFIG.txt, or leave it blank.")

    eac_text = cfg.get("EAC", "off").strip().lower()
    if eac_text in ("on", "yes", "true", "1"):
        eac_on = True
    elif eac_text in ("off", "no", "false", "0", ""):
        eac_on = False
    else:
        raise SystemExit("Set EAC in ADMIN-CONFIG.txt to on or off.")

    pack_urls = []
    for key in ("PACK_URL", "PACK_URL2", "PACK_URL3"):
        raw = cfg.get(key, "").strip()
        if not raw:
            continue
        for part in re.split(r"[|;]", raw):
            url = part.strip()
            if not url:
                continue
            if not url.lower().startswith(("http://", "https://")):
                raise SystemExit("PACK_URL must start with http:// or https://, or be left blank.")
            pack_urls.append(url)

    return {
        "server_name": server_name,
        "ip": ip,
        "port": int(port_text),
        "copy_folder": safe_name(copy_folder, "COPY_FOLDER_NAME"),
        "stem": safe_name(stem, "CLIENT_BAT_NAME"),
        "eac_on": eac_on,
        "pack_urls": pack_urls,
    }


def build_bat(opt: dict, mod_dir: Path, payload: str) -> str:
    server_name = opt["server_name"]
    ip = opt["ip"]
    copy_folder = opt["copy_folder"]

    installer = (ENGINE / "INSTALL-ServerSync.ps1").read_text(encoding="utf-8")

    header = f"""@echo off
setlocal
cd /d "%~dp0"
title ModSync - {server_name}

REM ============================================================
REM  ModSync for {server_name}
REM
REM  Double-click this file. It sets up your game for this
REM  server and puts a shortcut on your desktop.
REM  The mods themselves come from the server when you play.
REM
REM  Close 7 Days to Die before running this.
REM ============================================================

set "MODSYNC_SERVER_NAME={server_name}"
set "MODSYNC_IP={ip}"
set "MODSYNC_PORT={opt["port"]}"
set "MODSYNC_COPY_FOLDER={copy_folder}"
set "MODSYNC_MOD_FOLDER={mod_dir.name}"
set "MODSYNC_EAC={"on" if opt["eac_on"] else "off"}"
set "MODSYNC_PACK_URL={("|".join(opt["pack_urls"])).replace("%", "%%")}"
set "MODSYNC_BAT_PATH=%~f0"

set "MODSYNC_PS=%TEMP%\\ModSync-Setup-%RANDOM%.ps1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$t=[IO.File]::ReadAllText($env:MODSYNC_BAT_PATH); $m='{MARKER}'; $i=$t.LastIndexOf($m); if($i -lt 0){{ Write-Error 'This file is damaged.'; exit 1 }}; $enc=New-Object System.Text.UTF8Encoding $false; [IO.File]::WriteAllText($env:MODSYNC_PS, $t.Substring($i+$m.Length), $enc)"
if errorlevel 1 (
    echo.
    echo  Could not start ModSync. Ask the admin to send the file again.
    echo.
    pause
    exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%MODSYNC_PS%"
set "EXIT_CODE=%ERRORLEVEL%"
del "%MODSYNC_PS%" >nul 2>&1
if not "%EXIT_CODE%"=="0" (
    echo.
    echo  ModSync did not finish. Nothing was broken.
    echo.
)
pause
exit /b %EXIT_CODE%

"""

    lines = [
        B64_PREFIX + payload[i : i + B64_WIDTH]
        for i in range(0, len(payload), B64_WIDTH)
    ]

    return header + "\n".join(lines) + "\n\n" + MARKER + "\n" + installer


def build_update_bat(mod_dir: Path, payload: str) -> str:
    installer = (ENGINE / "UPDATE-ModSync.ps1").read_text(encoding="utf-8")
    installer = installer.replace("\r\n", "\n").replace("\r", "\n")

    header = f"""@echo off
setlocal
cd /d "%~dp0"
title UpdateModSync

set "MODSYNC_MOD_FOLDER={mod_dir.name}"
set "MODSYNC_BAT_PATH=%~f0"
set "MODSYNC_PS=%TEMP%\\UpdateModSync-%RANDOM%.ps1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$t=[IO.File]::ReadAllText($env:MODSYNC_BAT_PATH); $m='{MARKER}'; $i=$t.LastIndexOf($m); if($i -lt 0){{ exit 1 }}; $enc=New-Object System.Text.UTF8Encoding $false; [IO.File]::WriteAllText($env:MODSYNC_PS, $t.Substring($i+$m.Length), $enc)"
if not errorlevel 1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%MODSYNC_PS%"
set "EXIT_CODE=%ERRORLEVEL%"
del "%MODSYNC_PS%" >nul 2>&1
echo.
pause
exit /b %EXIT_CODE%

"""

    lines = [
        B64_PREFIX + payload[i : i + B64_WIDTH]
        for i in range(0, len(payload), B64_WIDTH)
    ]
    return header + "\n".join(lines) + "\n\n" + MARKER + "\n" + installer


def build_manda_bat(opt: dict, mod_dir: Path, payload: str) -> str:
    installer = (ENGINE / "UPDATE-Manda.ps1").read_text(encoding="utf-8")
    installer = installer.replace("\r\n", "\n").replace("\r", "\n")

    header = f"""@echo off
setlocal
cd /d "%~dp0"
title Update Manda

set "MODSYNC_SERVER_NAME={opt["server_name"]}"
set "MODSYNC_COPY_FOLDER={opt["copy_folder"]}"
set "MODSYNC_MOD_FOLDER={mod_dir.name}"
set "MODSYNC_BAT_PATH=%~f0"
set "MODSYNC_PS=%TEMP%\\UpdateManda-%RANDOM%.ps1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$t=[IO.File]::ReadAllText($env:MODSYNC_BAT_PATH); $m='{MARKER}'; $i=$t.LastIndexOf($m); if($i -lt 0){{ exit 1 }}; $enc=New-Object System.Text.UTF8Encoding $false; [IO.File]::WriteAllText($env:MODSYNC_PS, $t.Substring($i+$m.Length), $enc)"
if not errorlevel 1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%MODSYNC_PS%"
set "EXIT_CODE=%ERRORLEVEL%"
del "%MODSYNC_PS%" >nul 2>&1
echo.
pause
exit /b %EXIT_CODE%

"""

    lines = [
        B64_PREFIX + payload[i : i + B64_WIDTH]
        for i in range(0, len(payload), B64_WIDTH)
    ]
    return header + "\n".join(lines) + "\n\n" + MARKER + "\n" + installer


def main_manda() -> None:
    cfg = read_config(CONFIG_PATH)
    opt = check_config(cfg)
    mod_dir = find_mod_source(cfg)
    payload = pack_mod(mod_dir)
    text = build_manda_bat(opt, mod_dir, payload)
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    out_path = OUT_DIR / "UpdateManda.bat"
    out_path.write_bytes(text.replace("\n", "\r\n").encode("ascii", "strict"))
    size_kb = out_path.stat().st_size / 1024.0
    print(f"  Wrote {out_path}  ({size_kb:.0f} KB)")
    print("  Send that one file to players. Nothing else is needed.")


def main_update() -> None:
    mod_dir = find_mod_source({})
    payload = pack_mod(mod_dir)
    text = build_update_bat(mod_dir, payload)
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    out_path = OUT_DIR / "UpdateModSync.bat"
    out_path.write_bytes(text.replace("\n", "\r\n").encode("ascii", "strict"))
    size_kb = out_path.stat().st_size / 1024.0
    print(f"  Wrote {out_path}  ({size_kb:.0f} KB)")
    print("  Send that one file to players. Nothing else is needed.")


def main() -> None:
    cfg = read_config(CONFIG_PATH)
    opt = check_config(cfg)
    mod_dir = find_mod_source(cfg)
    payload = pack_mod(mod_dir)

    text = build_bat(opt, mod_dir, payload)

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    out_path = OUT_DIR / (opt["stem"] + ".bat")
    out_path.write_bytes(text.replace("\n", "\r\n").encode("ascii", "strict"))

    size_kb = out_path.stat().st_size / 1024.0
    print(f"  Wrote {out_path}  ({size_kb:.0f} KB)")
    print("  Send that one file to players. Nothing else is needed.")


if __name__ == "__main__":
    try:
        if "--manda" in sys.argv:
            main_manda()
        elif "--update" in sys.argv:
            main_update()
        else:
            main()
    except SystemExit as ex:
        if ex.code not in (0, None):
            print(f"\n  {ex.code}\n", file=sys.stderr)
            raise SystemExit(1)
        raise
