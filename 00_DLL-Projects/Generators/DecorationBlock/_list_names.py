import csv
import re

path = r"c:\GitHub\7D2D-Mods\01_Draft\AGF-VP-DecorationBlock-v3.0.3\Config\Localization.csv"
pats = ("guardrail", "bollard", "chainlink")
seen: set[str] = set()
with open(path, encoding="utf-8") as handle:
    for row in csv.DictReader(handle):
        key = row["Key"]
        low = key.lower()
        if not any(pat in low for pat in pats):
            continue
        if key.endswith(("Iron", "Steel", "Insecure")):
            continue
        english = re.sub(r" \[[0-9a-fA-F]{6}\]\([^)]*\)\[-\]\s*$", "", row["english"])
        if english in seen:
            continue
        seen.add(english)
        source = key[7:] if key.startswith("agfDeco") else key
        print(f"{source}\t{english}")
