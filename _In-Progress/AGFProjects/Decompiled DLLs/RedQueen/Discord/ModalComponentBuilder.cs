using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

internal class ModalComponentBuilder
{
	public const int MaxCustomIdLength = 100;

	public const int MaxActionRowCount = 5;

	private List<ActionRowBuilder> _actionRows;

	public List<ActionRowBuilder> ActionRows
	{
		get
		{
			return _actionRows;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "ActionRows cannot be null.");
			}
			if (value.Count > 5)
			{
				throw new ArgumentOutOfRangeException("value", $"Action row count must be less than or equal to {5}.");
			}
			_actionRows = value;
		}
	}

	public static ComponentBuilder FromComponents(IReadOnlyCollection<IMessageComponent> components)
	{
		ComponentBuilder componentBuilder = new ComponentBuilder();
		for (int i = 0; i != components.Count; i++)
		{
			IMessageComponent component = components.ElementAt(i);
			componentBuilder.AddComponent(component, i);
		}
		return componentBuilder;
	}

	internal void AddComponent(IMessageComponent component, int row)
	{
		if (!(component is TextInputComponent textInputComponent))
		{
			if (!(component is ActionRowComponent actionRowComponent))
			{
				return;
			}
			{
				foreach (IMessageComponent component2 in actionRowComponent.Components)
				{
					AddComponent(component2, row);
				}
				return;
			}
		}
		WithTextInput(textInputComponent.Label, textInputComponent.CustomId, textInputComponent.Style, textInputComponent.Placeholder, textInputComponent.MinLength, textInputComponent.MaxLength, row);
	}

	public ModalComponentBuilder WithTextInput(string label, string customId, TextInputStyle style = TextInputStyle.Short, string placeholder = null, int? minLength = null, int? maxLength = null, int row = 0, bool? required = null, string value = null)
	{
		return WithTextInput(new TextInputBuilder(label, customId, style, placeholder, minLength, maxLength, required, value), row);
	}

	public ModalComponentBuilder WithTextInput(TextInputBuilder text, int row = 0)
	{
		Preconditions.LessThan(row, 5, "row");
		TextInputComponent component = text.Build();
		if (_actionRows == null)
		{
			_actionRows = new List<ActionRowBuilder> { new ActionRowBuilder().AddComponent(component) };
		}
		else if (_actionRows.Count == row)
		{
			_actionRows.Add(new ActionRowBuilder().AddComponent(component));
		}
		else
		{
			ActionRowBuilder actionRowBuilder;
			if (_actionRows.Count > row)
			{
				actionRowBuilder = _actionRows.ElementAt(row);
			}
			else
			{
				actionRowBuilder = new ActionRowBuilder();
				_actionRows.Add(actionRowBuilder);
			}
			if (actionRowBuilder.CanTakeComponent(component))
			{
				actionRowBuilder.AddComponent(component);
			}
			else
			{
				if (row >= 5)
				{
					throw new InvalidOperationException("There are no more rows to add text to.");
				}
				WithTextInput(text, row + 1);
			}
		}
		return this;
	}

	public ModalComponent Build()
	{
		return new ModalComponent(ActionRows?.Select((ActionRowBuilder x) => x.Build()).ToList());
	}
}
