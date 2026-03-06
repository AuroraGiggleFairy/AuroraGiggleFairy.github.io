using System;
using System.Collections.Generic;

namespace Discord.Interactions.Builders;

internal abstract class InputComponentBuilder<TInfo, TBuilder> : IInputComponentBuilder where TInfo : InputComponentInfo where TBuilder : InputComponentBuilder<TInfo, TBuilder>
{
	private readonly List<Attribute> _attributes;

	protected abstract TBuilder Instance { get; }

	public ModalBuilder Modal { get; }

	public string CustomId { get; set; }

	public string Label { get; set; }

	public bool IsRequired { get; set; } = true;

	public ComponentType ComponentType { get; internal set; }

	public Type Type { get; private set; }

	public ComponentTypeConverter TypeConverter { get; private set; }

	public object DefaultValue { get; set; }

	public IReadOnlyCollection<Attribute> Attributes => _attributes;

	public InputComponentBuilder(ModalBuilder modal)
	{
		Modal = modal;
		_attributes = new List<Attribute>();
	}

	public TBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return Instance;
	}

	public TBuilder WithLabel(string label)
	{
		Label = label;
		return Instance;
	}

	public TBuilder SetIsRequired(bool isRequired)
	{
		IsRequired = isRequired;
		return Instance;
	}

	public TBuilder WithComponentType(ComponentType componentType)
	{
		ComponentType = componentType;
		return Instance;
	}

	public TBuilder WithType(Type type)
	{
		Type = type;
		TypeConverter = Modal._interactionService.GetComponentTypeConverter(type);
		return Instance;
	}

	public TBuilder SetDefaultValue(object value)
	{
		DefaultValue = value;
		return Instance;
	}

	public TBuilder WithAttributes(params Attribute[] attributes)
	{
		_attributes.AddRange(attributes);
		return Instance;
	}

	internal abstract TInfo Build(ModalInfo modal);

	IInputComponentBuilder IInputComponentBuilder.WithCustomId(string customId)
	{
		return WithCustomId(customId);
	}

	IInputComponentBuilder IInputComponentBuilder.WithLabel(string label)
	{
		return WithCustomId(label);
	}

	IInputComponentBuilder IInputComponentBuilder.WithType(Type type)
	{
		return WithType(type);
	}

	IInputComponentBuilder IInputComponentBuilder.SetDefaultValue(object value)
	{
		return SetDefaultValue(value);
	}

	IInputComponentBuilder IInputComponentBuilder.WithAttributes(params Attribute[] attributes)
	{
		return WithAttributes(attributes);
	}

	IInputComponentBuilder IInputComponentBuilder.SetIsRequired(bool isRequired)
	{
		return SetIsRequired(isRequired);
	}
}
