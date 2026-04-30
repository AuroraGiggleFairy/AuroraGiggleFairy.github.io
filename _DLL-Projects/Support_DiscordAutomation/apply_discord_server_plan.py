import argparse
import hashlib
import json
import os
import sys
import time
import urllib.error
import urllib.request
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple

API_BASE = "https://discord.com/api/v10"
DEFAULT_TOKEN_ENV = "AGF_DISCORD_BOT_TOKEN"

PERM_BITS = {
    "view": 1 << 10,      # VIEW_CHANNEL
    "read_history": 1 << 16,  # READ_MESSAGE_HISTORY
    "send": 1 << 11,      # SEND_MESSAGES
    "react": 1 << 6,      # ADD_REACTIONS
    "connect": 1 << 20,   # CONNECT (voice)
    "create_public_threads": 1 << 35,   # CREATE_PUBLIC_THREADS
    "create_private_threads": 1 << 36,  # CREATE_PRIVATE_THREADS
    "send_in_threads": 1 << 38,         # SEND_MESSAGES_IN_THREADS
}

CHANNEL_TYPES = {
    "text": 0,
    "announcement": 5,
    "news": 5,
    "voice": 2,
    "category": 4,
    "forum": 15,
}


@dataclass
class RunSummary:
    created_roles: List[str] = field(default_factory=list)
    role_order_changes: List[str] = field(default_factory=list)
    created_categories: List[str] = field(default_factory=list)
    renamed_categories: List[str] = field(default_factory=list)
    created_channels: List[str] = field(default_factory=list)
    renamed_channels: List[str] = field(default_factory=list)
    updated_channels: List[str] = field(default_factory=list)
    guild_changes: List[str] = field(default_factory=list)
    onboarding_changes: List[str] = field(default_factory=list)
    member_role_changes: List[str] = field(default_factory=list)
    warnings: List[str] = field(default_factory=list)


