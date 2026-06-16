using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefBool : XUiC_OptionEntryGamePrefAbs
{
	public delegate bool OverrideSettingValueDelegate(bool _settingValue, out bool _overrideValue);

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool combo;

	public OverrideSettingValueDelegate OverrideSettingValue;

	[XuiXmlAttribute("invert", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Invert
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SettingValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool CurrentValue
	{
		get
		{
			return combo?.Value ?? false;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			combo.Value = value;
		}
	}

	public override object SelectedValue
	{
		get
		{
			return CurrentValue ^ Invert;
		}
		set
		{
			if (!(value is bool flag))
			{
				Log.Error("[XUi] " + GetType().Name + ".SelectedValue: Given value is not a " + expectedPrefType.ToStringCached() + ". Hierarchy: " + GetXuiHierarchy());
			}
			else
			{
				CurrentValue = flag ^ Invert;
				invokeValueChanged();
			}
		}
	}

	public override bool IsChanged
	{
		get
		{
			if (CurrentValue != SettingValue)
			{
				if (DoSaveOverride != null)
				{
					return DoSaveOverride();
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsDefault
	{
		get
		{
			if (gamePref.HasValue)
			{
				return CurrentValue == getPrefDefault();
			}
			return true;
		}
	}

	public override GamePrefs.EnumType expectedPrefType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GamePrefs.EnumType.Bool;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefDefault()
	{
		return (bool)GamePrefs.GetDefault(gamePref.Value) ^ Invert;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefValue()
	{
		return GamePrefs.GetBool(gamePref.Value) ^ Invert;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(bool _value)
	{
		GamePrefs.Set(gamePref.Value, _value ^ Invert);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (gamePref.HasValue)
		{
			bool currentValue = (SettingValue = getPrefValue());
			CurrentValue = currentValue;
			XUiC_OptionEntryAbs.DebugLog($"GOT CURRENT BOOL FOR {gamePref}: {SettingValue}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool applyValueOverride(bool _value)
	{
		if (OverrideSettingValue != null && OverrideSettingValue(_value, out var _overrideValue))
		{
			_value = _overrideValue;
		}
		return _value;
	}

	public override void DiscardCurrentChange()
	{
		if (gamePref.HasValue)
		{
			CurrentValue = applyValueOverride(SettingValue);
			if (base.ApplyImmediately)
			{
				setPrefValue(CurrentValue);
			}
			XUiC_OptionEntryAbs.DebugLog($"DISCARDED BOOL FOR {gamePref} TO {SettingValue}");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void applySelectionInternal()
	{
		SettingValue = CurrentValue;
		setPrefValue(SettingValue);
		XUiC_OptionEntryAbs.DebugLog($"SAVED BOOL FOR {gamePref} TO {SettingValue}");
	}

	public override void ResetToDefault()
	{
		if (base.ApplyDefaults && gamePref.HasValue)
		{
			CurrentValue = getPrefDefault() ^ Invert;
			if (base.ApplyImmediately)
			{
				setPrefValue(CurrentValue);
			}
			XUiC_OptionEntryAbs.DebugLog($"RESET BOOL FOR {gamePref} TO {CurrentValue}");
			invokeValueChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void immediatelyApplyCurrentSelection()
	{
		setPrefValue(CurrentValue);
	}
}
