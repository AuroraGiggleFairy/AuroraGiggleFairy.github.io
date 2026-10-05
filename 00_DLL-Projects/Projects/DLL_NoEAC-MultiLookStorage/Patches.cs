using System;
using System.Reflection;
using HarmonyLib;

namespace MultiLookStorage
{
	[HarmonyPatch(typeof(TEFeatureAbs), nameof(TEFeatureAbs.IsSharedLock))]
	public static class Patch_SharedLock
	{
		static void Postfix(TEFeatureAbs __instance, ref bool __result)
		{
			if (__instance is TEFeatureStorage storage && StorageAccess.IsPlayerOwned(storage))
			{
				__result = true;
			}
		}
	}

	// 3.2 open tracking. OnLockedServer is gone on 3.3; Prepare keeps this patch off that version.
	[HarmonyPatch(typeof(TEFeatureStorage), "OnLockedServer")]
	public static class Patch_TrackOpen
	{
		static bool Prepare()
		{
			GameVersion.Initialize();
			return !GameVersion.UseV33
				&& typeof(TEFeatureStorage).GetMethod("OnLockedServer") != null;
		}

		static void Postfix(TEFeatureStorage __instance, bool _success, int _lockingPlayerID)
		{
			if (_success && __instance != null && StorageAccess.IsPlayerOwned(__instance))
			{
				MultiLookServer.MarkOpen(__instance.ToWorldPos(), _lockingPlayerID);
			}
		}
	}

	// 3.3 open tracking. A successful lock request is the player opening the container.
	[HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.OnLockRequestServer))]
	public static class Patch_TrackOpenV33
	{
		static bool Prepare()
		{
			GameVersion.Initialize();
			return GameVersion.UseV33;
		}

		static void Postfix(TEFeatureStorage __instance, int _lockingPlayerID, bool __result)
		{
			if (__result && __instance != null && StorageAccess.IsPlayerOwned(__instance))
			{
				MultiLookServer.MarkOpen(__instance.ToWorldPos(), _lockingPlayerID);
			}
		}
	}

	[HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.OnUnlockedServer))]
	public static class Patch_TrackClose
	{
		static void Postfix(TEFeatureStorage __instance, int _unlockingPlayerId)
		{
			if (__instance != null && StorageAccess.IsPlayerOwned(__instance))
			{
				MultiLookServer.MarkClosed(__instance.ToWorldPos(), _unlockingPlayerId);
			}
		}
	}

	[HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.Read))]
	public static class Patch_ReadItems
	{
		static void Prefix(TEFeatureStorage __instance, object[] __args, ref int __state)
		{
			__state = 0;
			if (__instance == null || !StorageAccess.IsPlayerOwned(__instance))
			{
				return;
			}

			string mode = __args != null && __args.Length > 1 ? __args[1]?.ToString() : null;
			if (mode == "FromClient")
			{
				MultiLookSync.PushDiscard();
				__state = 1;
			}
			else if (mode == "FromServer")
			{
				MultiLookSync.PushApply();
				__state = 2;
			}
		}

		static void Postfix(int __state)
		{
			if (__state == 1)
			{
				MultiLookSync.PopDiscard();
			}
			else if (__state == 2)
			{
				MultiLookSync.PopApply();
			}
		}
	}

	[HarmonyPatch(typeof(TileEntity), nameof(TileEntity.IsUserAccessing))]
	public static class Patch_UserAccessingDuringRead
	{
		static bool Prefix(ref bool __result)
		{
			if (MultiLookSync.ForceApply)
			{
				__result = false;
				return false;
			}

			if (MultiLookSync.ForceDiscard)
			{
				__result = true;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(XUiC_LootContainer), nameof(XUiC_LootContainer.HandleLootSlotChangedEvent))]
	public static class Patch_BlockClientContainerWrite
	{
		static bool Prefix(XUiC_LootContainer __instance)
		{
			return !MultiLookRules.IsPlayerStorage(StorageAccess.LootOf(__instance?.xui));
		}
	}

	[HarmonyPatch(typeof(XUiC_ItemStack), nameof(XUiC_ItemStack.HandleStackSwap))]
	public static class Patch_StackSwap
	{
		static bool Prefix(XUiC_ItemStack __instance)
		{
			if (!MultiLookClient.TryPlayerStorage(__instance, out _))
			{
				return true;
			}

			if (__instance.StackLock)
			{
				return false;
			}

			MultiLookClient.SubmitSwap(__instance, MultiLookOp.Swap);
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_ItemStack), nameof(XUiC_ItemStack.HandlePartialStackPickup))]
	public static class Patch_Half
	{
		static bool Prefix(XUiC_ItemStack __instance)
		{
			if (!MultiLookClient.TryPlayerStorage(__instance, out _))
			{
				return true;
			}

			MultiLookClient.SubmitSwap(__instance, MultiLookOp.Half);
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_ItemStack), nameof(XUiC_ItemStack.HandleDropOne))]
	public static class Patch_DropOne
	{
		static bool Prefix(XUiC_ItemStack __instance)
		{
			if (!MultiLookClient.TryPlayerStorage(__instance, out _))
			{
				return true;
			}

			MultiLookClient.SubmitSwap(__instance, MultiLookOp.DropOne);
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_ItemStack), nameof(XUiC_ItemStack.HandleMoveToPreferredLocation))]
	public static class Patch_ShiftTake
	{
		static bool Prefix(XUiC_ItemStack __instance)
		{
			if (!MultiLookClient.TryPlayerStorage(__instance, out _))
			{
				return true;
			}

			MultiLookClient.SubmitTake(__instance);
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiM_LootContainer), nameof(XUiM_LootContainer.StashItems))]
	public static class Patch_Stash
	{
		static bool Prefix(XUiC_ItemStackGrid _srcGrid, IInventory _dstInventory, ref (bool _allMoved, bool _anyMoved) __result)
		{
			if (_srcGrid is XUiC_LootContainer && MultiLookRules.IsPlayerStorage(StorageAccess.LootOf(_srcGrid.xui)))
			{
				MultiLookClient.SubmitTakeAll(_srcGrid.xui);
				__result = (false, false);
				return false;
			}

			if (_dstInventory is TEFeatureStorage storage && StorageAccess.IsPlayerOwned(storage))
			{
				MultiLookClient.SubmitDeposit(_srcGrid, storage);
				__result = (false, false);
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(XUiC_ContainerStandardControls), nameof(XUiC_ContainerStandardControls.Sort))]
	public static class Patch_Sort
	{
		static bool Prefix(XUiC_ContainerStandardControls __instance)
		{
			if (__instance?.WindowGroup == null || __instance.WindowGroup.Id != XUiC_LootWindowGroup.ID)
			{
				return true;
			}

			if (!MultiLookRules.IsPlayerStorage(StorageAccess.LootOf(__instance.xui)))
			{
				return true;
			}

			MultiLookClient.SubmitSort(__instance.xui);
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_LootWindowGroup), nameof(XUiC_LootWindowGroup.OnClose))]
	public static class Patch_ClearBusyOnClose
	{
		static void Postfix()
		{
			MultiLookClient.ClearBusy();
		}
	}
}
