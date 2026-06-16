namespace SandboxOptions;

public class SandboxOptionFloat : BaseSandboxOption
{
	public class DisabledOptionsOnValue
	{
		public float Value;

		public bool Inverted;

		public SandboxOptions[] DisabledOptions;

		public DisabledOptionsOnValue(SandboxOptions[] options, float value, bool inverted = false)
		{
			DisabledOptions = options;
			Value = value;
			Inverted = inverted;
		}
	}

	public float DefaultValue;

	public float CurrentValue = 1f;

	public bool IsPercent;

	public DisabledOptionsOnValue DisabledOptions;

	public override OptionTypes OptionType => OptionTypes.Float;

	public override string ValueText => CurrentValue.ToString();

	public SandboxOptionFloat(SandboxOptions option, string optionName, string categoryName, string valueSetName, float defaultValue, bool newUISection = false, DisabledOptionsOnValue disabledOptions = null)
		: base(option, optionName, categoryName, valueSetName, newUISection)
	{
		DefaultValue = defaultValue;
		DisabledOptions = disabledOptions;
	}

	public override float GetFloatValue()
	{
		return CurrentValue;
	}

	public override int GetIntValue()
	{
		return (int)(CurrentValue * 100f);
	}

	public override void SetValue(string value)
	{
		if (StringParsers.TryParseFloat(value, out var _result) && base.ValueOptions.GetFloatIndex(_result) != -1)
		{
			CurrentValue = _result;
		}
	}

	public override void SetFloat(float value)
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

	public override float GetDefaultFloatValue()
	{
		return DefaultValue;
	}

	public override int GetDefaultIndex()
	{
		return GetValueSet().GetFloatIndex(DefaultValue);
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
		return GetValueSet().GetFloatIndex(CurrentValue);
	}

	public override int GetValueIndex(string value)
	{
		if (StringParsers.TryParseFloat(value, out var _result))
		{
			return GetValueSet().GetFloatIndex(_result);
		}
		return GetValueSet().GetFloatIndex(DefaultValue);
	}

	public override void SetValueFromIndex(int index)
	{
		if (!GetValueSet().GetFloatValue(index, out CurrentValue))
		{
			CurrentValue = DefaultValue;
		}
	}
}
