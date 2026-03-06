using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

internal class SelectMenuBuilder
{
	public const int MaxPlaceholderLength = 100;

	public const int MaxValuesCount = 25;

	public const int MaxOptionCount = 25;

	private List<SelectMenuOptionBuilder> _options = new List<SelectMenuOptionBuilder>();

	private int _minValues = 1;

	private int _maxValues = 1;

	private string _placeholder;

	private string _customId;

	public string CustomId
	{
		get
		{
			return _customId;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Custom Id length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Custom Id length must be at least 1.");
				}
			}
			_customId = value;
		}
	}

	public string Placeholder
	{
		get
		{
			return _placeholder;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Placeholder length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Placeholder length must be at least 1.");
				}
			}
			_placeholder = value;
		}
	}

	public int MinValues
	{
		get
		{
			return _minValues;
		}
		set
		{
			Preconditions.AtMost(value, 25, "MinValues");
			_minValues = value;
		}
	}

	public int MaxValues
	{
		get
		{
			return _maxValues;
		}
		set
		{
			Preconditions.AtMost(value, 25, "MaxValues");
			_maxValues = value;
		}
	}

	public List<SelectMenuOptionBuilder> Options
	{
		get
		{
			return _options;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtMost(value.Count, 25, "Options");
				_options = value;
				return;
			}
			throw new ArgumentNullException("value", "Options cannot be null.");
		}
	}

	public bool IsDisabled { get; set; }

	public SelectMenuBuilder()
	{
	}

	public SelectMenuBuilder(SelectMenuComponent selectMenu)
	{
		Placeholder = selectMenu.Placeholder;
		CustomId = selectMenu.Placeholder;
		MaxValues = selectMenu.MaxValues;
		MinValues = selectMenu.MinValues;
		IsDisabled = selectMenu.IsDisabled;
		Options = selectMenu.Options?.Select((SelectMenuOption x) => new SelectMenuOptionBuilder(x.Label, x.Value, x.Description, x.Emote, x.IsDefault)).ToList();
	}

	public SelectMenuBuilder(string customId, List<SelectMenuOptionBuilder> options, string placeholder = null, int maxValues = 1, int minValues = 1, bool isDisabled = false)
	{
		CustomId = customId;
		Options = options;
		Placeholder = placeholder;
		IsDisabled = isDisabled;
		MaxValues = maxValues;
		MinValues = minValues;
	}

	public SelectMenuBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return this;
	}

	public SelectMenuBuilder WithPlaceholder(string placeholder)
	{
		Placeholder = placeholder;
		return this;
	}

	public SelectMenuBuilder WithMinValues(int minValues)
	{
		MinValues = minValues;
		return this;
	}

	public SelectMenuBuilder WithMaxValues(int maxValues)
	{
		MaxValues = maxValues;
		return this;
	}

	public SelectMenuBuilder WithOptions(List<SelectMenuOptionBuilder> options)
	{
		Options = options;
		return this;
	}

	public SelectMenuBuilder AddOption(SelectMenuOptionBuilder option)
	{
		if (Options.Count >= 25)
		{
			throw new InvalidOperationException($"Options count reached {25}.");
		}
		Options.Add(option);
		return this;
	}

	public SelectMenuBuilder AddOption(string label, string value, string description = null, IEmote emote = null, bool? isDefault = null)
	{
		AddOption(new SelectMenuOptionBuilder(label, value, description, emote, isDefault));
		return this;
	}

	public SelectMenuBuilder WithDisabled(bool isDisabled)
	{
		IsDisabled = isDisabled;
		return this;
	}

	public SelectMenuComponent Build()
	{
		List<SelectMenuOption> options = Options?.Select((SelectMenuOptionBuilder x) => x.Build()).ToList();
		return new SelectMenuComponent(CustomId, options, Placeholder, MinValues, MaxValues, IsDisabled);
	}
}
