using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestGuildEvent : RestEntity<ulong>, IGuildScheduledEvent, IEntity<ulong>
{
	public IGuild Guild { get; private set; }

	public ulong? ChannelId { get; private set; }

	public IUser Creator { get; private set; }

	public ulong CreatorId { get; private set; }

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

	internal RestGuildEvent(BaseDiscordClient client, IGuild guild, ulong id)
		: base(client, id)
	{
		Guild = guild;
	}

	internal static RestGuildEvent Create(BaseDiscordClient client, IGuild guild, GuildScheduledEvent model)
	{
		RestGuildEvent restGuildEvent = new RestGuildEvent(client, guild, model.Id);
		restGuildEvent.Update(model);
		return restGuildEvent;
	}

	internal static RestGuildEvent Create(BaseDiscordClient client, IGuild guild, IUser creator, GuildScheduledEvent model)
	{
		RestGuildEvent restGuildEvent = new RestGuildEvent(client, guild, model.Id);
		restGuildEvent.Update(model, creator);
		return restGuildEvent;
	}

	internal void Update(GuildScheduledEvent model, IUser creator)
	{
		Update(model);
		Creator = creator;
		CreatorId = creator.Id;
	}

	internal void Update(GuildScheduledEvent model)
	{
		if (model.Creator.IsSpecified)
		{
			Creator = RestUser.Create(base.Discord, model.Creator.Value);
		}
		CreatorId = model.CreatorId.ToNullable().GetValueOrDefault();
		ChannelId = (model.ChannelId.IsSpecified ? model.ChannelId.Value : ((ulong?)null));
		Name = model.Name;
		Description = model.Description.GetValueOrDefault();
		StartTime = model.ScheduledStartTime;
		EndTime = model.ScheduledEndTime;
		PrivacyLevel = model.PrivacyLevel;
		Status = model.Status;
		Type = model.EntityType;
		EntityId = model.EntityId;
		Location = model.EntityMetadata?.Location.GetValueOrDefault();
		UserCount = model.UserCount.ToNullable();
		CoverImageId = model.Image;
	}

	public string GetCoverImageUrl(ImageFormat format = ImageFormat.Auto, ushort size = 1024)
	{
		return CDN.GetEventCoverImageUrl(Guild.Id, base.Id, CoverImageId, format, size);
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

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteEventAsync(base.Discord, this, options);
	}

	public async Task ModifyAsync(Action<GuildScheduledEventsProperties> func, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyGuildEventAsync(base.Discord, func, this, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetUsersAsync(RequestOptions options = null)
	{
		return GuildHelper.GetEventUsersAsync(base.Discord, this, null, null, options);
	}

	public IAsyncEnumerable<IReadOnlyCollection<RestUser>> GetUsersAsync(ulong fromUserId, Direction dir, int limit = 100, RequestOptions options = null)
	{
		return GuildHelper.GetEventUsersAsync(base.Discord, this, fromUserId, dir, limit, options);
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
