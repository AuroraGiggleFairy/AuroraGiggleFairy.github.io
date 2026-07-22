import argparse
import json
import os
import re
import sys
import urllib.error
import urllib.request
from typing import List

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
DEFAULT_RELEASE_TEXT_PATH = os.path.join(SCRIPT_DIR, "discord-post.txt")
if not os.path.isfile(DEFAULT_RELEASE_TEXT_PATH):
    # Backward compatibility for older release-data layouts.
    DEFAULT_RELEASE_TEXT_PATH = os.path.join(
        SCRIPT_DIR, "..", "..", "..", "05_ReleaseData", "GigglePack", "Discord", "discord-post.txt"
    )
    DEFAULT_RELEASE_TEXT_PATH = os.path.normpath(DEFAULT_RELEASE_TEXT_PATH)
DISCORD_WEBHOOK_ENV_VAR = "AGF_DISCORD_WEBHOOK_URL"
SECTION_HEADER_PATTERN = re.compile(r"^-\s\*\*.+:\*\*\s*$")


def continued_section_header(header_line: str) -> str:
    stripped = header_line.rstrip("\n")
    match = re.match(r"^(-\s\*\*)(.+?)(:\*\*)\s*$", stripped)
    if not match:
        return header_line if header_line.endswith("\n") else f"{header_line}\n"

    prefix, label, suffix = match.groups()
    clean_label = label.strip()
    if "(continued)" not in clean_label.lower():
        clean_label = f"{clean_label} (continued)"
    return f"{prefix}{clean_label}{suffix}\n"


def split_discord_message(content: str, max_len: int = 2000) -> List[str]:
    if len(content) <= max_len:
        return [content]

    lines = content.splitlines(keepends=True)
    chunks: List[str] = []
    current = ""
    active_section_header = ""
    for line in lines:
        stripped_line = line.strip()
        if SECTION_HEADER_PATTERN.match(stripped_line):
            active_section_header = line if line.endswith("\n") else f"{line}\n"

        if len(current) + len(line) <= max_len:
            current += line
            continue

        if current:
            chunks.append(current.rstrip("\n"))
            current = ""

        # If a chunk begins with an indented list item, repeat the section header
        # so Discord keeps the nested bullet indentation context.
        if line.startswith("  - ") and active_section_header:
            continued_header = continued_section_header(active_section_header)
            if len(continued_header) + len(line) <= max_len:
                current += continued_header
            elif len(active_section_header) + len(line) <= max_len:
                # Fallback to the original header if the continued label would overflow.
                current += active_section_header

        while len(line) > max_len:
            if current:
                remaining = max_len - len(current)
                if remaining <= 0:
                    chunks.append(current.rstrip("\n"))
                    current = ""
                    continue
                current += line[:remaining]
                chunks.append(current.rstrip("\n"))
                line = line[remaining:]
                current = ""
            else:
                chunks.append(line[:max_len])
                line = line[max_len:]
        current += line

    if current:
        chunks.append(current.rstrip("\n"))
    return chunks


def post_discord_webhook_message(webhook_url: str, content: str) -> bool:
    parts = split_discord_message(content)
    for idx, part in enumerate(parts, start=1):
        payload = json.dumps({"content": part}).encode("utf-8")
        request = urllib.request.Request(
            webhook_url,
            data=payload,
            headers={"Content-Type": "application/json", "User-Agent": "AGF-GigglePack-Discord-Sender"},
            method="POST",
        )
        try:
            with urllib.request.urlopen(request, timeout=20) as response:
                status = response.getcode()
                if status not in (200, 204):
                    print(f"Discord webhook returned HTTP {status} on part {idx}/{len(parts)}")
                    return False
        except urllib.error.URLError as ex:
            print(f"Failed to post Discord webhook part {idx}/{len(parts)}: {ex}")
            return False
        except Exception as ex:
            print(f"Unexpected Discord webhook error on part {idx}/{len(parts)}: {ex}")
            return False

    return True


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Send the latest generated GigglePack Discord update")
    parser.add_argument(
        "--discord-webhook-url",
        default="",
        help=(
            "Discord webhook URL. "
            f"If omitted, uses environment variable {DISCORD_WEBHOOK_ENV_VAR}."
        ),
    )
    parser.add_argument(
        "--discord-text-file",
        default=DEFAULT_RELEASE_TEXT_PATH,
        help="Path to the Discord text file to send",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print the message that would be posted without sending it",
    )
    return parser


def main() -> int:
    parser = build_arg_parser()
    args = parser.parse_args()

    webhook_url = (args.discord_webhook_url or os.getenv(DISCORD_WEBHOOK_ENV_VAR, "")).strip()
    if not args.dry_run and not webhook_url:
        print(f"No webhook URL provided. Set --discord-webhook-url or env {DISCORD_WEBHOOK_ENV_VAR}.")
        return 1

    text_path = os.path.abspath(args.discord_text_file)
    if not os.path.isfile(text_path):
        print(f"Discord text file not found: {text_path}")
        return 1

    try:
        with open(text_path, "r", encoding="utf-8") as f:
            content = f.read().strip()
    except Exception as ex:
        print(f"Could not read Discord text file {text_path}: {ex}")
        return 1

    if not content:
        print("Discord text is empty. Nothing to send.")
        return 1

    if args.dry_run:
        print("[DRYRUN] Would send this Discord message:\n")
        print(content)
        return 0

    ok = post_discord_webhook_message(webhook_url, content)
    if ok:
        print("Discord webhook post completed")
        return 0

    return 1


if __name__ == "__main__":
    sys.exit(main())
