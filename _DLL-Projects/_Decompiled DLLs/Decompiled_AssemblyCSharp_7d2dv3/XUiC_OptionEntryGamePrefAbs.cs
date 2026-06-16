using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_OptionEntryGamePrefAbs : XUiC_OptionEntryComboAbs
{
	public Func<bool> DoSaveOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumGamePrefs? gamePref;

	public EnumGamePrefs? GamePref => gamePref;

	public abstract GamePrefs.EnumType expectedPrefType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public abstract object SelectedValue { get; set; }

	public override void Init()
	{
		base.Init();
		parseGamePref();
	}

	public sealed override void ApplySelection()
	{
		if (gamePref.HasValue)
		{
			if (DoSaveOverride != null && !DoSaveOverride())
			{
				XUiC_OptionEntryAbs.DebugLog($"*NOT* SAVING VALUE FOR {gamePref}, disabled by override");
				return;
			}
			applySelectionInternal();
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void applySelectionInternal();

	[PublicizedFrom(EAccessModifier.Private)]
	public void parseGamePref()
	{
		if (string.IsNullOrEmpty(base.PrefName))
		{
			Log.Error("[XUi] " + GetType().Name + ": No GamePref specified. Hierarchy: " + GetXuiHierarchy());
			return;
		}
		if (!EnumUtils.TryParse<EnumGamePrefs>(base.PrefName, out var _result, _ignoreCase: true))
		{
			Log.Error("[XUi] " + GetType().Name + ": Given GamePref '" + base.PrefName + "' does not exist. Hierarchy: " + GetXuiHierarchy());
			return;
		}
		GamePrefs.EnumType? prefType = GamePrefs.GetPrefType(_result);
		if (!prefType.HasValue)
		{
			Log.Error($"[XUi] {GetType().Name}: Given GamePref '{_result}' does not have a pref type. Hierarchy: {GetXuiHierarchy()}");
			return;
		}
		GamePrefs.EnumType enumType = expectedPrefType;
		if (prefType.Value != enumType)
		{
			Log.Error($"[XUi] {GetType().Name}: Given GamePref '{_result}' is not a {enumType.ToStringCached()} pref. Hierarchy: {GetXuiHierarchy()}");
		}
		else
		{
			gamePref = _result;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionEntryGamePrefAbs()
	{
	}
}
