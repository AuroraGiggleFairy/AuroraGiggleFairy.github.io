import argparse
import asyncio
import json
import os
import sys
import time
from urllib.parse import urlparse
from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple

import discord
from discord import app_commands
from discord.ext import tasks
from yt_dlp import YoutubeDL

DISCORD_BOT_TOKEN_ENV_VAR = "AGF_DISCORD_BOT_TOKEN"


class _SilentYdlLogger:
    def debug(self, msg: str) -> None:
        return

    def info(self, msg: str) -> None:
        return

    def warning(self, msg: str) -> None:
        return

    def error(self, msg: str) -> None:
        return


@dataclass
class RoleEntry:
    emoji: str
    role_name: str
    label: str


@dataclass
class PanelConfig:
    title: str
    description: str
    note: str
    roles: List[RoleEntry]


@dataclass
class AppConfig:
    guild_id: int
    channel_id: int
    message_ids: List[int]
    panels: List[PanelConfig]
    live_announcement_channel_id: int
    live_announcement_channel_name: str
    streamer_role_name: str
    stream_check_interval_seconds: int
    stream_grace_seconds: int


def load_config(path: str) -> AppConfig:
    with open(path, "r", encoding="utf-8") as f:
        raw = json.load(f)

    panels: List[PanelConfig] = []
    for panel_raw in raw.get("panels", []):
        roles = [
            RoleEntry(
                emoji=str(item["emoji"]),
                role_name=str(item["role_name"]),
                label=str(item.get("label", item["role_name"])),
            )
            for item in panel_raw.get("roles", [])
        ]
        panels.append(
            PanelConfig(
                title=str(panel_raw["title"]),
                description=str(panel_raw["description"]),
                note=str(panel_raw.get("note", "")),
                roles=roles,
            )
        )

    return AppConfig(
        guild_id=int(raw["guild_id"]),
        channel_id=int(raw["channel_id"]),
        message_ids=[int(mid) for mid in raw.get("message_ids", [])],
        panels=panels,
        live_announcement_channel_id=int(raw.get("live_announcement_channel_id", 0) or 0),
        live_announcement_channel_name=str(raw.get("live_announcement_channel_name", "promote-your-stream")),
        streamer_role_name=str(raw.get("streamer_role_name", "Streamer")),
        stream_check_interval_seconds=max(30, int(raw.get("stream_check_interval_seconds", 120) or 120)),
        stream_grace_seconds=max(0, int(raw.get("stream_grace_seconds", 60) or 60)),
    )


def save_config(path: str, config: AppConfig) -> None:
    payload = {
        "guild_id": config.guild_id,
        "channel_id": config.channel_id,
        "message_ids": config.message_ids,
        "live_announcement_channel_id": config.live_announcement_channel_id,
        "live_announcement_channel_name": config.live_announcement_channel_name,
        "streamer_role_name": config.streamer_role_name,
        "stream_check_interval_seconds": config.stream_check_interval_seconds,
        "stream_grace_seconds": config.stream_grace_seconds,
        "panels": [
            {
                "title": p.title,
                "description": p.description,
                "note": p.note,
                "roles": [
                    {"emoji": r.emoji, "role_name": r.role_name, "label": r.label}
                    for r in p.roles
                ],
            }
            for p in config.panels
        ],
    }
    with open(path, "w", encoding="utf-8") as f:
        json.dump(payload, f, indent=2)
        f.write("\n")


