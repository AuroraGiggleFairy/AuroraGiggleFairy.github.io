# ESCWindowPlus Pause Notes

Date: 2026-03-30

## 1) Possible Separator Line When Columns Are Used
- Idea: Add an optional vertical separator between body columns when bodyColumns is 2 or 3.
- Goal: Improve visual separation and readability of split content blocks.
- Suggested approach:
  - Add optional settings in ESCMenu.texts.txt, for example:
    - bodyColumnSeparator: true|false
    - bodyColumnSeparatorColor: R,G,B,A
    - bodyColumnSeparatorWidth: integer pixels
  - Render a sprite/rect between generated column labels in windows.xml output.
  - Apply consistently to Color, HighVisibility, join pages, and join HC pages.

## 2) Investigate Game Automatic Text Auto-Resizing
- Topic: Determine whether 7DTD UI labels are auto-adjusting effective text size or wrap behavior at runtime.
- Why this matters: Box-width math can look correct in generated XML but still wrap differently in-game.
- Investigation notes:
  - Compare same label text at identical font_size across:
    - normal page vs join page
    - Color vs HighVisibility
  - Test with and without effects (Outline8) to detect effective glyph-size differences.
  - Verify whether UI scaling or DPI options influence line wrap thresholds.
  - Capture exact longest test lines to tune estimator against real wrap breakpoints.

## Current Baseline Before Pause
- Body label temporary debug background is enabled via nested sprite in generated body labels.
- Box-centering estimator was widened to reduce wrapping; latest page 8 sample is wider than prior passes.

## Resume Checklist
- Re-test page 8 columns with current settings.
- Decide if separator feature should be added now or later.
- Run one controlled auto-resize experiment and document findings.
