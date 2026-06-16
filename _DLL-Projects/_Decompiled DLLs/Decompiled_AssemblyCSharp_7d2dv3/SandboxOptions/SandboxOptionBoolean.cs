namespace SandboxOptions;

public class SandboxOptionBoolean : BaseSandboxOption
{
	public class DisabledOptionsOnValue
	{
		public bool Value;

		public bool Inverted;

		public SandboxOptions[] DisabledOptions;

		public DisabledOptionsOnValue(SandboxOptions[] options, bool value, bool inverted = false)
		{
			DisabledOptions = options;
			Value = value;
			Inverted = inverted;
		}
	}

	public bool DefaultValue;

	public bool CurrentValue;

	public DisabledOptionsOnValue DisabledOptions;

	public override OptionTypes OptionType => OptionTypes.Bool;

	public override string ValueText => CurrentValue.ToString();

	public SandboxOptionBoolean(SandboxOptions option, string optionName, string categoryName, string valueSetName, bool defaultValue, bool newUISection = false, DisabledOptionsOnValue disabledOptions = null)
		: base(option, optionName, categoryName, valueSetName, newUISection)
	{
		CurrentValue = (DefaultValue = defaultValue);
		DisabledOptions = disabledOptions;
	}

	public override bool GetBoolValue()
	{
		return CurrentValue;
	}

	public override void SetValue(string value)
	{
		if (StringParsers.TryParseBool(value, out var _result))
		{
			CurrentValue = _result;
		}
	}

	public override void SetBool(bool value)
	{
		CurrentValue = value;
	}

	public override bool IsChanged()
	{
		return CurrentValue != DefaultValue;
	}

	public override object GetDefaultValue()
	{
		return DefaultValue;
	}

	public override bool GetDefaultBoolValue()
	{
		return DefaultValue;
	}

	public override int GetDefaultIndex()
	{
		return GetValueSet().GetBoolIndex(DefaultValue);
	}

	public override void SetToDefault()
	{
		CurrentValue = DefaultValue;
	}

	public override string GetValueText()
	{
		return $"{CurrentValue} | default:{DefaultValue}";
	}

	public override int GetValueIndex()
	{
		return GetValueSet().GetBoolIndex(CurrentValue);
	}

	public override int GetValueIndex(string value)
	{
		if (StringParsers.TryParseBool(value, out var _result))
		{
			return GetValueSet().GetBoolIndex(_result);
		}
		return GetValueSet().GetBoolIndex(DefaultValue);
	}

	public override void SetValueFromIndex(int index)
	{
		if (!GetValueSet().GetBoolValue(index, out CurrentValue))
		{
			CurrentValue = DefaultValue;
		}
	}
}
