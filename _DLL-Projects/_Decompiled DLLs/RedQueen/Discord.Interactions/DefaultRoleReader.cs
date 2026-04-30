using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultRoleReader<T> : DefaultSnowflakeReader<T> where T : class, IRole
{
	protected override Task<T> GetEntity(ulong id, IInteractionContext ctx)
	{
		return Task.FromResult(ctx.Guild?.GetRole(id) as T);
	}
}
