namespace SandboxOptions;

public class SandboxOptionInt : BaseSandboxOption
{
	public class DisabledOptionsOnValue
	{
		public int Value;

		public bool Inverted;

		public SandboxOptions[] DisabledOptions;

		public DisabledOptionsOnValue(SandboxOptions[] options, int value, bool inverted = false)
		{
			DisabledOptions = options;
			Value = value;
			Inverted = inverted;
		}
	}

	public int DefaultValue;

	public int CurrentValue = 1;

	public DisabledOptionsOnValue DisabledOptions;

	public override OptionTypes OptionType => OptionTypes.Int;

	public override string ValueText => CurrentValue.ToString();

	public SandboxOptionInt(SandboxOptions option, string optionName, string categoryName, string valueSetName, int defaultValue, bool newUISection = false, DisabledOptionsOnValue disabledOptions = null)
		: base(option, optionName, categoryName, valueSetName, newUISection)
	{
		DefaultValue = defaultValue;
		DisabledOptions = disabledOptions;
	}

	public override int GetIntValue()
	{
		return CurrentValue;
	}

	public override float GetFloatValue()
	{
		return (float)CurrentValue / 100f;
	}

	public override void SetValue(string value)
	{
		if (StringParsers.TryParseSInt32(value, out var _result) && base.ValueOptions.GetIntIndex(_result) != -1)
		{
			CurrentValue = _result;
		}
	}

	public override void SetInt(int value)
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

	public override int GetDefaultIntValue()
	{
		return DefaultValue;
	}

	public override int GetDefaultIndex()
	{
		return GetValueSet().GetIntIndex(DefaultValue);
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
		return GetValueSet().GetIntIndex(CurrentValue);
	}

	public override int GetValueIndex(string value)
	{
		if (StringParsers.TryParseSInt32(value, out var _result))
		{
			return GetValueSet().GetIntIndex(_result);
		}
		return GetValueSet().GetIntIndex(DefaultValue);
	}

	public override void SetValueFromIndex(int index)
	{
		if (!GetValueSet().GetIntValue(index, out CurrentValue))
		{
			CurrentValue = DefaultValue;
		}
	}
}