class DiscordApi:
    def __init__(self, token: str, dry_run: bool = True, verbose: bool = False) -> None:
        self.token = token.strip()
        self.dry_run = dry_run
        self.verbose = verbose
        self.max_rate_limit_retries = 5
        self.max_transient_retries = 5

    def _request(self, method: str, path: str, body: Optional[dict] = None) -> dict:
        url = API_BASE + path
        attempt = 0
        while True:
            data = None
            headers = {
                "Authorization": f"Bot {self.token}",
                "User-Agent": "AGF-Discord-Server-Plan",
            }
            if body is not None:
                data = json.dumps(body).encode("utf-8")
                headers["Content-Type"] = "application/json"

            if self.verbose:
                print(f"[DEBUG] {method} {path} (attempt {attempt + 1})")

            req = urllib.request.Request(url, data=data, headers=headers, method=method)
            try:
                with urllib.request.urlopen(req, timeout=30) as resp:
                    raw = resp.read().decode("utf-8")
                    if not raw:
                        return {}
                    return json.loads(raw)
            except urllib.error.HTTPError as ex:
                msg = ex.read().decode("utf-8", errors="replace")

                if ex.code == 429 and attempt < self.max_rate_limit_retries:
                    retry_after = 2.0
                    try:
                        payload = json.loads(msg)
                        retry_after = float(payload.get("retry_after", retry_after))
                    except Exception:
                        pass

                    retry_after = max(0.5, retry_after)
                    if self.verbose:
                        print(f"[DEBUG] Rate limited on {method} {path}, waiting {retry_after:.2f}s before retry")
                    time.sleep(retry_after)
                    attempt += 1
                    continue

                if ex.code in (500, 502, 503, 504) and attempt < self.max_transient_retries:
                    retry_after = min(20.0, 1.5 * (2 ** attempt))
                    if self.verbose:
                        print(
                            f"[DEBUG] Transient HTTP {ex.code} on {method} {path}, "
                            f"waiting {retry_after:.2f}s before retry"
                        )
                    time.sleep(retry_after)
                    attempt += 1
                    continue

                raise RuntimeError(f"Discord API error {ex.code} on {method} {path}: {msg}") from ex
            except urllib.error.URLError as ex:
                raise RuntimeError(f"Network error on {method} {path}: {ex}") from ex

    def get_guild_roles(self, guild_id: str) -> List[dict]:
        return self._request("GET", f"/guilds/{guild_id}/roles")

    def get_current_user(self) -> dict:
        return self._request("GET", "/users/@me")

    def create_role(self, guild_id: str, name: str) -> dict:
        if self.dry_run:
            return {"id": f"dryrun-role-{name}", "name": name}
        return self._request("POST", f"/guilds/{guild_id}/roles", {"name": name})

    def reorder_roles(self, guild_id: str, payload: List[dict]) -> List[dict]:
        if self.dry_run:
            return payload
        return self._request("PATCH", f"/guilds/{guild_id}/roles", payload)

    def get_guild_channels(self, guild_id: str) -> List[dict]:
        return self._request("GET", f"/guilds/{guild_id}/channels")

    def get_guild(self, guild_id: str) -> dict:
        return self._request("GET", f"/guilds/{guild_id}")

    def get_guild_members(self, guild_id: str, limit: int = 1000, after: Optional[str] = None) -> List[dict]:
        path = f"/guilds/{guild_id}/members?limit={int(limit)}"
        if after:
            path += f"&after={after}"
        return self._request("GET", path)

    def add_member_role(self, guild_id: str, user_id: str, role_id: str) -> dict:
        if self.dry_run:
            return {"guild_id": guild_id, "user_id": user_id, "role_id": role_id}
        return self._request("PUT", f"/guilds/{guild_id}/members/{user_id}/roles/{role_id}")

    def patch_guild(self, guild_id: str, body: dict) -> dict:
        if self.dry_run:
            return body
        return self._request("PATCH", f"/guilds/{guild_id}", body)

    def create_channel(self, guild_id: str, body: dict) -> dict:
        if self.dry_run:
            channel_type = body.get("type", 0)
            kind = (
                "category" if channel_type == 4 else
                (
                    "voice" if channel_type == 2 else
                    ("announcement" if channel_type == 5 else ("forum" if channel_type == 15 else "text"))
                )
            )
            return {
                "id": f"dryrun-channel-{kind}-{body.get('name', 'unknown')}",
                "name": body.get("name", "unknown"),
                "type": body.get("type", 0),
                "parent_id": body.get("parent_id"),
            }
        return self._request("POST", f"/guilds/{guild_id}/channels", body)

    def patch_channel(self, channel_id: str, body: dict) -> dict:
        if self.dry_run:
            return {"id": channel_id, **body}
        return self._request("PATCH", f"/channels/{channel_id}", body)

    def get_guild_onboarding(self, guild_id: str) -> dict:
        return self._request("GET", f"/guilds/{guild_id}/onboarding")

    def patch_guild_onboarding(self, guild_id: str, body: dict) -> dict:
        if self.dry_run:
            return body
        return self._request("PUT", f"/guilds/{guild_id}/onboarding", body)


def normalize_name_map(items: List[dict]) -> Dict[str, dict]:
    out: Dict[str, dict] = {}
    for item in items:
        out[item["name"].casefold()] = item
    return out


def get_channel_key(channel: dict, parent_name_by_id: Dict[str, str]) -> str:
    parent_id = channel.get("parent_id")
    parent_name = parent_name_by_id.get(parent_id, "") if parent_id else ""
    return f"{parent_name.casefold()}::{channel['name'].casefold()}::{channel.get('type', 0)}"


def get_channel_key_by_parent_id(channel: dict) -> str:
    parent_id = str(channel.get("parent_id", ""))
    return f"{parent_id}::{channel['name'].casefold()}::{channel.get('type', 0)}"


def get_aliases(config_item: dict) -> List[str]:
    aliases = config_item.get("aliases", [])
    if not isinstance(aliases, list):
        return []
    return [str(a) for a in aliases if str(a).strip()]


