"""
Converts zzzAGF-Special-Compatibilities Config files to the ModPatches include structure.

For each XML config file:
  - Conditional blocks gated on a non-AGF mod are extracted into
    Config/ModPatches/<ModName>/<filename>.xml  (or XUi/ModPatches for windows.xml)
  - The primary file becomes an include dispatcher using mod_loaded() guards
  - Non-conditional content (standalone patches with no mod gate) stays in the main file
  - Each patch file starts with <!-- MOD NAME --> comment under <zzzAGF-Compatibilities>
"""

import re
import os
import shutil

BASE       = r"c:\GitHub\7D2D-Mods\02_ActiveBuild\zzzAGF-Special-Compatibilities-v4.1.1"
CONFIG_DIR = os.path.join(BASE, "Config")
BACKUP_DIR = os.path.join(BASE, "Config_BACKUP")

# Files to process (relative to CONFIG_DIR)
FILES_TO_PROCESS = [
    "blocks.xml",
    "buffs.xml",
    "items.xml",
    "materials.xml",
    "progression.xml",
    "recipes.xml",
    "vehicles.xml",
    os.path.join("XUi", "windows.xml"),
    # traders.xml has no non-AGF mods - will be left alone if no patches found
    "traders.xml",
]


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def get_first_non_agf_mod(cond_text):
    """Return the first non-AGF mod name found in a cond string, or None."""
    for m in re.findall(r"mod_loaded\('([^']+)'\)", cond_text):
        if not m.startswith("AGF-"):
            return m
    return None


def strip_mod_from_cond(cond_str, mod_name):
    """Remove mod_loaded('mod_name') and any connecting 'and' from cond_str."""
    escaped = re.escape(mod_name)
    pat     = rf"mod_loaded\('{escaped}'\)"

    # "MOD and REST"
    r = re.sub(rf"{pat}\s+and\s+", "", cond_str)
    if r != cond_str:
        return r.strip()
    # "REST and MOD"
    r = re.sub(rf"\s+and\s+{pat}", "", cond_str)
    if r != cond_str:
        return r.strip()
    # MOD alone
    r = re.sub(pat, "", cond_str)
    return r.strip()


def transform_block(block, primary_mod):
    """Strip primary_mod from every cond="..." attribute in the block."""
    def replace_cond(m):
        original = m.group(1)
        new_cond = strip_mod_from_cond(original, primary_mod)
        return f'cond="{new_cond}"' if new_cond else 'cond="__EMPTY__"'
    return re.sub(r'cond="([^"]+)"', replace_cond, block)


def simplify_empty_conditionals(block):
    """
    If a <conditional> has a single <if cond="__EMPTY__">, unwrap it —
    just emit the inner content since the include itself is the gate.
    If there are multiple <if>s, at least one should have content left;
    we simply drop the empty-cond <if> and its content (shouldn't happen
    in practice but handled gracefully).
    """
    if "__EMPTY__" not in block:
        return block

    # Count all <if> elements
    all_ifs = re.findall(r'<if\s+cond="([^"]+)"', block)
    empty_count = all_ifs.count("__EMPTY__")

    if empty_count == len(all_ifs) == 1:
        # Single <if cond="__EMPTY__"> — unwrap entirely
        inner_match = re.search(
            r'<conditional>\s*<if\s+cond="__EMPTY__">(.*?)</if>\s*</conditional>',
            block, re.DOTALL
        )
        if inner_match:
            return inner_match.group(1)

    # Multiple <if>s: drop any <if cond="__EMPTY__">...</if> blocks
    # (keep only the ones with real remaining conditions)
    cleaned = re.sub(
        r'\s*<if\s+cond="__EMPTY__">.*?</if>', "", block, flags=re.DOTALL
    )
    return cleaned


# ---------------------------------------------------------------------------
# Core processing
# ---------------------------------------------------------------------------

