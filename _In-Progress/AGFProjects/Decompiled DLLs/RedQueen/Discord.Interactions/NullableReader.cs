using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal class NullableReader<T> : TypeReader<T>
{
	private readonly TypeReader _typeReader;

	public NullableReader(InteractionService interactionService, IServiceProvider services)
	{
		Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
		if ((object)underlyingType == null)
		{
			throw new ArgumentException("No type TypeConverter is defined for this " + underlyingType.FullName, "type");
		}
		_typeReader = interactionService.GetTypeReader(underlyingType, services);
	}

	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services)
	{
		if (!string.IsNullOrEmpty(option))
		{
			return _typeReader.ReadAsync(context, option, services);
		}
		return Task.FromResult(TypeConverterResult.FromSuccess(null));
	}
}
