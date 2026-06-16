using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefIntIndex : XUiC_OptionEntryGamePrefAbs
{
	public Func<int, int> MapGamePrefToListIndex;

	public Func<int, int> MapListIndexToGamePref;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origEntriesLength;

	[XuiXmlAttribute("allow_custom", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AllowCustomValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

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
			return (int)(comboGeneric?.ValueGeneric ?? 0);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			comboGeneric.ValueGeneric = Mathf.Clamp(value, (int)comboGeneric.ValueMinGeneric, (int)comboGeneric.ValueMaxGeneric);
		}
	}

	public override object SelectedValue
	{
		get
		{
			int num = CurrentValue;
			if (MapListIndexToGamePref != null)
			{
				num = MapListIndexToGamePref(num);
			}
			return num;
		}
		set
		{
			if (!(value is int num))
			{
				Log.Error("[XUi] " + GetType().Name + ".SelectedValue: Given value is not a " + expectedPrefType.ToStringCached() + ". Hierarchy: " + GetXuiHierarchy());
			}
			else
			{
				if (MapGamePrefToListIndex != null)
				{
					num = MapGamePrefToListIndex(num);
				}
				CurrentValue = num;
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

	public override void Init()
	{
		base.Init();
		if (AllowCustomValue && comboGeneric is XUiC_ComboBoxList<string> xUiC_ComboBoxList)
		{
			origEntriesLength = (int)(comboGeneric.ValueMaxGeneric + 1);
			comboGeneric.ValueMaxGeneric = origEntriesLength - 1;
			xUiC_ComboBoxList.Elements.Add("Custom");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefDefault()
	{
		int num = (int)GamePrefs.GetDefault(gamePref.Value);
		if (MapGamePrefToListIndex != null)
		{
			num = MapGamePrefToListIndex(num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefValue()
	{
		int num = GamePrefs.GetInt(gamePref.Value);
		if (MapGamePrefToListIndex != null)
		{
			num = MapGamePrefToListIndex(num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(int _value)
	{
		if (MapListIndexToGamePref != null)
		{
			_value = MapListIndexToGamePref(_value);
		}
		GamePrefs.Set(gamePref.Value, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (gamePref.HasValue)
		{
			int prefValue = getPrefValue();
			if (AllowCustomValue && prefValue >= origEntriesLength)
			{
				comboGeneric.ValueMaxGeneric = origEntriesLength;
				prefValue = origEntriesLength;
			}
			int currentValue = (SettingValue = prefValue);
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
		if (!AllowCustomValue || comboGeneric.ValueGeneric < origEntriesLength)
		{
			SettingValue = CurrentValue;
			setPrefValue(SettingValue);
		}
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