def compute_overwrites(channel_rules: Dict[str, dict], role_id_by_name: Dict[str, str], everyone_role_id: str) -> List[dict]:
    overwrites: List[dict] = []

    for role_name, perms in channel_rules.items():
        target_id = everyone_role_id if role_name == "@everyone" else role_id_by_name.get(role_name.casefold())
        if not target_id:
            continue

        allow = 0
        deny = 0
        for perm_name, state in perms.items():
            bit = PERM_BITS.get(perm_name)
            if bit is None:
                continue
            if state == "allow":
                allow |= bit
            elif state == "deny":
                deny |= bit

        # Keep message history readable wherever a role can view a message channel,
        # unless a config explicitly sets read_history for that overwrite.
        if "read_history" not in perms and "view" in perms:
            if perms.get("view") == "allow":
                allow |= PERM_BITS["read_history"]
            elif perms.get("view") == "deny":
                deny |= PERM_BITS["read_history"]

        overwrites.append(
            {
                "id": target_id,
                "type": 0,
                "allow": str(allow),
                "deny": str(deny),
            }
        )

    return overwrites


def merge_overwrites(existing_overwrites: List[dict], planned_overwrites: List[dict]) -> List[dict]:
    by_id: Dict[str, dict] = {}
    for item in existing_overwrites:
        item_id = str(item.get("id", ""))
        if item_id:
            by_id[item_id] = item

    for item in planned_overwrites:
        item_id = str(item.get("id", ""))
        if item_id:
            by_id[item_id] = item

    return list(by_id.values())


def subset_equals(current: object, desired: object) -> bool:
    if isinstance(desired, dict):
        if not isinstance(current, dict):
            return False
        for key, desired_val in desired.items():
            if key not in current:
                return False
            if not subset_equals(current[key], desired_val):
                return False
        return True

    if isinstance(desired, list):
        if not isinstance(current, list):
            return False
        if len(current) != len(desired):
            return False
        return all(subset_equals(cur_item, des_item) for cur_item, des_item in zip(current, desired))

    return current == desired


def resolve_channel_ref(
    channel_ref: object,
    channels: List[dict],
    category_name_by_id: Dict[str, str],
    summary: RunSummary,
) -> Optional[str]:
    ref = str(channel_ref or "").strip()
    if not ref:
        return None

    if ref.isdigit():
        return ref

    if "/" in ref:
        raw_parent, raw_name = ref.split("/", 1)
        parent = raw_parent.strip().casefold()
        name = raw_name.strip().casefold()
        for channel in channels:
            if channel.get("type") == CHANNEL_TYPES["category"]:
                continue
            parent_name = category_name_by_id.get(str(channel.get("parent_id", "")), "").casefold()
            if parent_name == parent and str(channel.get("name", "")).casefold() == name:
                return str(channel.get("id"))
        summary.warnings.append(f"Onboarding channel ref not found: {ref}")
        return None

    matches = [
        c for c in channels
        if c.get("type") != CHANNEL_TYPES["category"] and str(c.get("name", "")).casefold() == ref.casefold()
    ]
    if not matches:
        summary.warnings.append(f"Onboarding channel ref not found: {ref}")
        return None
    if len(matches) > 1:
        summary.warnings.append(f"Onboarding channel ref is ambiguous, using first match: {ref}")
    return str(matches[0].get("id"))


def channel_everyone_can_view(channel: dict, everyone_role_id: str) -> bool:
    for overwrite in channel.get("permission_overwrites", []):
        if str(overwrite.get("id", "")) != everyone_role_id:
            continue

        allow = int(str(overwrite.get("allow", "0") or "0"))
        deny = int(str(overwrite.get("deny", "0") or "0"))
        if deny & PERM_BITS["view"]:
            return False
        return bool(allow & PERM_BITS["view"])

    return False


def collect_public_channel_ids(channels: List[dict], everyone_role_id: str) -> List[str]:
    public_channel_ids: List[str] = []
    for channel in channels:
        if channel.get("type") == CHANNEL_TYPES["category"]:
            continue
        channel_id = str(channel.get("id", "")).strip()
        if not channel_id:
            continue
        if channel_everyone_can_view(channel, everyone_role_id):
            public_channel_ids.append(channel_id)
    return public_channel_ids


