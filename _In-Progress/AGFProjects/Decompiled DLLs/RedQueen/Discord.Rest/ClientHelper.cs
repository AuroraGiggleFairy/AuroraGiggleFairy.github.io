using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class ClientHelper
{
	public static async Task<RestApplication> GetApplicationInfoAsync(BaseDiscordClient client, RequestOptions options)
	{
		return RestApplication.Create(client, await client.ApiClient.GetMyApplicationAsync(options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestChannel> GetChannelAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Channel channel = await client.ApiClient.GetChannelAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (channel != null)
		{
			return RestChannel.Create(client, channel);
		}
		return null;
	}

	public static async Task<IReadOnlyCollection<IRestPrivateChannel>> GetPrivateChannelsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetMyPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((Channel x) => RestChannel.CreatePrivate(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestDMChannel>> GetDMChannelsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (from x in await client.ApiClient.GetMyPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false)
			where x.Type == ChannelType.DM
			select RestDMChannel.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestGroupChannel>> GetGroupChannelsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (from x in await client.ApiClient.GetMyPrivateChannelsAsync(options).ConfigureAwait(continueOnCapturedContext: false)
			where x.Type == ChannelType.Group
			select RestGroupChannel.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestConnection>> GetConnectionsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetMyConnectionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((Connection model) => RestConnection.Create(client, model)).ToImmutableArray();
	}

	public static async Task<RestInviteMetadata> GetInviteAsync(BaseDiscordClient client, string inviteId, RequestOptions options)
	{
		InviteMetadata inviteMetadata = await client.ApiClient.GetInviteAsync(inviteId, options).ConfigureAwait(continueOnCapturedContext: false);
		if (inviteMetadata != null)
		{
			return RestInviteMetadata.Create(client, null, null, inviteMetadata);
		}
		return null;
	}

	public static async Task<RestGuild> GetGuildAsync(BaseDiscordClient client, ulong id, bool withCounts, RequestOptions options)
	{
		Guild guild = await client.ApiClient.GetGuildAsync(id, withCounts, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guild != null)
		{
			return RestGuild.Create(client, guild);
		}
		return null;
	}

	public static async Task<RestGuildWidget?> GetGuildWidgetAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		GuildWidget guildWidget = await client.ApiClient.GetGuildWidgetAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildWidget != null)
		{
			return RestGuildWidget.Create(guildWidget);
		}
		return null;
	}

	public static IAsyncEnumerable<IReadOnlyCollection<RestUserGuild>> GetGuildSummariesAsync(BaseDiscordClient client, ulong? fromGuildId, int? limit, RequestOptions options)
	{
		return new PagedAsyncEnumerable<RestUserGuild>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetGuildSummariesParams getGuildSummariesParams = new GetGuildSummariesParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getGuildSummariesParams.AfterGuildId = info.Position.Value;
			}
			return (await client.ApiClient.GetMyGuildsAsync(getGuildSummariesParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((UserGuild x) => RestUserGuild.Create(client, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<RestUserGuild> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((RestUserGuild x) => x.Id);
			return true;
		}, fromGuildId, limit);
	}

	public static async Task<IReadOnlyCollection<RestGuild>> GetGuildsAsync(BaseDiscordClient client, bool withCounts, RequestOptions options)
	{
		IEnumerable<RestUserGuild> enumerable = await GetGuildSummariesAsync(client, null, null, options).FlattenAsync().ConfigureAwait(continueOnCapturedContext: false);
		System.Collections.Immutable.ImmutableArray<RestGuild>.Builder guilds = System.Collections.Immutable.ImmutableArray.CreateBuilder<RestGuild>();
		foreach (RestUserGuild item in enumerable)
		{
			Guild guild = await client.ApiClient.GetGuildAsync(item.Id, withCounts).ConfigureAwait(continueOnCapturedContext: false);
			if (guild != null)
			{
				guilds.Add(RestGuild.Create(client, guild));
			}
		}
		return guilds.ToImmutable();
	}

	public static async Task<RestGuild> CreateGuildAsync(BaseDiscordClient client, string name, IVoiceRegion region, Stream jpegIcon, RequestOptions options)
	{
		CreateGuildParams createGuildParams = new CreateGuildParams(name, region.Id);
		if (jpegIcon != null)
		{
			createGuildParams.Icon = new Discord.API.Image(jpegIcon);
		}
		return RestGuild.Create(client, await client.ApiClient.CreateGuildAsync(createGuildParams, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<RestUser> GetUserAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		User user = await client.ApiClient.GetUserAsync(id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (user != null)
		{
			return RestUser.Create(client, user);
		}
		return null;
	}

	public static async Task<RestGuildUser> GetGuildUserAsync(BaseDiscordClient client, ulong guildId, ulong id, RequestOptions options)
	{
		RestGuild guild = await GetGuildAsync(client, guildId, withCounts: false, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guild == null)
		{
			return null;
		}
		GuildMember guildMember = await client.ApiClient.GetGuildMemberAsync(guildId, id, options).ConfigureAwait(continueOnCapturedContext: false);
		if (guildMember != null)
		{
			return RestGuildUser.Create(client, guild, guildMember);
		}
		return null;
	}

	public static async Task<RestWebhook> GetWebhookAsync(BaseDiscordClient client, ulong id, RequestOptions options)
	{
		Webhook webhook = await client.ApiClient.GetWebhookAsync(id).ConfigureAwait(continueOnCapturedContext: false);
		if (webhook != null)
		{
			return RestWebhook.Create(client, (IGuild)null, webhook);
		}
		return null;
	}

	public static async Task<IReadOnlyCollection<RestVoiceRegion>> GetVoiceRegionsAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(client, x)).ToImmutableArray();
	}

	public static async Task<RestVoiceRegion> GetVoiceRegionAsync(BaseDiscordClient client, string id, RequestOptions options)
	{
		return (await client.ApiClient.GetVoiceRegionsAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Select((VoiceRegion x) => RestVoiceRegion.Create(client, x)).FirstOrDefault((RestVoiceRegion x) => x.Id == id);
	}

	public static async Task<int> GetRecommendShardCountAsync(BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetBotGatewayAsync(options).ConfigureAwait(continueOnCapturedContext: false)).Shards;
	}

	public static async Task<BotGateway> GetBotGatewayAsync(BaseDiscordClient client, RequestOptions options)
	{
		GetBotGatewayResponse getBotGatewayResponse = await client.ApiClient.GetBotGatewayAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		return new BotGateway
		{
			Url = getBotGatewayResponse.Url,
			Shards = getBotGatewayResponse.Shards,
			SessionStartLimit = new SessionStartLimit
			{
				Total = getBotGatewayResponse.SessionStartLimit.Total,
				Remaining = getBotGatewayResponse.SessionStartLimit.Remaining,
				ResetAfter = getBotGatewayResponse.SessionStartLimit.ResetAfter,
				MaxConcurrency = getBotGatewayResponse.SessionStartLimit.MaxConcurrency
			}
		};
	}

	public static async Task<IReadOnlyCollection<RestGlobalCommand>> GetGlobalApplicationCommandsAsync(BaseDiscordClient client, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		ApplicationCommand[] source = await client.ApiClient.GetGlobalApplicationCommandsAsync(withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
		if (!source.Any())
		{
			return Array.Empty<RestGlobalCommand>();
		}
		return source.Select((ApplicationCommand x) => RestGlobalCommand.Create(client, x)).ToArray();
	}

	public static async Task<RestGlobalCommand> GetGlobalApplicationCommandAsync(BaseDiscordClient client, ulong id, RequestOptions options = null)
	{
		ApplicationCommand applicationCommand = await client.ApiClient.GetGlobalApplicationCommandAsync(id, options);
		return (applicationCommand != null) ? RestGlobalCommand.Create(client, applicationCommand) : null;
	}

	public static async Task<IReadOnlyCollection<RestGuildCommand>> GetGuildApplicationCommandsAsync(BaseDiscordClient client, ulong guildId, bool withLocalizations = false, string locale = null, RequestOptions options = null)
	{
		ApplicationCommand[] source = await client.ApiClient.GetGuildApplicationCommandsAsync(guildId, withLocalizations, locale, options).ConfigureAwait(continueOnCapturedContext: false);
		if (!source.Any())
		{
			return System.Collections.Immutable.ImmutableArray.Create<RestGuildCommand>();
		}
		return source.Select((ApplicationCommand x) => RestGuildCommand.Create(client, x, guildId)).ToImmutableArray();
	}

	public static async Task<RestGuildCommand> GetGuildApplicationCommandAsync(BaseDiscordClient client, ulong id, ulong guildId, RequestOptions options = null)
	{
		ApplicationCommand applicationCommand = await client.ApiClient.GetGuildApplicationCommandAsync(guildId, id, options);
		return (applicationCommand != null) ? RestGuildCommand.Create(client, applicationCommand, guildId) : null;
	}

	public static async Task<RestGuildCommand> CreateGuildApplicationCommandAsync(BaseDiscordClient client, ulong guildId, ApplicationCommandProperties properties, RequestOptions options = null)
	{
		return RestGuildCommand.Create(client, await InteractionHelper.CreateGuildCommandAsync(client, guildId, properties, options), guildId);
	}

	public static async Task<RestGlobalCommand> CreateGlobalApplicationCommandAsync(BaseDiscordClient client, ApplicationCommandProperties properties, RequestOptions options = null)
	{
		return RestGlobalCommand.Create(client, await InteractionHelper.CreateGlobalCommandAsync(client, properties, options));
	}

	public static async Task<IReadOnlyCollection<RestGlobalCommand>> BulkOverwriteGlobalApplicationCommandAsync(BaseDiscordClient client, ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		return (await InteractionHelper.BulkOverwriteGlobalCommandsAsync(client, properties, options)).Select((ApplicationCommand x) => RestGlobalCommand.Create(client, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestGuildCommand>> BulkOverwriteGuildApplicationCommandAsync(BaseDiscordClient client, ulong guildId, ApplicationCommandProperties[] properties, RequestOptions options = null)
	{
		return (await InteractionHelper.BulkOverwriteGuildCommandsAsync(client, guildId, properties, options)).Select((ApplicationCommand x) => RestGuildCommand.Create(client, x, guildId)).ToImmutableArray();
	}

	public static Task AddRoleAsync(BaseDiscordClient client, ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		return client.ApiClient.AddRoleAsync(guildId, userId, roleId, options);
	}

	public static Task RemoveRoleAsync(BaseDiscordClient client, ulong guildId, ulong userId, ulong roleId, RequestOptions options = null)
	{
		return client.ApiClient.RemoveRoleAsync(guildId, userId, roleId, options);
	}
}
