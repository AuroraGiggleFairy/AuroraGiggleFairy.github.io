using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace ExpandedInteractionPrompts
{
    public static class PromptStateHelpers
    {
        public const string SplitToken = "\n\x1e\n";

        public const string IconLoot = "ui_game_symbol_loot_sack";
        public const string IconFuel = "ui_game_symbol_fire";
        public const string IconSmelt = "ui_game_symbol_forge";
        public const string IconLock = "ui_game_symbol_lock";
        public const string IconUnlock = "ui_game_symbol_unlock";
        public const string IconPlayer = "ui_game_symbol_player";
        public const string IconSeats = "ui_game_symbol_seats";
        public const string IconCraft = "ui_game_symbol_hammer";
        public const string IconCookware = "ui_game_symbol_cookware";
        public const string IconWrench = "ui_game_symbol_wrench";
        public const string IconRepair = "ui_game_symbol_wrench";
        public const string IconGas = "ui_game_symbol_gas";
        public const string IconPower = "ui_game_symbol_electric_switch";
        public const string IconMaxOutput = "ui_game_symbol_electric_max_power";
        public const string IconWatts = "ui_game_symbol_electric_power";
        public const string IconAmmo = "ui_game_symbol_pistol";
        public const string IconTargeting = "ui_game_symbol_map_cursor";
        public const string IconTargetSelf = "ui_game_symbol_character";
        public const string IconTargetAllies = "ui_game_symbol_allies";
        public const string IconTargetStrangers = "ui_game_symbol_knife";
        public const string IconTargetZombies = "ui_game_symbol_zombie";
        public const string IconRefresh = "server_refresh";
        public const string IconFollow = "ui_game_symbol_run";
        public const string IconStay = "ui_game_symbol_run_and_gun";
        public const string IconTalk = "ui_game_symbol_talk";
        public const string TargetRowPrefix = "@t:";
        public const string ItemIconAtlas = "ItemIconAtlas";
        public const string UiAtlas = "UIAtlas";
        // HUDPlus compass bag orange, grey when empty / for max.
        public const string ColorBagUsed = "ff8000";
        public const string ColorMuted = "9a9a9a";
        public const string ColorQueueUsed = "decea3";
        public const string ColorOn = "59f266";
        // Vanilla XUi selectedColor / beige for selected targeting buttons.
        public const string ColorTargetSelected = "decea3";
        // HUDPlus right-HUD vehicle bars.
        public const string FillDurability = "48,194,214,170";
        public const string FillGas = "236,174,44,170";
        public const string FillPower = "255,255,0,170";

        public const int MaxRows = 8;
        public const int MaxStatCells = 8;
        public const int SlotL0 = 0;
        public const int SlotR0 = 1;
        public const int SlotL1 = 2;
        public const int SlotR1 = 3;
        public const int SlotL2 = 4;
        public const int SlotR2 = 5;
        // Cells 0-5 are drawn before the owner, so this empty vehicle cell sits on the pill above the owner.
        public const int SlotAboveOwner = SlotR2;
        public const int SlotLock = 6;
        // Deco helper catalog: Campfire = locked, loot = unlocked.
        public const string ColorLocked = "f0beb9";
        public const string ColorUnlocked = "c8e6be";
        public const int SlotOwner = 7;
        private const float VehicleFuelDisplayScale = 25f;
        private static int s_fuelTeId = int.MinValue;
        private static float s_fuelTeSeconds;
        private static float s_fuelSampleAt;

        private static bool IsLockableWorkstationsActive()
        {
            return LockableStationsClient.IsAvailable;
        }

        public static bool ShouldShowOwner()
        {
            if (GameManager.IsDedicatedServer)
                return true;

            PersistentPlayerList list = GameManager.Instance?.GetPersistentPlayerList()
                ?? GameManager.Instance?.persistentPlayers;
            if (list?.Players == null)
                return false;

            return list.Players.Count > 1;
        }

        public static bool TryApplyDeniedLockPrompt(ref string result, WorldBase world, Vector3i blockPos)
        {
            if (!LockableStationsClient.CanShowStationLock(world, blockPos, out bool isLocked, out var owner, out bool denied) || !denied)
                return false;

            var lines = new List<string>
            {
                FormatLock(isLocked)
            };
            string ownerLine = FormatOwner(ResolveOwnerName(owner));
            if (!string.IsNullOrEmpty(ownerLine))
                lines.Add(ownerLine);

            result = BuildPrompt(result, lines);
            return true;
        }

        public static bool ShouldHideDetails(ref string result, WorldBase world, Vector3i blockPos)
        {
            if (TryApplyDeniedLockPrompt(ref result, world, blockPos))
                return true;
            return IsJammedPrompt(result);
        }

        public static bool IsJammedPrompt(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (!IsLockableWorkstationsActive())
                return false;

            string deniedAgf = Localization.Get("xuiDeniedAGF");
            if (!string.IsNullOrEmpty(deniedAgf) && text.Contains(deniedAgf))
                return true;

            if (text.Contains(Localization.Get("tooltipJammed")))
                return true;

            string jammedAgf = Localization.Get("xuiJammedAGF");
            if (!string.IsNullOrEmpty(jammedAgf) && text.Contains(jammedAgf))
                return true;

            return false;
        }

        public static bool IsDeniedStoragePrompt(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (text.Contains(Localization.Get("tooltipJammed")))
                return true;

            if (text.Contains(Localization.Get("tooltipLocked")))
                return true;

            string deniedAgf = Localization.Get("xuiDeniedAGF");
            if (!string.IsNullOrEmpty(deniedAgf) && text.Contains(deniedAgf))
                return true;

            string jammedAgf = Localization.Get("xuiJammedAGF");
            if (!string.IsNullOrEmpty(jammedAgf) && text.Contains(jammedAgf))
                return true;

            return false;
        }

        public static int CountUsedSlots(ItemStack[] items)
        {
            if (items == null)
                return 0;

            int used = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && !items[i].IsEmpty())
                    used++;
            }
            return used;
        }

        public static string Colorize(string hex, string value)
        {
            return "[" + hex + "]" + value + "[-]";
        }

        public static string HexToRgba(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length < 6)
                return "255,255,255,255";

            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return r + "," + g + "," + b + ",255";
        }

        public static string StripColor(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '[')
                {
                    int close = text.IndexOf(']', i + 1);
                    if (close >= 0)
                    {
                        i = close;
                        continue;
                    }
                }
                sb.Append(text[i]);
            }
            return sb.ToString();
        }

        public static string FormatCount(int used, int total)
        {
            return used + "/" + total;
        }

        // HUDPlus backpack: orange used, gold max.
        public static string FormatBagCount(int used, int total)
        {
            string usedHex = used > 0 ? ColorBagUsed : ColorMuted;
            return Colorize(usedHex, used.ToString()) + Colorize(ColorMuted, "/" + total);
        }

        // Queue / smelt: gold used when > 0, muted grey max (and grey 0).
        public static string FormatQueueCount(int used, int total)
        {
            string usedHex = used > 0 ? ColorQueueUsed : ColorMuted;
            return Colorize(usedHex, used.ToString()) + Colorize(ColorMuted, "/" + total);
        }

        public static string FormatIconRow(string icon, string text, int slot, string colorHex = null, float fill = -1f, string fillRgba = null, bool blink = false)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            string display = text;
            if (!string.IsNullOrEmpty(colorHex))
                display = Colorize(colorHex, StripColor(text));
            string row = icon + "\t" + display + "\t" + slot;
            bool hasFill = fill >= 0f && !string.IsNullOrEmpty(fillRgba);
            if (!string.IsNullOrEmpty(colorHex) || hasFill)
                row += "\t" + (colorHex ?? string.Empty);
            if (hasFill)
            {
                row += "\t" + fill.ToString("0.###", CultureInfo.InvariantCulture);
                row += "\t" + fillRgba;
            }
            if (blink)
                row += "\tblink";
            return row;
        }

        public static string FormatOutput(int used, int total, int slot = SlotR0)
        {
            return FormatIconRow(IconLoot, FormatBagCount(used, total), slot, blink: used > 0);
        }

        public static string FormatSlots(int used, int total, int slot = SlotL0, string icon = null)
        {
            return FormatIconRow(string.IsNullOrEmpty(icon) ? IconLoot : icon, FormatBagCount(used, total), slot);
        }

        // Power banks / generators: fill the item slots. Incomplete (including empty) is orange; full is grey.
        public static string FormatFillGoalCount(int used, int total)
        {
            string usedHex = (total > 0 && used >= total) ? ColorMuted : ColorBagUsed;
            return Colorize(usedHex, used.ToString()) + Colorize(ColorMuted, "/" + total);
        }

        public static string FormatPowerSlots(int used, int total, int slot, string icon)
        {
            return FormatIconRow(string.IsNullOrEmpty(icon) ? IconLoot : icon, FormatFillGoalCount(used, total), slot);
        }

        private static readonly Dictionary<int, int> s_stackMaxByClassId = new Dictionary<int, int>(32);
        private static readonly Dictionary<int, string> s_iconByClassId = new Dictionary<int, string>(32);
        private static string s_wattSuffix;

        public static string ItemIconName(ItemClass itemClass)
        {
            if (itemClass == null)
                return null;
            int id = itemClass.Id;
            if (s_iconByClassId.TryGetValue(id, out string cached))
                return string.IsNullOrEmpty(cached) ? null : cached;

            string icon = null;
            try { icon = itemClass.GetIconName(); } catch { }
            if (string.IsNullOrEmpty(icon))
                icon = itemClass.GetItemName();
            s_iconByClassId[id] = icon ?? string.Empty;
            return string.IsNullOrEmpty(icon) ? null : icon;
        }

        public static bool IsItemIcon(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
                return false;
            // server_refresh is a UIAtlas sprite (toolbelt quick-swap / loot refresh), not an item icon.
            if (string.Equals(spriteName, IconRefresh, StringComparison.Ordinal))
                return false;
            return !spriteName.StartsWith("ui_game_", StringComparison.Ordinal)
                && !spriteName.StartsWith("menu_", StringComparison.Ordinal);
        }

        public static bool IsTargetRow(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(TargetRowPrefix, StringComparison.Ordinal);
        }

        public static int ParseTargetIcons(string text, string[] dest)
        {
            if (dest == null)
                return 0;
            for (int i = 0; i < dest.Length; i++)
                dest[i] = string.Empty;
            if (!IsTargetRow(text) || text.Length <= TargetRowPrefix.Length)
                return 0;

            string[] parts = text.Substring(TargetRowPrefix.Length).Split(',');
            int n = 0;
            for (int i = 0; i < parts.Length && n < dest.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i]))
                    continue;
                dest[n++] = parts[i];
            }
            return n;
        }

        public static string FormatLootRefresh(TEFeatureStorage storage)
        {
            if (storage == null || StoragePromptAccess.IsPlayerStorage(storage))
                return null;
            if (storage.Parent != null && storage.Parent.PlayerPlaced)
                return null;
            if (!StoragePromptAccess.IsTouched(storage))
                return null;
            if (!WorldLootCanRefresh(storage))
                return null;

            int days = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
            if (days <= 0)
                return null;

            ulong touched = StoragePromptAccess.WorldTimeTouched(storage);
            if (touched == 0)
                return null;

            ulong refreshAt = touched + (ulong)days * 24000UL;
            var (day, hour, minute) = GameUtils.WorldTimeToElements(refreshAt);
            string clock = hour.ToString("D2") + ":" + minute.ToString("D2");
            string text = string.Format(Localization.Get("xuiLootRefreshAGF"), day, clock);
            return FormatIconRow(IconRefresh, text, SlotR0);
        }

        // destroy_on_close true = gone when closed (airdrops). empty = gone or swapped to a
        // non-loot "Open" model when emptied (safes, backpacks, nests). Those never refill.
        public static bool WorldLootCanRefresh(TEFeatureStorage storage)
        {
            LootContainer loot = LootContainer.GetLootContainer(storage.lootListName, _errorOnMiss: false);
            if (loot == null)
                return false;
            return loot.destroyOnClose == LootContainer.DestroyOnClose.False;
        }

        public static string FormatSmelting(int used, int total, int slot = SlotL1)
        {
            return FormatIconRow(IconSmelt, FormatQueueCount(used, total), slot, blink: used > 0);
        }

        public static string FormatSeats(int seats, int slot = SlotR1)
        {
            return FormatIconRow(IconSeats, seats.ToString(), slot);
        }

        public static string FormatOwner(string ownerName)
        {
            if (!ShouldShowOwner() || string.IsNullOrEmpty(ownerName))
                return null;
            return FormatIconRow(IconPlayer, ownerName, SlotOwner);
        }

        public static string FormatLock(bool locked, int slot = SlotLock)
        {
            return FormatIconRow(
                locked ? IconLock : IconUnlock,
                locked ? Localization.Get("xuiLockedAGF") : Localization.Get("xuiUnlockedAGF"),
                slot,
                locked ? ColorLocked : ColorUnlocked);
        }

        public static string FormatDurability(int current, int max, int slot = SlotL0)
        {
            if (current < 0)
                current = 0;
            if (max < 0)
                max = 0;
            float fill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            return FormatIconRow(IconRepair, current + "/" + max, slot, null, fill, FillDurability);
        }

        public static string FormatGas(float current, float max, int slot = SlotR0)
        {
            if (current < 0f)
                current = 0f;
            if (max < 0f)
                max = 0f;
            float fill = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            return FormatIconRow(IconGas, Mathf.FloorToInt(current) + "/" + Mathf.FloorToInt(max), slot, null, fill, FillGas);
        }

        public static string FormatVehicleGas(float current, float max, int slot = SlotL1)
        {
            if (max <= 0f)
                return null;
            return FormatGas(current * VehicleFuelDisplayScale, max * VehicleFuelDisplayScale, slot);
        }

        public static bool VehicleUsesGas(Vehicle vehicle)
        {
            return vehicle != null && vehicle.GetMaxFuelLevel() > 0f;
        }

        public static bool WorkstationHasFuelModule(Block block)
        {
            if (block?.Properties?.Classes == null)
                return false;
            if (!block.Properties.Classes.TryGetValue("Workstation", out var workstation) || workstation == null)
                return false;
            string modules = workstation.GetString("Modules");
            if (string.IsNullOrEmpty(modules))
                return false;
            string[] parts = modules.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i].Trim(), "fuel", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static string FormatAmmo(int current, int max, int slot = SlotR0, string icon = null)
        {
            if (current < 0)
                current = 0;
            if (max < 0)
                max = 0;
            return FormatIconRow(string.IsNullOrEmpty(icon) ? IconAmmo : icon, FormatBagCount(current, max), slot);
        }

        public static bool TryGetTurretMagazine(EntityTurret turret, out int current, out int max, out string ammoIcon)
        {
            current = 0;
            max = 0;
            ammoIcon = IconAmmo;
            if (turret?.OriginalItemValue == null)
                return false;

            var itemValue = turret.OriginalItemValue;
            var itemClass = itemValue.ItemClass;
            if (itemClass?.Actions == null || itemClass.Actions.Length == 0)
                return false;

            var attack = itemClass.Actions[0] as ItemActionAttack;
            if (attack == null || attack.MagazineItemNames == null || attack.MagazineItemNames.Length == 0)
                return false;

            current = turret.AmmoCount;
            max = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, attack.BulletsPerMagazine);
            if (max < 0)
                max = 0;
            ammoIcon = ItemIconName(ItemClass.GetItemClass(attack.MagazineItemNames[0])) ?? IconAmmo;
            return true;
        }

        public static string FormatPowerState(bool isOn)
        {
            return FormatIconRow(
                IconPower,
                isOn ? Localization.Get("xuiOnAGF") : Localization.Get("xuiOffAGF"),
                SlotL0,
                isOn ? ColorOn : ColorLocked);
        }

        public static string FormatDroneOrder(bool isFollow, int slot)
        {
            return FormatIconRow(
                isFollow ? IconFollow : IconStay,
                Localization.Get(isFollow ? "entitycommand_drone_command_follow" : "entitycommand_drone_command_stay"),
                slot,
                isFollow ? ColorOn : ColorLocked);
        }

        public static string FormatQuietMode(bool isQuiet, int slot)
        {
            return FormatIconRow(
                IconTalk,
                Localization.Get(isQuiet ? "xuiDroneMuteAGF" : "xuiDroneTalkingAGF"),
                slot,
                isQuiet ? ColorLocked : ColorOn);
        }

        public static string FormatPowerCombined(int current, int max, int slot)
        {
            if (current < 0)
                current = 0;
            if (max < 0)
                max = 0;
            float fill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            string w = WattSuffix();
            return FormatIconRow(IconMaxOutput, current + w + " / " + max + w, slot, null, fill, FillPower);
        }

        private static string WattSuffix()
        {
            if (s_wattSuffix != null)
                return s_wattSuffix;
            string loc = Localization.Get("xuiWattAGF");
            s_wattSuffix = string.IsNullOrEmpty(loc) ? "w" : loc;
            return s_wattSuffix;
        }

        public static void AddLockableWorkstationRows(List<string> lines, WorldBase world, Vector3i blockPos)
        {
            if (lines == null || world == null)
                return;
            if (!LockableStationsClient.CanShowStationLock(world, blockPos, out bool isLocked, out var owner, out _))
                return;

            lines.Add(FormatLock(isLocked));
            string ownerLine = FormatOwner(ResolveOwnerName(owner));
            if (!string.IsNullOrEmpty(ownerLine))
                lines.Add(ownerLine);
        }

        public static bool TryGetBlockHitpoints(WorldBase world, Vector3i blockPos, out int current, out int max)
        {
            current = 0;
            max = 0;
            if (world == null)
                return false;

            BlockValue bv = world.GetBlock(blockPos);
            if (bv.ischild && bv.Block?.multiBlockPos != null)
            {
                blockPos = bv.Block.multiBlockPos.GetParentPos(blockPos, bv);
                bv = world.GetBlock(blockPos);
            }

            Block block = bv.Block;
            if (block == null)
                return false;

            max = block.MaxDamage;
            if (max <= 0)
                return false;

            current = Mathf.Max(0, max - bv.damage);
            return true;
        }

        public static void CountTrapAmmo(TileEntityPoweredRangedTrap trap, out int current, out int max)
        {
            current = 0;
            max = 0;
            if (trap == null)
                return;

            int fallback = 0;
            var ammoItems = trap.AmmoItems;
            if (ammoItems != null)
            {
                for (int i = 0; i < ammoItems.Length; i++)
                    fallback = Mathf.Max(fallback, CachedStackMax(ammoItems[i]));
            }

            var slots = trap.ItemSlots;
            if (slots == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                var stack = slots[i];
                if (stack != null && !stack.IsEmpty())
                {
                    current += StackAccess.Count(stack);
                    int stackMax = CachedStackMax(StackAccess.Value(stack)?.ItemClass);
                    max += stackMax > 0 ? stackMax : fallback;
                }
                else
                    max += fallback;
            }

            if (max <= 0 && slots.Length > 0)
            {
                current = CountUsedSlots(slots);
                max = slots.Length;
            }
        }

        private static int CachedStackMax(ItemClass itemClass)
        {
            if (itemClass == null)
                return 0;
            int id = itemClass.Id;
            if (s_stackMaxByClassId.TryGetValue(id, out int max))
                return max;
            max = itemClass.Stacknumber != null ? itemClass.Stacknumber.Value : 0;
            s_stackMaxByClassId[id] = max;
            return max;
        }

        public static string FormatTargeting(TileEntityPoweredRangedTrap trap, int slot = SlotL1)
        {
            if (trap == null || !trap.ShowTargeting)
                return null;

            var parts = new List<string>(4);
            if (trap.TargetSelf)
                parts.Add(IconTargetSelf);
            if (trap.TargetAllies)
                parts.Add(IconTargetAllies);
            if (trap.TargetStrangers)
                parts.Add(IconTargetStrangers);
            if (trap.TargetZombies)
                parts.Add(IconTargetZombies);

            return FormatIconRow(IconTargeting, TargetRowPrefix + string.Join(",", parts.ToArray()), slot);
        }

        public static string TrapAmmoIcon(TileEntityPoweredRangedTrap trap)
        {
            var slots = trap?.ItemSlots;
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] == null || slots[i].IsEmpty())
                        continue;
                    string icon = ItemIconName(StackAccess.Value(slots[i])?.ItemClass);
                    if (!string.IsNullOrEmpty(icon))
                        return icon;
                }
            }

            var ammoItems = trap?.AmmoItems;
            if (ammoItems != null)
            {
                for (int i = 0; i < ammoItems.Length; i++)
                {
                    string icon = ItemIconName(ammoItems[i]);
                    if (!string.IsNullOrEmpty(icon))
                        return icon;
                }
            }

            return IconAmmo;
        }

        public static void AddRangedTrapRows(List<string> lines, WorldBase world, Vector3i blockPos)
        {
            if (lines == null || world == null)
                return;

            var trap = world.GetTileEntity(blockPos) as TileEntityPoweredRangedTrap;
            if (trap == null)
                return;

            CountTrapAmmo(trap, out int ammo, out int ammoMax);
            lines.Add(FormatAmmo(ammo, ammoMax, SlotL0, TrapAmmoIcon(trap)));
            if (TryGetBlockHitpoints(world, blockPos, out int hp, out int hpMax))
                lines.Add(FormatDurability(hp, hpMax, SlotR0));
            string targeting = FormatTargeting(trap, SlotL1);
            if (!string.IsNullOrEmpty(targeting))
                lines.Add(targeting);
        }

        public static bool HasFuelReady(TileEntityWorkstation tileEntity, out bool isBurning, out bool hasFuelSlot, out float burnTimeLeft)
        {
            isBurning = false;
            hasFuelSlot = false;
            burnTimeLeft = 0f;
            if (tileEntity == null)
                return false;

            try { isBurning = tileEntity.IsBurning; } catch { }
            try { burnTimeLeft = tileEntity.BurnTimeLeft; } catch { }

            var fuelArray = tileEntity.Fuel;
            if (fuelArray != null)
            {
                for (int i = 0; i < fuelArray.Length; i++)
                {
                    if (fuelArray[i] != null && !fuelArray[i].IsEmpty())
                    {
                        hasFuelSlot = true;
                        break;
                    }
                }
            }

            return isBurning || hasFuelSlot || burnTimeLeft > 0f;
        }

        public static string FuelCraftingStatus(TileEntityWorkstation tileEntity)
        {
            if (tileEntity == null || !tileEntity.hasRecipeInQueue())
                return string.Empty;

            HasFuelReady(tileEntity, out bool isBurning, out bool hasFuelSlot, out float burnTimeLeft);
            if (isBurning && (hasFuelSlot || burnTimeLeft > 0f))
                return Localization.Get("xuiQueued");
            if (!isBurning && (hasFuelSlot || burnTimeLeft > 0f))
                return Localization.Get("xuiNeed2TurnOn");
            return Localization.Get("xuiNoFuel");
        }

        public static string FormatFuelStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return null;

            string queued = Localization.Get("xuiQueued");
            string icon = (!string.IsNullOrEmpty(queued) && status.IndexOf(queued, StringComparison.OrdinalIgnoreCase) >= 0)
                ? IconCraft
                : IconFuel;
            return FormatIconRow(icon, status, SlotL1);
        }

        public static string FormatQueue(TileEntityWorkstation tileEntity, int slot = SlotL0)
        {
            var queue = tileEntity?.Queue;
            int total = queue?.Length ?? 0;
            int used = 0;
            if (queue != null)
            {
                for (int i = 0; i < queue.Length; i++)
                {
                    var item = queue[i];
                    if (item != null && item.Recipe != null && item.Multiplier > 0)
                        used++;
                }
            }
            return FormatIconRow(IconCraft, FormatQueueCount(used, total), slot, blink: used > 0);
        }

        public static string FormatTools(TileEntityWorkstation tileEntity, string icon)
        {
            var tools = tileEntity?.Tools;
            if (tools == null || tools.Length == 0)
                return null;

            var names = new List<string>();
            for (int i = 0; i < tools.Length; i++)
            {
                var stack = tools[i];
                ItemValue held = stack == null ? null : StackAccess.Value(stack);
                if (stack == null || stack.IsEmpty() || held == null)
                    continue;

                var itemClass = held.ItemClass;
                if (itemClass == null)
                    continue;

                string name = itemClass.GetLocalizedItemName();
                if (string.IsNullOrEmpty(name))
                    name = itemClass.GetItemName();
                if (held.HasQuality && StackAccess.Quality(held) > 0)
                    name += " T" + StackAccess.Quality(held);
                names.Add(name);
            }

            if (names.Count == 0)
                return null;
            return FormatIconRow(icon, string.Join(", ", names.ToArray()), SlotR0);
        }

        public static string FormatFuelTime(TileEntityWorkstation tileEntity, int slot = SlotL0)
        {
            float seconds = ReadFuelSeconds(tileEntity);
            if (seconds < 0f)
                seconds = 0f;

            int total = Mathf.FloorToInt(seconds);
            string clock = (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
            return FormatIconRow(IconFuel, clock, slot);
        }

        // Workstation burn time only steps on world ticks (~2s). Interpolate while burning
        // so the mm:ss label can change every second.
        private static float ReadFuelSeconds(TileEntityWorkstation tileEntity)
        {
            if (tileEntity == null)
                return 0f;

            bool burning = false;
            float teSeconds = 0f;
            try { burning = tileEntity.IsBurning; } catch { }
            try
            {
                teSeconds = tileEntity.BurnTotalTimeLeft;
            }
            catch
            {
                teSeconds = 0f;
            }

            if (!burning)
            {
                s_fuelTeId = int.MinValue;
                return teSeconds;
            }

            int id = tileEntity.ToWorldPos().GetHashCode();
            float now = Time.unscaledTime;
            if (id != s_fuelTeId || teSeconds > s_fuelTeSeconds + 0.25f || teSeconds < s_fuelTeSeconds - 2.6f)
            {
                s_fuelTeId = id;
                s_fuelTeSeconds = teSeconds;
                s_fuelSampleAt = now;
            }

            return Mathf.Max(0f, s_fuelTeSeconds - (now - s_fuelSampleAt));
        }

        public static string GetActivateMarkup()
        {
            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player?.playerInput == null)
                return string.Empty;
            return player.playerInput.Activate.GetBindingXuiMarkupString()
                + player.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
        }

        public static string ResolveOwnerName(PlatformUserIdentifierAbs owner)
        {
            if (owner == null)
                return null;

            PersistentPlayerData data = GameManager.Instance?.GetPersistentPlayerList()?.GetPlayerData(owner);
            if (data == null)
                data = GameManager.Instance?.persistentPlayers?.GetPlayerData(owner);

            string name = data?.PlayerName?.DisplayName;
            if (string.IsNullOrEmpty(name))
                return null;
            return name;
        }

        public static string BuildPrompt(string header, List<string> lines)
        {
            var sb = new StringBuilder();
            sb.Append(header ?? string.Empty);
            sb.Append(SplitToken);
            if (lines == null)
                return sb.ToString();

            bool first = true;
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;
                if (!first)
                    sb.Append('\n');
                first = false;
                sb.Append(lines[i]);
            }
            return sb.ToString();
        }

        public static void SplitPrompt(string raw, out string header, out string details)
        {
            header = raw ?? string.Empty;
            details = string.Empty;
            if (string.IsNullOrEmpty(raw))
                return;

            int split = raw.IndexOf(SplitToken, StringComparison.Ordinal);
            if (split >= 0)
            {
                header = raw.Substring(0, split);
                details = raw.Substring(split + SplitToken.Length);
            }
        }

        public static void ParseRows(string details, string[] icons, string[] texts, string[] colors, string[] fills = null, string[] fillColors = null, bool[] blinks = null)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = string.Empty;
                texts[i] = string.Empty;
                if (colors != null && i < colors.Length)
                    colors[i] = string.Empty;
                if (fills != null && i < fills.Length)
                    fills[i] = string.Empty;
                if (fillColors != null && i < fillColors.Length)
                    fillColors[i] = string.Empty;
                if (blinks != null && i < blinks.Length)
                    blinks[i] = false;
            }

            if (string.IsNullOrEmpty(details))
                return;

            string[] lines = details.Split('\n');
            int row = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;

                string icon = string.Empty;
                string text = lines[i];
                string color = string.Empty;
                string fill = string.Empty;
                string fillColor = string.Empty;
                bool blink = false;
                int slot = -1;

                int tab = lines[i].IndexOf('\t');
                if (tab >= 0)
                {
                    icon = lines[i].Substring(0, tab);
                    string rest = lines[i].Substring(tab + 1);
                    int tab2 = rest.IndexOf('\t');
                    if (tab2 >= 0)
                    {
                        text = rest.Substring(0, tab2);
                        string[] parts = rest.Substring(tab2 + 1).Split('\t');
                        int n = parts.Length;
                        if (n > 0 && parts[n - 1] == "blink")
                        {
                            blink = true;
                            n--;
                        }
                        if (n > 0)
                            int.TryParse(parts[0], out slot);
                        if (n > 1)
                            color = parts[1];
                        if (n > 2)
                            fill = parts[2];
                        if (n > 3)
                            fillColor = parts[3];
                    }
                    else
                    {
                        text = rest;
                    }
                }

                int dest = slot;
                if (dest < 0 || dest >= icons.Length)
                {
                    if (row >= icons.Length)
                        continue;
                    dest = row;
                    row++;
                }

                icons[dest] = icon;
                texts[dest] = text;
                if (colors != null && dest < colors.Length)
                    colors[dest] = color;
                if (fills != null && dest < fills.Length)
                    fills[dest] = fill;
                if (fillColors != null && dest < fillColors.Length)
                    fillColors[dest] = fillColor;
                if (blinks != null && dest < blinks.Length)
                    blinks[dest] = blink;
            }
        }
    }

    public static class PromptFeatureFlags
    {
        public static bool EnableSecureLootPrompt = true;
        public static bool EnableForgePrompt = true;
    }

    [HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.GetActivationText))]
    public static class Patch_TEFeatureStorage_GetActivationText
    {
        public static void Postfix(ref string __result, TEFeatureStorage __instance, string _activateHotkeyMarkup, string _focusedTileEntityName)
        {
            if (!PromptFeatureFlags.EnableSecureLootPrompt)
                return;
            if (__instance == null)
                return;
            var items = StoragePromptAccess.Items(__instance);
            if (items == null)
                return;
            if (PromptStateHelpers.IsDeniedStoragePrompt(__result))
                return;

            if (!StoragePromptAccess.IsPlayerStorage(__instance) && !StoragePromptAccess.IsTouched(__instance))
                return;
            int used = PromptStateHelpers.CountUsedSlots(items);
            var lines = new List<string>
            {
                PromptStateHelpers.FormatSlots(used, items.Length)
            };
            string refreshLine = PromptStateHelpers.FormatLootRefresh(__instance);
            if (!string.IsNullOrEmpty(refreshLine))
                lines.Add(refreshLine);

            var lockFeature = __instance.lockFeature;
            if (lockFeature != null)
            {
                lines.Add(PromptStateHelpers.FormatLock(lockFeature.IsLocked()));
                string ownerLine = PromptStateHelpers.FormatOwner(
                    PromptStateHelpers.ResolveOwnerName(lockFeature.GetOwner() ?? __instance.Parent?.Owner));
                if (!string.IsNullOrEmpty(ownerLine))
                    lines.Add(ownerLine);
            }

            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(BlockForge), nameof(BlockForge.GetActivationText))]
    public static class Patch_BlockForge_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            _ = _entityFocusing;
            if (!PromptFeatureFlags.EnableForgePrompt)
                return;

            Vector3i forgePos = _blockPos;
            BlockValue forgeBlockValue = _blockValue;
            if (_blockValue.ischild && _blockValue.Block != null && _blockValue.Block.multiBlockPos != null)
            {
                forgePos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                forgeBlockValue = _world.GetBlock(forgePos);
            }

            if (PromptStateHelpers.ShouldHideDetails(ref __result, _world, forgePos))
                return;

            if (!(forgeBlockValue.Block is BlockForge))
                return;

            var teForge = _world.GetTileEntity(forgePos) as TileEntityWorkstation;
            if (teForge == null)
                return;

            int smeltUsed = 0;
            int smeltTotal = 0;
            var inputArray = teForge.Input;
            if (inputArray != null)
            {
                int primaryInputSlots = Math.Min(teForge.InputSlotCount, inputArray.Length);
                smeltTotal = primaryInputSlots;
                for (int i = 0; i < primaryInputSlots; i++)
                {
                    if (inputArray[i] != null && !inputArray[i].IsEmpty())
                        smeltUsed++;
                }
            }

            var outputArray = teForge.Output;
            int outputTotal = outputArray?.Length ?? 0;
            int outputUsed = PromptStateHelpers.CountUsedSlots(outputArray);

            var lines = new List<string>
            {
                PromptStateHelpers.FormatFuelTime(teForge, PromptStateHelpers.SlotL0),
                PromptStateHelpers.FormatQueue(teForge, PromptStateHelpers.SlotR0),
                PromptStateHelpers.FormatSmelting(smeltUsed, smeltTotal, PromptStateHelpers.SlotL1),
                PromptStateHelpers.FormatOutput(outputUsed, outputTotal, PromptStateHelpers.SlotR1)
            };
            PromptStateHelpers.AddLockableWorkstationRows(lines, _world, forgePos);

            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.GetActivationText))]
    public static class Patch_BlockWorkstation_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            _ = _entityFocusing;
            if (_blockValue.Block is BlockForge)
                return;
            if (PromptStateHelpers.ShouldHideDetails(ref __result, _world, _blockPos))
                return;

            var tileEntity = _world.GetTileEntity(_blockPos) as TileEntityWorkstation;
            var outputArray = tileEntity?.Output;
            int total = outputArray?.Length ?? 0;
            int used = PromptStateHelpers.CountUsedSlots(outputArray);

            bool hasFuel = PromptStateHelpers.WorkstationHasFuelModule(_blockValue.Block);

            var lines = new List<string>();
            if (hasFuel)
            {
                lines.Add(PromptStateHelpers.FormatFuelTime(tileEntity, PromptStateHelpers.SlotL0));
                lines.Add(PromptStateHelpers.FormatQueue(tileEntity, PromptStateHelpers.SlotR0));
                lines.Add(PromptStateHelpers.FormatOutput(used, total, PromptStateHelpers.SlotL1));
            }
            else
            {
                lines.Add(PromptStateHelpers.FormatQueue(tileEntity, PromptStateHelpers.SlotL0));
                lines.Add(PromptStateHelpers.FormatOutput(used, total, PromptStateHelpers.SlotR0));
            }
            PromptStateHelpers.AddLockableWorkstationRows(lines, _world, _blockPos);

            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.GetActivationText))]
    public static class Patch_EntityVehicle_GetActivationText
    {
        public static void Postfix(EntityVehicle __instance, ref string __result)
        {
            if (__instance == null)
                return;

            var lines = new List<string>();

            var items = StoragePromptAccess.BagSlots(__instance.bag);
            int slotTotal = items?.Length ?? 0;
            int slotUsed = PromptStateHelpers.CountUsedSlots(items);
            lines.Add(PromptStateHelpers.FormatSlots(slotUsed, slotTotal, PromptStateHelpers.SlotL0));

            var vehicle = __instance.GetVehicle();
            if (vehicle != null)
            {
                int current = vehicle.GetHealth();
                int max = vehicle.GetMaxHealth();
                if (max > 0 && current > max)
                    current = max;
                lines.Add(PromptStateHelpers.FormatDurability(current, max, PromptStateHelpers.SlotR0));
            }
            else
                lines.Add(PromptStateHelpers.FormatDurability(0, 0, PromptStateHelpers.SlotR0));

            if (PromptStateHelpers.VehicleUsesGas(vehicle))
                lines.Add(PromptStateHelpers.FormatVehicleGas(vehicle.GetFuelLevel(), vehicle.GetMaxFuelLevel(), PromptStateHelpers.SlotL1));

            lines.Add(PromptStateHelpers.FormatSeats(__instance.GetAttachMaxCount(), PromptStateHelpers.SlotR1));

            lines.Add(PromptStateHelpers.FormatLock(__instance.isLocked));

            string flightModeLine = GyroFlightModesClient.TryFormatFlightModeRow(__instance);
            if (!string.IsNullOrEmpty(flightModeLine))
                lines.Add(flightModeLine);

            string ownerLine = PromptStateHelpers.FormatOwner(PromptStateHelpers.ResolveOwnerName(__instance.GetOwner()));
            if (!string.IsNullOrEmpty(ownerLine))
                lines.Add(ownerLine);

            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    // The placeable item's MaxUseTimes is stored as Health.BaseMax and is the durability that sticks.
    // Health.Max can sit higher, so GetHealth()/GetMaxHealth() shows a gap on a full vehicle
    // and a repair kit adds HP that the stat drops on the next tick.
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.GetMaxHealth))]
    public static class Patch_Vehicle_GetMaxHealth
    {
        public static void Postfix(Vehicle __instance, ref int __result)
        {
            var stat = __instance?.entity?.Stats?.Health;
            if (stat == null)
                return;

            int baseMax = (int)stat.BaseMax;
            if (baseMax > 0)
                __result = baseMax;
        }
    }

    [HarmonyPatch(typeof(BlockCollector), nameof(BlockCollector.GetActivationText))]
    public static class Patch_BlockCollector_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, Vector3i _blockPos)
        {
            if (PromptStateHelpers.ShouldHideDetails(ref __result, _world, _blockPos))
                return;

            var lines = new List<string>();
            PromptStateHelpers.AddLockableWorkstationRows(lines, _world, _blockPos);
            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(BlockPowerSource), nameof(BlockPowerSource.GetActivationText))]
    public static class Patch_BlockPowerSource_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, Vector3i _blockPos)
        {
            Vector3i pos = LockableStationsClient.ResolveParentIfChild(_world, _blockPos);
            if (PromptStateHelpers.ShouldHideDetails(ref __result, _world, pos))
                return;
            var lines = new List<string>();
            var powerTe = _world.GetTileEntity(pos) as TileEntityPowerSource;
            if (powerTe != null)
            {
                bool isOn = powerTe.IsOn;
                bool isGenerator = powerTe.PowerItemType == PowerItem.PowerItemTypes.Generator;
                bool showGas = isGenerator && powerTe.MaxFuel > 0;
                lines.Add(PromptStateHelpers.FormatPowerState(isOn));
                if (showGas)
                    lines.Add(PromptStateHelpers.FormatGas(powerTe.CurrentFuel, powerTe.MaxFuel, PromptStateHelpers.SlotR0));
                lines.Add(PromptStateHelpers.FormatPowerCombined(
                    powerTe.LastOutput, powerTe.MaxOutput,
                    showGas ? PromptStateHelpers.SlotL1 : PromptStateHelpers.SlotR0));

                var items = powerTe.ItemSlots;
                int slotTotal = items?.Length ?? 0;
                string slotIcon = PromptStateHelpers.ItemIconName(powerTe.SlotItem)
                    ?? PromptStateHelpers.IconLoot;
                lines.Add(PromptStateHelpers.FormatPowerSlots(
                    PromptStateHelpers.CountUsedSlots(items), slotTotal,
                    showGas ? PromptStateHelpers.SlotR1 : PromptStateHelpers.SlotL1, slotIcon));
            }
            PromptStateHelpers.AddLockableWorkstationRows(lines, _world, pos);
            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(BlockLauncher), nameof(BlockLauncher.GetActivationText))]
    public static class Patch_BlockLauncher_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, Vector3i _blockPos)
        {
            Vector3i pos = LockableStationsClient.ResolveParentIfChild(_world, _blockPos);
            if (PromptStateHelpers.ShouldHideDetails(ref __result, _world, pos))
                return;
            var lines = new List<string>();
            PromptStateHelpers.AddRangedTrapRows(lines, _world, pos);
            PromptStateHelpers.AddLockableWorkstationRows(lines, _world, pos);
            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(BlockRanged), nameof(BlockRanged.GetActivationText))]
    public static class Patch_BlockRanged_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, Vector3i _blockPos)
        {
            Vector3i pos = LockableStationsClient.ResolveParentIfChild(_world, _blockPos);
            if (PromptStateHelpers.ShouldHideDetails(ref __result, _world, pos))
                return;
            var lines = new List<string>();
            PromptStateHelpers.AddRangedTrapRows(lines, _world, pos);
            PromptStateHelpers.AddLockableWorkstationRows(lines, _world, pos);
            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(EntityTurret), nameof(EntityTurret.GetActivationText))]
    public static class Patch_EntityTurret_GetActivationText
    {
        public static void Postfix(EntityTurret __instance, ref string __result)
        {
            if (__instance == null)
                return;

            var lines = new List<string>();
            var itemValue = __instance.OriginalItemValue;
            bool hasMag = PromptStateHelpers.TryGetTurretMagazine(__instance, out int ammo, out int mag, out string ammoIcon);
            if (hasMag)
                lines.Add(PromptStateHelpers.FormatAmmo(ammo, mag, PromptStateHelpers.SlotL0, ammoIcon));
            if (itemValue != null)
            {
                int hpSlot = hasMag ? PromptStateHelpers.SlotR0 : PromptStateHelpers.SlotL0;
                lines.Add(PromptStateHelpers.FormatDurability(__instance.Health, itemValue.MaxUseTimes, hpSlot));
            }

            string ownerLine = PromptStateHelpers.FormatOwner(
                PromptStateHelpers.ResolveOwnerName(__instance.OwnerID));
            if (!string.IsNullOrEmpty(ownerLine))
                lines.Add(ownerLine);

            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }

    [HarmonyPatch(typeof(EntityDrone), nameof(EntityDrone.GetActivationText))]
    public static class Patch_EntityDrone_GetActivationText
    {
        public static void Postfix(EntityDrone __instance, ref string __result)
        {
            if (__instance == null)
                return;

            var lines = new List<string>();
            var items = StoragePromptAccess.BagSlots(__instance.bag);
            int slotTotal = items?.Length ?? 0;
            int slotUsed = PromptStateHelpers.CountUsedSlots(items);
            lines.Add(PromptStateHelpers.FormatSlots(slotUsed, slotTotal, PromptStateHelpers.SlotL0));
            lines.Add(PromptStateHelpers.FormatDurability(__instance.Health, __instance.GetMaxHealth(), PromptStateHelpers.SlotR0));
            lines.Add(PromptStateHelpers.FormatDroneOrder(
                __instance.OrderState == EntityDrone.Orders.Follow, PromptStateHelpers.SlotL1));
            lines.Add(PromptStateHelpers.FormatQuietMode(__instance.isQuietMode, PromptStateHelpers.SlotR1));

            string ownerLine = PromptStateHelpers.FormatOwner(
                PromptStateHelpers.ResolveOwnerName(__instance.OwnerID ?? __instance.GetOwner()));
            if (!string.IsNullOrEmpty(ownerLine))
                lines.Add(ownerLine);

            __result = PromptStateHelpers.BuildPrompt(__result, lines);
        }
    }
}
