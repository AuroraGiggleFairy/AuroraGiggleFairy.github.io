using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord;

internal static class DiscordClientExtensions
{
	public static async Task<IPrivateChannel> GetPrivateChannelAsync(this IDiscordClient client, ulong id)
	{
		return (await client.GetChannelAsync(id).ConfigureAwait(continueOnCapturedContext: false)) as IPrivateChannel;
	}

	public static async Task<IDMChannel> GetDMChannelAsync(this IDiscordClient client, ulong id)
	{
		return (await client.GetPrivateChannelAsync(id).ConfigureAwait(continueOnCapturedContext: false)) as IDMChannel;
	}

	public static async Task<IEnumerable<IDMChannel>> GetDMChannelsAsync(this IDiscordClient client)
	{
		return (await client.GetPrivateChannelsAsync().ConfigureAwait(continueOnCapturedContext: false)).OfType<IDMChannel>();
	}

	public static async Task<IGroupChannel> GetGroupChannelAsync(this IDiscordClient client, ulong id)
	{
		return (await client.GetPrivateChannelAsync(id).ConfigureAwait(continueOnCapturedContext: false)) as IGroupChannel;
	}

	public static async Task<IEnumerable<IGroupChannel>> GetGroupChannelsAsync(this IDiscordClient client)
	{
		return (await client.GetPrivateChannelsAsync().ConfigureAwait(continueOnCapturedContext: false)).OfType<IGroupChannel>();
	}

	public static async Task<IVoiceRegion> GetOptimalVoiceRegionAsync(this IDiscordClient discord)
	{
		return (await discord.GetVoiceRegionsAsync().ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault((IVoiceRegion x) => x.IsOptimal);
	}
}
