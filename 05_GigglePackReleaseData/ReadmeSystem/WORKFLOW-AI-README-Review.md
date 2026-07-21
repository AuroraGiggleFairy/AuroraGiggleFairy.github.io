# WORKFLOW - AI README Review

## Purpose
- Provide a stable wording-review ruleset for section-by-section README work.
- Keep reviews consistent across chats.
- Default to discussion-only unless implementation is explicitly requested.

## Quick Start (Read First)
- Use the User-Defined 3-Step Flow as the default operating mode.
- Step 1: Review code/config and current top README sections.
- Step 2: Provide wording suggestions for:
  - One-line Summary (72-character hard limit unless overridden)
  - FEATURES
  - OTHER DETAILS (if present)
- Step 2 output format:
  - Use a txt fenced text box.
  - Use clear indentation.
  - Wrap lines at 72 characters max.
- Step 3: Wait for explicit user confirmation (DONE) before changelog/version bump.

## Focus Model
- Work one mod at a time.
- Work one section at a time.
- Prioritize clarity for mixed skill levels: new players, server admins, advanced users.
- Treat code behavior as source of truth for wording.

## Model and Token Policy
- Default mode for README wording work: Ask.
- Default effort for normal production wording passes: Medium.
- Use High for workflow calibration turns (rule updates, style-conflict resolution, process tuning).
- Use Low only for micro-polish on one to two lines.
- Keep per-turn scope to one mod and one section.
- For token efficiency, send only the relevant section text unless cross-section context is required.

## Code Truth Rule (Required)
- README wording must match what the code/config actually does.
- Do not infer behavior from old wording alone.
- If behavior cannot be confirmed from source, mark wording as unverified and avoid hard claims.
- If wording and code conflict, code wins and wording must be revised.

## Factual Language Rule (Required)
- Use factual, player-observable, code-traceable wording only.
- Do not use opinion or aesthetic claims (for example: cleaner, nicer, better) unless measured and verifiable.
- For FEATURES, describe practical gameplay outcomes, not subjective judgments.
- For OTHER DETAILS, include technical specifics that directly support the FEATURES claims.

## Review Sequence
- Use this as expanded guidance. For execution gating, the
  User-Defined 3-Step Flow takes precedence.
- Step 1: Confirm target mod and target section.
- Step 2: Verify behavior against source (Config/XML/DLL/script as applicable).
- Step 3: Evaluate wording against section intent.
- Step 4: Suggest revised wording options.
- Step 5: Mark fit risk for image/publication surfaces.
- Step 6: Wait for user pick or merge direction before any implementation.

## Execution Contract (User-Defined 3-Step Flow)
- Step 1: Review code/config behavior and current README top sections for the target mod.
- Step 2: Provide wording suggestions for the intended section types:
  - One-line Summary
  - FEATURES
  - OTHER DETAILS (if present)
- Step 2 behavior note: If current wording has issues, correct by suggestion output; do not enumerate error lists unless the user explicitly asks.
- Step 2 output format: present suggestions in txt fenced text boxes with clear indentation and wrapped lines.
- Step 3: Do not run changelog or version bump until the user explicitly confirms completion (for example: DONE).
- After explicit completion confirmation, execute Post-Top-Section Finalization defaults unless user overrides them.

## Post-Top-Section Finalization (Default)
- Trigger: after top README section wording is finalized, unless user explicitly says to skip.
- Update ModInfo version using default semantic bump rule:
  - Default bump: middle version number (minor).
  - Patch resets to 0.
  - Example: 2.1.7 -> 2.2.0.
- Do not manually update README MOD SCOPE `Mod Version`; this field is script-automated.
- Update changelog by adding a new top entry that matches the new version.
- Use this changelog block exactly, preserving indentation style:
  - vX.X.X
      - Overhauled all README files and their management workflow.
      - Updated mod descriptions and details.
      - Moved preview images into the designated folder.
- Keep existing changelog separator and spacing format consistent with the file.
- If user requests a different bump type or changelog text, user instruction overrides these defaults.

## Verification Minimums (Per Pass)
- Confirm at least one direct source reference for each substantive claim.
- For FEATURES, verify each bullet is supported by source behavior.
- For OTHER DETAILS, verify mechanics, constraints, and compatibility statements.
- Prefer specific wording (what it does) over speculative wording (what it might do).

