using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal abstract class DefaultSnowflakeReader<T> : TypeReader<T> where T : class, ISnowflakeEntity
{
	protected abstract Task<T> GetEntity(ulong id, IInteractionContext ctx);

	public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services)
	{
		if (!ulong.TryParse(option, out var result))
		{
			return TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, option + " isn't a valid snowflake thus cannot be converted into " + typeof(T).Name);
		}
		T val = await GetEntity(result, context).ConfigureAwait(continueOnCapturedContext: false);
		return (val != null) ? TypeConverterResult.FromSuccess(val) : TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, option + " must be a valid " + typeof(T).Name + " snowflake to be parsed.");
	}

	public override Task<string> SerializeAsync(object obj, IServiceProvider services)
	{
		return Task.FromResult((obj as ISnowflakeEntity)?.Id.ToString());
	}
}
