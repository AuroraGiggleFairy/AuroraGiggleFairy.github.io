using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultUserReader<T> : DefaultSnowflakeReader<T> where T : class, IUser
{
	protected override async Task<T> GetEntity(ulong id, IInteractionContext ctx)
	{
		return (await ctx.Client.GetUserAsync(id, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T;
	}
}
