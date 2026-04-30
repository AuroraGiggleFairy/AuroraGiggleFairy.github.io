using System;
using HarmonyLib;
using Platform;
namespace ExpandedInteractionPrompts
{
    public static class PromptStateHelpers
    {
        private static bool _checkedLockableWorkstations;
        private static bool _isLockableWorkstationsActive;

        private static bool IsLockableWorkstationsActive()
        {
            if (_checkedLockableWorkstations)
                return _isLockableWorkstationsActive;

            _checkedLockableWorkstations = true;
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    string name = assemblies[i].GetName().Name;
                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (name.IndexOf("LockableWorkstations", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _isLockableWorkstationsActive = true;
                        break;
                    }
                }
            }
            catch
            {
                _isLockableWorkstationsActive = false;
            }

            return _isLockableWorkstationsActive;
        }

        public static bool IsJammedPrompt(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Compatibility behavior: only suppress extra details when LockableWorkstations is active.
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
    }

    public static class PromptFeatureFlags
    {
        // Set true to enable secure loot/player storage prompt details.
        public static bool EnableSecureLootPrompt = true;
        // Set true to enable forge prompt details.
        public static bool EnableForgePrompt = true;
    }

    // Safe formatter to prevent FormatException from mismatched arguments
    public static class SafeFormatter
    {
        // Handles up to two arguments, ignores extra placeholders, and fills missing with empty string
        public static string BulletproofFormat(string format, string arg0, string arg1)
        {
            if (format == null) return string.Empty;
            // Replace {0} and {1} safely
            string result = format.Replace("{0}", arg0 ?? "");
            result = result.Replace("{1}", arg1 ?? "");
            // Remove any remaining {0} or {1} to avoid confusion
            result = result.Replace("{0}", "").Replace("{1}", "");
            return result;
        }

        public static string FocusAwareFormat(string format, object arg0, object arg1)
        {
            string result = BulletproofFormat(format, arg0?.ToString(), arg1?.ToString());
            if (string.IsNullOrEmpty(result))
                return result;

            if (!string.Equals(format, Localization.Get("tooltipInteract"), System.StringComparison.Ordinal))
                return result;

            var hitInfo = Voxel.voxelRayHitInfo;
            if (!hitInfo.bHitValid || hitInfo.transform == null || string.IsNullOrEmpty(hitInfo.tag) || !hitInfo.tag.StartsWith("E_"))
                return result;

            var entityVehicle = EntityVehicle.FindCollisionEntity(hitInfo.transform);
            if (entityVehicle == null)
                return result;

            bool isOwner = entityVehicle.LocalPlayerIsOwner();
            bool isAllowed = entityVehicle.isAllowedUser(PlatformManager.InternalLocalUserIdentifier);

            // Show seat count for everyone
            int seatCount = entityVehicle.GetAttachMaxCount();
            if (seatCount > 0)
            {
                string seatInfo = Localization.Get("xuiSeatsAGF").Replace("{0}", seatCount.ToString());
                result = result.TrimEnd() + " " + seatInfo;
            }

            // Show lock status for everyone
            string lockStatus = entityVehicle.isLocked 
                ? Localization.Get("xuiLockedAGF")
                : Localization.Get("xuiUnlockedAGF");
            result = result.TrimEnd() + " " + lockStatus;

            // Show storage info after lock status, only for owner/ally
            if (isOwner || isAllowed)
            {
                // Vehicle storage items are stored in bag, not lootContainer
                var items = entityVehicle.bag?.GetSlots();
                if (items != null && items.Length > 0)
                {
                    int used = 0;
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i] != null && !items[i].IsEmpty())
                            used++;
                    }

