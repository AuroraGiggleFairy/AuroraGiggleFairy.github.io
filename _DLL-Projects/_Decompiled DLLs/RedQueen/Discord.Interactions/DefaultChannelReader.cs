using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultChannelReader<T> : DefaultSnowflakeReader<T> where T : class, IChannel
{
	protected override async Task<T> GetEntity(ulong id, IInteractionContext ctx)
	{
		return (await ctx.Client.GetChannelAsync(id, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T;
	}
}
