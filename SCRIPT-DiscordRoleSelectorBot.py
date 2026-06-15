import argparse
import json
import os
import sys
from dataclasses import dataclass
from typing import Dict, List, Optional

import discord
from discord import app_commands

DISCORD_BOT_TOKEN_ENV_VAR = "AGF_DISCORD_BOT_TOKEN"


@dataclass
class RoleOption:
    role_id: int
    label: str
    description: str
    emoji: Optional[str] = None


@dataclass
class RolePanelConfig:
    guild_id: int
    channel_id: int
    message_id: int
    panel_title: str
    panel_description: str
    placeholder: str
    min_values: int
    max_values: int
    role_options: List[RoleOption]
    custom_id: str
    enable_select_menu: bool
    enable_buttons: bool
    enable_reactions: bool


def load_config(config_path: str) -> RolePanelConfig:
    with open(config_path, "r", encoding="utf-8") as f:
        raw = json.load(f)

    role_options = [
        RoleOption(
            role_id=int(item["role_id"]),
            label=str(item["label"]),
            description=str(item.get("description", "")),
            emoji=item.get("emoji"),
        )
        for item in raw.get("role_options", [])
    ]

    if not role_options:
        raise ValueError("Config must contain at least one role option")

    max_values = int(raw.get("max_values", len(role_options)))
    max_values = max(1, min(max_values, len(role_options)))

    return RolePanelConfig(
        guild_id=int(raw["guild_id"]),
        channel_id=int(raw["channel_id"]),
        message_id=int(raw.get("message_id", 0)),
        panel_title=str(raw.get("panel_title", "Choose your roles")),
        panel_description=str(raw.get("panel_description", "Pick one or more roles below.")),
        placeholder=str(raw.get("placeholder", "Select roles")),
        min_values=int(raw.get("min_values", 0)),
        max_values=max_values,
        role_options=role_options,
        custom_id=str(raw.get("custom_id", "agf_role_selector")),
        enable_select_menu=bool(raw.get("enable_select_menu", False)),
        enable_buttons=bool(raw.get("enable_buttons", True)),
        enable_reactions=bool(raw.get("enable_reactions", True)),
    )


def save_config(config_path: str, config: RolePanelConfig) -> None:
    payload = {
        "guild_id": config.guild_id,
        "channel_id": config.channel_id,
        "message_id": config.message_id,
        "panel_title": config.panel_title,
        "panel_description": config.panel_description,
        "placeholder": config.placeholder,
        "min_values": config.min_values,
        "max_values": config.max_values,
        "custom_id": config.custom_id,
        "enable_select_menu": config.enable_select_menu,
        "enable_buttons": config.enable_buttons,
        "enable_reactions": config.enable_reactions,
        "role_options": [
            {
                "role_id": item.role_id,
                "label": item.label,
                "description": item.description,
                "emoji": item.emoji,
            }
            for item in config.role_options
        ],
    }
    with open(config_path, "w", encoding="utf-8") as f:
        json.dump(payload, f, indent=2)
        f.write("\n")


