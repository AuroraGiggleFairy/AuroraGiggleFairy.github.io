using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal class DefaultValueConverter<T> : TypeConverter<T> where T : IConvertible
{
	public override ApplicationCommandOptionType GetDiscordType()
	{
		switch (Type.GetTypeCode(typeof(T)))
		{
		case TypeCode.Boolean:
			return ApplicationCommandOptionType.Boolean;
		case TypeCode.Char:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Single:
		case TypeCode.DateTime:
		case TypeCode.String:
			return ApplicationCommandOptionType.String;
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
			return ApplicationCommandOptionType.Integer;
		case TypeCode.Double:
		case TypeCode.Decimal:
			return ApplicationCommandOptionType.Number;
		default:
			throw new InvalidOperationException("Parameter Type " + typeof(T).FullName + " is not supported by Discord.");
		}
	}

	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
	{
		object value = ((!(option.Value is Optional<object> optional)) ? option.Value : (optional.IsSpecified ? optional.Value : ((object)default(T))));
		try
		{
			return Task.FromResult(TypeConverterResult.FromSuccess(Convert.ChangeType(value, typeof(T))));
		}
		catch (InvalidCastException exception)
		{
			return Task.FromResult(TypeConverterResult.FromError(exception));
		}
	}
}
