using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal abstract class TypeConverter : ITypeConverter<IApplicationCommandInteractionDataOption>
{
	public abstract bool CanConvertTo(Type type);

	public abstract ApplicationCommandOptionType GetDiscordType();

	public abstract Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services);

	public virtual void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameter)
	{
	}
}
internal abstract class TypeConverter<T> : TypeConverter
{
	public sealed override bool CanConvertTo(Type type)
	{
		return typeof(T).IsAssignableFrom(type);
	}
}