def make_stable_snowflake(seed: str) -> str:
    digest = hashlib.sha256(seed.encode("utf-8")).digest()
    value = int.from_bytes(digest[:8], "big") & ((1 << 63) - 1)
    if value == 0:
        value = 1
    return str(value)


def build_onboarding_payload(
    onboarding_cfg: dict,
    channels: List[dict],
    category_name_by_id: Dict[str, str],
    role_id_by_name: Dict[str, str],
    everyone_role_id: str,
    current_onboarding: dict,
    summary: RunSummary,
) -> dict:
    payload: Dict[str, object] = {}

    if "enabled" in onboarding_cfg:
        payload["enabled"] = bool(onboarding_cfg.get("enabled"))
    if "mode" in onboarding_cfg:
        payload["mode"] = int(onboarding_cfg.get("mode"))

    default_channel_ids: List[str] = []
    for ref in onboarding_cfg.get("default_channels", []):
        channel_id = resolve_channel_ref(ref, channels, category_name_by_id, summary)
        if channel_id:
            default_channel_ids.append(channel_id)
    if bool(onboarding_cfg.get("include_all_public_channels", False)):
        for channel_id in collect_public_channel_ids(channels, everyone_role_id):
            if channel_id not in default_channel_ids:
                default_channel_ids.append(channel_id)
    if default_channel_ids:
        payload["default_channel_ids"] = default_channel_ids

    existing_prompt_id_by_title: Dict[str, str] = {}
    existing_option_id_by_prompt_and_title: Dict[Tuple[str, str], str] = {}
    for existing_prompt in current_onboarding.get("prompts", []):
        existing_id = str(existing_prompt.get("id", "")).strip()
        existing_title = str(existing_prompt.get("title", "")).strip().casefold()
        if not existing_id:
            continue
        if existing_title and existing_title not in existing_prompt_id_by_title:
            existing_prompt_id_by_title[existing_title] = existing_id
        for existing_option in existing_prompt.get("options", []):
            existing_option_id = str(existing_option.get("id", "")).strip()
            existing_option_title = str(existing_option.get("title", "")).strip().casefold()
            if existing_option_id and existing_title and existing_option_title:
                existing_option_id_by_prompt_and_title[(existing_title, existing_option_title)] = existing_option_id

    prompts_payload: List[dict] = []
    for prompt_index, prompt_cfg in enumerate(onboarding_cfg.get("prompts", [])):
        prompt: Dict[str, object] = {
            "title": str(prompt_cfg.get("title", "")).strip(),
            "single_select": bool(prompt_cfg.get("single_select", False)),
            "required": bool(prompt_cfg.get("required", False)),
            "in_onboarding": bool(prompt_cfg.get("in_onboarding", True)),
            "type": int(prompt_cfg.get("type", 0)),
        }

        if prompt_cfg.get("id"):
            prompt["id"] = str(prompt_cfg.get("id"))
        else:
            title_key = str(prompt_cfg.get("title", "")).strip().casefold()
            resolved_prompt_id = existing_prompt_id_by_title.get(title_key)

            if resolved_prompt_id:
                prompt["id"] = resolved_prompt_id
            else:
                prompt["id"] = make_stable_snowflake(f"prompt::{prompt.get('title', '')}")
                summary.warnings.append(
                    f"Onboarding prompt id generated for title: {prompt.get('title', '')}"
                )

        options_payload: List[dict] = []
        for opt_cfg in prompt_cfg.get("options", []):
            option: Dict[str, object] = {
                "title": str(opt_cfg.get("title", "")).strip(),
            }

            option_id = str(opt_cfg.get("id", "")).strip()
            if option_id:
                option["id"] = option_id
            else:
                option_title_key = str(opt_cfg.get("title", "")).strip().casefold()
                prompt_title_key = str(prompt_cfg.get("title", "")).strip().casefold()
                existing_option_id = existing_option_id_by_prompt_and_title.get((prompt_title_key, option_title_key))
                if existing_option_id:
                    option["id"] = existing_option_id
                else:
                    option["id"] = make_stable_snowflake(
                        f"option::{prompt.get('id', '')}::{option.get('title', '')}"
                    )

            description = str(opt_cfg.get("description", "")).strip()
            if description:
                option["description"] = description

            emoji_name = str(opt_cfg.get("emoji_name", "")).strip()
            if emoji_name:
                option["emoji_name"] = emoji_name

            channel_ids: List[str] = []
            for ch_ref in opt_cfg.get("channel_refs", []):
                channel_id = resolve_channel_ref(ch_ref, channels, category_name_by_id, summary)
                if channel_id:
                    channel_ids.append(channel_id)
            if channel_ids:
                option["channel_ids"] = channel_ids

            role_ids: List[str] = []
            for role_name in opt_cfg.get("role_names", []):
                role_id = role_id_by_name.get(str(role_name).casefold())
                if role_id:
                    role_ids.append(role_id)
                else:
                    summary.warnings.append(f"Onboarding role not found: {role_name}")
            if role_ids:
                option["role_ids"] = role_ids

            options_payload.append(option)

        prompt["options"] = options_payload
        prompts_payload.append(prompt)

    if prompts_payload:
        payload["prompts"] = prompts_payload

    return payload


