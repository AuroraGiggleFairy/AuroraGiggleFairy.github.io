using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_HideDlcCosmeticsOptions : XUiController
{
	private static readonly List<XUiC_HideDlcCosmeticsOptions> LiveControllers = new List<XUiC_HideDlcCosmeticsOptions>();
	private static readonly MethodInfo InvokeValueChangedMethod = typeof(XUiController).GetMethod("invokeValueChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	private static readonly MethodInfo SetChangedMethod = typeof(XUiC_OptionsDialogBase).GetMethod("SetChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	private static bool applying;
	private static bool resetting;

	private const string PresetId = "HideDlcPreset";
	private const string CosmeticListId = "HideDlcCosmeticList";
	private const string PresetShowAll = "ShowAll";
	private const string PresetHideAll = "HideAll";
	private const string PresetCustom = "Custom";
	private const string ValueShow = "Show";
	private const string ValueHide = "Hide";

	private bool suppressEvents;
	private bool isReady;
	private XUiC_ComboBoxList<string> presetCombo;
	private XUiC_ComboBoxList<string> cosmeticListCombo;
	private readonly XUiC_ComboBoxList<string>[] packCombos = new XUiC_ComboBoxList<string>[HideDlcCatalog.Packs.Length];

	public override void Init()
	{
		base.Init();
		RegisterInstance(this);
		ResolveControls();
		foreach (XUiC_ComboBoxList<string> combo in GetCombos())
		{
			if (combo != null)
			{
				combo.OnValueChanged += OnAnyValueChanged;
				combo.OnValueChangedGeneric += OnAnyValueChangedGeneric;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RegisterInstance(this);
		isReady = false;
		EnsureControlsResolved();
		RefreshFromSettings();
		isReady = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		UnregisterInstance(this);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		UnregisterInstance(this);
	}

	private static void RegisterInstance(XUiC_HideDlcCosmeticsOptions controller)
	{
		if (controller != null && !LiveControllers.Contains(controller))
		{
			LiveControllers.Add(controller);
		}
	}

	private static void UnregisterInstance(XUiC_HideDlcCosmeticsOptions controller)
	{
		if (controller != null)
		{
			LiveControllers.Remove(controller);
		}
	}

	public static void ReloadAllOpenControllers()
	{
		for (int i = LiveControllers.Count - 1; i >= 0; i--)
		{
			XUiC_HideDlcCosmeticsOptions controller = LiveControllers[i];
			if (controller == null)
			{
				LiveControllers.RemoveAt(i);
				continue;
			}

			controller.ReloadFromSettingsForUi();
		}
	}

	public static bool ApplyAllOpenControllers()
	{
		if (applying)
		{
			return true;
		}

		applying = true;
		bool applied = false;
		try
		{
			for (int i = LiveControllers.Count - 1; i >= 0; i--)
			{
				XUiC_HideDlcCosmeticsOptions controller = LiveControllers[i];
				if (controller == null)
				{
					LiveControllers.RemoveAt(i);
					continue;
				}

				controller.ApplyUiToSettings();
				controller.ReloadFromSettingsForUi();
				applied = true;
			}

			if (applied)
			{
				HideDlcRefresh.Apply();
			}

			return applied;
		}
		finally
		{
			applying = false;
		}
	}

	public static bool ApplyUsingOptionsGeneral(XUiC_OptionsGeneral optionsGeneral)
	{
		if (applying)
		{
			return true;
		}

		XUiC_HideDlcCosmeticsOptions controller = ResolveForOptionsGeneral(optionsGeneral);
		if (controller == null)
		{
			return ApplyAllOpenControllers();
		}

		applying = true;
		try
		{
			controller.ApplyUiToSettings();
			controller.ReloadFromSettingsForUi();
			HideDlcRefresh.Apply();
			return true;
		}
		finally
		{
			applying = false;
		}
	}

	public static void ResetAllOpenControllersToDefaults()
	{
		if (resetting)
		{
			return;
		}

		resetting = true;
		try
		{
			for (int i = LiveControllers.Count - 1; i >= 0; i--)
			{
				XUiC_HideDlcCosmeticsOptions controller = LiveControllers[i];
				if (controller == null)
				{
					LiveControllers.RemoveAt(i);
					continue;
				}

				controller.ResetUiToDefaults();
				controller.RefreshBindingsSelfAndChildren();
			}
		}
		finally
		{
			resetting = false;
		}
	}

	public static bool ResetUsingOptionsGeneral(XUiC_OptionsGeneral optionsGeneral)
	{
		if (resetting)
		{
			return true;
		}

		XUiC_HideDlcCosmeticsOptions controller = ResolveForOptionsGeneral(optionsGeneral);
		if (controller == null)
		{
			ResetAllOpenControllersToDefaults();
			return LiveControllers.Count > 0;
		}

		resetting = true;
		try
		{
			controller.ResetUiToDefaults();
			controller.RefreshBindingsSelfAndChildren();
			return true;
		}
		finally
		{
			resetting = false;
		}
	}

	public static bool HasAnyOpenControllerPendingChanges()
	{
		for (int i = LiveControllers.Count - 1; i >= 0; i--)
		{
			XUiC_HideDlcCosmeticsOptions controller = LiveControllers[i];
			if (controller == null)
			{
				LiveControllers.RemoveAt(i);
				continue;
			}

			if (controller.HasPendingChangesAgainstSettings())
			{
				return true;
			}
		}

		return false;
	}

	public static bool HasPendingChangeForOption(string name)
	{
		XUiC_HideDlcCosmeticsOptions controller = FirstLiveController();
		if (controller == null)
		{
			return false;
		}

		string optionName = NormalizeOptionName(name);
		if (string.Equals(optionName, PresetId, StringComparison.OrdinalIgnoreCase))
		{
			return !string.Equals(NormalizePreset(controller.presetCombo != null ? controller.presetCombo.Value : null), HideDlcSettings.GetPreset(), StringComparison.OrdinalIgnoreCase);
		}

		if (string.Equals(optionName, CosmeticListId, StringComparison.OrdinalIgnoreCase))
		{
			return !string.Equals(HideDlcSettings.NormalizeCosmeticList(controller.cosmeticListCombo != null ? controller.cosmeticListCombo.Value : null), HideDlcSettings.CosmeticListFilter, StringComparison.OrdinalIgnoreCase);
		}

		for (int i = 0; i < HideDlcCatalog.Packs.Length; i++)
		{
			if (string.Equals(HideDlcCatalog.Packs[i].ComboName, optionName, StringComparison.OrdinalIgnoreCase))
			{
				return IsHidden(controller.packCombos[i]) != HideDlcSettings.IsHidden(HideDlcCatalog.Packs[i].Key);
			}
		}

		return controller.HasPendingChangesAgainstSettings();
	}

	public static bool AreAllOpenControllersAtDefaults()
	{
		XUiC_HideDlcCosmeticsOptions controller = FirstLiveController();
		return controller == null || controller.IsAtDefaultValues();
	}

	private static XUiC_HideDlcCosmeticsOptions FirstLiveController()
	{
		for (int i = LiveControllers.Count - 1; i >= 0; i--)
		{
			if (LiveControllers[i] != null)
			{
				return LiveControllers[i];
			}

			LiveControllers.RemoveAt(i);
		}

		return null;
	}

	private static XUiC_HideDlcCosmeticsOptions ResolveForOptionsGeneral(XUiC_OptionsGeneral optionsGeneral)
	{
		if (optionsGeneral == null)
		{
			return FirstLiveController();
		}

		XUiC_HideDlcCosmeticsOptions fromDialog = optionsGeneral.GetChildByType<XUiC_HideDlcCosmeticsOptions>();
		if (fromDialog != null)
		{
			return fromDialog;
		}

		XUiC_HideDlcCosmeticsOptions fromWindow = optionsGeneral.windowGroup?.Controller?.GetChildByType<XUiC_HideDlcCosmeticsOptions>();
		return fromWindow ?? FirstLiveController();
	}

	private static string NormalizeOptionName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}

		if (name.StartsWith("Option", StringComparison.OrdinalIgnoreCase))
		{
			return name.Substring("Option".Length);
		}

		return name;
	}

	public void ReloadFromSettingsForUi()
	{
		isReady = false;
		EnsureControlsResolved();
		RefreshFromSettings();
		isReady = true;
		RefreshBindingsSelfAndChildren();
	}

	private void OnAnyValueChanged(XUiController sender, string oldValue, string newValue)
	{
		HandleValueChanged(sender);
	}

	private void OnAnyValueChangedGeneric(XUiController sender)
	{
		HandleValueChanged(sender);
	}

	private void HandleValueChanged(XUiController sender)
	{
		if (suppressEvents || !isReady)
		{
			return;
		}

		EnsureControlsResolved();
		if (ReferenceEquals(sender, presetCombo))
		{
			ApplyPresetSelection();
		}
		else if (IsPackCombo(sender))
		{
			SyncPresetFromPacks();
		}
		else if (!ReferenceEquals(sender, cosmeticListCombo))
		{
			return;
		}

		MarkOptionsEntryDirty(sender);
	}

	private void ApplyPresetSelection()
	{
		string preset = NormalizePreset(presetCombo != null ? presetCombo.Value : null);
		if (string.Equals(preset, PresetCustom, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		bool hideAll = string.Equals(preset, PresetHideAll, StringComparison.OrdinalIgnoreCase);
		suppressEvents = true;
		try
		{
			for (int i = 0; i < packCombos.Length; i++)
			{
				Set(packCombos[i], hideAll ? ValueHide : ValueShow);
			}
		}
		finally
		{
			suppressEvents = false;
		}
	}

	private void SyncPresetFromPacks()
	{
		string resolved = ResolvePresetFromPacks();
		if (string.Equals(presetCombo != null ? presetCombo.Value : string.Empty, resolved, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		suppressEvents = true;
		try
		{
			Set(presetCombo, resolved);
		}
		finally
		{
			suppressEvents = false;
		}
	}

	private string ResolvePresetFromPacks()
	{
		int hiddenCount = 0;
		for (int i = 0; i < packCombos.Length; i++)
		{
			if (IsHidden(packCombos[i]))
			{
				hiddenCount++;
			}
		}

		if (hiddenCount <= 0)
		{
			return PresetShowAll;
		}

		if (hiddenCount >= HideDlcCatalog.Packs.Length)
		{
			return PresetHideAll;
		}

		return PresetCustom;
	}

	private void ApplyUiToSettings()
	{
		EnsureControlsResolved();
		List<string> hiddenKeys = new List<string>();
		for (int i = 0; i < HideDlcCatalog.Packs.Length; i++)
		{
			if (IsHidden(packCombos[i]))
			{
				hiddenKeys.Add(HideDlcCatalog.Packs[i].Key);
			}
		}

		HideDlcSettings.SetHiddenKeys(hiddenKeys);
		HideDlcSettings.CosmeticListFilter = HideDlcSettings.NormalizeCosmeticList(cosmeticListCombo != null ? cosmeticListCombo.Value : null);
	}

	private void ResetUiToDefaults()
	{
		suppressEvents = true;
		try
		{
			for (int i = 0; i < packCombos.Length; i++)
			{
				Set(packCombos[i], ValueShow);
			}

			Set(presetCombo, PresetShowAll);
			Set(cosmeticListCombo, HideDlcSettings.CosmeticListShow);
		}
		finally
		{
			suppressEvents = false;
		}
	}

	private bool HasPendingChangesAgainstSettings()
	{
		EnsureControlsResolved();
		if (!string.Equals(HideDlcSettings.NormalizeCosmeticList(cosmeticListCombo != null ? cosmeticListCombo.Value : null), HideDlcSettings.CosmeticListFilter, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (!string.Equals(NormalizePreset(presetCombo != null ? presetCombo.Value : null), HideDlcSettings.GetPreset(), StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		for (int i = 0; i < HideDlcCatalog.Packs.Length; i++)
		{
			if (IsHidden(packCombos[i]) != HideDlcSettings.IsHidden(HideDlcCatalog.Packs[i].Key))
			{
				return true;
			}
		}

		return false;
	}

	private bool IsAtDefaultValues()
	{
		EnsureControlsResolved();
		if (!string.Equals(HideDlcSettings.NormalizeCosmeticList(cosmeticListCombo != null ? cosmeticListCombo.Value : null), HideDlcSettings.CosmeticListShow, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (!string.Equals(NormalizePreset(presetCombo != null ? presetCombo.Value : null), PresetShowAll, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		for (int i = 0; i < packCombos.Length; i++)
		{
			if (IsHidden(packCombos[i]))
			{
				return false;
			}
		}

		return true;
	}

	private void RefreshFromSettings()
	{
		EnsureControlsResolved();
		suppressEvents = true;
		try
		{
			for (int i = 0; i < packCombos.Length; i++)
			{
				Set(packCombos[i], HideDlcSettings.IsHidden(HideDlcCatalog.Packs[i].Key) ? ValueHide : ValueShow);
			}

			Set(presetCombo, HideDlcSettings.GetPreset());
			Set(cosmeticListCombo, HideDlcSettings.CosmeticListFilter);
		}
		finally
		{
			suppressEvents = false;
		}
	}

	private static void MarkOptionsEntryDirty(XUiController source)
	{
		if (source == null)
		{
			return;
		}

		if (InvokeValueChangedMethod != null)
		{
			try
			{
				InvokeValueChangedMethod.Invoke(source, null);
				return;
			}
			catch
			{
			}
		}

		XUiC_OptionsDialogBase optionsDialog = source.windowGroup?.Controller as XUiC_OptionsDialogBase;
		if (optionsDialog != null && SetChangedMethod != null)
		{
			try
			{
				SetChangedMethod.Invoke(optionsDialog, null);
			}
			catch
			{
			}
		}
	}

	private void EnsureControlsResolved()
	{
		if (presetCombo == null || cosmeticListCombo == null)
		{
			ResolveControls();
		}
	}

	private void ResolveControls()
	{
		presetCombo = ResolveStringListCombo(PresetId);
		cosmeticListCombo = ResolveStringListCombo(CosmeticListId);
		for (int i = 0; i < HideDlcCatalog.Packs.Length; i++)
		{
			packCombos[i] = ResolveStringListCombo(HideDlcCatalog.Packs[i].ComboName);
		}
	}

	private XUiC_ComboBoxList<string>[] GetCombos()
	{
		XUiC_ComboBoxList<string>[] combos = new XUiC_ComboBoxList<string>[packCombos.Length + 2];
		combos[0] = cosmeticListCombo;
		combos[1] = presetCombo;
		for (int i = 0; i < packCombos.Length; i++)
		{
			combos[i + 2] = packCombos[i];
		}

		return combos;
	}

	private bool IsPackCombo(XUiController sender)
	{
		for (int i = 0; i < packCombos.Length; i++)
		{
			if (ReferenceEquals(sender, packCombos[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsHidden(XUiC_ComboBoxList<string> combo)
	{
		return string.Equals(combo != null ? combo.Value : null, ValueHide, StringComparison.OrdinalIgnoreCase);
	}

	private static string NormalizePreset(string raw)
	{
		if (string.Equals(raw, PresetShowAll, StringComparison.OrdinalIgnoreCase))
		{
			return PresetShowAll;
		}

		if (string.Equals(raw, PresetHideAll, StringComparison.OrdinalIgnoreCase))
		{
			return PresetHideAll;
		}

		return PresetCustom;
	}

	private static void Set(XUiC_ComboBoxList<string> combo, string value)
	{
		if (combo != null)
		{
			combo.Value = value;
		}
	}

	private XUiC_ComboBoxList<string> ResolveStringListCombo(string id)
	{
		XUiController byId = GetChildById(id);
		XUiC_ComboBoxList<string> resolved = (byId as XUiC_ComboBoxList<string>) ?? byId?.GetChildByType<XUiC_ComboBoxList<string>>();
		if (resolved != null)
		{
			return resolved;
		}

		XUiC_ComboBoxList<string>[] all = GetChildrenByType<XUiC_ComboBoxList<string>>();
		for (int i = 0; i < all.Length; i++)
		{
			string candidateId = all[i]?.ViewComponent?.ID;
			if (string.Equals(candidateId, id, StringComparison.OrdinalIgnoreCase))
			{
				return all[i];
			}
		}

		return null;
	}
}
