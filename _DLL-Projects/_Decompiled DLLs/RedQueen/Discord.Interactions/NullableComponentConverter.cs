using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal class NullableComponentConverter<T> : ComponentTypeConverter<T>
{
	private readonly ComponentTypeConverter _typeConverter;

	public NullableComponentConverter(InteractionService interactionService, IServiceProvider services)
	{
		Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
		if ((object)underlyingType == null)
		{
			throw new ArgumentException("No type TypeConverter is defined for this " + underlyingType.FullName, "type");
		}
		_typeConverter = interactionService.GetComponentTypeConverter(underlyingType, services);
	}

	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IComponentInteractionData option, IServiceProvider services)
	{
		if (!string.IsNullOrEmpty(option.Value))
		{
			return _typeConverter.ReadAsync(context, option, services);
		}
		return Task.FromResult(TypeConverterResult.FromSuccess(null));
	}
}
