import argparse
import asyncio
import csv
import json
import os
import re
import sys
from dataclasses import dataclass
from datetime import datetime, timezone
from typing import Dict, List, Optional

import discord

DISCORD_BOT_TOKEN_ENV_VAR = "AGF_DISCORD_BOT_TOKEN"


@dataclass
class IntakeConfig:
    guild_id: int
    forum_channel_id: int
    forum_channel_name: str
    tracker_csv: str
    seed_csv: str
    tag_needed: str
    tag_submitted: str


def load_config(config_path: str) -> IntakeConfig:
    with open(config_path, "r", encoding="utf-8") as f:
        raw = json.load(f)

    return IntakeConfig(
        guild_id=int(raw["guild_id"]),
        forum_channel_id=int(raw.get("forum_channel_id", 0) or 0),
        forum_channel_name=str(raw.get("forum_channel_name", "mod-image-intake")),
        tracker_csv=str(raw.get("tracker_csv", os.path.join("Workflow", "Discord", "mod-image-intake-tracker.csv"))),
        seed_csv=str(raw.get("seed_csv", os.path.join("Workflow", "Discord", "mod-image-intake-thread-seed.csv"))),
        tag_needed=str(raw.get("tag_needed", "Needed")),
        tag_submitted=str(raw.get("tag_submitted", "Submitted")),
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="AGF Discord mod-image-intake forum helper")
    parser.add_argument(
        "--config",
        default=os.path.join("Workflow", "Discord", "mod-image-intake-config.json"),
        help="Path to mod image intake config JSON",
    )
    parser.add_argument(
        "--action",
        choices=["inspect", "seed", "sync", "seed-sync", "update-messages"],
        default="inspect",
        help="Action to run",
    )
    parser.add_argument(
        "--discord-bot-token",
        default="",
        help=f"Discord bot token. If omitted, uses env {DISCORD_BOT_TOKEN_ENV_VAR}.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would happen without creating threads or updating tracker",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Optional limit for number of seed rows to process (0 = no limit)",
    )
    return parser.parse_args()


def read_csv_rows(csv_path: str) -> List[Dict[str, str]]:
    with open(csv_path, "r", encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f))


