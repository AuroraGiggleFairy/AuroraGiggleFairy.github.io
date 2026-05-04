"""
One-time migration script: converts old ## 5. Features / FEATURES START/END blocks
to the new two-section format (FEATURES-SUMMARY START/END + FEATURES-DETAILED START/END).

Existing content moves into the Detailed block. Summary is left empty.

Targets:
  - 02_ActiveBuild/**/README.md
  - 03_ReleaseSource/**/README.md
  - Steam Mods/**/*.md  (optional, pass --steam)

Usage:
  python SCRIPT-MigrateFeatureSections.py [--dry-run] [--steam]
"""
import re
import sys
import pathlib

DRY_RUN = "--dry-run" in sys.argv
INCLUDE_STEAM = "--steam" in sys.argv

SCRIPT_DIR = pathlib.Path(__file__).parent

SEARCH_ROOTS = [
    SCRIPT_DIR / "02_ActiveBuild",
    SCRIPT_DIR / "03_ReleaseSource",
]
if INCLUDE_STEAM:
    SEARCH_ROOTS.append(
        pathlib.Path(r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods")
    )

OLD_PATTERN = re.compile(
    r"(## 5\. Features\n)<!-- FEATURES START -->([\s\S]*?)<!-- FEATURES END -->",
    re.MULTILINE,
)

NEW_TEMPLATE = (
    "{heading}"
    "### A. Summary\n"
    "<!-- FEATURES-SUMMARY START -->\n"
    "\n"
    "<!-- FEATURES-SUMMARY END -->\n"
    "\n"
    "---\n"
    "\n"
    "### B. Detailed\n"
    "<!-- FEATURES-DETAILED START -->{existing_content}<!-- FEATURES-DETAILED END -->"
)


def migrate_file(path: pathlib.Path) -> bool:
    text = path.read_text(encoding="utf-8")
    match = OLD_PATTERN.search(text)
    if not match:
        return False

    heading = match.group(1)
    existing_content = match.group(2)  # includes leading/trailing newlines

    replacement = NEW_TEMPLATE.format(
        heading=heading,
        existing_content=existing_content,
    )

    new_text = text[: match.start()] + replacement + text[match.end() :]

    if DRY_RUN:
        print(f"[DRY RUN] Would migrate: {path}")
    else:
        path.write_text(new_text, encoding="utf-8")
        print(f"Migrated: {path}")
    return True


def main():
    total = 0
    for root in SEARCH_ROOTS:
        if not root.exists():
            print(f"Skipping missing root: {root}")
            continue
        for readme in root.rglob("README.md"):
            if migrate_file(readme):
                total += 1

    print(f"\n{'[DRY RUN] ' if DRY_RUN else ''}Done. {total} file(s) {'would be ' if DRY_RUN else ''}migrated.")


if __name__ == "__main__":
    main()
