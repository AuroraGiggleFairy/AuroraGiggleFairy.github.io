using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class ThreadHelper
{
	public static async Task<Channel> CreateThreadAsync(BaseDiscordClient client, ITextChannel channel, string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		GuildFeatures features = channel.Guild.Features;
		if (autoArchiveDuration == ThreadArchiveDuration.OneWeek && !features.HasFeature(GuildFeature.SevenDayThreadArchive))
		{
			throw new ArgumentException("The guild " + channel.Guild.Name + " does not have the SEVEN_DAY_THREAD_ARCHIVE feature!", "autoArchiveDuration");
		}
		if (autoArchiveDuration == ThreadArchiveDuration.ThreeDays && !features.HasFeature(GuildFeature.ThreeDayThreadArchive))
		{
			throw new ArgumentException("The guild " + channel.Guild.Name + " does not have the THREE_DAY_THREAD_ARCHIVE feature!", "autoArchiveDuration");
		}
		if (type == ThreadType.PrivateThread && !features.HasFeature(GuildFeature.PrivateThreads))
		{
			throw new ArgumentException("The guild " + channel.Guild.Name + " does not have the PRIVATE_THREADS feature!", "type");
		}
		if (channel is INewsChannel && type != ThreadType.NewsThread)
		{
			throw new ArgumentException(string.Format("{0} must be a {1} in News channels", "type", ThreadType.NewsThread));
		}
		StartThreadParams args = new StartThreadParams
		{
			Name = name,
			Duration = autoArchiveDuration,
			Type = type,
			Invitable = (invitable.HasValue ? ((Optional<bool>)invitable.Value) : Optional<bool>.Unspecified),
			Ratelimit = (slowmode.HasValue ? ((Optional<int?>)slowmode.Value) : Optional<int?>.Unspecified)
		};
		return (message == null) ? (await client.ApiClient.StartThreadAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false)) : (await client.ApiClient.StartThreadAsync(channel.Id, message.Id, args, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public static async Task<Channel> ModifyAsync(IThreadChannel channel, BaseDiscordClient client, Action<TextChannelProperties> func, RequestOptions options)
	{
		TextChannelProperties textChannelProperties = new TextChannelProperties();
		func(textChannelProperties);
		ModifyThreadParams args = new ModifyThreadParams
		{
			Name = textChannelProperties.Name,
			Archived = textChannelProperties.Archived,
			AutoArchiveDuration = textChannelProperties.AutoArchiveDuration,
			Locked = textChannelProperties.Locked,
			Slowmode = textChannelProperties.SlowModeInterval
		};
		return await client.ApiClient.ModifyThreadAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetActiveThreadsAsync(IGuild guild, BaseDiscordClient client, RequestOptions options)
	{
		return (await client.ApiClient.GetActiveThreadsAsync(guild.Id, options).ConfigureAwait(continueOnCapturedContext: false)).Threads.Select((Channel x) => RestThreadChannel.Create(client, guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetPublicArchivedThreadsAsync(IGuildChannel channel, BaseDiscordClient client, int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return (await client.ApiClient.GetPublicArchivedThreadsAsync(channel.Id, before, limit, options)).Threads.Select((Channel x) => RestThreadChannel.Create(client, channel.Guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetPrivateArchivedThreadsAsync(IGuildChannel channel, BaseDiscordClient client, int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return (await client.ApiClient.GetPrivateArchivedThreadsAsync(channel.Id, before, limit, options)).Threads.Select((Channel x) => RestThreadChannel.Create(client, channel.Guild, x)).ToImmutableArray();
	}

	public static async Task<IReadOnlyCollection<RestThreadChannel>> GetJoinedPrivateArchivedThreadsAsync(IGuildChannel channel, BaseDiscordClient client, int? limit = null, DateTimeOffset? before = null, RequestOptions options = null)
	{
		return (await client.ApiClient.GetJoinedPrivateArchivedThreadsAsync(channel.Id, before, limit, options)).Threads.Select((Channel x) => RestThreadChannel.Create(client, channel.Guild, x)).ToImmutableArray();
	}

	public static async Task<RestThreadUser[]> GetUsersAsync(IThreadChannel channel, BaseDiscordClient client, RequestOptions options = null)
	{
		return (await client.ApiClient.ListThreadMembersAsync(channel.Id, options)).Select((ThreadMember x) => RestThreadUser.Create(client, channel.Guild, x, channel)).ToArray();
	}

	public static async Task<RestThreadUser> GetUserAsync(ulong userId, IThreadChannel channel, BaseDiscordClient client, RequestOptions options = null)
	{
		ThreadMember model = await client.ApiClient.GetThreadMemberAsync(channel.Id, userId, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestThreadUser.Create(client, channel.Guild, model, channel);
	}

	public static async Task<RestThreadChannel> CreatePostAsync(IForumChannel channel, BaseDiscordClient client, string title, ThreadArchiveDuration archiveDuration = ThreadArchiveDuration.OneDay, int? slowmode = null, string text = null, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None)
	{
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		if (stickers != null)
		{
			Preconditions.AtMost(stickers.Length, 3, "stickers", "A max of 3 stickers are allowed.");
		}
		if (flags != MessageFlags.None && flags != MessageFlags.SuppressEmbeds)
		{
			throw new ArgumentException("The only valid MessageFlags are SuppressEmbeds and none.", "flags");
		}
		CreatePostParams args = new CreatePostParams
		{
			Title = title,
			ArchiveDuration = archiveDuration,
			Slowmode = slowmode,
			Message = new ForumThreadMessage
			{
				AllowedMentions = allowedMentions.ToModel(),
				Content = text,
				Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified),
				Flags = flags,
				Components = ((components?.Components?.Any() == true) ? ((Optional<Discord.API.ActionRowComponent[]>)components.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray()) : Optional<Discord.API.ActionRowComponent[]>.Unspecified),
				Stickers = ((stickers?.Any() ?? false) ? ((Optional<ulong[]>)stickers.Select((ISticker x) => x.Id).ToArray()) : Optional<ulong[]>.Unspecified)
			}
		};
		Channel model = await client.ApiClient.CreatePostAsync(channel.Id, args, options).ConfigureAwait(continueOnCapturedContext: false);
		return RestThreadChannel.Create(client, channel.Guild, model);
	}

	public static async Task<RestThreadChannel> CreatePostAsync(IForumChannel channel, BaseDiscordClient client, string title, IEnumerable<FileAttachment> attachments, ThreadArchiveDuration archiveDuration, int? slowmode, string text, Embed embed, RequestOptions options, AllowedMentions allowedMentions, MessageComponent components, ISticker[] stickers, Embed[] embeds, MessageFlags flags)
	{
		if (embeds == null)
		{
			embeds = Array.Empty<Embed>();
		}
		if (embed != null)
		{
			embeds = new Embed[1] { embed }.Concat(embeds).ToArray();
		}
		Preconditions.AtMost((allowedMentions?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
		Preconditions.AtMost((allowedMentions?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
		Preconditions.AtMost(embeds.Length, 10, "embeds", "A max of 10 embeds are allowed.");
		if (allowedMentions != null && allowedMentions.AllowedTypes.HasValue)
		{
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && allowedMentions.UserIds != null && allowedMentions.UserIds.Count > 0)
			{
				throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
			}
			if (allowedMentions.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && allowedMentions.RoleIds != null && allowedMentions.RoleIds.Count > 0)
			{
				throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
			}
		}
		if (stickers != null)
		{
			Preconditions.AtMost(stickers.Length, 3, "stickers", "A max of 3 stickers are allowed.");
		}
		if (flags != MessageFlags.None && flags != MessageFlags.SuppressEmbeds)
		{
			throw new ArgumentException("The only valid MessageFlags are SuppressEmbeds and none.", "flags");
		}
		CreateMultipartPostAsync args = new CreateMultipartPostAsync(attachments.ToArray())
		{
			AllowedMentions = allowedMentions.ToModel(),
			ArchiveDuration = archiveDuration,
			Content = text,
			Embeds = (embeds.Any() ? ((Optional<Discord.API.Embed[]>)embeds.Select((Embed x) => x.ToModel()).ToArray()) : Optional<Discord.API.Embed[]>.Unspecified),
			Flags = flags,
			MessageComponent = ((components?.Components?.Any() == true) ? ((Optional<Discord.API.ActionRowComponent[]>)components.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray()) : Optional<Discord.API.ActionRowComponent[]>.Unspecified),
			Slowmode = slowmode,
			Stickers = ((stickers?.Any() ?? false) ? ((Optional<ulong[]>)stickers.Select((ISticker x) => x.Id).ToArray()) : Optional<ulong[]>.Unspecified),
			Title = title
		};
		Channel model = await client.ApiClient.CreatePostAsync(channel.Id, args, options);
		return RestThreadChannel.Create(client, channel.Guild, model);
	}
}
