using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal abstract class DefaultEntityTypeConverter<T> : TypeConverter<T> where T : class
{
	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
	{
		if (option.Value as T != null)
		{
			return Task.FromResult(TypeConverterResult.FromSuccess(option.Value as T));
		}
		return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Provided input cannot be read as IChannel"));
	}
}
