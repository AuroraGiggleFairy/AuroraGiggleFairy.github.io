import csv
import pathlib

repo = pathlib.Path('c:/GitHub/7D2D-Mods')
game = pathlib.Path('c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config/Localization.csv')
out = repo / '_DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Config/Localization.csv'
legacy = repo / '_DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Config/Localization.txt'

with game.open('r', encoding='utf-8', newline='') as f:
    g = csv.reader(f)
    game_header = next(g)
with out.open('r', encoding='utf-8', newline='') as f:
    r = csv.reader(f)
    out_header = next(r)
    rows = list(r)

bad = [i+2 for i,row in enumerate(rows) if len(row) != len(out_header)]
key_idx = 0
keep_idx = out_header.index('KeepLoaded') if 'KeepLoaded' in out_header else -1
ctx_idx = out_header.index('Context / Alternate Text') if 'Context / Alternate Text' in out_header else -1
eng_idx = out_header.index('english') if 'english' in out_header else -1

agf_rows = [row for row in rows if row and row[0].lower().startswith('agf')]
nonempty_keep = sum(1 for row in rows if keep_idx >= 0 and keep_idx < len(row) and (row[keep_idx] or '').strip())
nonempty_ctx = sum(1 for row in rows if ctx_idx >= 0 and ctx_idx < len(row) and (row[ctx_idx] or '').strip())
missing_english = sum(1 for row in agf_rows if eng_idx >=0 and eng_idx < len(row) and not (row[eng_idx] or '').strip())

legacy_count = 0
if legacy.exists():
    with legacy.open('r', encoding='utf-8', newline='') as f:
        rr = csv.reader(f)
        next(rr, None)
        legacy_count = sum(1 for _ in rr)

print('game_header_cols', len(game_header))
print('out_header_cols', len(out_header))
print('header_exact_match', game_header == out_header)
print('out_row_count', len(rows))
print('bad_row_count', len(bad))
print('agf_row_count', len(agf_rows))
print('agf_rows_missing_english', missing_english)
print('rows_with_keepLoaded', nonempty_keep)
print('rows_with_context_alt_text', nonempty_ctx)
print('legacy_txt_row_count', legacy_count)
print('sample_bad_rows', bad[:20])