class ReactionRoleBot(discord.Client):
    def __init__(self, config: AppConfig, config_path: str, watchlist_path: str, publish_on_start: bool, purge_on_start: bool):
        intents = discord.Intents.none()
        intents.guilds = True
        intents.members = True
        intents.guild_reactions = True
        super().__init__(intents=intents)
        self.tree = app_commands.CommandTree(self)
        self.config = config
        self.config_path = config_path
        self.watchlist_path = watchlist_path
        self.publish_on_start = publish_on_start
        self.purge_on_start = purge_on_start
        self.lookup: Dict[int, Dict[str, int]] = {}
        self.live_state: Dict[int, Dict[str, object]] = self._load_watchlist()
        self.stream_watch_loop.change_interval(seconds=self.config.stream_check_interval_seconds)

    def _load_watchlist(self) -> Dict[int, Dict[str, object]]:
        if not os.path.isfile(self.watchlist_path):
            return {}
        try:
            with open(self.watchlist_path, "r", encoding="utf-8") as f:
                raw = json.load(f)
            users = raw.get("users", {})
            parsed: Dict[int, Dict[str, object]] = {}
            for user_id_str, user_payload in users.items():
                user_id = int(user_id_str)
                streams = user_payload.get("streams", []) if isinstance(user_payload, dict) else []
                pending = user_payload.get("pending_announcement") if isinstance(user_payload, dict) else None
                parsed_payload: Dict[str, object] = {"streams": streams}
                if isinstance(pending, dict):
                    parsed_payload["pending_announcement"] = pending
                parsed[user_id] = parsed_payload
            return parsed
        except Exception:
            return {}

    def _save_watchlist(self) -> None:
        os.makedirs(os.path.dirname(self.watchlist_path), exist_ok=True)
        payload_users: Dict[str, Dict[str, object]] = {}
        for user_id, user_payload in self.live_state.items():
            entry: Dict[str, object] = {"streams": user_payload.get("streams", [])}
            pending = user_payload.get("pending_announcement")
            if isinstance(pending, dict):
                entry["pending_announcement"] = pending
            payload_users[str(user_id)] = entry
        payload = {
            "users": payload_users,
        }
        with open(self.watchlist_path, "w", encoding="utf-8") as f:
            json.dump(payload, f, indent=2)
            f.write("\n")

    def _normalize_platform(self, platform: str) -> str:
        lowered = platform.strip().lower()
        aliases = {
            "yt": "youtube",
            "you tube": "youtube",
            "youtube": "youtube",
            "tt": "tiktok",
            "tik tok": "tiktok",
            "tiktok": "tiktok",
            "tw": "twitch",
            "twitch": "twitch",
            "kick": "kick",
        }
        return aliases.get(lowered, lowered)

    def _supported_platform(self, platform: str) -> bool:
        return platform in {"twitch", "youtube", "tiktok", "kick"}

    def _detect_platform_from_url(self, url: str) -> str:
        try:
            host = urlparse(url).netloc.strip().lower()
        except Exception:
            host = ""
        if host.startswith("www."):
            host = host[4:]

        if "twitch.tv" in host:
            return "twitch"
        if "youtube.com" in host or "youtu.be" in host:
            return "youtube"
        if "tiktok.com" in host:
            return "tiktok"
        if "kick.com" in host:
            return "kick"
        return ""

    def _probe_stream_status(self, url: str) -> Tuple[bool, str, str, str, str]:
        options = {
            "quiet": True,
            "no_warnings": True,
            "skip_download": True,
            "noplaylist": True,
            "logger": _SilentYdlLogger(),
        }
        with YoutubeDL(options) as ydl:
            info = ydl.extract_info(url, download=False)

        if info is None:
            return False, "", "", url, ""

        # yt-dlp live_status commonly returns: is_live, is_upcoming, was_live, not_live
        live_status = str(info.get("live_status", "")).strip().lower()
        is_live = live_status == "is_live" or bool(info.get("is_live"))

        title = str(info.get("title", "")).strip()
        game = str(info.get("game", "")).strip()
        if not game:
            categories = info.get("categories", [])
            if isinstance(categories, list) and categories:
                game = str(categories[0]).strip()

        out_url = str(info.get("webpage_url", "")).strip() or url
        stream_id = str(info.get("id", "")).strip() or out_url
        return is_live, title, game, out_url, stream_id

    async def _resolve_live_channel(self, guild: discord.Guild) -> Optional[discord.TextChannel]:
        if self.config.live_announcement_channel_id:
            channel = guild.get_channel(self.config.live_announcement_channel_id)
            if channel is None:
                try:
                    channel = await self.fetch_channel(self.config.live_announcement_channel_id)
                except discord.HTTPException:
                    channel = None
            if isinstance(channel, discord.TextChannel):
                return channel

        by_name = discord.utils.get(guild.text_channels, name=self.config.live_announcement_channel_name)
        if isinstance(by_name, discord.TextChannel):
            return by_name
        return None

    def _member_has_streamer_role(self, member: discord.Member) -> bool:
        target = self.config.streamer_role_name.casefold()
        return any(role.name.casefold() == target for role in member.roles)

    async def _check_single_stream(self, member: discord.Member, stream_item: Dict[str, object]) -> Tuple[bool, Optional[Dict[str, str]]]:
        url = str(stream_item.get("url", "")).strip()
        if not url:
            return False, None

        try:
            is_live, title, game, live_url, stream_id = await asyncio.to_thread(self._probe_stream_status, url)
        except Exception:
            return False, None

        was_live = bool(stream_item.get("is_live", False))
        last_stream_id = str(stream_item.get("last_stream_id", "")).strip()
        changed = False

        stream_item["is_live"] = is_live
        if was_live != is_live:
            changed = True

        if is_live:
            stream_item["last_stream_id"] = stream_id
            if stream_id != last_stream_id:
                changed = True

        if (not was_live) and is_live and stream_id and stream_id != last_stream_id:
            platform = str(stream_item.get("platform", "")).strip().title() or "Stream"
            announcement = {
                "platform": platform,
                "title": title,
                "game": game,
                "registered_url": url,
                "live_url": live_url,
            }
            return changed, announcement

        return changed, None

    async def _post_combined_live_announcement(self, member: discord.Member, announcements: List[Dict[str, str]]) -> None:
        if not announcements:
            return

        channel = await self._resolve_live_channel(member.guild)
        if channel is None:
            return

        streamer_name = member.display_name or member.name
        first_title = next((a.get("title", "").strip() for a in announcements if a.get("title", "").strip()), "")
        first_game = next((a.get("game", "").strip() for a in announcements if a.get("game", "").strip()), "")

        link_lines: List[str] = []
        for item in announcements:
            platform = item.get("platform", "Stream")
            registered_url = item.get("registered_url", "").strip()
            live_url = item.get("live_url", "").strip()
            if registered_url:
                line = f"{platform}: {registered_url}"
                if live_url and live_url != registered_url:
                    line += f" (Live: {live_url})"
                link_lines.append(line)
            elif live_url:
                link_lines.append(f"{platform}: {live_url}")

        embed = discord.Embed(
            title=f"{streamer_name} is Live!",
            color=discord.Color.red(),
        )
        if first_title:
            embed.add_field(name="Title", value=first_title, inline=False)
        if first_game:
            embed.add_field(name="Game", value=first_game, inline=True)
        if link_lines:
            embed.add_field(name="Links", value="\n".join(link_lines), inline=False)

        await channel.send(embed=embed)

    @tasks.loop(seconds=120)
    async def stream_watch_loop(self) -> None:
        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            return

        changed = False
        now_ts = time.time()
        for user_id, payload in list(self.live_state.items()):
            member = guild.get_member(user_id)
            if member is None:
                try:
                    member = await guild.fetch_member(user_id)
                except discord.HTTPException:
                    continue

            if not self._member_has_streamer_role(member):
                continue

            streams = payload.get("streams", [])
            if not isinstance(streams, list):
                continue

            user_announcements: List[Dict[str, str]] = []

            for stream_item in streams:
                if not isinstance(stream_item, dict):
                    continue
                stream_changed, announcement = await self._check_single_stream(member, stream_item)
                if stream_changed:
                    changed = True
                if announcement is not None:
                    user_announcements.append(announcement)

            pending = payload.get("pending_announcement")
            if not isinstance(pending, dict):
                pending = {"first_detected_at": 0.0, "items": []}
                payload["pending_announcement"] = pending

            pending_items = pending.get("items", [])
            if not isinstance(pending_items, list):
                pending_items = []

            if user_announcements:
                if not pending_items:
                    pending["first_detected_at"] = now_ts

                existing_keys = {
                    (str(item.get("platform", "")).strip().casefold(), str(item.get("registered_url", "")).strip().casefold())
                    for item in pending_items
                    if isinstance(item, dict)
                }
                for item in user_announcements:
                    key = (
                        str(item.get("platform", "")).strip().casefold(),
                        str(item.get("registered_url", "")).strip().casefold(),
                    )
                    if key not in existing_keys:
                        pending_items.append(item)
                        existing_keys.add(key)
                        changed = True

            live_urls = {
                str(s.get("url", "")).strip().casefold()
                for s in streams
                if isinstance(s, dict) and bool(s.get("is_live", False))
            }
            filtered_items = [
                item
                for item in pending_items
                if isinstance(item, dict)
                and str(item.get("registered_url", "")).strip().casefold() in live_urls
            ]
            if len(filtered_items) != len(pending_items):
                pending_items = filtered_items
                changed = True

            pending["items"] = pending_items
            first_detected_at = float(pending.get("first_detected_at", 0.0) or 0.0)
            if pending_items and (now_ts - first_detected_at) >= float(self.config.stream_grace_seconds):
                await self._post_combined_live_announcement(member, pending_items)
                pending["items"] = []
                pending["first_detected_at"] = 0.0
                changed = True

        if changed:
            self._save_watchlist()

    @stream_watch_loop.before_loop
    async def before_stream_watch_loop(self) -> None:
        await self.wait_until_ready()

    def _build_panel_message(self, panel: PanelConfig, panel_index: int) -> str:
        divider = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        lines: List[str] = []

        # Divider above panels 2, 3, and 4 (0-based indexes 1+).
        if panel_index >= 1:
            lines.append(divider)
            lines.append("")

        lines.extend([f"# {panel.title}", f"### *{panel.description}*"])
        if panel.note:
            lines.append("")
            lines.append(panel.note)
        lines.append("")
        for role in panel.roles:
            lines.append(f"- {role.emoji} | {role.label}")
        return "\n".join(lines)

    async def _resolve_role_id(self, guild: discord.Guild, role_name: str) -> Optional[int]:
        role = discord.utils.get(guild.roles, name=role_name)
        if role is None:
            return None
        return role.id

    async def rebuild_lookup(self) -> None:
        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            guild = await self.fetch_guild(self.config.guild_id)

        self.lookup = {}
        for idx, message_id in enumerate(self.config.message_ids):
            if idx >= len(self.config.panels):
                continue
            panel = self.config.panels[idx]
            emoji_map: Dict[str, int] = {}
            for role in panel.roles:
                role_id = await self._resolve_role_id(guild, role.role_name)
                if role_id is not None:
                    emoji_map[role.emoji] = role_id
            self.lookup[message_id] = emoji_map

    async def purge_channel_messages(self, channel: discord.TextChannel) -> int:
        count = 0
        async for msg in channel.history(limit=None, oldest_first=False):
            try:
                await msg.delete()
                count += 1
            except discord.HTTPException:
                continue
        return count

    async def publish_panels(self, purge_channel: bool) -> None:
        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            guild = await self.fetch_guild(self.config.guild_id)

        channel = guild.get_channel(self.config.channel_id)
        if channel is None:
            channel = await self.fetch_channel(self.config.channel_id)
        if not isinstance(channel, discord.TextChannel):
            raise TypeError("Configured channel is not a text channel")

        if purge_channel:
            removed = await self.purge_channel_messages(channel)
            print(f"Purged messages from channel: {removed}")

        new_message_ids: List[int] = []
        for idx, panel in enumerate(self.config.panels):
            message = await channel.send(self._build_panel_message(panel, idx))
            for role in panel.roles:
                try:
                    await message.add_reaction(role.emoji)
                except discord.HTTPException:
                    pass
            new_message_ids.append(message.id)

        self.config.message_ids = new_message_ids
        save_config(self.config_path, self.config)
        await self.rebuild_lookup()
        print("Published panels: " + ", ".join(str(mid) for mid in self.config.message_ids))

    async def _apply_role(self, guild: discord.Guild, user_id: int, role_id: int, should_add: bool) -> None:
        member = guild.get_member(user_id)
        if member is None:
            try:
                member = await guild.fetch_member(user_id)
            except discord.HTTPException:
                return

        role = guild.get_role(role_id)
        if role is None:
            return

        try:
            if should_add:
                await member.add_roles(role, reason="Reaction role add")
            else:
                await member.remove_roles(role, reason="Reaction role remove")
        except discord.HTTPException:
            return

    async def setup_hook(self) -> None:
        @self.tree.command(name="publishreactionpanels", description="Republish role panels in the configured channel")
        @app_commands.describe(purge_channel="Delete all existing messages in the channel before publishing")
        async def publishreactionpanels(interaction: discord.Interaction, purge_channel: bool = False) -> None:
            if not interaction.user.guild_permissions.manage_messages:
                await interaction.response.send_message("You need Manage Messages permission.", ephemeral=True)
                return

            await interaction.response.defer(ephemeral=True)
            try:
                await self.publish_panels(purge_channel=purge_channel)
                await interaction.followup.send("Role panels published.", ephemeral=True)
            except Exception as ex:
                await interaction.followup.send(f"Publish failed: {ex}", ephemeral=True)

        @self.tree.command(name="golive", description="Post a live announcement (Streamer role required)")
        @app_commands.describe(link="Direct stream URL")
        @app_commands.describe(title="Optional short title for this stream")
        async def golive(interaction: discord.Interaction, link: str, title: str = "") -> None:
            if interaction.guild is None or not isinstance(interaction.user, discord.Member):
                await interaction.response.send_message("This command can only be used in the server.", ephemeral=True)
                return

            if not self._member_has_streamer_role(interaction.user):
                await interaction.response.send_message(
                    f"You need the {self.config.streamer_role_name} role to use this command.",
                    ephemeral=True,
                )
                return

            guild = interaction.guild
            channel = await self._resolve_live_channel(guild)
            if channel is None:
                await interaction.response.send_message(
                    "Live announcement channel is not configured or not found.",
                    ephemeral=True,
                )
                return

            clean_link = link.strip()
            clean_title = title.strip()

            if not (clean_link.startswith("http://") or clean_link.startswith("https://")):
                await interaction.response.send_message("Please provide a full stream URL starting with http:// or https://.", ephemeral=True)
                return

            detected_platform = self._detect_platform_from_url(clean_link)
            if not detected_platform:
                await interaction.response.send_message(
                    "Unsupported link. Use Twitch, YouTube, TikTok, or Kick URLs.",
                    ephemeral=True,
                )
                return

            message_lines = [
                f"{interaction.user.display_name} is Live!",
                f"Link: {clean_link}",
            ]
            if clean_title:
                message_lines.insert(1, f"Title: {clean_title}")

            embed = discord.Embed(
                title=message_lines[0],
                color=discord.Color.red(),
            )
            if clean_title:
                embed.add_field(name="Title", value=clean_title, inline=False)
            embed.add_field(name="Links", value=f"Manual: {clean_link}", inline=False)

            await channel.send(embed=embed)

            await interaction.response.send_message(
                f"Live announcement posted in #{channel.name}.",
                ephemeral=True,
            )

        @self.tree.command(name="setstream", description="Register a stream link for automatic live announcements")
        @app_commands.describe(link="Your channel or live URL")
        async def setstream(interaction: discord.Interaction, link: str) -> None:
            if interaction.guild is None or not isinstance(interaction.user, discord.Member):
                await interaction.response.send_message("This command can only be used in the server.", ephemeral=True)
                return

            if not self._member_has_streamer_role(interaction.user):
                await interaction.response.send_message(
                    f"You need the {self.config.streamer_role_name} role before registering stream links.",
                    ephemeral=True,
                )
                return

            clean_link = link.strip()
            norm_platform = self._detect_platform_from_url(clean_link)
            if not norm_platform:
                await interaction.response.send_message(
                    "Unsupported link. Use Twitch, YouTube, TikTok, or Kick URLs.",
                    ephemeral=True,
                )
                return

            if not (clean_link.startswith("http://") or clean_link.startswith("https://")):
                await interaction.response.send_message(
                    "Please provide a full URL starting with http:// or https://.",
                    ephemeral=True,
                )
                return

            user_payload = self.live_state.setdefault(interaction.user.id, {"streams": []})
            streams = user_payload.setdefault("streams", [])
            if not isinstance(streams, list):
                streams = []
                user_payload["streams"] = streams

            for item in streams:
                if isinstance(item, dict) and str(item.get("url", "")).strip().casefold() == clean_link.casefold():
                    item["platform"] = norm_platform
                    self._save_watchlist()
                    await interaction.response.send_message("Updated existing stream link.", ephemeral=True)
                    return

            streams.append(
                {
                    "platform": norm_platform,
                    "url": clean_link,
                    "is_live": False,
                    "last_stream_id": "",
                }
            )
            self._save_watchlist()
            await interaction.response.send_message(
                f"Stream link registered for automatic live checks ({norm_platform}).",
                ephemeral=True,
            )

        @self.tree.command(name="removestream", description="Remove one of your registered stream links")
        @app_commands.describe(link="Exact stream URL to remove")
        async def removestream(interaction: discord.Interaction, link: str) -> None:
            if interaction.guild is None or not isinstance(interaction.user, discord.Member):
                await interaction.response.send_message("This command can only be used in the server.", ephemeral=True)
                return

            clean_link = link.strip()
            user_payload = self.live_state.get(interaction.user.id)
            if not user_payload:
                await interaction.response.send_message("You have no registered stream links.", ephemeral=True)
                return

            streams = user_payload.get("streams", [])
            if not isinstance(streams, list):
                await interaction.response.send_message("You have no registered stream links.", ephemeral=True)
                return

            kept = [
                item
                for item in streams
                if not (isinstance(item, dict) and str(item.get("url", "")).strip().casefold() == clean_link.casefold())
            ]
            if len(kept) == len(streams):
                await interaction.response.send_message("No matching stream link found.", ephemeral=True)
                return

            if kept:
                user_payload["streams"] = kept
            else:
                self.live_state.pop(interaction.user.id, None)
            self._save_watchlist()
            await interaction.response.send_message("Stream link removed.", ephemeral=True)

        @self.tree.command(name="mystreams", description="Show your registered stream links")
        async def mystreams(interaction: discord.Interaction) -> None:
            if interaction.guild is None or not isinstance(interaction.user, discord.Member):
                await interaction.response.send_message("This command can only be used in the server.", ephemeral=True)
                return

            user_payload = self.live_state.get(interaction.user.id, {})
            streams = user_payload.get("streams", [])
            if not isinstance(streams, list) or not streams:
                await interaction.response.send_message("You have no registered stream links.", ephemeral=True)
                return

            lines: List[str] = []
            for item in streams:
                if not isinstance(item, dict):
                    continue
                platform = str(item.get("platform", "unknown"))
                url = str(item.get("url", ""))
                lines.append(f"- {platform}: {url}")

            if not lines:
                await interaction.response.send_message("You have no registered stream links.", ephemeral=True)
                return

            await interaction.response.send_message("\n".join(lines), ephemeral=True)

        guild_obj = discord.Object(id=self.config.guild_id)
        self.tree.copy_global_to(guild=guild_obj)
        await self.tree.sync(guild=guild_obj)

    async def on_ready(self) -> None:
        print(f"Logged in as {self.user} (ID: {self.user.id})")
        if self.publish_on_start:
            await self.publish_panels(purge_channel=self.purge_on_start)
        else:
            await self.rebuild_lookup()
        if not self.stream_watch_loop.is_running():
            self.stream_watch_loop.start()
        print("Reaction role bot is ready.")

    async def on_raw_reaction_add(self, payload: discord.RawReactionActionEvent) -> None:
        if payload.user_id == self.user.id:
            return
        if payload.guild_id != self.config.guild_id:
            return
        role_map = self.lookup.get(payload.message_id)
        if not role_map:
            return

        role_id = role_map.get(str(payload.emoji))
        if role_id is None:
            return

        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            return
        await self._apply_role(guild, payload.user_id, role_id, should_add=True)

    async def on_raw_reaction_remove(self, payload: discord.RawReactionActionEvent) -> None:
        if payload.guild_id != self.config.guild_id:
            return
        role_map = self.lookup.get(payload.message_id)
        if not role_map:
            return

        role_id = role_map.get(str(payload.emoji))
        if role_id is None:
            return

        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            return
        await self._apply_role(guild, payload.user_id, role_id, should_add=False)


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Discord multi-panel reaction role bot")
    parser.add_argument(
        "--config",
        default=os.path.join("DiscordManagement", "Automation", "reaction-role-panels.json"),
        help="Path to reaction role panel config JSON",
    )
    parser.add_argument(
        "--watchlist",
        default=os.path.join("DiscordManagement", "Automation", "streamer-watchlist.json"),
        help="Path to streamer watchlist JSON",
    )
    parser.add_argument("--discord-bot-token", default="", help=f"Bot token or env {DISCORD_BOT_TOKEN_ENV_VAR}")
    parser.add_argument("--publish-on-start", action="store_true", help="Publish all panels when bot starts")
    parser.add_argument("--purge-channel-on-start", action="store_true", help="Delete all channel messages before publish")
    return parser


def main() -> int:
    parser = build_arg_parser()
    args = parser.parse_args()

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

    bot = ReactionRoleBot(
        config=config,
        config_path=config_path,
        watchlist_path=os.path.abspath(args.watchlist),
        publish_on_start=args.publish_on_start,
        purge_on_start=args.purge_channel_on_start,
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
