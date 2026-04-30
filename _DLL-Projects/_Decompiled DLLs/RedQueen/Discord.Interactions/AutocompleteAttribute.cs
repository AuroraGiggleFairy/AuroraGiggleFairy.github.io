using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class AutocompleteAttribute : Attribute
{
	public Type AutocompleteHandlerType { get; }

	public AutocompleteAttribute(Type autocompleteHandlerType)
	{
		if (!typeof(IAutocompleteHandler).IsAssignableFrom(autocompleteHandlerType))
		{
			throw new InvalidOperationException(autocompleteHandlerType.FullName + " isn't a valid IAutocompleteHandler type");
		}
		AutocompleteHandlerType = autocompleteHandlerType;
	}

	public AutocompleteAttribute()
	{
	}
}