## Output Contract (Default)
- Provide exactly two options unless the user explicitly asks for a different count.
- Present suggestions in a txt fenced block.
- Wrap suggestion lines at 72 characters max.
- Use 4-space indentation for wrapped continuation lines.
- Keep indentation stable across turns. Do not change indentation style unless the user explicitly asks.
- Use this shape:
  - Suggestion 1
  - One-line Summary
  - FEATURES
  - OTHER DETAILS (if present)
  - Suggestion 2
  - One-line Summary
  - FEATURES
  - OTHER DETAILS (if present)
  - Recommended: 1 or 2, with a short reason.
- Keep rationale concise by default.
- If a substantive claim cannot be verified from source, mark it Unverified and avoid hard-claim wording.

## Indentation Contract (Required)
- One-line Summary output:
  - Plain sentence only.
  - No dash prefix.
  - Column 0 (no leading indentation).
  - 72-character maximum unless user explicitly overrides.
- FEATURES and OTHER DETAILS output:
  - Main bullets must use exactly one indent level, then dash.
  - In txt output, one indent level = 4 leading spaces.
  - Format: `    - ` followed by text.
- Changelog output:
  - Bullet lines must use exactly two indent levels, then dash.
  - In txt output, two indent levels = 8 leading spaces.
  - Format: `        - ` followed by text.
- Wrapped continuation lines:
  - Keep logical alignment with the parent line.
  - Do not reduce indentation depth on wrapped lines.

## Output Contract (Calibration Turns)
- Use this shape when actively tuning workflow rules:
  - Suggestion 1
  - Suggestion 2
  - Recommended: 1 or 2, with a short reason.
  - Workflow Delta:
    - Keep
    - Add
    - Adjust
    - Remove

## Section Intent Rules
- One-line Summary (ModInfo + top README)
  - Goal: one clear value sentence.
  - Hard limit: 72 characters maximum unless user explicitly overrides.

- FEATURES
  - Goal: quick-scan benefits list.
  - Target: 3 to 6 bullets.
  - One core idea per bullet when possible.
  - Avoid pointer-only bullets in README FEATURES.

- OTHER DETAILS
  - Goal: expanded mechanics and nuance.
  - Longer technical notes belong here.

## Surface Rules
- README FEATURES
  - No pointer-only lines like see-readme phrasing.

- Image and publication feature surfaces
  - Overflow pointer is allowed.
  - Keep top-priority bullets first.
  - If truncated, append: See this mod's README for full details.

## Image Fit Heuristics (Current Setup)
- Rough pre-shrink wrapped-line capacity: about 16 lines.
- Rough single-line bullets with separators: about 7 bullets.
- Typical one-line char fit at current style: about 30 to 35 chars.
- These are guidance values, not strict pixel guarantees.

## Prioritization When Space Is Tight
- Priority 1: Core player benefit.
- Priority 2: Main behavior or mechanic.
- Priority 3: Compatibility or safety-impacting info.
- Priority 4: Nice-to-have extras.

## Quality Checklist
- New player can understand value in 5 seconds.
- Admin can assess deploy relevance quickly.
- Advanced user can find deeper behavior in OTHER DETAILS.
- Avoid unnecessary duplicate messaging between summary and features.
- Intentional overlap is allowed when the one-line summary must stand alone for quick readers.
- All claims are code-accurate and currently verifiable.

## Chat Redirect Use
- To re-focus in a new chat, use:
  - Follow 05_GigglePackReleaseData/ReadmeSystem/WORKFLOW-AI-README-Review.md using the Quick Start and User-Defined 3-Step Flow.
- Redirect phrase for the 3-step flow:
  - Follow 05_GigglePackReleaseData/ReadmeSystem/WORKFLOW-AI-README-Review.md using the Execution Contract (User-Defined 3-Step Flow): review code + current sections, provide suggestions for Summary/FEATURES/OTHER DETAILS, then wait for DONE before changelog/version bump.

## New Chat Bootstrap
- Preferred first message for normal README wording passes:
  - Follow 05_GigglePackReleaseData/ReadmeSystem/WORKFLOW-AI-README-Review.md. Wording review only, one mod and one section at a time, default to Medium effort, and output Suggestion 1, Suggestion 2, and Recommended in a txt boxed format with 72-character wrapping.
- Preferred first message when refining workflow/rules in that turn:
  - Follow 05_GigglePackReleaseData/ReadmeSystem/WORKFLOW-AI-README-Review.md. Wording review only, one mod and one section at a time, use High effort for calibration, output Suggestion 1, Suggestion 2, Recommended, and Workflow Delta (Keep/Add/Adjust/Remove).
