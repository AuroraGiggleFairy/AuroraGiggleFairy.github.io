using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

internal class ActionRowBuilder
{
	public const int MaxChildCount = 5;

	private List<IMessageComponent> _components = new List<IMessageComponent>();

	public List<IMessageComponent> Components
	{
		get
		{
			return _components;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "Components cannot be null.");
			}
			int count = value.Count;
			if (count <= 5)
			{
				if (count == 0)
				{
					throw new ArgumentOutOfRangeException("value", "There must be at least 1 component in a row.");
				}
				_components = value;
				return;
			}
			throw new ArgumentOutOfRangeException("value", $"Action row can only contain {5} child components!");
		}
	}

	public ActionRowBuilder WithComponents(List<IMessageComponent> components)
	{
		Components = components;
		return this;
	}

	public ActionRowBuilder AddComponent(IMessageComponent component)
	{
		if (Components.Count >= 5)
		{
			throw new InvalidOperationException($"Components count reached {5}");
		}
		Components.Add(component);
		return this;
	}

	public ActionRowBuilder WithSelectMenu(string customId, List<SelectMenuOptionBuilder> options, string placeholder = null, int minValues = 1, int maxValues = 1, bool disabled = false)
	{
		return WithSelectMenu(new SelectMenuBuilder().WithCustomId(customId).WithOptions(options).WithPlaceholder(placeholder)
			.WithMaxValues(maxValues)
			.WithMinValues(minValues)
			.WithDisabled(disabled));
	}

	public ActionRowBuilder WithSelectMenu(SelectMenuBuilder menu)
	{
		if (menu.Options.Distinct().Count() != menu.Options.Count)
		{
			throw new InvalidOperationException("Please make sure that there is no duplicates values.");
		}
		SelectMenuComponent component = menu.Build();
		if (Components.Count != 0)
		{
			throw new InvalidOperationException("A Select Menu cannot exist in a pre-occupied ActionRow.");
		}
		AddComponent(component);
		return this;
	}

	public ActionRowBuilder WithButton(string label = null, string customId = null, ButtonStyle style = ButtonStyle.Primary, IEmote emote = null, string url = null, bool disabled = false)
	{
		ButtonBuilder button = new ButtonBuilder().WithLabel(label).WithStyle(style).WithEmote(emote)
			.WithCustomId(customId)
			.WithUrl(url)
			.WithDisabled(disabled);
		return WithButton(button);
	}

	public ActionRowBuilder WithButton(ButtonBuilder button)
	{
		ButtonComponent component = button.Build();
		if (Components.Count >= 5)
		{
			throw new InvalidOperationException($"Components count reached {5}");
		}
		if (Components.Any((IMessageComponent x) => x.Type == ComponentType.SelectMenu))
		{
			throw new InvalidOperationException("A button cannot be added to a row with a SelectMenu");
		}
		AddComponent(component);
		return this;
	}

	public ActionRowComponent Build()
	{
		return new ActionRowComponent(_components);
	}

	internal bool CanTakeComponent(IMessageComponent component)
	{
		switch (component.Type)
		{
		case ComponentType.ActionRow:
			return false;
		case ComponentType.Button:
			if (Components.Any((IMessageComponent x) => x.Type == ComponentType.SelectMenu))
			{
				return false;
			}
			return Components.Count < 5;
		case ComponentType.SelectMenu:
			return Components.Count == 0;
		default:
			return false;
		}
	}
}
