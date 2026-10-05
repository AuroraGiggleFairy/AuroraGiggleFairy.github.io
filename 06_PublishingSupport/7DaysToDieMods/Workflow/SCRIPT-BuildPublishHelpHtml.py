"""Build a one-file 7DaysToDieMods PublishHelp HTML packet with copy buttons."""
import html
import json
import os
import re
import sys

sys.dont_write_bytecode = True

WORKFLOW_DIR = os.path.dirname(os.path.abspath(__file__))
SITE_ROOT = os.path.dirname(WORKFLOW_DIR)
PUBLISH_HELP_DIR = os.path.join(SITE_ROOT, "PublishHelp")
NEXUS_HELP_DIR = os.path.join(
    os.path.dirname(SITE_ROOT), "NexusMods", "PublishHelp"
)


def load_text(path: str) -> str:
    with open(path, "r", encoding="utf-8") as handle:
        return handle.read()


def bbcode_to_site_html(text: str) -> str:
    """Turn AGF Nexus BBCode into the HTML/Markdown mix already live on 7DTDMods."""
    s = text.replace("\r\n", "\n").replace("\r", "\n").strip()
    s = s.replace("[heading]", "").replace("[/heading]", "")
    s = re.sub(r"\[size=\d+\]", "", s)
    s = s.replace("[/size]", "")
    s = re.sub(
        r"\[url=([^\]]+)\](.*?)\[/url\]",
        r'<a href="\1">\2</a>',
        s,
        flags=re.S,
    )
    s = re.sub(
        r"\[color=([^\]]+)\](.*?)\[/color\]",
        r'<span style="color:\1">\2</span>',
        s,
        flags=re.S,
    )
    s = s.replace("[b]", "**").replace("[/b]", "**")
    s = s.replace("[i]", "*").replace("[/i]", "*")

    lines = s.split("\n")
    out: list[str] = []
    stack: list[str] = []

    def close_lists() -> None:
        while stack:
            out.append(f"</{stack.pop()}>")

    for raw in lines:
        line = raw.strip()
        if line in ("[list]", "[list=1]"):
            tag = "ol" if line == "[list=1]" else "ul"
            stack.append(tag)
            out.append(f"<{tag}>")
            continue
        if line == "[/list]":
            if stack:
                out.append(f"</{stack.pop()}>")
            continue
        item = re.match(r"^\[\*\](.*)(?:\[/\*\])?$", line)
        if item:
            body = item.group(1).strip()
            if body.endswith("[/*]"):
                body = body[:-4].strip()
            out.append(f"<li>{body}</li>")
            continue
        if not line:
            if not stack:
                out.append("")
            continue
        if not stack:
            out.append(line)

    close_lists()
    html_out = "\n".join(out)
    html_out = re.sub(r"\n{3,}", "\n\n", html_out).strip() + "\n"
    return html_out


def extract_latest_changelog(details_md: str) -> tuple[str, str]:
    match = re.search(
        r"### v([0-9.]+)\n```text\n(.*?)```",
        details_md,
        flags=re.S,
    )
    if not match:
        return "", ""
    return match.group(1), match.group(2).strip()


