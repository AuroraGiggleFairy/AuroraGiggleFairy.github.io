using HarmonyLib;
using UnityEngine;

namespace MapPlus
{
	[HarmonyPatch(typeof(EntityPlayer), "onNewPrefabEntered")]
	public static class Patch_EntityPlayer_onNewPrefabEntered
	{
		public static void Postfix(EntityPlayer __instance, PrefabInstance _prefabInstance)
		{
			try
			{
				if (__instance == null || _prefabInstance == null)
				{
					return;
				}

				MapPlusVisits.RecordIfEntered(__instance, _prefabInstance);
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_Location), nameof(XUiC_Location.Update))]
	public static class Patch_XUiC_Location_Update
	{
		public static void Postfix(XUiC_Location __instance)
		{
			try
			{
				EntityPlayerLocal player = __instance?.xui?.playerUI?.entityPlayer;
				PrefabInstance prefab = player?.prefab;
				if (prefab == null)
				{
					return;
				}

				if (prefab.IsWithinInfoArea(player.position))
				{
					MapPlusVisits.RecordIfEntered(prefab);
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_MapArea), nameof(XUiC_MapArea.Update))]
	public static class Patch_XUiC_MapArea_Update
	{
		public static void Postfix(XUiC_MapArea __instance)
		{
			try
			{
				if (__instance?.xui?.playerUI?.CursorController == null)
				{
					return;
				}

				bool mouseOverMap = __instance.bMouseOverMap;
				Vector3 worldPos = Vector3.zero;
				if (mouseOverMap)
				{
					Vector3 screenPos = __instance.xui.playerUI.CursorController.GetScreenPosition();
					worldPos = __instance.screenPosToWorldPos(screenPos);
				}

				MapPlusHover.RefreshFromMap(__instance, worldPos, mouseOverMap);
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(XUiController), nameof(XUiController.OnClose))]
	public static class Patch_XUiController_OnClose
	{
		public static void Postfix(XUiController __instance)
		{
			try
			{
				if (__instance is XUiC_MapArea mapArea)
				{
					MapPlusHover.Hide(mapArea);
				}
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_MapStats), "GetBindingValueInternal")]
	public static class Patch_XUiC_MapStats_GetBindingValueInternal
	{
		public static bool Prefix(ref bool __result, ref string value, string bindingName)
		{
			if (!MapPlusHover.TryHandleBinding(bindingName, ref value))
			{
				return true;
			}

			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiController), "GetBindingValueInternal")]
	public static class Patch_XUiController_GetBindingValueInternal
	{
		public static bool Prefix(XUiController __instance, ref bool __result, ref string _value, string _bindingName)
		{
			if (!(__instance is XUiC_MapArea))
			{
				return true;
			}

			if (!MapPlusHover.TryHandleBinding(_bindingName, ref _value))
			{
				return true;
			}

			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(GameManager), "SaveLocalPlayerData")]
	public static class Patch_GameManager_SaveLocalPlayerData
	{
		public static void Postfix()
		{
			try
			{
				MapPlusVisits.Save();
			}
			catch
			{
			}
		}
	}

	[HarmonyPatch(typeof(GameManager), "SaveWorld")]
	public static class Patch_GameManager_SaveWorld
	{
		public static void Postfix()
		{
			try
			{
				MapPlusVisits.Save();
			}
			catch
			{
			}
		}
	}
}
