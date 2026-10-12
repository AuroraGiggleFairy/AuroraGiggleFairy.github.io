using HarmonyLib;
using UnityEngine;

namespace RepairToolsPlus
{
    [HarmonyPatch(typeof(ItemActionRepair), nameof(ItemActionRepair.OnHoldingUpdate))]
    public static class Patch_ItemActionRepair_OnHoldingUpdate
    {
        public static void Postfix(ItemActionData _actionData)
        {
            if (_actionData?.invData?.holdingEntity is EntityPlayerLocal player)
            {
                RepairHighlightManager.Instance.OnHoldingUpdate(player, _actionData);
            }
        }
    }

    [HarmonyPatch(typeof(ItemActionRepair), nameof(ItemActionRepair.StopHolding))]
    public static class Patch_ItemActionRepair_StopHolding
    {
        public static void Postfix(ItemActionData _data)
        {
            if (_data?.invData?.holdingEntity is EntityPlayerLocal player)
            {
                RepairHighlightManager.Instance.OnStopHolding(player);
            }
        }
    }

    [HarmonyPatch(typeof(EntityPlayerLocal), nameof(EntityPlayerLocal.OnUpdateLive))]
    public static class Patch_EntityPlayerLocal_OnUpdateLive
    {
        public static void Postfix(EntityPlayerLocal __instance)
        {
            if (__instance != null)
            {
                RepairHighlightManager.Instance.CheckPeriodicTimeout();
            }
        }
    }

    [HarmonyPatch(typeof(ItemActionRepair), nameof(ItemActionRepair.ExecuteAction))]
    public static class Patch_ItemActionRepair_ExecuteAction
    {
        public static void Prefix(ItemActionRepair __instance, ItemActionData _actionData, bool _bReleased, out HitInfoDetails? __state)
        {
            __state = null;
            if (_bReleased) return;

            if (_actionData?.invData?.holdingEntity is EntityPlayerLocal player && player.HitInfo != null && player.HitInfo.bHitValid)
            {
                Vector3i bestTarget = RepairTargetFinder.GetBestTarget(player, __instance, _actionData);
                if (bestTarget != Vector3i.zero && bestTarget != player.HitInfo.hit.blockPos)
                {
                    __state = player.HitInfo.hit.Clone();
                    player.HitInfo.hit.blockPos = bestTarget;
                    player.HitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(player.world, bestTarget);
                }
            }
        }

        public static void Postfix(ItemActionRepair __instance, ItemActionData _actionData, bool _bReleased, HitInfoDetails? __state)
        {
            if (__state.HasValue && _actionData?.invData?.holdingEntity is EntityPlayerLocal player && player.HitInfo != null)
            {
                player.HitInfo.hit.CopyFrom(__state.Value);
            }
        }
    }

    // Radial Menu Hooks: Hold R on repair tools
    [HarmonyPatch(typeof(ItemActionAttack), nameof(ItemActionAttack.HasRadial))]
    public static class Patch_ItemActionAttack_HasRadial
    {
        public static void Postfix(ItemActionAttack __instance, ref bool __result)
        {
            EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null && RepairToolsRadialManager.IsHoldingRepairTool(player))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(ItemActionAttack), nameof(ItemActionAttack.SetupRadial))]
    public static class Patch_ItemActionAttack_SetupRadial
    {
        public static bool Prefix(ItemActionAttack __instance, XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
        {
            EntityPlayerLocal player = _epl ?? GameManager.Instance?.World?.GetPrimaryPlayer();
            if (player != null && RepairToolsRadialManager.IsHoldingRepairTool(player))
            {
                RepairToolsRadialManager.SetupRadial(_xuiRadialWindow, player);
                return false;
            }
            return true;
        }
    }

    // Light button "F" Hook: Toggle Damage Vision and target cycling while holding a repair tool
    [HarmonyPatch(typeof(XUiC_Radial), nameof(XUiC_Radial.SetActivatableItemData))]
    public static class Patch_XUiC_Radial_SetActivatableItemData
    {
        public static bool Prefix(XUiC_Radial __instance, EntityPlayerLocal _epl)
        {
            if (_epl != null && RepairToolsRadialManager.IsHoldingRepairTool(_epl))
            {
                RepairToolsSettings.IsDamageVisionActive = !RepairToolsSettings.IsDamageVisionActive;
                _epl.PlayOneShot("ui_tab");
                GameManager.ShowTooltip(_epl, RepairToolsSettings.IsDamageVisionActive ? "Damage Vision: ON" : "Damage Vision: OFF");
                __instance.Close();
                return false;
            }
            return true;
        }
    }

    // Cap Enforcement: Prevent upgrading past selected cap
    [HarmonyPatch(typeof(ItemActionRepair), "CanRemoveRequiredResource")]
    public static class Patch_ItemActionRepair_CanRemoveRequiredResource
    {
        public static bool Prefix(ItemInventoryData data, BlockValue blockValue, ref bool __result)
        {
            if (blockValue.Block != null && !RepairToolsSettings.CanUpgradeToCap(blockValue.Block))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemActionRepair), "isShowOverlay")]
    public static class Patch_ItemActionRepair_isShowOverlay
    {
        public static void Postfix(ItemActionRepair __instance, ItemActionData _actionData, ref bool __result)
        {
            if (!__result && _actionData?.invData?.holdingEntity is EntityPlayerLocal player)
            {
                if (RepairTargetFinder.CurrentTarget != Vector3i.zero)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemActionRepair), "getOverlayData")]
    public static class Patch_ItemActionRepair_getOverlayData
    {
        public static void Postfix(ItemActionRepair __instance, ItemActionData _actionData, ref float _perc, ref string _text)
        {
            if (_actionData?.invData?.holdingEntity is EntityPlayerLocal player)
            {
                Vector3i target = RepairTargetFinder.CurrentTarget;
                if (target != Vector3i.zero && player.world != null)
                {
                    BlockValue targetBv = player.world.GetBlock(target);
                    if (!targetBv.isair && targetBv.damage > 0)
                    {
                        int maxDamage = targetBv.Block.GetShownMaxDamage();
                        if (maxDamage > 0)
                        {
                            _perc = ((float)maxDamage - (float)targetBv.damage) / (float)maxDamage;
                            _text = $"{Utils.FastMax(0, maxDamage - targetBv.damage)}/{maxDamage}";
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemActionRepair), nameof(ItemActionRepair.GetCrosshairType))]
    public static class Patch_ItemActionRepair_GetCrosshairType
    {
        public static void Postfix(ItemActionRepair __instance, ItemActionData _actionData, ref ItemClass.EnumCrosshairType __result)
        {
            if (__result == ItemClass.EnumCrosshairType.Plus && RepairTargetFinder.CurrentTarget != Vector3i.zero)
            {
                __result = ItemClass.EnumCrosshairType.Repair;
            }
        }
    }
}