def ensure_onboarding(
    api: DiscordApi,
    guild_id: str,
    config: dict,
    role_id_by_name: Dict[str, str],
    summary: RunSummary,
) -> None:
    onboarding_cfg = config.get("onboarding")
    if not isinstance(onboarding_cfg, dict) or not onboarding_cfg:
        return

    if not bool(onboarding_cfg.get("manage", False)):
        return

    channels = api.get_guild_channels(guild_id)
    categories = [c for c in channels if c.get("type") == CHANNEL_TYPES["category"]]
    category_name_by_id = {str(c.get("id")): str(c.get("name", "")) for c in categories}
    roles = api.get_guild_roles(guild_id)
    everyone_role = next((r for r in roles if r.get("name") == "@everyone"), None)
    if not everyone_role:
        raise RuntimeError("Could not find @everyone role in guild roles.")
    current = api.get_guild_onboarding(guild_id)

    payload = build_onboarding_payload(
        onboarding_cfg,
        channels,
        category_name_by_id,
        role_id_by_name,
        str(everyone_role["id"]),
        current,
        summary,
    )

    if not payload:
        summary.warnings.append("Onboarding manage=true but no onboarding payload could be built.")
        return

    if subset_equals(current, payload):
        summary.onboarding_changes.append("No onboarding changes needed")
        return

    api.patch_guild_onboarding(guild_id, payload)
    summary.onboarding_changes.append("Updated onboarding configuration")


def ensure_guild_settings(
    api: DiscordApi,
    guild_id: str,
    config: dict,
    summary: RunSummary,
) -> None:
    guild_cfg = config.get("guild_settings")
    if not isinstance(guild_cfg, dict) or not guild_cfg:
        return

    channels = api.get_guild_channels(guild_id)
    categories = [c for c in channels if c.get("type") == CHANNEL_TYPES["category"]]
    category_name_by_id = {str(c.get("id")): str(c.get("name", "")) for c in categories}

    patch_body: Dict[str, object] = {}
    rules_channel_ref = guild_cfg.get("rules_channel_ref")
    if rules_channel_ref:
        rules_channel_id = resolve_channel_ref(rules_channel_ref, channels, category_name_by_id, summary)
        if rules_channel_id:
            patch_body["rules_channel_id"] = rules_channel_id

    if not patch_body:
        return

    current = api.get_guild(guild_id)
    if subset_equals(current, patch_body):
        summary.guild_changes.append("No guild settings changes needed")
        return

    api.patch_guild(guild_id, patch_body)
    if "rules_channel_id" in patch_body:
        summary.guild_changes.append("Updated guild rules channel")


