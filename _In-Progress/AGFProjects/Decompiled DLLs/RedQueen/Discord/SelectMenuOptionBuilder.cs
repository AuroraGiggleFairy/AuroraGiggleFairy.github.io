using System;

namespace Discord;

internal class SelectMenuOptionBuilder
{
	public const int MaxSelectLabelLength = 100;

	public const int MaxDescriptionLength = 100;

	public const int MaxSelectValueLength = 100;

	private string _label;

	private string _value;

	private string _description;

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Label length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Label length must be at least 1.");
				}
			}
			_label = value;
		}
	}

	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Value length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Value length must be at least 1.");
				}
			}
			_value = value;
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Description length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Description length must be at least 1.");
				}
			}
			_description = value;
		}
	}

	public IEmote Emote { get; set; }

	public bool? IsDefault { get; set; }

	public SelectMenuOptionBuilder()
	{
	}

	public SelectMenuOptionBuilder(string label, string value, string description = null, IEmote emote = null, bool? isDefault = null)
	{
		Label = label;
		Value = value;
		Description = description;
		Emote = emote;
		IsDefault = isDefault;
	}

	public SelectMenuOptionBuilder(SelectMenuOption option)
	{
		Label = option.Label;
		Value = option.Value;
		Description = option.Description;
		Emote = option.Emote;
		IsDefault = option.IsDefault;
	}

	public SelectMenuOptionBuilder WithLabel(string label)
	{
		Label = label;
		return this;
	}

	public SelectMenuOptionBuilder WithValue(string value)
	{
		Value = value;
		return this;
	}

	public SelectMenuOptionBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public SelectMenuOptionBuilder WithEmote(IEmote emote)
	{
		Emote = emote;
		return this;
	}

	public SelectMenuOptionBuilder WithDefault(bool isDefault)
	{
		IsDefault = isDefault;
		return this;
	}

	public SelectMenuOption Build()
	{
		return new SelectMenuOption(Label, Value, Description, Emote, IsDefault);
	}
}