def write_csv_rows(csv_path: str, rows: List[Dict[str, str]]) -> None:
    if not rows:
        return
    fieldnames = list(rows[0].keys())
    with open(csv_path, "w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def is_image_attachment(attachment: discord.Attachment) -> bool:
    content_type = (attachment.content_type or "").lower()
    if content_type.startswith("image/"):
        return True
    return attachment.filename.lower().endswith((".png", ".jpg", ".jpeg", ".webp", ".gif"))


def extract_thread_id_from_url(url: str) -> Optional[int]:
    if not url:
        return None
    m = re.search(r"/channels/(\d+)/(\d+)(?:/(\d+))?$", url.strip())
    if not m:
        return None
    return int(m.group(2))


def _sort_key(value: str) -> str:
    return (value or "").casefold()


def build_thread_title(mod_name: str) -> str:
    return f"Help! Send image for mod {mod_name}"


def build_legacy_thread_title(mod_name: str) -> str:
    return f"[Needed] {mod_name} - Image Intake"


def build_thread_body(mod_name: str, generated_image_path: str) -> str:
    return (
        "See image below for mod details.\n"
        "Need an image or a collection of images that fit a 1920x1080 or similar 16:9 ratio."
    )


class IntakeBot(discord.Client):
    def __init__(
        self,
        config: IntakeConfig,
        action: str,
        dry_run: bool,
        limit: int,
        config_path: str,
    ):
        intents = discord.Intents.none()
        intents.guilds = True
        intents.messages = True
        super().__init__(intents=intents)
        self.config = config
        self.action = action
        self.dry_run = dry_run
        self.limit = limit
        self.config_path = config_path

    async def _forum_threads_by_name(self, guild: discord.Guild, forum: discord.ForumChannel) -> Dict[str, discord.Thread]:
        threads: Dict[str, discord.Thread] = {}

        for thread in forum.threads:
            threads[thread.name] = thread

        try:
            active_threads = await guild.active_threads()
        except discord.HTTPException:
            active_threads = []

        for thread in active_threads:
            if thread.parent_id == forum.id:
                threads[thread.name] = thread

        return threads

    async def setup_hook(self) -> None:
        self.loop.create_task(self._run_once())

    async def _resolve_guild(self) -> discord.Guild:
        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            guild = await self.fetch_guild(self.config.guild_id)
        return guild

    async def _resolve_forum(self, guild: discord.Guild) -> discord.ForumChannel:
        channel: Optional[discord.abc.GuildChannel] = None

        if self.config.forum_channel_id:
            channel = guild.get_channel(self.config.forum_channel_id)
            if channel is None:
                fetched = await self.fetch_channel(self.config.forum_channel_id)
                if isinstance(fetched, discord.abc.GuildChannel):
                    channel = fetched

        if channel is None:
            channel = next(
                (c for c in guild.channels if isinstance(c, discord.ForumChannel) and c.name == self.config.forum_channel_name),
                None,
            )

        if not isinstance(channel, discord.ForumChannel):
            raise RuntimeError(
                f"Could not resolve forum channel. Checked id={self.config.forum_channel_id} and name='{self.config.forum_channel_name}'."
            )

        if self.config.forum_channel_id != channel.id:
            self.config.forum_channel_id = channel.id
            self._save_config()

        return channel

    def _save_config(self) -> None:
        payload = {
            "guild_id": self.config.guild_id,
            "forum_channel_id": self.config.forum_channel_id,
            "forum_channel_name": self.config.forum_channel_name,
            "tracker_csv": self.config.tracker_csv,
            "seed_csv": self.config.seed_csv,
            "tag_needed": self.config.tag_needed,
            "tag_submitted": self.config.tag_submitted,
        }
        with open(self.config_path, "w", encoding="utf-8") as f:
            json.dump(payload, f, indent=2)
            f.write("\n")

    @staticmethod
    def _tag_by_name(forum: discord.ForumChannel, tag_name: str) -> Optional[discord.ForumTag]:
        tag_cmp = tag_name.casefold()
        for tag in forum.available_tags:
            if tag.name.casefold() == tag_cmp:
                return tag
        return None

    async def _inspect(self, guild: discord.Guild, forum: discord.ForumChannel) -> None:
        threads_by_name = await self._forum_threads_by_name(guild, forum)
        threads = sorted(threads_by_name.values(), key=lambda t: _sort_key(t.name))

        print(f"Guild: {guild.name} ({guild.id})")
        print(f"Forum: {forum.name} ({forum.id})")
        print(f"Available tags: {', '.join(t.name for t in forum.available_tags) if forum.available_tags else '(none)'}")
        print(f"Active threads: {len(threads)}")

        preview = threads
        for thread in preview[:15]:
            tag_text = ", ".join(t.name for t in thread.applied_tags) if thread.applied_tags else "(no tags)"
            print(f"- {thread.name} | tags: {tag_text} | url: {thread.jump_url}")

    async def _seed(self, forum: discord.ForumChannel) -> None:
        seed_rows = read_csv_rows(self.config.seed_csv)
        seed_rows = sorted(seed_rows, key=lambda r: _sort_key(r.get("mod_folder", "")))
        if self.limit > 0:
            seed_rows = seed_rows[: self.limit]

        guild = await self._resolve_guild()
        existing_by_name = await self._forum_threads_by_name(guild, forum)
        needed_tag = self._tag_by_name(forum, self.config.tag_needed)

        created = 0
        skipped = 0
        for row in seed_rows:
            mod_folder = (row.get("mod_folder") or "").strip()
            image_rel = (row.get("generated_image_path") or "").strip()
            title = build_thread_title(mod_folder)
            body = build_thread_body(mod_folder, image_rel)
            if not title:
                continue

            existing = existing_by_name.get(title)
            if existing is not None:
                skipped += 1
                continue

            if self.dry_run:
                print(f"[dry-run] would create thread: {title}")
                created += 1
                continue

            files: List[discord.File] = []
            if image_rel:
                image_abs = os.path.abspath(image_rel)
                if os.path.isfile(image_abs):
                    files.append(discord.File(image_abs, filename=os.path.basename(image_abs)))

            kwargs: Dict[str, object] = {
                "name": title,
                "content": body,
                "allowed_mentions": discord.AllowedMentions.none(),
            }
            if files:
                kwargs["files"] = files
            if needed_tag is not None:
                kwargs["applied_tags"] = [needed_tag]

            result = await forum.create_thread(**kwargs)
            created += 1
            print(f"created thread: {result.thread.name} | {result.thread.jump_url}")
            # Gentle pacing helps avoid long 429 backoff windows during bulk thread creation.
            await asyncio.sleep(1.1)

        print(f"seed summary: created={created}, skipped_existing={skipped}, total_input={len(seed_rows)}")

    async def _sync(self, guild: discord.Guild, forum: discord.ForumChannel) -> None:
        tracker_rows = read_csv_rows(self.config.tracker_csv)
        tracker_rows = sorted(tracker_rows, key=lambda r: _sort_key(r.get("mod_folder", "")))
        rows_by_mod = {row.get("mod_folder", ""): row for row in tracker_rows}

        threads_by_name = await self._forum_threads_by_name(guild, forum)

        updated = 0
        for mod_folder, row in rows_by_mod.items():
            expected_title = build_thread_title(mod_folder)
            thread = threads_by_name.get(expected_title)
            if thread is None:
                thread = threads_by_name.get(build_legacy_thread_title(mod_folder))

            thread_id = extract_thread_id_from_url(row.get("discord_thread_url", ""))
            if thread is None and thread_id is not None:
                fetched = guild.get_thread(thread_id)
                if fetched is None:
                    try:
                        fetched = await self.fetch_channel(thread_id)
                    except discord.HTTPException:
                        fetched = None
                if isinstance(fetched, discord.Thread):
                    thread = fetched

            if thread is None:
                continue

            image_count = 0
            async for msg in thread.history(limit=None, oldest_first=True):
                if msg.author.bot:
                    continue
                for at in msg.attachments:
                    if is_image_attachment(at):
                        image_count += 1

            new_status = "Submitted" if image_count > 0 else "Needed"
            now_utc = datetime.now(timezone.utc).replace(microsecond=0).isoformat()

            changed = False
            if row.get("discord_thread_url", "") != thread.jump_url:
                row["discord_thread_url"] = thread.jump_url
                changed = True
            if row.get("submitted_count", "0") != str(image_count):
                row["submitted_count"] = str(image_count)
                changed = True
            if row.get("main_submission_status", "") != new_status:
                row["main_submission_status"] = new_status
                changed = True
            if row.get("intake_status", "") != new_status:
                row["intake_status"] = new_status
                changed = True
            if changed:
                row["last_update_utc"] = now_utc
                updated += 1

        if self.dry_run:
            print(f"[dry-run] sync would update {updated} tracker rows")
            return

        write_csv_rows(self.config.tracker_csv, tracker_rows)
        print(f"sync summary: updated_rows={updated}, tracker={self.config.tracker_csv}")

    async def _update_messages(self, guild: discord.Guild, forum: discord.ForumChannel) -> None:
        tracker_rows = read_csv_rows(self.config.tracker_csv)
        tracker_rows = sorted(tracker_rows, key=lambda r: _sort_key(r.get("mod_folder", "")))
        threads_by_name = await self._forum_threads_by_name(guild, forum)

        updated = 0
        missing = 0
        for row in tracker_rows:
            mod_folder = (row.get("mod_folder") or "").strip()
            if not mod_folder:
                continue

            expected_title = build_thread_title(mod_folder)
            thread = threads_by_name.get(expected_title)
            if thread is None:
                thread = threads_by_name.get(build_legacy_thread_title(mod_folder))

            if thread is None:
                thread_id = extract_thread_id_from_url(row.get("discord_thread_url", ""))
                if thread_id is not None:
                    fetched = guild.get_thread(thread_id)
                    if fetched is None:
                        try:
                            fetched = await self.fetch_channel(thread_id)
                        except discord.HTTPException:
                            fetched = None
                    if isinstance(fetched, discord.Thread):
                        thread = fetched

            if thread is None:
                missing += 1
                continue

            image_rel = (row.get("generated_image_path") or "").strip()
            new_body = build_thread_body(mod_folder, image_rel)
            new_title = build_thread_title(mod_folder)

            if self.dry_run:
                print(f"[dry-run] would update starter message: {thread.name}")
                updated += 1
                continue

            try:
                starter = await thread.fetch_message(thread.id)
            except discord.HTTPException:
                missing += 1
                continue

            if (starter.content or "").strip() == new_body.strip():
                if thread.name == new_title:
                    continue

            await starter.edit(content=new_body, allowed_mentions=discord.AllowedMentions.none())
            if thread.name != new_title:
                await thread.edit(name=new_title)
            updated += 1
            await asyncio.sleep(0.6)

        print(f"update-messages summary: updated={updated}, missing_threads={missing}, total_rows={len(tracker_rows)}")

    async def _run_once(self) -> None:
        try:
            guild = await self._resolve_guild()
            forum = await self._resolve_forum(guild)

            if self.action == "inspect":
                await self._inspect(guild, forum)
            elif self.action == "seed":
                await self._seed(forum)
            elif self.action == "sync":
                await self._sync(guild, forum)
            elif self.action == "seed-sync":
                await self._seed(forum)
                await self._sync(guild, forum)
            elif self.action == "update-messages":
                await self._update_messages(guild, forum)
            else:
                raise RuntimeError(f"Unsupported action: {self.action}")
        except Exception as ex:
            print(f"Action failed: {ex}")
        finally:
            await self.close()


def main() -> int:
    args = parse_args()

    token = (args.discord_bot_token or os.getenv(DISCORD_BOT_TOKEN_ENV_VAR, "")).strip()
    if not token:
        print(f"No bot token provided. Set --discord-bot-token or env {DISCORD_BOT_TOKEN_ENV_VAR}.")
        return 1

    config_path = os.path.abspath(args.config)
    if not os.path.isfile(config_path):
        print(f"Config file not found: {config_path}")
        return 1

    try:
        config = load_config(config_path)
    except Exception as ex:
        print(f"Failed to load config: {ex}")
        return 1

    bot = IntakeBot(
        config=config,
        action=args.action,
        dry_run=bool(args.dry_run),
        limit=max(0, int(args.limit)),
        config_path=config_path,
    )

    try:
        bot.run(token)
    except KeyboardInterrupt:
        return 0
    except Exception as ex:
        print(f"Bot startup failed: {ex}")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
