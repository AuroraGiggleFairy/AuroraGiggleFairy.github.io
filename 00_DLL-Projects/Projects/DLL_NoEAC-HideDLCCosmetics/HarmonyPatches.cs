using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Equipment), "GetCosmeticSlot")]
public static class Patch_Equipment_GetCosmeticSlot
{
	public static bool Prefix(Equipment __instance, int index, bool useTemporary, ref ItemClass __result, EntityAlive ___m_entity)
	{
		if (!(___m_entity is EntityPlayer) || ___m_entity is EntityPlayerLocal)
		{
			return true;
		}

		ItemClass item;
		if (useTemporary && __instance.tempCosmeticSlotIndex == index)
		{
			item = __instance.tempCosmeticSlot;
		}
		else
		{
			ItemClass[] slots = __instance.CosmeticSlots;
			if (slots == null || index < 0 || index >= slots.Length)
			{
				return true;
			}

			item = slots[index];
		}

		if (!HideDlcCatalog.IsHiddenItem(item))
		{
			return true;
		}

		__result = null;
		return false;
	}
}

[HarmonyPatch(typeof(EntityPlayerLocal), "AfterPlayerRespawn")]
public static class Patch_EntityPlayerLocal_AfterPlayerRespawn_HideDlc
{
	public static void Postfix()
	{
		HideDlcRefresh.ApplyLocal();
	}
}

[HarmonyPatch(typeof(XUiC_CharacterCosmeticList), "SetCosmeticList")]
public static class Patch_CharacterCosmeticList_SetCosmeticList
{
	public static void Prefix(XUiC_CharacterCosmeticList __instance, ref List<ItemClass> _currentItems)
	{
		if (_currentItems == null || _currentItems.Count == 0)
		{
			return;
		}

		Equipment equipment = __instance.xui != null && __instance.xui.PlayerEquipment != null
			? __instance.xui.PlayerEquipment.Equipment
			: null;

		for (int i = _currentItems.Count - 1; i >= 0; i--)
		{
			if (HideDlcCatalog.ShouldHideFromWardrobeList(equipment, _currentItems[i]))
			{
				_currentItems.RemoveAt(i);
			}
		}
	}
}

[HarmonyPatch(typeof(XUiC_OptionsGeneral), "Init")]
internal static class Patch_XUiC_OptionsGeneral_Init_HideDlc
{
	internal static string[] OptionNames
	{
		get
		{
			if (optionNames == null)
			{
				optionNames = new string[HideDlcCatalog.Packs.Length + 2];
				optionNames[0] = "HideDlcCosmeticList";
				optionNames[1] = "HideDlcPreset";
				for (int i = 0; i < HideDlcCatalog.Packs.Length; i++)
				{
					optionNames[i + 2] = HideDlcCatalog.Packs[i].ComboName;
				}
			}

			return optionNames;
		}
	}

	private static string[] optionNames;

	private static void Postfix(XUiC_OptionsGeneral __instance)
	{
		BindAllCustomOptions(__instance);
	}

	internal static void BindAllCustomOptions(XUiC_OptionsGeneral instance)
	{
		if (instance == null)
		{
			return;
		}

		Dictionary<string, XUiC_OptionEntryCustom> optionMap = BuildCustomOptionMap(instance);
		for (int i = 0; i < OptionNames.Length; i++)
		{
			BindCustomOption(instance, OptionNames[i], optionMap);
		}
	}

	private static Dictionary<string, XUiC_OptionEntryCustom> BuildCustomOptionMap(XUiC_OptionsGeneral instance)
	{
		Dictionary<string, XUiC_OptionEntryCustom> map = new Dictionary<string, XUiC_OptionEntryCustom>(StringComparer.OrdinalIgnoreCase);

		void AddOption(XUiC_OptionEntryCustom option)
		{
			string id = option?.ViewComponent?.ID;
			if (!string.IsNullOrEmpty(id) && !map.ContainsKey(id))
			{
				map[id] = option;
			}
		}

		XUiC_OptionEntryCustom[] localOptions = instance.GetChildrenByType<XUiC_OptionEntryCustom>() ?? Array.Empty<XUiC_OptionEntryCustom>();
		for (int i = 0; i < localOptions.Length; i++)
		{
			AddOption(localOptions[i]);
		}

		XUiC_OptionEntryCustom[] rootOptions = instance.windowGroup?.Controller?.GetChildrenByType<XUiC_OptionEntryCustom>() ?? Array.Empty<XUiC_OptionEntryCustom>();
		for (int i = 0; i < rootOptions.Length; i++)
		{
			AddOption(rootOptions[i]);
		}

		return map;
	}

