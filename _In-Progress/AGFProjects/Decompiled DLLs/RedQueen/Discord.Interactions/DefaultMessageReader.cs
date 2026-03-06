using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultMessageReader<T> : DefaultSnowflakeReader<T> where T : class, IMessage
{
	protected override async Task<T> GetEntity(ulong id, IInteractionContext ctx)
	{
		return (await ctx.Channel.GetMessageAsync(id, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T;
	}
}
