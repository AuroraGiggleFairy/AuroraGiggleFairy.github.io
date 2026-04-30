using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

internal class SocketGuildEvent : SocketEntity<ulong>, IGuildScheduledEvent, IEntity<ulong>
{
	public SocketGuild Guild { get; private set; }

	public SocketGuildChannel Channel { get; private set; }

	public SocketGuildUser Creator { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public string CoverImageId { get; private set; }

	public DateTimeOffset StartTime { get; private set; }

	public DateTimeOffset? EndTime { get; private set; }

	public GuildScheduledEventPrivacyLevel PrivacyLevel { get; private set; }

	public GuildScheduledEventStatus Status { get; private set; }

	public GuildScheduledEventType Type { get; private set; }

	public ulong? EntityId { get; private set; }

	public string Location { get; private set; }

	public int? UserCount { get; private set; }

	IGuild IGuildScheduledEvent.Guild => Guild;

	IUser IGuildScheduledEvent.Creator => Creator;

	ulong? IGuildScheduledEvent.ChannelId => Channel?.Id;

	internal SocketGuildEvent(DiscordSocketClient client, SocketGuild guild, ulong id)
		: base(client, id)
	{
		Guild = guild;
	}

	internal static SocketGuildEvent Create(DiscordSocketClient client, SocketGuild guild, GuildScheduledEvent model)
	{
		SocketGuildEvent socketGuildEvent = new SocketGuildEvent(client, guild, model.Id);
		socketGuildEvent.Update(model);
		return socketGuildEvent;
	}

	internal void Update(GuildScheduledEvent model)
	{
		if (model.ChannelId.IsSpecified && model.ChannelId.Value.HasValue)
		{
			Channel = Guild.GetChannel(model.ChannelId.Value.Value);
		}
		if (model.CreatorId.IsSpecified)
		{
			SocketGuildUser user = Guild.GetUser(model.CreatorId.Value);
			if (user != null)
			{
				if (model.Creator.IsSpecified)
				{
					user.Update(base.Discord.State, model.Creator.Value);
				}
				Creator = user;
			}
			else if (user == null && model.Creator.IsSpecified)
			{
				user = SocketGuildUser.Create(Guild, base.Discord.State, model.Creator.Value);
				Creator = user;
			}
		}
		Name = model.Name;
		Description = model.Description.GetValueOrDefault();
		EntityId = model.EntityId;
		Location = model.EntityMetadata?.Location.GetValueOrDefault();
		Type = model.EntityType;
		PrivacyLevel = model.PrivacyLevel;
		EndTime = model.ScheduledEndTime;
		StartTime = model.ScheduledStartTime;
		Status = model.Status;
		UserCount = model.UserCount.ToNullable();
		CoverImageId = model.Image;
	}

	public string GetCoverImageUrl(ImageFormat format = ImageFormat.Auto, ushort size = 1024)
	{
		return CDN.GetEventCoverImageUrl(Guild.Id, base.Id, CoverImageId, format, size);
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteEventAsync(base.Discord, this, options);
	}

	public Task StartAsync(RequestOptions options = null)
	{
		return ModifyAsync(delegate(GuildScheduledEventsProperties x)
		{
			x.Status = GuildScheduledEventStatus.Active;
		});
	}

	public Task EndAsync(RequestOptions options = null)
	{
		return ModifyAsync(delegate(GuildScheduledEventsProperties x)
		{
			x.Status = ((Status == GuildScheduledEventStatus.Scheduled) ? GuildScheduledEventStatus.Cancelled : GuildScheduledEventStatus.Completed);
		});
	}

	public async Task ModifyAsync(Action<GuildScheduledEventsProperties> func, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyGuildEventAsync(base.Discord, func, this, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public Task<IReadOnlyCollection<RestUser>> GetUsersAsync(int limit = 100, RequestOptions options = null)
	{
		return GuildHelper.GetEventUsersAsync(base.Discord, this, limit, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetUsersAsync(RequestOptions options = null)
	{
		return GuildHelper.GetEventUsersAsync(base.Discord, this, null, null, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetUsersAsync(ulong fromUserId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return GuildHelper.GetEventUsersAsync(base.Discord, this, fromUserId, dir, limit, options);
	}

	internal SocketGuildEvent Clone()
	{
		return MemberwiseClone() as SocketGuildEvent;
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IGuildScheduledEvent.GetUsersAsync(RequestOptions options)
	{
		return GetUsersAsync(options);
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IGuildScheduledEvent.GetUsersAsync(ulong fromUserId, Direction dir, int limit, RequestOptions options)
	{
		return GetUsersAsync(fromUserId, dir, limit, options);
	}
}