def process_file(filepath):
    """
    Returns:
        main_parts  – list of text segments that stay in the dispatcher file
        patches     – dict { mod_name: [transformed_block, ...] }
        is_xui      – whether this file lives under XUi/
        filename    – basename of the file
    """
    with open(filepath, encoding="utf-8") as fh:
        content = fh.read()

    filename = os.path.basename(filepath)
    is_xui   = os.sep + "XUi" + os.sep in filepath or "/XUi/" in filepath

    # Split content on top-level <conditional>...</conditional> blocks.
    # Non-conditional segments (including comments) interleave between them.
    parts = re.split(r'(<conditional>.*?</conditional>)', content, flags=re.DOTALL)

    patches    = {}   # mod_name -> [block_text, ...]
    main_parts = []

    for part in parts:
        stripped = part.strip()
        if stripped.startswith("<conditional>") and stripped.endswith("</conditional>"):
            # Identify the primary non-AGF mod
            all_conds   = re.findall(r'<if\s+cond="([^"]+)"', part)
            primary_mod = None
            for cond in all_conds:
                primary_mod = get_first_non_agf_mod(cond)
                if primary_mod:
                    break

            if primary_mod:
                transformed = transform_block(part, primary_mod)
                transformed = simplify_empty_conditionals(transformed)
                patches.setdefault(primary_mod, []).append(transformed.strip())
                # Keep a placeholder comment in the main file so the position is clear
                # (optional – we don't add noise; the include statement replaces it)
            else:
                # No non-AGF mod — keep as-is in main file
                main_parts.append(part)
        else:
            main_parts.append(part)

    return main_parts, patches, is_xui, filename


def write_patch_file(patch_dir, filename, mod_name, blocks):
    os.makedirs(patch_dir, exist_ok=True)
    out_path = os.path.join(patch_dir, filename)
    lines    = [f"<zzzAGF-Compatibilities>\n<!-- {mod_name} -->\n"]
    for block in blocks:
        lines.append("\n" + block)
    lines.append("\n\n</zzzAGF-Compatibilities>")
    with open(out_path, "w", encoding="utf-8") as fh:
        fh.write("".join(lines))
    print(f"    Created: ModPatches/{mod_name}/{filename}")


def write_dispatcher_file(filepath, main_parts, patches, is_xui):
    filename = os.path.basename(filepath)

    # Collapse the non-conditional content, stripping the old root tags
    raw = "".join(main_parts)
    # Remove the outermost opening and closing root tag
    raw = re.sub(r"^\s*<[A-Za-z][A-Za-z0-9_-]*[^>]*>\s*", "", raw)
    raw = re.sub(r"\s*</[A-Za-z][A-Za-z0-9_-]*>\s*$", "", raw)
    leftover = raw.strip()

    out = ["<zzzAGF-Compatibilities>\n"]

    if leftover:
        out.append("\n" + leftover + "\n")

    for mod_name in sorted(patches.keys()):
        if is_xui:
            include_path = f"ModPatches/{mod_name}/{filename}"
        else:
            include_path = f"ModPatches/{mod_name}/{filename}"
        out.append(
            f'\n<conditional>\n'
            f'\t<if cond="mod_loaded(\'{mod_name}\')">\n'
            f'\t\t<include filename="{include_path}"/>\n'
            f'\t</if>\n'
            f'</conditional>\n'
        )

    out.append("\n</zzzAGF-Compatibilities>")

    with open(filepath, "w", encoding="utf-8") as fh:
        fh.write("".join(out))
    print(f"    Rewrote dispatcher: {filename}")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    # Backup (skip if already done)
    if not os.path.exists(BACKUP_DIR):
        shutil.copytree(CONFIG_DIR, BACKUP_DIR)
        print(f"Backed up Config → Config_BACKUP")
    else:
        print("Backup already exists, skipping.")

    for rel in FILES_TO_PROCESS:
        filepath = os.path.join(CONFIG_DIR, rel)
        if not os.path.exists(filepath):
            print(f"  SKIP (not found): {rel}")
            continue

        print(f"\nProcessing: {rel}")
        main_parts, patches, is_xui, filename = process_file(filepath)

        if not patches:
            print("  No non-AGF mod conditionals found — file unchanged.")
            continue

        # Write patch files
        for mod_name, blocks in patches.items():
            if is_xui:
                patch_dir = os.path.join(CONFIG_DIR, "XUi", "ModPatches", mod_name)
            else:
                patch_dir = os.path.join(CONFIG_DIR, "ModPatches", mod_name)
            write_patch_file(patch_dir, filename, mod_name, blocks)

        # Rewrite main file as dispatcher
        write_dispatcher_file(filepath, main_parts, patches, is_xui)

    print("\nDone!")


if __name__ == "__main__":
    main()
