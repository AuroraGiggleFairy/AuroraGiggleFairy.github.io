using System;
using System.Collections.Generic;

namespace Discord.Interactions.Builders;

internal interface IInputComponentBuilder
{
	ModalBuilder Modal { get; }

	string CustomId { get; }

	string Label { get; }

	bool IsRequired { get; }

	ComponentType ComponentType { get; }

	Type Type { get; }

	ComponentTypeConverter TypeConverter { get; }

	object DefaultValue { get; }

	IReadOnlyCollection<Attribute> Attributes { get; }

	IInputComponentBuilder WithCustomId(string customId);

	IInputComponentBuilder WithLabel(string label);

	IInputComponentBuilder SetIsRequired(bool isRequired);

	IInputComponentBuilder WithType(Type type);

	IInputComponentBuilder SetDefaultValue(object value);

	IInputComponentBuilder WithAttributes(params Attribute[] attributes);
}