def ensure_roles(api: DiscordApi, guild_id: str, config: dict, summary: RunSummary) -> Dict[str, str]:
    roles = api.get_guild_roles(guild_id)
    role_by_name = normalize_name_map(roles)

    for name in config.get("required_roles", []):
        key = name.casefold()
        if key not in role_by_name:
            created = api.create_role(guild_id, name)
            role_by_name[key] = created
            summary.created_roles.append(name)

    role_id_by_name: Dict[str, str] = {}
    for key, role in role_by_name.items():
        role_id_by_name[key] = role["id"]

    enforce_role_hierarchy = bool(config.get("enforce_role_hierarchy", True))
    desired_top_to_bottom: List[str] = config.get("role_hierarchy_top_to_bottom", [])
    if enforce_role_hierarchy and desired_top_to_bottom:
        current_roles = [r for r in api.get_guild_roles(guild_id) if r.get("name") != "@everyone"]
        if not current_roles:
            return role_id_by_name

        max_pos = max(int(r.get("position", 0)) for r in current_roles)
        payload: List[dict] = []
        pos = max_pos

        for role_name in desired_top_to_bottom:
            role = next((r for r in current_roles if r.get("name", "").casefold() == role_name.casefold()), None)
            if not role:
                summary.warnings.append(f"Role missing for hierarchy ordering: {role_name}")
                continue
            payload.append({"id": role["id"], "position": pos})
            summary.role_order_changes.append(f"{role_name} -> position {pos}")
            pos -= 1

        if payload:
            try:
                api.reorder_roles(guild_id, payload)
            except Exception as ex:
                summary.warnings.append(f"Role reorder failed (often permissions): {ex}")

    return role_id_by_name


def ensure_member_baseline_roles(
    api: DiscordApi,
    guild_id: str,
    config: dict,
    role_id_by_name: Dict[str, str],
    summary: RunSummary,
) -> None:
    baseline_cfg = config.get("member_baseline_roles", {})
    if not isinstance(baseline_cfg, dict) or not baseline_cfg:
        return

    if not bool(baseline_cfg.get("enabled", False)):
        return

    role_names = [str(n).strip() for n in baseline_cfg.get("all_members_role_names", []) if str(n).strip()]
    if not role_names:
        summary.warnings.append("member_baseline_roles enabled but no all_members_role_names configured")
        return

    include_bots = bool(baseline_cfg.get("include_bots", False))

    role_pairs: List[Tuple[str, str]] = []
    missing_role_names: List[str] = []
    for role_name in role_names:
        role_id = role_id_by_name.get(role_name.casefold())
        if role_id:
            role_pairs.append((role_name, role_id))
        else:
            missing_role_names.append(role_name)

    for role_name in missing_role_names:
        summary.warnings.append(f"Baseline role not found: {role_name}")

    if not role_pairs:
        return

    after: Optional[str] = None
    try:
        while True:
            members = api.get_guild_members(guild_id, limit=1000, after=after)
            if not members:
                break

            for member in members:
                user = member.get("user", {})
                user_id = str(user.get("id", "")).strip()
                if not user_id:
                    continue

                if bool(user.get("bot", False)) and not include_bots:
                    continue

                member_roles = {str(rid) for rid in member.get("roles", [])}
                for role_name, role_id in role_pairs:
                    if role_id in member_roles:
                        continue
                    api.add_member_role(guild_id, user_id, role_id)
                    summary.member_role_changes.append(f"Added {role_name} -> {user.get('username', user_id)} ({user_id})")

            last_user = members[-1].get("user", {})
            after = str(last_user.get("id", "")).strip() or None
            if len(members) < 1000:
                break
    except Exception as ex:
        summary.warnings.append(
            "Skipping member baseline role assignment. "
            "Bot likely lacks member-list access (Server Members Intent and/or permissions). "
            f"Details: {ex}"
        )


