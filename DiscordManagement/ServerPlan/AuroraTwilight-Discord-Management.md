# Aurora Twilight Discord Management

This document is the concise, current source of truth for operating Aurora Twilight.

## 1. Purpose

Aurora Twilight is a welcoming 7 Days to Die community focused on:

1. Friendly support for setup, mods, and compatibility.
2. Clear AGF mod updates and status posts.
3. Community interaction (chat, media, group play, streams).
4. Lightweight onboarding and self-service role selection.

## 2. How To Use This Document

1. Use this file to review behavior and intent quickly.
2. Use discord_server_plan.json (in this same ServerPlan folder) as the technical enforcement source.
3. If this file and automation differ, update both in the same change.

## 3. Roles (Operational)

- The Giggle Fairy: owner/admin authority.
- 2nd In Command: high-trust operations and moderation support.
- Moderator: moderation and support workflow authority.
- Survivor: default member role.
- Survivor baseline: one-time backfill completed for existing members.
- Mod Tester, Server Player, Streamer, Modder, Conan Exiles, Helldivers 2: opt-in functional roles.
- AGF-ServerBot (optional): automation for ticket launcher workflows.
- Role choosing is handled through Discord onboarding prompts (no third-party role picker required).

## 4. Permission Rules

- View: can see channel.
- Post: can send normal messages in the channel.
- React: can add reactions.
- Threads (Everyone): thread behavior for normal members.
- Read history: for message channels, read history is automatically allowed whenever view is allowed (unless explicitly overridden in config).

## 5. Full Channel Permission Matrix

