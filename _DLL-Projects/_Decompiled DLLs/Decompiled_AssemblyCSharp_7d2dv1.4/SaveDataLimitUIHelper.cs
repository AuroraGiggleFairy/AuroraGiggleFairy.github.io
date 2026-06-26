using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class SaveDataLimitUIHelper
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ConditionalWeakTable<XUiC_ComboBoxEnum<SaveDataLimitType>, object> s_saveDataLimitComboBoxes = new ConditionalWeakTable<XUiC_ComboBoxEnum<SaveDataLimitType>, object>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataLimitType s_defaultValue = (PlatformOptimizations.LimitedSaveData ? SaveDataLimitType.VeryLong : SaveDataLimitType.Unlimited);

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataLimitType s_currentValue = Load();

	public static Action OnValueChanged;

	public static SaveDataLimitType CurrentValue => s_currentValue;

	public static XUiC_ComboBoxEnum<SaveDataLimitType> AddComboBox(XUiC_ComboBoxEnum<SaveDataLimitType> saveDataLimitComboBox)
	{
		AddComboBoxInternal(saveDataLimitComboBox);
		return saveDataLimitComboBox;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataLimitType Load()
	{
		if (!EnumUtils.TryParse<SaveDataLimitType>(GamePrefs.GetString(EnumGamePrefs.SaveDataLimitType), out var _result, _ignoreCase: true) || !_result.IsSupported())
		{
			return s_defaultValue;
		}
		return _result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Save()
	{
		GamePrefs.Set(EnumGamePrefs.SaveDataLimitType, s_currentValue.ToStringCached());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddComboBoxInternal(XUiC_ComboBoxEnum<SaveDataLimitType> saveDataLimitComboBox)
	{
		if (saveDataLimitComboBox != null && !s_saveDataLimitComboBoxes.TryGetValue(saveDataLimitComboBox, out var _))
		{
			s_saveDataLimitComboBoxes.Add(saveDataLimitComboBox, null);
			if (PlatformOptimizations.LimitedSaveData)
			{
				saveDataLimitComboBox.SetMinMax(SaveDataLimitType.Short, EnumUtils.MaxValue<SaveDataLimitType>());
			}
			saveDataLimitComboBox.Value = s_currentValue;
			saveDataLimitComboBox.OnValueChanged += SaveDataLimitComboBox_OnValueChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetCurrentValue(SaveDataLimitType limitType)
	{
		if (!limitType.IsSupported())
		{
			Log.Error($"Can not set unsupported limit: {limitType}");
		}
		else
		{
			if (s_currentValue == limitType)
			{
				return;
			}
			s_currentValue = limitType;
			foreach (KeyValuePair<XUiC_ComboBoxEnum<SaveDataLimitType>, object> item in (IEnumerable<KeyValuePair<XUiC_ComboBoxEnum<SaveDataLimitType>, object>>)s_saveDataLimitComboBoxes)
			{
				item.Deconstruct(out var key, out var _);
				key.Value = s_currentValue;
			}
			Save();
			OnValueChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveDataLimitComboBox_OnValueChanged(XUiController _sender, SaveDataLimitType _oldvalue, SaveDataLimitType _newvalue)
	{
		SetCurrentValue(_newvalue);
	}
}