def ensure_categories_and_channels(
    api: DiscordApi,
    guild_id: str,
    config: dict,
    role_id_by_name: Dict[str, str],
    summary: RunSummary,
) -> None:
    allow_create_missing = bool(config.get("allow_create_missing", True))
    strict_permission_overwrites = bool(config.get("strict_permission_overwrites", True))
    enforce_exact_names = bool(config.get("enforce_exact_names", True))

    channels = api.get_guild_channels(guild_id)

    categories = [c for c in channels if c.get("type") == CHANNEL_TYPES["category"]]
    category_by_name = normalize_name_map(categories)

    roles = api.get_guild_roles(guild_id)
    everyone_role = next((r for r in roles if r.get("name") == "@everyone"), None)
    if not everyone_role:
        raise RuntimeError("Could not find @everyone role in guild roles.")
    everyone_role_id = everyone_role["id"]

    for category_cfg in config.get("categories", []):
        category_name = category_cfg["name"]
        category = category_by_name.get(category_name.casefold())
        if not category:
            for alias in get_aliases(category_cfg):
                category = category_by_name.get(alias.casefold())
                if category:
                    break

        if not category:
            if not allow_create_missing:
                summary.warnings.append(f"Missing category and create disabled: {category_name}")
                continue
            created = api.create_channel(
                guild_id,
                {
                    "name": category_name,
                    "type": CHANNEL_TYPES["category"],
                },
            )
            category = created
            category_by_name[category_name.casefold()] = category
            summary.created_categories.append(category_name)
        elif enforce_exact_names and category.get("name") != category_name:
            api.patch_channel(category["id"], {"name": category_name})
            summary.renamed_categories.append(f"{category.get('name', 'unknown')} -> {category_name}")
            category["name"] = category_name

        channels = api.get_guild_channels(guild_id)
        key_to_channel = {}
        for ch in channels:
            if ch.get("type") == CHANNEL_TYPES["category"]:
                continue
            key_to_channel[get_channel_key_by_parent_id(ch)] = ch

        for ch_cfg in category_cfg.get("channels", []):
            ch_name = ch_cfg["name"]
            ch_type_str = ch_cfg["type"]
            ch_type = CHANNEL_TYPES[ch_type_str]
            category_id = str(category["id"])
            key = f"{category_id}::{ch_name.casefold()}::{ch_type}"

            existing = key_to_channel.get(key)
            if not existing:
                channel_aliases = get_aliases(ch_cfg)
                for alias in channel_aliases:
                    alias_key = f"{category_id}::{alias.casefold()}::{ch_type}"
                    existing = key_to_channel.get(alias_key)
                    if existing:
                        break

            if not existing:
                if not allow_create_missing:
                    summary.warnings.append(f"Missing channel and create disabled: {category_name}/{ch_name}")
                    continue
                created = api.create_channel(
                    guild_id,
                    {
                        "name": ch_name,
                        "type": ch_type,
                        "parent_id": category["id"],
                    },
                )
                existing = created
                summary.created_channels.append(f"{category_name}/{ch_name}")
            elif enforce_exact_names and existing.get("name") != ch_name:
                old_name = existing.get("name", "unknown")
                api.patch_channel(existing["id"], {"name": ch_name})
                summary.renamed_channels.append(f"{category_name}/{old_name} -> {ch_name}")
                existing["name"] = ch_name

            planned_overwrites = compute_overwrites(
                ch_cfg.get("permissions", {}),
                role_id_by_name,
                everyone_role_id,
            )

            existing_overwrites = existing.get("permission_overwrites", [])
            overwrites = planned_overwrites
            if not strict_permission_overwrites:
                overwrites = merge_overwrites(existing_overwrites, planned_overwrites)

            patch_body = {
                "parent_id": category["id"],
                "permission_overwrites": overwrites,
            }
            api.patch_channel(existing["id"], patch_body)
            summary.updated_channels.append(f"{category_name}/{ch_name}")