class RoleSelect(discord.ui.Select):
    def __init__(self, config: RolePanelConfig):
        self._config = config
        options = [
            discord.SelectOption(
                label=item.label[:100],
                value=str(item.role_id),
                description=item.description[:100] if item.description else None,
                emoji=item.emoji,
            )
            for item in config.role_options
        ]

        super().__init__(
            placeholder=config.placeholder[:150],
            min_values=max(0, config.min_values),
            max_values=max(1, config.max_values),
            options=options,
            custom_id=config.custom_id,
        )

    async def callback(self, interaction: discord.Interaction) -> None:
        if not interaction.guild or not isinstance(interaction.user, discord.Member):
            await interaction.response.send_message("This can only be used in a server.", ephemeral=True)
            return

        selected_role_ids = {int(role_id) for role_id in self.values}
        managed_role_ids = {item.role_id for item in self._config.role_options}
        current_role_ids = {role.id for role in interaction.user.roles}

        to_add = selected_role_ids - current_role_ids
        to_remove = (managed_role_ids & current_role_ids) - selected_role_ids

        added_names = []
        removed_names = []

        for role_id in sorted(to_add):
            role = interaction.guild.get_role(role_id)
            if role is None:
                continue
            try:
                await interaction.user.add_roles(role, reason="Self-assigned role selection")
                added_names.append(role.name)
            except discord.Forbidden:
                pass

        for role_id in sorted(to_remove):
            role = interaction.guild.get_role(role_id)
            if role is None:
                continue
            try:
                await interaction.user.remove_roles(role, reason="Self-assigned role selection")
                removed_names.append(role.name)
            except discord.Forbidden:
                pass

        if not added_names and not removed_names:
            message = "No role changes were applied. Check role hierarchy and permissions."
        else:
            parts = []
            if added_names:
                parts.append(f"Added: {', '.join(added_names)}")
            if removed_names:
                parts.append(f"Removed: {', '.join(removed_names)}")
            message = "\n".join(parts)

        if interaction.response.is_done():
            await interaction.followup.send(message, ephemeral=True)
        else:
            await interaction.response.send_message(message, ephemeral=True)


class RoleToggleButton(discord.ui.Button):
    def __init__(self, role_option: RoleOption, custom_id_prefix: str, row_index: int):
        label = role_option.label[:80]
        super().__init__(
            label=label,
            emoji=role_option.emoji,
            style=discord.ButtonStyle.secondary,
            custom_id=f"{custom_id_prefix}_button_{role_option.role_id}",
            row=row_index,
        )
        self.role_id = role_option.role_id

    async def callback(self, interaction: discord.Interaction) -> None:
        bot: "RoleSelectorBot" = interaction.client  # type: ignore[assignment]
        if not interaction.guild or not isinstance(interaction.user, discord.Member):
            await interaction.response.send_message("This can only be used in a server.", ephemeral=True)
            return

        changed, message, role_added = await bot.toggle_role_by_id(interaction.guild, interaction.user, self.role_id)
        if changed:
            await bot.sync_reaction_for_button_toggle(interaction.guild, interaction.user, self.role_id, role_added)
            # Acknowledge the interaction without posting per-click status messages.
            if not interaction.response.is_done():
                await interaction.response.defer()
            return

        error_message = message or "No role changes were applied. Check role hierarchy and permissions."
        if interaction.response.is_done():
            await interaction.followup.send(error_message, ephemeral=True)
        else:
            await interaction.response.send_message(error_message, ephemeral=True)


class RoleSelectorView(discord.ui.View):
    def __init__(self, config: RolePanelConfig):
        super().__init__(timeout=None)
        if config.enable_select_menu:
            self.add_item(RoleSelect(config))

        if config.enable_buttons:
            if len(config.role_options) > 25:
                raise ValueError("Button mode supports up to 25 role options")

            for idx, role_option in enumerate(config.role_options):
                row_index = idx // 5
                self.add_item(RoleToggleButton(role_option, config.custom_id, row_index))


