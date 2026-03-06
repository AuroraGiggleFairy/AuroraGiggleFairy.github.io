using System;

namespace Discord;

internal interface IIntegration
{
	ulong Id { get; }

	string Name { get; }

	string Type { get; }

	bool IsEnabled { get; }

	bool? IsSyncing { get; }

	ulong? RoleId { get; }

	bool? HasEnabledEmoticons { get; }

	IntegrationExpireBehavior? ExpireBehavior { get; }

	int? ExpireGracePeriod { get; }

	IUser User { get; }

	IIntegrationAccount Account { get; }

	DateTimeOffset? SyncedAt { get; }

	int? SubscriberCount { get; }

	bool? IsRevoked { get; }

	IIntegrationApplication Application { get; }

	IGuild Guild { get; }

	ulong GuildId { get; }
}