def load_plan(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def normalize_token(raw_token: str) -> str:
    token = (raw_token or "").strip()
    if token.lower().startswith("bot "):
        token = token[4:].strip()
    if len(token) >= 2 and token[0] == token[-1] and token[0] in ('"', "'"):
        token = token[1:-1].strip()
    return token


def validate_token_shape(token: str) -> Optional[str]:
    # Basic format check only. Discord token formats can evolve, so keep this loose.
    if " " in token or "\t" in token or "\n" in token or "\r" in token:
        return "Token contains whitespace characters."
    if token.count(".") < 2:
        return "Token appears malformed (expected at least 3 dot-separated parts)."
    return None


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Apply a Discord server plan from JSON")
    parser.add_argument(
        "--config",
        default=os.path.join(os.path.dirname(__file__), "discord_server_plan.json"),
        help="Path to the server plan JSON file",
    )
    parser.add_argument(
        "--token",
        default="",
        help=f"Discord bot token. If omitted, uses env var {DEFAULT_TOKEN_ENV}",
    )
    parser.add_argument(
        "--apply",
        action="store_true",
        help="Apply changes. If omitted, runs in dry-run mode.",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Print debug request info.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    token = normalize_token(args.token or os.getenv(DEFAULT_TOKEN_ENV, ""))
    if not token:
        print(f"Missing Discord bot token. Set --token or env {DEFAULT_TOKEN_ENV}.")
        return 1

    token_issue = validate_token_shape(token)
    if token_issue:
        print(f"Token validation failed: {token_issue}")
        print("Tip: paste only the raw token value without quotes and without 'Bot ' prefix.")
        return 1

    plan_path = os.path.abspath(args.config)
    if not os.path.isfile(plan_path):
        print(f"Plan file not found: {plan_path}")
        return 1

    try:
        plan = load_plan(plan_path)
    except Exception as ex:
        print(f"Could not read plan file: {ex}")
        return 1

    guild_id = str(plan.get("guild_id", "")).strip()
    if not guild_id:
        print("Plan missing guild_id")
        return 1

    api = DiscordApi(token=token, dry_run=not args.apply, verbose=args.verbose)
    summary = RunSummary()

    print("Mode:", "APPLY" if args.apply else "DRY-RUN")
    print("Guild:", guild_id)

    try:
        me = api.get_current_user()
        bot_name = me.get("username", "unknown")
        print("Authenticated bot:", bot_name)

        role_id_by_name = ensure_roles(api, guild_id, plan, summary)
        ensure_member_baseline_roles(api, guild_id, plan, role_id_by_name, summary)
        ensure_categories_and_channels(api, guild_id, plan, role_id_by_name, summary)
        ensure_guild_settings(api, guild_id, plan, summary)
        ensure_onboarding(api, guild_id, plan, role_id_by_name, summary)
    except Exception as ex:
        print(f"Failed: {ex}")
        print("Troubleshooting: ensure token is current, app has a Bot user, and bot is invited to the target guild.")
        return 1

    print("\nSummary")
    print("Created roles:", len(summary.created_roles))
    for item in summary.created_roles:
        print("  +", item)

    print("Role ordering updates:", len(summary.role_order_changes))
    for item in summary.role_order_changes:
        print("  *", item)

    print("Member role updates:", len(summary.member_role_changes))
    for item in summary.member_role_changes:
        print("  *", item)

    print("Created categories:", len(summary.created_categories))
    for item in summary.created_categories:
        print("  +", item)

    print("Renamed categories:", len(summary.renamed_categories))
    for item in summary.renamed_categories:
        print("  ~", item)

    print("Created channels:", len(summary.created_channels))
    for item in summary.created_channels:
        print("  +", item)

    print("Renamed channels:", len(summary.renamed_channels))
    for item in summary.renamed_channels:
        print("  ~", item)

    print("Updated channel permissions:", len(summary.updated_channels))
    for item in summary.updated_channels:
        print("  *", item)

    print("Guild settings updates:", len(summary.guild_changes))
    for item in summary.guild_changes:
        print("  *", item)

    print("Onboarding updates:", len(summary.onboarding_changes))
    for item in summary.onboarding_changes:
        print("  *", item)

    if summary.warnings:
        print("Warnings:")
        for w in summary.warnings:
            print("  !", w)

    if not args.apply:
        print("\nDry-run complete. Re-run with --apply to make changes.")

    return 0


if __name__ == "__main__":
    sys.exit(main())
