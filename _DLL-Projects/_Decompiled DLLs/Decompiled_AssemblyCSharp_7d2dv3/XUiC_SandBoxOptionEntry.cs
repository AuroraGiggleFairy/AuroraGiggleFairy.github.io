using System;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandBoxOptionEntry : XUiController
{
	public enum EOptionValueType
	{
		Null,
		Custom,
		Any,
		Int,
		Float,
		String
	}

	public readonly struct SandboxOptionValue : IEquatable<SandboxOptionValue>
	{
		public readonly EOptionValueType Type;

		public readonly int IntValue;

		public readonly float FloatValue;

		public readonly string StringValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string DisplayName;

		public SandboxOptionValue(EOptionValueType _type, string _displayName)
		{
			Type = _type;
			IntValue = -1;
			FloatValue = -1f;
			StringValue = null;
			DisplayName = _displayName;
		}

		public SandboxOptionValue(int _intValue, string _displayName)
		{
			Type = EOptionValueType.Int;
			IntValue = _intValue;
			FloatValue = -1f;
			StringValue = null;
			DisplayName = _displayName;
		}

		public SandboxOptionValue(float _floatValue, string _displayName)
		{
			Type = EOptionValueType.Float;
			IntValue = -1;
			FloatValue = _floatValue;
			StringValue = null;
			DisplayName = _displayName;
		}

		public SandboxOptionValue(string _stringValue, string _displayName)
		{
			Type = EOptionValueType.String;
			IntValue = -1;
			FloatValue = -1f;
			StringValue = _stringValue;
			DisplayName = _displayName;
		}

		public override string ToString()
		{
			return DisplayName;
		}

		public bool Equals(SandboxOptionValue _other)
		{
			if (Type != _other.Type)
			{
				return false;
			}
			return Type switch
			{
				EOptionValueType.Custom => true, 
				EOptionValueType.Any => true, 
				EOptionValueType.Int => IntValue == _other.IntValue, 
				EOptionValueType.Float => Mathf.Approximately(FloatValue, _other.FloatValue), 
				EOptionValueType.String => StringValue == _other.StringValue, 
				_ => false, 
			};
		}

		public override bool Equals(object _obj)
		{
			if (_obj is SandboxOptionValue other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((((((int)Type * 397) ^ IntValue) * 397) ^ ((StringValue != null) ? StringValue.GetHashCode() : 0)) * 397) ^ ((DisplayName != null) ? DisplayName.GetHashCode() : 0);
		}
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<SandboxOptionValue> controlCombo;

	public string CategoryName;

	[PublicizedFrom(EAccessModifier.Private)]
	public global::SandboxOptions.SandboxOptions sandboxOption = global::SandboxOptions.SandboxOptions.Max;

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseSandboxOption.OptionTypes valueType;

	public Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public SandboxOptionValue originalValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSeparator;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public BaseSandboxOption Option
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public SandboxOptionValue OriginalValue => originalValue;

	[XuiXmlBinding("option_enabled")]
	public bool Enabled
	{
		get
		{
			if (HasEntry && SandboxOptionManager.Current.IsEnabled(sandboxOption))
			{
				return !SandboxOptionManager.Current.IsOverriden(sandboxOption);
			}
			return false;
		}
	}

	[XuiXmlBinding("option_disabled_by_mod")]
	public bool DisabledByMod
	{
		get
		{
			if (HasEntry)
			{
				return SandboxOptionManager.Current.IsOverriden(sandboxOption);
			}
			return false;
		}
	}

	[XuiXmlBinding("option_disabled_by_option")]
	public bool DisabledByOption
	{
		get
		{
			if (HasEntry)
			{
				return !SandboxOptionManager.Current.IsEnabled(sandboxOption);
			}
			return false;
		}
	}

	[XuiXmlBinding("option_changed")]
	public bool IsChanged
	{
		get
		{
			if (HasEntry)
			{
				return !originalValue.Equals(controlCombo.Value);
			}
			return false;
		}
	}

	[XuiXmlBinding("title")]
	public string Title
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Option?.OptionNameText ?? "";
		}
	}

	[XuiXmlBinding("tooltip_key")]
	public string TooltipKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Option?.DescriptionText ?? "";
		}
	}

	[XuiXmlBinding("tooltip_key_disabledoption")]
	public string TooltipKeyDisabledOption
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Option?.DisabledByText ?? "";
		}
	}

	[XuiXmlBinding("is_separator")]
	public bool IsSeparator
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return isSeparator;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			isSeparator = value;
		}
	}

	[XuiXmlBinding("has_entry")]
	public bool HasEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Option != null;
		}
	}

	public override void Init()
	{
		base.Init();
		sandboxOption = EnumUtils.Parse<global::SandboxOptions.SandboxOptions>(viewComponent.ID);
		setupOptions();
	}

	public void setupSeparator()
	{
		isSeparator = true;
		Option = null;
		sandboxOption = global::SandboxOptions.SandboxOptions.Max;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void setupOption(BaseSandboxOption _baseSandboxOption)
	{
		isSeparator = false;
		Option = _baseSandboxOption;
		if (_baseSandboxOption == null)
		{
			sandboxOption = global::SandboxOptions.SandboxOptions.Max;
		}
		else
		{
			sandboxOption = Option.Option;
			valueType = Option.OptionType;
			CategoryName = Option.CategoryName;
			setupOptions();
			setComboBoxValue(_useCurrent: false, _storeAsOriginal: false);
		}
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (handleDirtyUpdateDefault() && HasEntry)
		{
			controlCombo.ValueTextOverride = (SandboxOptionManager.Current.IsEnabled(sandboxOption) ? null : "----");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		controlCombo.Elements.Clear();
		if (Option == null)
		{
			return;
		}
		SandboxOptionValueSet valueSet = Option.GetValueSet();
		for (int i = 0; i < valueSet.DisplayValues.Length; i++)
		{
			switch (Option.OptionType)
			{
			case BaseSandboxOption.OptionTypes.Int:
			{
				if (valueSet.GetIntValue(i, out var val2))
				{
					controlCombo.Elements.Add(new SandboxOptionValue(val2, valueSet.GetDisplayAtIndex(i)));
				}
				break;
			}
			case BaseSandboxOption.OptionTypes.Float:
			{
				if (valueSet.GetFloatValue(i, out var val))
				{
					controlCombo.Elements.Add(new SandboxOptionValue(val, valueSet.GetDisplayAtIndex(i)));
				}
				break;
			}
			case BaseSandboxOption.OptionTypes.Bool:
				controlCombo.Elements.Add(new SandboxOptionValue(valueSet.GetBoolValue(i) ? 1 : 0, valueSet.DisplayValues[i]));
				break;
			}
		}
	}

	[XuiBindEvent("OnValueChanged", "controlCombo")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, SandboxOptionValue _oldValue, SandboxOptionValue _newValue)
	{
		OnValueChanged?.Invoke(this, sandboxOption);
		IsDirty = true;
	}

	public void ForceOriginalValue(SandboxOptionValue _value)
	{
		originalValue = _value;
	}

	public void ResetToDefault(bool _storeAsOriginal)
	{
		SandboxOptionManager.Current.SetOptionToDefault(sandboxOption);
		setComboBoxValue(_useCurrent: false, _storeAsOriginal);
	}

	public void ApplyChange(bool _forceChange)
	{
		if (!originalValue.Equals(controlCombo.Value) || _forceChange)
		{
			switch (valueType)
			{
			case BaseSandboxOption.OptionTypes.Int:
				SandboxOptionManager.Current.SetOption(sandboxOption, controlCombo.Value.IntValue);
				break;
			case BaseSandboxOption.OptionTypes.Float:
				SandboxOptionManager.Current.SetOption(sandboxOption, controlCombo.Value.FloatValue);
				break;
			case BaseSandboxOption.OptionTypes.Bool:
				SandboxOptionManager.Current.SetOption(sandboxOption, controlCombo.Value.IntValue == 1);
				break;
			case BaseSandboxOption.OptionTypes.String:
				SandboxOptionManager.Current.SetOption(sandboxOption, controlCombo.Value.StringValue);
				break;
			default:
				throw new Exception("Illegal option setup for " + sandboxOption.ToStringCached());
			}
			IsDirty = true;
		}
	}

	public void SetComboBoxIndex(int _index)
	{
		if (controlCombo.Elements.Count > _index)
		{
			controlCombo.SelectedIndex = _index;
		}
		originalValue = controlCombo.Value;
		OnValueChanged?.Invoke(this, sandboxOption);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setComboBoxValue(bool _useCurrent, bool _storeAsOriginal)
	{
		try
		{
			switch (valueType)
			{
			case BaseSandboxOption.OptionTypes.Int:
			{
				int num = (_useCurrent ? controlCombo.Value.IntValue : SandboxOptionManager.GetInt(sandboxOption));
				bool flag = false;
				for (int i = 1; i < controlCombo.Elements.Count; i++)
				{
					if (controlCombo.Elements[i].IntValue == num)
					{
						controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				int num2 = -1;
				int num3 = int.MaxValue;
				for (int j = 0; j < controlCombo.Elements.Count; j++)
				{
					int num4 = Math.Abs(controlCombo.Elements[j].IntValue - num);
					if (num2 < 0 || num4 < num3)
					{
						num2 = j;
						num3 = num4;
						if (num3 <= 0)
						{
							break;
						}
					}
				}
				if (num2 >= 0)
				{
					controlCombo.SelectedIndex = num2;
					flag = true;
				}
				break;
			}
			case BaseSandboxOption.OptionTypes.Float:
			{
				float num5 = (_useCurrent ? controlCombo.Value.FloatValue : SandboxOptionManager.GetFloat(sandboxOption));
				bool flag2 = false;
				for (int k = 1; k < controlCombo.Elements.Count; k++)
				{
					if (Mathf.Approximately(controlCombo.Elements[k].FloatValue, num5))
					{
						controlCombo.SelectedIndex = k;
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
				int num6 = -1;
				float num7 = float.MaxValue;
				for (int l = 0; l < controlCombo.Elements.Count; l++)
				{
					float num8 = Math.Abs(controlCombo.Elements[l].FloatValue - num5);
					if (num6 < 0 || !(num8 >= num7))
					{
						num6 = l;
						num7 = num8;
						if (num7 <= 0f)
						{
							break;
						}
					}
				}
				if (num6 >= 0)
				{
					controlCombo.SelectedIndex = num6;
					flag2 = true;
				}
				break;
			}
			case BaseSandboxOption.OptionTypes.Bool:
				controlCombo.SelectedIndex = (_useCurrent ? controlCombo.Value.IntValue : (SandboxOptionManager.GetBool(sandboxOption) ? 1 : 0));
				break;
			default:
				throw new Exception("Illegal option setup for " + sandboxOption.ToStringCached());
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		if (!_useCurrent && _storeAsOriginal)
		{
			originalValue = controlCombo.Value;
		}
		OnValueChanged?.Invoke(this, sandboxOption);
		IsDirty = true;
	}

	[XuiXmlBinding("option_default")]
	public bool IsDefaultValue()
	{
		if (Option == null)
		{
			BaseSandboxOption baseSandboxOption = (Option = SandboxOptionManager.GetOption(sandboxOption));
		}
		if (!HasEntry)
		{
			return true;
		}
		return valueType switch
		{
			BaseSandboxOption.OptionTypes.Int => controlCombo.Value.IntValue == (int)Option.GetDefaultValue(), 
			BaseSandboxOption.OptionTypes.Float => Mathf.Approximately(controlCombo.Value.FloatValue, (float)Option.GetDefaultValue()), 
			BaseSandboxOption.OptionTypes.String => controlCombo.Value.StringValue == (string)Option.GetDefaultValue(), 
			BaseSandboxOption.OptionTypes.Bool => controlCombo.Value.IntValue == 1 == (bool)Option.GetDefaultValue(), 
			_ => false, 
		};
	}
}
