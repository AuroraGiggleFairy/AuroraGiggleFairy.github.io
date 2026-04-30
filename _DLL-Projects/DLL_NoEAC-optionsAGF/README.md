# optionsAGF

Options host DLL for AGF ESC options rows.

## Current row

- Screamer Alert mode (tri-state): `Off`, `On`, `On + #`
- Visual Entity Tracker mode (on/off): `Off`, `On`

## Requirements

- Requires ESC window mod content to render in the `options` rect.
- Works without editing other mods.
- Detects ScreamerAlert runtime presence and shows `Not Installed` when unavailable.

## Multiplayer behavior

- Settings are stored per-player key in `Config/options_settings.tsv`.
- Each player can choose their own mode.

## Console fallback

- `agfoptions` shows current rows and values.
- `agfoptions screamer_alert_mode off`
- `agfoptions screamer_alert_mode on`
- `agfoptions screamer_alert_mode numbers`
- `agfoptions screamer_alert_mode cycle`
- `agfoptions visual_entity_tracker_mode off`
- `agfoptions visual_entity_tracker_mode on`