	private static void BindCustomOption(XUiC_OptionsGeneral instance, string name, Dictionary<string, XUiC_OptionEntryCustom> optionMap)
	{
		string optionId = "Option" + name;
		XUiC_OptionEntryCustom option = null;
		if (optionMap != null)
		{
			optionMap.TryGetValue(optionId, out option);
			if (option == null)
			{
				optionMap.TryGetValue(name, out option);
			}
		}

		if (option == null)
		{
			XUiController controller =
				instance.GetChildById(optionId)
				?? instance.GetChildById(name)
				?? instance.windowGroup?.Controller?.GetChildById(optionId)
				?? instance.windowGroup?.Controller?.GetChildById(name);
			option = (controller as XUiC_OptionEntryCustom) ?? controller?.GetChildByType<XUiC_OptionEntryCustom>();
		}

		if (option == null)
		{
			return;
		}

		option.GetSettingValue = () => XUiC_HideDlcCosmeticsOptions.ReloadAllOpenControllers();
		option.DiscardChanges = () => XUiC_HideDlcCosmeticsOptions.ReloadAllOpenControllers();
		option.ApplyChanges = _ => XUiC_HideDlcCosmeticsOptions.ApplyUsingOptionsGeneral(instance);
		option.ResetDefaults = () => XUiC_HideDlcCosmeticsOptions.ResetUsingOptionsGeneral(instance);
		option.IsChangedDelegate = () => XUiC_HideDlcCosmeticsOptions.HasPendingChangeForOption(name);
		option.IsDefaultDelegate = () => XUiC_HideDlcCosmeticsOptions.AreAllOpenControllersAtDefaults();
	}

	internal static bool IsHideDlcOption(XUiC_OptionEntryCustom option)
	{
		string id = option?.ViewComponent?.ID ?? string.Empty;
		if (id.StartsWith("Option", StringComparison.OrdinalIgnoreCase))
		{
			id = id.Substring("Option".Length);
		}

		for (int i = 0; i < OptionNames.Length; i++)
		{
			if (string.Equals(OptionNames[i], id, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}
}

[HarmonyPatch(typeof(XUiC_OptionsGeneral), "OnOpen")]
internal static class Patch_XUiC_OptionsGeneral_OnOpen_HideDlc
{
	private static void Postfix(XUiC_OptionsGeneral __instance)
	{
		Patch_XUiC_OptionsGeneral_Init_HideDlc.BindAllCustomOptions(__instance);
	}
}

[HarmonyPatch(typeof(XUiC_OptionsDialogBase), "saveChanges")]
internal static class Patch_OptionsDialogBase_SaveChanges_HideDlc
{
	private static void Postfix(XUiC_OptionsDialogBase __instance)
	{
		XUiC_OptionsGeneral optionsGeneral = __instance as XUiC_OptionsGeneral;
		if (optionsGeneral != null)
		{
			XUiC_HideDlcCosmeticsOptions.ApplyUsingOptionsGeneral(optionsGeneral);
		}
	}
}

[HarmonyPatch(typeof(XUiC_OptionsDialogBase), "discardChanges")]
internal static class Patch_OptionsDialogBase_DiscardChanges_HideDlc
{
	private static void Postfix(XUiC_OptionsDialogBase __instance)
	{
		if (__instance is XUiC_OptionsGeneral)
		{
			XUiC_HideDlcCosmeticsOptions.ReloadAllOpenControllers();
		}
	}
}

[HarmonyPatch(typeof(XUiC_OptionsDialogBase), "resetToDefaults")]
internal static class Patch_OptionsDialogBase_ResetToDefaults_HideDlc
{
	private static void Postfix(XUiC_OptionsDialogBase __instance)
	{
		if (!(__instance is XUiC_OptionsGeneral))
		{
			return;
		}

		XUiC_HideDlcCosmeticsOptions.ResetAllOpenControllersToDefaults();
		__instance.SetChanged();
	}
}

[HarmonyPatch(typeof(XUiC_OptionsDialogBase), "Update")]
internal static class Patch_OptionsDialogBase_Update_HideDlc
{
	private static int lastPendingCheckFrame = -1;

	private static void Postfix(XUiC_OptionsDialogBase __instance)
	{
		if (!(__instance is XUiC_OptionsGeneral) || __instance.UnsavedChanges)
		{
			return;
		}

		int frame = Time.frameCount;
		if (lastPendingCheckFrame == frame)
		{
			return;
		}

		lastPendingCheckFrame = frame;
		if (XUiC_HideDlcCosmeticsOptions.HasAnyOpenControllerPendingChanges())
		{
			__instance.SetChanged();
		}
	}
}

[HarmonyPatch(typeof(XUiC_OptionEntryCustom), "SelectionChanged")]
internal static class Patch_OptionEntryCustom_SelectionChanged_HideDlc
{
	private static void Postfix(XUiC_OptionEntryCustom __instance)
	{
		if (__instance == null || !(__instance.parentOptionsDialog is XUiC_OptionsGeneral) || !Patch_XUiC_OptionsGeneral_Init_HideDlc.IsHideDlcOption(__instance))
		{
			return;
		}

		__instance.parentOptionsDialog.SetChanged();
	}
}
