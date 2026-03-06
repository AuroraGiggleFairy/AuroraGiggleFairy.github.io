using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal class NullableConverter<T> : TypeConverter<T>
{
	private readonly TypeConverter _typeConverter;

	public NullableConverter(InteractionService interactionService, IServiceProvider services)
	{
		Type typeFromHandle = typeof(T);
		Type underlyingType = Nullable.GetUnderlyingType(typeFromHandle);
		if ((object)underlyingType == null)
		{
			throw new ArgumentException("No type TypeConverter is defined for this " + typeFromHandle.FullName, "type");
		}
		_typeConverter = interactionService.GetTypeConverter(underlyingType, services);
	}

	public override ApplicationCommandOptionType GetDiscordType()
	{
		return _typeConverter.GetDiscordType();
	}

	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
	{
		return _typeConverter.ReadAsync(context, option, services);
	}

	public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
	{
		_typeConverter.Write(properties, parameter);
	}
}
