using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefFloat : XUiC_OptionEntryGamePrefAbs
{
	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxFloat combo;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float SettingValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float CurrentValue
	{
		get
		{
			return (float)(combo?.Value ?? 0.0);
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
			if (!(value is float currentValue))
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

	public override bool IsChanged => !Mathf.Approximately(CurrentValue, SettingValue);

	public override bool IsDefault
	{
		get
		{
			if (gamePref.HasValue)
			{
				return Mathf.Approximately(CurrentValue, getPrefDefault());
			}
			return true;
		}
	}

	public override GamePrefs.EnumType expectedPrefType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GamePrefs.EnumType.Float;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getPrefDefault()
	{
		return (float)GamePrefs.GetDefault(gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getPrefValue()
	{
		return GamePrefs.GetFloat(gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(float _value)
	{
		GamePrefs.Set(gamePref.Value, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (gamePref.HasValue)
		{
			float currentValue = (SettingValue = getPrefValue());
			CurrentValue = currentValue;
			XUiC_OptionEntryAbs.DebugLog($"GOT CURRENT FLOAT FOR {gamePref}: {SettingValue}");
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
			XUiC_OptionEntryAbs.DebugLog($"DISCARDED FLOAT FOR {gamePref} TO {SettingValue}");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void applySelectionInternal()
	{
		SettingValue = CurrentValue;
		setPrefValue(SettingValue);
		XUiC_OptionEntryAbs.DebugLog($"SAVED FLOAT FOR {gamePref} TO {SettingValue}");
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
			XUiC_OptionEntryAbs.DebugLog($"RESET FLOAT FOR {gamePref} TO {CurrentValue}");
			invokeValueChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void immediatelyApplyCurrentSelection()
	{
		setPrefValue(CurrentValue);
	}
}
