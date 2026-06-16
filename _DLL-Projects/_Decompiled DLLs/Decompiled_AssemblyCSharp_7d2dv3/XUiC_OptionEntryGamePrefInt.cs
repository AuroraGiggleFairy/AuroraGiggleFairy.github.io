using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefInt : XUiC_OptionEntryGamePrefAbs
{
	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt combo;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SettingValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int CurrentValue
	{
		get
		{
			return (int)(combo?.Value ?? 0);
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
			return CurrentValue;
		}
		set
		{
			if (!(value is int currentValue))
			{
				Log.Error("[XUi] " + GetType().Name + ".SelectedValue: Given value is not a " + expectedPrefType.ToStringCached() + ". Hierarchy: " + GetXuiHierarchy());
			}
			else
			{
				CurrentValue = currentValue;
				invokeValueChanged();
			}
		}
	}

	public override bool IsChanged => CurrentValue != SettingValue;

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
			return GamePrefs.EnumType.Int;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefDefault()
	{
		return (int)GamePrefs.GetDefault(gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefValue()
	{
		return GamePrefs.GetInt(gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(int _value)
	{
		GamePrefs.Set(gamePref.Value, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (gamePref.HasValue)
		{
			int currentValue = (SettingValue = getPrefValue());
			CurrentValue = currentValue;
			XUiC_OptionEntryAbs.DebugLog($"GOT CURRENT INT FOR {gamePref}: {SettingValue}");
		}
	}

	public override void DiscardCurrentChange()
	{
		if (gamePref.HasValue)
		{
			CurrentValue = SettingValue;
			if (base.ApplyImmediately)
			{
				setPrefValue(CurrentValue);
			}
			XUiC_OptionEntryAbs.DebugLog($"DISCARDED INT FOR {gamePref} TO {SettingValue}");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void applySelectionInternal()
	{
		SettingValue = CurrentValue;
		setPrefValue(SettingValue);
		XUiC_OptionEntryAbs.DebugLog($"SAVED INT FOR {gamePref} TO {SettingValue}");
	}

	public override void ResetToDefault()
	{
		if (base.ApplyDefaults && gamePref.HasValue)
		{
			CurrentValue = getPrefDefault();
			if (base.ApplyImmediately)
			{
				setPrefValue(CurrentValue);
			}
			XUiC_OptionEntryAbs.DebugLog($"RESET INT FOR {gamePref} TO {CurrentValue}");
			invokeValueChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void immediatelyApplyCurrentSelection()
	{
		setPrefValue(CurrentValue);
	}
}