| Category | Channel | Type | Everyone View | Everyone Post | Everyone React | Threads (Everyone) | Special role overrides | Read history |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| FOR AGF | my-notes | text | deny | deny | deny | inherited (not explicitly set) | The Giggle Fairy + Moderator + AGF-ServerBot full access | denied for Everyone |
| FOR AGF | bot-updates | text | deny | deny | deny | inherited (not explicitly set) | The Giggle Fairy + Moderator + AGF-ServerBot full access | denied for Everyone |
| ENTRANCE PORTAL | welcome | text | allow | deny | allow | inherited (not explicitly set) | Moderator can post | allow when view=allow |
| ENTRANCE PORTAL | rules | text | allow | deny | allow | inherited (not explicitly set) | Moderator can post | allow when view=allow |
| ENTRANCE PORTAL | start-here | text | allow | deny | allow | inherited (not explicitly set) | Moderator can post | allow when view=allow |
| ENTRANCE PORTAL | roles-selection | text | allow | deny | allow | inherited (not explicitly set) | AGF-ServerBot + Moderator can post | allow when view=allow |
| ENTRANCE PORTAL | pronoun-requests | text | allow | deny | allow | inherited (not explicitly set) | Survivor + Moderator can post/react | allow when view=allow |
| AGF MODS & UPDATES | updates | announcement | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy + 2nd In Command can post | allow when view=allow |
| AGF MODS & UPDATES | latest-mod-updates | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy can post | allow when view=allow |
| AGF MODS & UPDATES | mod-download-links | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy can post | allow when view=allow |
| AGF MODS & UPDATES | from-the-giggle-fairy | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy can post | allow when view=allow |
| AGF MODS & UPDATES | support-the-creator | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy can post | allow when view=allow |
| AGF MODS & UPDATES | test-for-agf | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| NEED HELP? | agf-full-mod-guide | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy + 2nd In Command can post | allow when view=allow |
| NEED HELP? | ask-for-help-here | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| NEED HELP? | live-help | voice | allow | n/a | n/a | n/a (voice channel) | Everyone can connect | n/a (voice channel) |
| REQUESTS? | compatibility-requests | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| REQUESTS? | localization-requests | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| REQUESTS? | mod-requests | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| 7D2D HANGOUT | 7d2d-media | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| 7D2D HANGOUT | 7d2d-discussions | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| 7D2D HANGOUT | community-mods | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| 7D2D HANGOUT | looking-for-group | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| 7D2D HANGOUT | fun-pimps-announcements | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy + 2nd In Command + Moderator can post/react | allow when view=allow |
| 7D2D COMMUNITY SERVER | agf-server-info | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy + 2nd In Command can post | allow when view=allow |
| 7D2D COMMUNITY SERVER | server-chat | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| 7D2D COMMUNITY SERVER | server-updates | text | allow | deny | allow | inherited (not explicitly set) | The Giggle Fairy + 2nd In Command can post | allow when view=allow |
| 7D2D COMMUNITY SERVER | playing-now | voice | allow | n/a | n/a | n/a (voice channel) | Everyone can connect | n/a (voice channel) |
| STREAM LOUNGE | promote-your-stream | text | allow | deny | allow | inherited (not explicitly set) | Streamer + The Giggle Fairy + 2nd In Command can post | allow when view=allow |
| STREAM LOUNGE | clips | text | allow | deny | allow | inherited (not explicitly set) | Streamer + The Giggle Fairy + 2nd In Command can post | allow when view=allow |
| STREAM LOUNGE | lounge-chat | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |
| STREAM LOUNGE | waiting-room | voice | allow | n/a | n/a | n/a (voice channel) | Everyone can connect | n/a (voice channel) |
| STREAM LOUNGE | streaming | voice | deny | n/a | n/a | n/a (voice channel) | The Giggle Fairy + 2nd In Command + Moderator can view/connect | n/a (voice channel) |
| CONAN EXILES | looking-for-group | text | deny | deny | deny | inherited (not explicitly set) | Conan Exiles + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| CONAN EXILES | conan-chat | text | deny | deny | deny | inherited (not explicitly set) | Conan Exiles + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| CONAN EXILES | mod-support | text | deny | deny | deny | inherited (not explicitly set) | Conan Exiles + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| CONAN EXILES | conan-voice | voice | deny | n/a | n/a | n/a (voice channel) | Conan Exiles + The Giggle Fairy + 2nd In Command + Moderator can view/connect | n/a (voice channel) |
| HELLDIVERS 2 | giggle-battle-recommendations | text | deny | deny | deny | inherited (not explicitly set) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| HELLDIVERS 2 | chat | text | deny | deny | deny | inherited (not explicitly set) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| HELLDIVERS 2 | rankings | text | deny | deny | deny | inherited (not explicitly set) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| HELLDIVERS 2 | looking-for-group | text | deny | deny | deny | inherited (not explicitly set) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| HELLDIVERS 2 | helldivers-chat | text | deny | deny | deny | inherited (not explicitly set) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can post/react | allow for allowed roles |
| HELLDIVERS 2 | helldivers-voice | voice | deny | n/a | n/a | n/a (voice channel) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can view/connect | n/a (voice channel) |
| HELLDIVERS 2 | giggle-squad | voice | deny | n/a | n/a | n/a (voice channel) | Helldivers 2 + The Giggle Fairy + 2nd In Command + Moderator can view/connect | n/a (voice channel) |
| SURVIVE THE NIGHTS GAME | agf-tips | text | allow | allow | allow | inherited (not explicitly set) | no additional overrides | allow when view=allow |

## 6. Onboarding (Current)

- Default channels: welcome, rules, start-here, roles-selection, pronoun-requests.
- Prompt set:
1. What do you seek in my realm? (required; each option also assigns Survivor)
2. Tell me about yourself.
3. May I interest you in joining me on these adventures? (includes Server Player, Mod Tester, Conan Exiles, Helldivers 2)
4. Pronouns.

- Pronoun set currently in use:
She/Her/Hers/Herself, He/Him/His/Himself, They/Them/Theirs/Themself, Ze/Hir/Hirs/Hirself, Xe/Xem/Xirs/Xemself, Ver/Vir/Vis/Verself, Te/Tem/Ter/Temself, Ey/Em/Eir/Emself, It/Its/Its/Itself, Any/All Pronouns.

## 7. Rules and Support Flow (Current)

- Rules gate is active through rules/membership screening behavior.
- New members receive Survivor through the required first onboarding prompt after rules acceptance.
- Game role selection (Conan Exiles and Helldivers 2) is handled by onboarding options in prompt #3.
- ask-for-help-here, compatibility-requests, localization-requests, and mod-requests are open support/request channels.
- pronoun-requests is regular text workflow for member requests and moderator actioning.

## 8. Operating Checklist

When making changes:

1. Update discord_server_plan.json.
2. Update this document only if human-facing behavior changed.
3. Run run_discord_plan.ps1 (dry-run first, then -Apply).
4. Validate in Discord UI for channel visibility, posting, reactions, and role panel behavior.