                    string slotInfo = Localization.Get("xuiSlots").Replace("{0}", $"{used}/{items.Length}");
                    result = result.TrimEnd() + " " + slotInfo;
                }
            }

            return result;
        }
    }
    // =========================================
    // === PATCH: Secure Loot Container Prompts ===
    // =========================================
    [HarmonyPatch(typeof(BlockSecureLoot), nameof(BlockSecureLoot.GetActivationText))]
    public static class Patch_BlockSecureLoot_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            if (!PromptFeatureFlags.EnableSecureLootPrompt)
                return;

            var tileEntity = _world.GetTileEntity(_blockPos) as TileEntitySecureLootContainer;
            if (tileEntity == null)
                tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainer;
            if (tileEntity == null)
                return;
            // Show for unlocked or locked; do not show for jammed.
            var secureBlock = _blockValue.Block as BlockSecureLoot;
            bool isJammed = tileEntity.IsLocked()
                && secureBlock != null
                && secureBlock.lockPickItem == null
                && !tileEntity.LocalPlayerIsOwner();
            if (isJammed)
                return; // Don't append slot info for jammed

            // Count filled slots
            var items = tileEntity.items;
            int total = items?.Length ?? 0;
            int used = 0;
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null && !items[i].IsEmpty())
                        used++;
                }
            }
            string slotInfo = (used == 0)
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{used}/{total}");

            __result = __result.TrimEnd() + " " + slotInfo;
        }
    }

    [HarmonyPatch(typeof(BlockSecureLootSigned), nameof(BlockSecureLootSigned.GetActivationText))]
    public static class Patch_BlockSecureLootSigned_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            if (!PromptFeatureFlags.EnableSecureLootPrompt)
                return;

            var tileEntity = _world.GetTileEntity(_blockPos) as TileEntitySecureLootContainer;
            if (tileEntity == null)
                tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainer;
            if (tileEntity == null)
                return;

            var secureBlock = _blockValue.Block as BlockSecureLoot;
            bool isJammed = tileEntity.IsLocked()
                && secureBlock != null
                && secureBlock.lockPickItem == null
                && !tileEntity.LocalPlayerIsOwner();
            if (isJammed)
                return;

            var items = tileEntity.items;
            int total = items?.Length ?? 0;
            int used = 0;
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null && !items[i].IsEmpty())
                        used++;
                }
            }
            string slotInfo = (used == 0)
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{used}/{total}");

            __result = __result.TrimEnd() + " " + slotInfo;
        }
    }

    // Add slot info to regular loot prompts in the touched/non-empty state.
    [HarmonyPatch(typeof(BlockLoot), nameof(BlockLoot.GetActivationText))]
    public static class Patch_BlockLoot_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            var tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityLootContainer;
            if (tileEntity == null)
                tileEntity = _world.GetTileEntity(_blockPos) as TileEntityLootContainer;
            if (tileEntity == null)
                return;

            // Match lootTooltipTouched only.
            if (!tileEntity.bTouched || tileEntity.IsEmpty())
                return;

            var items = tileEntity.items;
            int total = items?.Length ?? 0;
            int used = 0;
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null && !items[i].IsEmpty())
                        used++;
                }
            }

            string slotInfo = (used == 0)
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{used}/{total}");

            __result = (__result ?? string.Empty).TrimEnd() + " " + slotInfo;
        }
    }

    // Composite storage-backed player containers use TEFeatureStorage activation text.
    [HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.GetActivationText))]
    public static class Patch_TEFeatureStorage_GetActivationText
    {
        public static void Postfix(ref string __result, TEFeatureStorage __instance)
        {
            if (!PromptFeatureFlags.EnableSecureLootPrompt)
                return;
            if (__instance == null || __instance.items == null)
                return;

            // Skip jammed prompts.
            if (!string.IsNullOrEmpty(__result) && __result.Contains(Localization.Get("tooltipJammed")))
                return;

            var items = __instance.items;
            int total = items.Length;
            int used = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && !items[i].IsEmpty())
                    used++;
            }

            string slotInfo = (used == 0)
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{used}/{total}");

            __result = (__result ?? string.Empty).TrimEnd() + " " + slotInfo;
        }
    }
    // =============================
    // === PATCH: useForge Prompt ===
    // =============================
    [HarmonyPatch(typeof(BlockForge), nameof(BlockForge.GetActivationText))]
    public static class Patch_BlockForge_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            if (!PromptFeatureFlags.EnableForgePrompt)
                return;

            if (PromptStateHelpers.IsJammedPrompt(__result))
                return;

            // Resolve parent position for multiblock child hits.
            Vector3i forgePos = _blockPos;
            BlockValue forgeBlockValue = _blockValue;
            if (_blockValue.ischild && _blockValue.Block != null && _blockValue.Block.multiBlockPos != null)
            {
                forgePos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
                forgeBlockValue = _world.GetBlock(forgePos);
            }

            // Ensure this is only applied to actual forges.
            if (!(forgeBlockValue.Block is BlockForge))
                return;

            // Forge uses TileEntityWorkstation in current versions.
            var teForge = _world.GetTileEntity(forgePos) as TileEntityWorkstation;
            if (teForge == null)
                teForge = _world.GetTileEntity(_clrIdx, forgePos) as TileEntityWorkstation;
            if (teForge == null)
                return;

            // 1. Smelting Data
            int smeltUsed = 0;
            int smeltTotal = 0;
            var inputArray = teForge.Input;
            if (inputArray != null)
            {
                // Count only primary smelting input slots, not internal material bins.
                int primaryInputSlots = System.Math.Min(3, inputArray.Length);
                smeltTotal = primaryInputSlots;
                for (int i = 0; i < primaryInputSlots; i++)
                {
                    if (inputArray[i] != null && !inputArray[i].IsEmpty())
                        smeltUsed++;
                }
            }
            string smeltInfo = Localization.Get("xuiSmeltingAGF").Replace("{0}", smeltUsed.ToString()).Replace("{1}", smeltTotal.ToString());

            // 2. Crafting Data
            string craftingStatus = string.Empty;
            bool hasQueue = false;
            bool isBurning = false;
            bool hasFuelSlot = false;
            float burnTimeLeft = 0f;
            try { hasQueue = teForge.hasRecipeInQueue(); } catch { }
            try { isBurning = teForge.IsBurning; } catch { }
            var fuelArray = teForge.Fuel;
            if (fuelArray != null)
            {
                foreach (var stack in fuelArray)
                {
                    if (stack != null && !stack.IsEmpty())
                    {
                        hasFuelSlot = true;
                        break;
                    }
                }
            }
            try { burnTimeLeft = teForge.BurnTimeLeft; } catch { }
            if (hasQueue)
            {
                if (isBurning && (hasFuelSlot || burnTimeLeft > 0f))
                    craftingStatus = Localization.Get("xuiQueued");
                else if (!isBurning && (hasFuelSlot || burnTimeLeft > 0f))
                    craftingStatus = Localization.Get("xuiNeed2TurnOn");
                else
                    craftingStatus = Localization.Get("xuiNoFuel");
            }

            // 3. Output (use same output slots as workstation UI)
            int outputUsed = 0;
            int outputTotal = 0;
            var outputArray = teForge.Output;
            if (outputArray != null)
            {
                outputTotal = outputArray.Length;
                for (int i = 0; i < outputArray.Length; i++)
                {
                    if (outputArray[i] != null && !outputArray[i].IsEmpty())
                        outputUsed++;
                }
            }
            string outputInfo = (outputUsed == 0)
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{outputUsed}/{outputTotal}");

            // Compose prompt: Smelting (parenthesis), crafting status, output
            string slotInfo = smeltInfo;
            if (!string.IsNullOrEmpty(craftingStatus))
                slotInfo += " " + craftingStatus;
            slotInfo += " " + outputInfo;

            // Compatibility mode: do not touch {1}; always append details.
            __result = __result.TrimEnd() + " " + slotInfo.Trim();
        }
    }

    // ================================
    // === PATCH: useCampfire Prompt ===
    // ================================
    [HarmonyPatch(typeof(BlockCampfire), nameof(BlockCampfire.GetActivationText))]
    public static class Patch_BlockCampfire_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            if (PromptStateHelpers.IsJammedPrompt(__result))
                return;

            var tileEntity = _world.GetTileEntity(_blockPos) as TileEntityWorkstation;
            int used = 0;
            int total = 0;
            string slotInfo = string.Empty;
            // Output slots
            var outputArray = tileEntity?.Output;
            if (outputArray != null && outputArray.Length > 0)
            {
                total = outputArray.Length;
                for (int i = 0; i < outputArray.Length; i++)
                {
                    if (outputArray[i] != null && !outputArray[i].IsEmpty())
                        used++;
                }
            }
            bool hasQueue = tileEntity != null && tileEntity.hasRecipeInQueue();
            bool isBurning = tileEntity != null && tileEntity.IsBurning;
            bool hasFuelSlot = false;
            if (tileEntity?.fuel != null)
            {
                foreach (var stack in tileEntity.fuel)
                {
                    if (stack != null && !stack.IsEmpty())
                    {
                        hasFuelSlot = true;
                        break;
                    }
                }
            }
            float burnTimeLeft = 0f;
            try { if (tileEntity != null) burnTimeLeft = tileEntity.BurnTimeLeft; } catch { }
            string craftingStatus = string.Empty;
            if (hasQueue)
            {
                if (isBurning && (hasFuelSlot || burnTimeLeft > 0f))
                    craftingStatus = Localization.Get("xuiQueued");
                else if (!isBurning && (hasFuelSlot || burnTimeLeft > 0f))
                    craftingStatus = Localization.Get("xuiNeed2TurnOn");
                else
                    craftingStatus = Localization.Get("xuiNoFuel");
            }
            if (!string.IsNullOrEmpty(craftingStatus))
                slotInfo += craftingStatus + " ";
            slotInfo += (used == 0
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{used}/{total}"));
            // Compatibility mode: do not touch {1}; always append details.
            __result = __result.TrimEnd() + " " + slotInfo;
        }
    }

    // ===================================
    // === PATCH: useWorkstation Prompt ===
    // ===================================
    [HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.GetActivationText))]
    public static class Patch_BlockWorkstation_GetActivationText
    {
        public static void Postfix(ref string __result, WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
        {
            if (PromptStateHelpers.IsJammedPrompt(__result))
                return;

            var tileEntity = _world.GetTileEntity(_blockPos) as TileEntityWorkstation;
            int used = 0;
            int total = 0;
            string slotInfo = string.Empty;
            // Output slots
            var outputArray = tileEntity?.Output;
            if (outputArray != null && outputArray.Length > 0)
            {
                total = outputArray.Length;
                for (int i = 0; i < outputArray.Length; i++)
                {
                    if (outputArray[i] != null && !outputArray[i].IsEmpty())
                        used++;
                }
            }
            bool hasQueue = tileEntity != null && tileEntity.hasRecipeInQueue();
            if (hasQueue)
                slotInfo += Localization.Get("xuiQueued");
            slotInfo += (used == 0
                ? Localization.Get("xuiEmpty")
                : Localization.Get("xuiSlots").Replace("{0}", $"{used}/{total}"));
            // Compatibility mode: do not touch {1}; always append details.
            __result = __result.TrimEnd() + " " + slotInfo;
        }
    }
}
