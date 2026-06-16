using System;

namespace SandboxOptions;

public class BaseSandboxOption
{
	public enum OptionTypes
	{
		Invalid,
		Int,
		Float,
		String,
		Bool
	}

	public string OptionName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string optionNameText;

	public string OverrideOptionName;

	public string DisabledByText = "";

	public bool IsEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string descriptionText;

	public string OverrideDescriptionName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ValueSetName = "";

	public SandboxOptions Option;

	public string CategoryName = "";

	public bool NewUISection;

	public virtual OptionTypes OptionType => OptionTypes.Invalid;

	public string OptionNameText
	{
		get
		{
			if (optionNameText == null)
			{
				if (OverrideOptionName == null)
				{
					string key = "go" + Option;
					optionNameText = (Localization.Exists(key) ? Localization.Get(key) : ("*" + OptionName));
				}
				else
				{
					optionNameText = (Localization.Exists(OverrideOptionName) ? Localization.Get(OverrideOptionName) : "");
				}
			}
			return optionNameText;
		}
	}

	public string DescriptionText
	{
		get
		{
			if (descriptionText == null)
			{
				if (OverrideDescriptionName == null)
				{
					string key = "go" + Option.ToString() + "Desc";
					descriptionText = (Localization.Exists(key) ? Localization.Get(key) : "");
				}
				else
				{
					descriptionText = (Localization.Exists(OverrideDescriptionName) ? Localization.Get(OverrideDescriptionName) : "");
				}
			}
			return descriptionText;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SandboxOptionValueSet ValueOptions
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual string ValueText => "";

	public BaseSandboxOption(SandboxOptions option, string name, string categoryName, string valueSetName, bool newUISection)
	{
		Option = option;
		OptionName = name;
		CategoryName = categoryName;
		ValueSetName = valueSetName;
		NewUISection = newUISection;
	}

	public virtual bool GetBoolValue()
	{
		return false;
	}

	public virtual float GetFloatValue()
	{
		return 0f;
	}

	public virtual int GetIntValue()
	{
		return 0;
	}

	public SandboxOptionValueSet GetValueSet()
	{
		if (ValueOptions == null)
		{
			if (!SandboxOptionManager.Current.ValueSets.ContainsKey(ValueSetName))
			{
				throw new Exception("BaseSandboxOption: ValueOption " + ValueSetName + " does not exist.");
			}
			ValueOptions = SandboxOptionManager.Current.ValueSets[ValueSetName];
		}
		return ValueOptions;
	}

	public virtual void SetValueFromIndex(int index)
	{
	}

	public virtual void SetValue(string value)
	{
	}

	public virtual void SetFloat(float value)
	{
	}

	public virtual void SetInt(int value)
	{
	}

	public virtual void SetBool(bool value)
	{
	}

	public virtual bool IsChanged()
	{
		return false;
	}

	public virtual object GetDefaultValue()
	{
		return null;
	}

	public virtual float GetDefaultFloatValue()
	{
		return -1f;
	}

	public virtual int GetDefaultIntValue()
	{
		return -1;
	}

	public virtual bool GetDefaultBoolValue()
	{
		return false;
	}

	public virtual int GetDefaultIndex()
	{
		return -1;
	}

	public virtual void SetToDefault()
	{
	}

	public virtual string GetDefaultValueText()
	{
		return GetValueSet().GetDisplayAtIndex(GetDefaultIndex());
	}

	public virtual string GetValueTextFromIndex(int index)
	{
		return GetValueSet().GetDisplayAtIndex(index);
	}

	public virtual string GetValueText()
	{
		return "";
	}

	public virtual int GetValueIndex()
	{
		return 0;
	}

	public virtual int GetValueIndex(string value)
	{
		return 0;
	}
}
