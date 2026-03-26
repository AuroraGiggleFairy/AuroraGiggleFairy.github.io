# AGF Discord Update Template

Use this template for release posts in Discord.

## Copy/Paste Format

<!-- DISCORD_TEMPLATE_START -->
#   GigglePack v{{GIGGLEPACK_VERSION}} - *{{RELEASE_STAMP}}*
## [Click Here to Download]({{LATEST_ZIP_URL}})
--------------------------------------------------------------------
###  **Change summary: +{{NEW_COUNT}} new, ~{{UPDATED_COUNT}} updated, -{{REMOVED_COUNT}} removed**
- **New mods:**
{{NEW_MOD_LINES}}
- **Updated existing mods:**
{{UPDATED_MOD_LINES}}
- **Removed mods:**
{{REMOVED_MOD_LINES}}
--------------------------------------------------------------------
## Remember, all downloads are available here:  https://auroragigglefairy.github.io/
<!-- DISCORD_TEMPLATE_END -->

## Notes

- Remove any section that has no entries (for example, remove "Removed mods" when count is 0).
- Use `{{NEW_MOD_LINES}}`, `{{UPDATED_MOD_LINES}}`, and `{{REMOVED_MOD_LINES}}` to keep sections uncapped.
- Keep indentation exactly as shown for clean Discord formatting.
- For v1.0.0 baseline style, keep only the lines you want visible and delete unused placeholders.
