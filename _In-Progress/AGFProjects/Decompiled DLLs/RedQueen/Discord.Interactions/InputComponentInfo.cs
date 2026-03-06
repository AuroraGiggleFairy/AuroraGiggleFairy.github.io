using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal abstract class InputComponentInfo
{
	public ModalInfo Modal { get; }

	public string CustomId { get; }

	public string Label { get; }

	public bool IsRequired { get; }

	public ComponentType ComponentType { get; }

	public Type Type { get; }

	public ComponentTypeConverter TypeConverter { get; }

	public object DefaultValue { get; }

	public IReadOnlyCollection<Attribute> Attributes { get; }

	protected InputComponentInfo(IInputComponentBuilder builder, ModalInfo modal)
	{
		Modal = modal;
		CustomId = builder.CustomId;
		Label = builder.Label;
		IsRequired = builder.IsRequired;
		ComponentType = builder.ComponentType;
		Type = builder.Type;
		TypeConverter = builder.TypeConverter;
		DefaultValue = builder.DefaultValue;
		Attributes = builder.Attributes.ToImmutableArray();
	}
}
