using System;
using System.Threading.Tasks;

namespace Discord.Rest;

internal static class ClientExtensions
{
	public static Task AddGuildUserAsync(this BaseDiscordClient client, ulong guildId, ulong userId, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null)
	{
		return GuildHelper.AddGuildUserAsync(guildId, client, userId, accessToken, func, options);
	}
}
