using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

internal class ModalBuilder
{
	private string _customId;

	public ModalComponentBuilder Components { get; set; } = new ModalComponentBuilder();

	public string Title { get; set; }

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

	public ModalBuilder()
	{
	}

	public ModalBuilder(string title, string customId, ModalComponentBuilder components = null)
	{
		Title = title;
		CustomId = customId;
		Components = components ?? new ModalComponentBuilder();
	}

	public ModalBuilder WithTitle(string title)
	{
		Title = title;
		return this;
	}

	public ModalBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return this;
	}

	public ModalBuilder AddTextInput(TextInputBuilder component)
	{
		Components.WithTextInput(component);
		return this;
	}

	public ModalBuilder AddTextInput(string label, string customId, TextInputStyle style = TextInputStyle.Short, string placeholder = "", int? minLength = null, int? maxLength = null, bool? required = null, string value = null)
	{
		return AddTextInput(new TextInputBuilder(label, customId, style, placeholder, minLength, maxLength, required, value));
	}

	public ModalBuilder AddComponents(List<IMessageComponent> components, int row)
	{
		components.ForEach(delegate(IMessageComponent x)
		{
			Components.AddComponent(x, row);
		});
		return this;
	}

	public Modal Build()
	{
		if (string.IsNullOrEmpty(CustomId))
		{
			throw new ArgumentException("Modals must have a custom id.", "CustomId");
		}
		if (string.IsNullOrWhiteSpace(Title))
		{
			throw new ArgumentException("Modals must have a title.", "Title");
		}
		List<ActionRowBuilder> actionRows = Components.ActionRows;
		if (actionRows != null && actionRows.SelectMany((ActionRowBuilder x) => x.Components).Any((IMessageComponent x) => x.Type != ComponentType.TextInput))
		{
			throw new ArgumentException("Only TextInputComponents are allowed.", "Components");
		}
		return new Modal(Title, CustomId, Components.Build());
	}
}
