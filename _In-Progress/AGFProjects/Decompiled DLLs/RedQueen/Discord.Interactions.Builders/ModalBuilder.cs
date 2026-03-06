using System;
using System.Collections.Generic;

namespace Discord.Interactions.Builders;

internal class ModalBuilder
{
	internal readonly InteractionService _interactionService;

	internal readonly List<IInputComponentBuilder> _components;

	public ModalInitializer ModalInitializer { get; internal set; }

	public string Title { get; set; }

	public Type Type { get; }

	public IReadOnlyCollection<IInputComponentBuilder> Components => _components;

	internal ModalBuilder(Type type, InteractionService interactionService)
	{
		if (!typeof(IModal).IsAssignableFrom(type))
		{
			throw new ArgumentException("Must be an implementation of IModal", "type");
		}
		_interactionService = interactionService;
		_components = new List<IInputComponentBuilder>();
	}

	public ModalBuilder(Type type, ModalInitializer modalInitializer, InteractionService interactionService)
		: this(type, interactionService)
	{
		ModalInitializer = modalInitializer;
	}

	public ModalBuilder WithTitle(string title)
	{
		Title = title;
		return this;
	}

	public ModalBuilder AddTextComponent(Action<TextInputComponentBuilder> configure)
	{
		TextInputComponentBuilder textInputComponentBuilder = new TextInputComponentBuilder(this);
		configure(textInputComponentBuilder);
		_components.Add(textInputComponentBuilder);
		return this;
	}

	internal ModalInfo Build()
	{
		return new ModalInfo(this);
	}
}
