using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultArrayComponentConverter<T> : ComponentTypeConverter<T>
{
	private readonly TypeReader _typeReader;

	private readonly Type _underlyingType;

	public DefaultArrayComponentConverter(InteractionService interactionService)
	{
		if (!typeof(T).IsArray)
		{
			throw new InvalidOperationException("DefaultArrayComponentConverter cannot be used to convert a non-array type.");
		}
		_underlyingType = typeof(T).GetElementType();
		_typeReader = interactionService.GetTypeReader(_underlyingType);
	}

	public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IComponentInteractionData option, IServiceProvider services)
	{
		List<TypeConverterResult> results = new List<TypeConverterResult>();
		foreach (string value in option.Values)
		{
			TypeConverterResult typeConverterResult = await _typeReader.ReadAsync(context, value, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!typeConverterResult.IsSuccess)
			{
				return typeConverterResult;
			}
			results.Add(typeConverterResult);
		}
		Array array = Array.CreateInstance(_underlyingType, results.Count);
		for (int i = 0; i < results.Count; i++)
		{
			array.SetValue(results[i].Value, i);
		}
		return TypeConverterResult.FromSuccess(array);
	}
}
