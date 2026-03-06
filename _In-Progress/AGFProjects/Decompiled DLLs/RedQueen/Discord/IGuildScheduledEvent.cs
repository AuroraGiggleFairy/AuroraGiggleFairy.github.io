using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal interface IGuildScheduledEvent : IEntity<ulong>
{
	IGuild Guild { get; }

	ulong? ChannelId { get; }

	IUser Creator { get; }

	string Name { get; }

	string Description { get; }

	string CoverImageId { get; }

	DateTimeOffset StartTime { get; }

	DateTimeOffset? EndTime { get; }

	GuildScheduledEventPrivacyLevel PrivacyLevel { get; }

	GuildScheduledEventStatus Status { get; }

	GuildScheduledEventType Type { get; }

	ulong? EntityId { get; }

	string Location { get; }

	int? UserCount { get; }

	string GetCoverImageUrl(ImageFormat format = ImageFormat.Auto, ushort size = 1024);

	Task StartAsync(RequestOptions options = null);

	Task EndAsync(RequestOptions options = null);

	Task ModifyAsync(Action<GuildScheduledEventsProperties> func, RequestOptions options = null);

	Task DeleteAsync(RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(RequestOptions options = null);

	IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(ulong fromUserId, Direction dir, int limit = 100, RequestOptions options = null);
}
