using System;
using System.Threading.Tasks;

namespace Discord;

internal interface IThreadChannel : ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	ThreadType Type { get; }

	bool HasJoined { get; }

	bool IsArchived { get; }

	ThreadArchiveDuration AutoArchiveDuration { get; }

	DateTimeOffset ArchiveTimestamp { get; }

	bool IsLocked { get; }

	int MemberCount { get; }

	int MessageCount { get; }

	bool? IsInvitable { get; }

	new DateTimeOffset CreatedAt { get; }

	Task JoinAsync(RequestOptions options = null);

	Task LeaveAsync(RequestOptions options = null);

	Task AddUserAsync(IGuildUser user, RequestOptions options = null);

	Task RemoveUserAsync(IGuildUser user, RequestOptions options = null);
}