class RoleSelectorBot(discord.Client):
    def __init__(self, config: RolePanelConfig, auto_publish: bool, config_path: str):
        intents = discord.Intents.none()
        intents.guilds = True
        intents.members = True
        intents.guild_reactions = True
        super().__init__(intents=intents)
        self.tree = app_commands.CommandTree(self)
        self.config = config
        self.auto_publish = auto_publish
        self.config_path = config_path
        self.emoji_to_role_id: Dict[str, int] = {}
        self.role_id_to_emoji: Dict[int, str] = {}
        for role_option in config.role_options:
            if role_option.emoji:
                emoji_value = str(role_option.emoji)
                self.emoji_to_role_id[emoji_value] = role_option.role_id
                self.role_id_to_emoji[role_option.role_id] = emoji_value

    async def _apply_role_change(
        self,
        guild: discord.Guild,
        member: discord.Member,
        role_id: int,
        should_add: bool,
    ) -> bool:
        role = guild.get_role(role_id)
        if role is None:
            return False

        try:
            if should_add:
                await member.add_roles(role, reason="Self-assigned role selection")
            else:
                await member.remove_roles(role, reason="Self-assigned role selection")
            return True
        except discord.Forbidden:
            return False
        except discord.HTTPException:
            return False

    async def toggle_role_by_id(self, guild: discord.Guild, member: discord.Member, role_id: int) -> tuple[bool, str, bool]:
        role = guild.get_role(role_id)
        if role is None:
            return False, "Role no longer exists.", False

        has_role = role in member.roles
        changed = await self._apply_role_change(guild, member, role_id, should_add=not has_role)
        if not changed:
            return False, "No role changes were applied. Check role hierarchy and permissions.", False

        if has_role:
            return True, f"Removed: {role.name}", False
        return True, f"Added: {role.name}", True

    async def sync_reaction_for_button_toggle(
        self,
        guild: discord.Guild,
        member: discord.Member,
        role_id: int,
        role_added: bool,
    ) -> None:
        if not self.config.enable_reactions or not self.config.message_id:
            return

        emoji_value = self.role_id_to_emoji.get(role_id)
        if not emoji_value:
            return

        channel = guild.get_channel(self.config.channel_id)
        if channel is None:
            try:
                fetched_channel = await self.fetch_channel(self.config.channel_id)
            except discord.HTTPException as ex:
                print(f"Reaction sync skipped: could not fetch channel {self.config.channel_id}: {ex}")
                return
            if not isinstance(fetched_channel, discord.TextChannel):
                print("Reaction sync skipped: configured channel is not a text channel")
                return
            channel = fetched_channel
        if not isinstance(channel, discord.TextChannel):
            print("Reaction sync skipped: configured channel is not a text channel")
            return

        try:
            panel_message = await channel.fetch_message(self.config.message_id)
        except discord.HTTPException as ex:
            print(f"Reaction sync skipped: could not fetch panel message {self.config.message_id}: {ex}")
            return

        try:
            if role_added:
                # Discord does not allow bots to add reactions on behalf of another user.
                # Ensure the emoji exists on the panel for users to click.
                await panel_message.add_reaction(emoji_value)
                print(
                    f"Reaction sync note: role added via button for user {member.id}; "
                    "user must click emoji manually for reaction state"
                )
            else:
                # Keep reaction state tidy when a user removes a role via button click.
                await panel_message.remove_reaction(emoji_value, member)
                print(f"Reaction sync: removed reaction {emoji_value} for user {member.id}")
        except discord.Forbidden:
            print(
                "Reaction sync skipped: missing permission to remove another user's reaction "
                "(Manage Messages)"
            )
            return
        except discord.HTTPException as ex:
            print(f"Reaction sync skipped: HTTP error while syncing reactions: {ex}")
            return

    async def apply_reaction_role(
        self,
        guild_id: int,
        user_id: int,
        emoji_value: str,
        should_add: bool,
    ) -> None:
        role_id = self.emoji_to_role_id.get(emoji_value)
        if role_id is None:
            return

        guild = self.get_guild(guild_id)
        if guild is None:
            return

        member = guild.get_member(user_id)
        if member is None:
            return

        await self._apply_role_change(guild, member, role_id, should_add=should_add)

    async def setup_hook(self) -> None:
        self.add_view(RoleSelectorView(self.config))

        @self.tree.command(name="publishroles", description="Publish or refresh the role selection panel")
        async def publishroles(interaction: discord.Interaction) -> None:
            if not interaction.user.guild_permissions.manage_roles:
                await interaction.response.send_message(
                    "You need Manage Roles permission to use this command.",
                    ephemeral=True,
                )
                return

            try:
                message = await self.publish_or_update_role_panel()
                await interaction.response.send_message(
                    f"Role panel published/updated in <#{self.config.channel_id}> (message ID: {message.id}).",
                    ephemeral=True,
                )
            except Exception as ex:
                await interaction.response.send_message(f"Failed to publish panel: {ex}", ephemeral=True)

        guild = discord.Object(id=self.config.guild_id)
        self.tree.copy_global_to(guild=guild)
        await self.tree.sync(guild=guild)

    async def on_ready(self) -> None:
        print(f"Logged in as {self.user} (ID: {self.user.id})")
        print("Bot is ready.")
        if self.auto_publish:
            await self.publish_or_update_role_panel()

    async def on_raw_reaction_add(self, payload: discord.RawReactionActionEvent) -> None:
        if not self.config.enable_reactions:
            return
        if payload.user_id == self.user.id:
            return
        if payload.message_id != self.config.message_id:
            return
        if payload.guild_id != self.config.guild_id:
            return

        await self.apply_reaction_role(
            guild_id=payload.guild_id,
            user_id=payload.user_id,
            emoji_value=str(payload.emoji),
            should_add=True,
        )

    async def on_raw_reaction_remove(self, payload: discord.RawReactionActionEvent) -> None:
        if not self.config.enable_reactions:
            return
        if payload.message_id != self.config.message_id:
            return
        if payload.guild_id != self.config.guild_id:
            return

        await self.apply_reaction_role(
            guild_id=payload.guild_id,
            user_id=payload.user_id,
            emoji_value=str(payload.emoji),
            should_add=False,
        )

    async def publish_or_update_role_panel(self) -> discord.Message:
        guild = self.get_guild(self.config.guild_id)
        if guild is None:
            guild = await self.fetch_guild(self.config.guild_id)

        channel = guild.get_channel(self.config.channel_id)
        if channel is None:
            channel = await self.fetch_channel(self.config.channel_id)

        if not isinstance(channel, discord.TextChannel):
            raise TypeError("Configured channel is not a text channel")

        me = guild.me
        if me is not None:
            perms = channel.permissions_for(me)
            if self.config.enable_reactions and not perms.manage_messages:
                print(
                    "Warning: bot lacks Manage Messages in panel channel; "
                    "button-remove cannot clear user reactions"
                )

        embed = discord.Embed(title=self.config.panel_title, description=self.config.panel_description, color=0x3A8EBA)
        mode_parts: List[str] = []
        if self.config.enable_buttons:
            mode_parts.append("buttons")
        if self.config.enable_reactions:
            mode_parts.append("reactions")
        if self.config.enable_select_menu:
            mode_parts.append("select menu")
        modes_text = ", ".join(mode_parts) if mode_parts else "no inputs enabled"
        embed.set_footer(text=f"Use {modes_text} to update your roles.")

        lines: List[str] = []
        for role_option in self.config.role_options:
            emoji_cell = role_option.emoji if role_option.emoji else "▫️"
            lines.append(f"{emoji_cell}  {role_option.label}")
        if lines:
            embed.add_field(name="Available Roles", value="\n".join(lines)[:1024], inline=False)

        view = RoleSelectorView(self.config)

        message: Optional[discord.Message] = None
        if self.config.message_id:
            try:
                message = await channel.fetch_message(self.config.message_id)
            except discord.NotFound:
                message = None

        if message is None:
            message = await channel.send(embed=embed, view=view)
            self.config.message_id = message.id
            save_config(self.config_path, self.config)
            print(f"Created new role panel message: {message.id}")
        else:
            await message.edit(embed=embed, view=view)
            print(f"Updated role panel message: {message.id}")

        if self.config.enable_reactions:
            for role_option in self.config.role_options:
                if not role_option.emoji:
                    continue
                try:
                    await message.add_reaction(role_option.emoji)
                except discord.HTTPException:
                    continue

        return message


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="AGF Discord self-role panel bot (buttons and reactions)")
    parser.add_argument(
        "--config",
        default=os.path.join("Workflow", "Discord", "role-selector-config.json"),
        help="Path to role selector config JSON",
    )
    parser.add_argument(
        "--discord-bot-token",
        default="",
        help=f"Discord bot token. If omitted, uses env {DISCORD_BOT_TOKEN_ENV_VAR}.",
    )
    parser.add_argument(
        "--publish-on-start",
        action="store_true",
        help="Publish or refresh the role panel immediately when the bot starts",
    )
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

    bot = RoleSelectorBot(config=config, auto_publish=args.publish_on_start, config_path=config_path)

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