def build_html(payload: dict) -> str:
    data_json = json.dumps(payload, ensure_ascii=False)
    return f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{html.escape(payload["title"])} — 7DaysToDieMods PublishHelp</title>
  <style>
    :root {{
      --bg: #16141c;
      --card: #221f2b;
      --ink: #f4f1ea;
      --muted: #b7b0c4;
      --line: #5F5980;
      --head: #8DB580;
      --hi: #DDCDFA;
      --btn: #3d3550;
    }}
    * {{ box-sizing: border-box; }}
    body {{
      margin: 0;
      font-family: Segoe UI, sans-serif;
      background: var(--bg);
      color: var(--ink);
      line-height: 1.45;
    }}
    header {{
      padding: 20px 24px 12px;
      border-bottom: 1px solid var(--line);
    }}
    header h1 {{ margin: 0 0 6px; font-size: 1.35rem; color: var(--head); }}
    header p {{ margin: 0; color: var(--muted); }}
    main {{ max-width: 920px; margin: 0 auto; padding: 16px 20px 48px; }}
    .remind {{
      background: #2a2436;
      border: 1px solid var(--line);
      border-radius: 10px;
      padding: 12px 14px;
      color: var(--muted);
      margin: 16px 0 20px;
    }}
    .field {{
      background: var(--card);
      border: 1px solid #3a3448;
      border-radius: 10px;
      padding: 12px 14px;
      margin: 0 0 12px;
    }}
    .row {{
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 8px;
    }}
    .label {{ font-weight: 700; }}
    .hint {{ color: var(--muted); font-size: 0.85rem; }}
    button {{
      background: var(--btn);
      color: var(--hi);
      border: 1px solid var(--line);
      border-radius: 8px;
      padding: 6px 12px;
      cursor: pointer;
      font-weight: 600;
    }}
    button.ok {{ background: #2d4a36; color: #c8efc4; }}
    pre, .preview {{
      white-space: pre-wrap;
      word-break: break-word;
      background: #120f18;
      border-radius: 8px;
      padding: 10px 12px;
      margin: 0;
      max-height: 280px;
      overflow: auto;
      font-size: 0.92rem;
    }}
    .preview {{ max-height: 420px; }}
    .btns {{ display: flex; gap: 8px; flex-wrap: wrap; }}
  </style>
</head>
<body>
  <header>
    <h1 id="page-title"></h1>
    <p>One-file 7DaysToDieMods packet. Click Copy, then paste into the matching site field.</p>
  </header>
  <main>
    <div class="remind">
      Skip <b>File</b> and <b>External link</b>. Drag images yourself.
      After the page exists, use <b>GitHub sync</b> with the repo name below.
      Changelog is plain lines, no bullets.
    </div>
    <div id="fields"></div>
  </main>
  <script>
    const DATA = {data_json};
    document.getElementById("page-title").textContent = DATA.title + "  v" + DATA.version;

    const fields = [
      ["title", "Title", "Site title field"],
      ["summary", "Summary", "The one-line field"],
      ["server_side", "Server / Client", "Dropdown: Server Side Only / Client Side Only / Server & Client Side"],
      ["category", "Category", "Dropdown"],
      ["game_version", "Game version", "Dropdown"],
      ["credits", "Credits", "Final publish page"],
      ["github_repo", "GitHub sync repo", "Connect this repo on the last page"],
      ["version", "Version number", "Changelog page version field"],
      ["changelog", "Changelog", "Paste these lines into the text field. No bullets."],
      ["description_html", "Description (HTML)", "Try this first if BBCode looked wrong"],
      ["description_bbcode", "Description (BBCode)", "Use the editor BBCode tab"]
    ];

    const root = document.getElementById("fields");
    for (const [key, label, hint] of fields) {{
      const wrap = document.createElement("section");
      wrap.className = "field";
      wrap.innerHTML =
        '<div class="row"><div><div class="label"></div><div class="hint"></div></div>' +
        '<div class="btns"><button type="button">Copy</button></div></div>' +
        (key === "description_html" ? '<div class="preview"></div>' : "<pre></pre>");
      wrap.querySelector(".label").textContent = label;
      wrap.querySelector(".hint").textContent = hint;
      const box = wrap.querySelector("pre, .preview");
      box.textContent = DATA[key] || "";
      wrap.querySelector("button").addEventListener("click", async (ev) => {{
        const btn = ev.currentTarget;
        try {{
          await navigator.clipboard.writeText(DATA[key] || "");
          btn.textContent = "Copied";
          btn.classList.add("ok");
          setTimeout(() => {{ btn.textContent = "Copy"; btn.classList.remove("ok"); }}, 1200);
        }} catch (err) {{
          btn.textContent = "Copy failed";
        }}
      }});
      root.appendChild(wrap);
    }}
  </script>
</body>
</html>
"""


def main() -> int:
    mod = "AGF-BackpackPlus-072Slots"
    details = load_text(os.path.join(NEXUS_HELP_DIR, f"{mod}.md"))
    bbcode_match = re.search(r"(?ms)^##\s*1b\)\s*Full Description\s*\n.*?```text\s*\n(.*?)```", details)
    bbcode = (bbcode_match.group(1).strip() + "\n") if bbcode_match else ""
    version, changelog = extract_latest_changelog(details)
    payload = {
        "title": "AGF - V3 - BackpackPlus - 072Slots",
        "summary": "72-slot backpack, encumbrance accounted for, light UI shrinking.",
        "server_side": "Server Side Only",
        "category": "Quality of Life",
        "game_version": "V3 Mods",
        "credits": "AuroraGiggleFairy",
        "github_repo": "AuroraGiggleFairy/AGF-BackpackPlus-072Slots",
        "version": version or "4.1.1",
        "changelog": changelog,
        "description_bbcode": bbcode,
        "description_html": bbcode_to_site_html(bbcode),
    }
    out_dir = os.path.join(PUBLISH_HELP_DIR, mod)
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, f"{mod}.html")
    with open(out_path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(build_html(payload))
    print(out_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
