using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class ThreadCreateAuditLogData : IAuditLogData
{
	public IThreadChannel Thread { get; }

	public ulong ThreadId { get; }

	public string ThreadName { get; }

	public ThreadType ThreadType { get; }

	public bool IsArchived { get; }

	public ThreadArchiveDuration AutoArchiveDuration { get; }

	public bool IsLocked { get; }

	public int? SlowModeInterval { get; }

	private ThreadCreateAuditLogData(IThreadChannel thread, ulong id, string name, ThreadType type, bool archived, ThreadArchiveDuration autoArchiveDuration, bool locked, int? rateLimit)
	{
		Thread = thread;
		ThreadId = id;
		ThreadName = name;
		ThreadType = type;
		IsArchived = archived;
		AutoArchiveDuration = autoArchiveDuration;
		IsLocked = locked;
		SlowModeInterval = rateLimit;
	}

	internal static ThreadCreateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		ulong id = entry.TargetId.Value;
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange2 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "type");
		AuditLogChange auditLogChange3 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "archived");
		AuditLogChange auditLogChange4 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "auto_archive_duration");
		AuditLogChange auditLogChange5 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "locked");
		AuditLogChange auditLogChange6 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "rate_limit_per_user");
		string name = auditLogChange.NewValue.ToObject<string>(discord.ApiClient.Serializer);
		ThreadType type = auditLogChange2.NewValue.ToObject<ThreadType>(discord.ApiClient.Serializer);
		bool archived = auditLogChange3.NewValue.ToObject<bool>(discord.ApiClient.Serializer);
		ThreadArchiveDuration autoArchiveDuration = auditLogChange4.NewValue.ToObject<ThreadArchiveDuration>(discord.ApiClient.Serializer);
		bool locked = auditLogChange5.NewValue.ToObject<bool>(discord.ApiClient.Serializer);
		int? rateLimit = auditLogChange6?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		Channel channel = log.Threads.FirstOrDefault((Channel x) => x.Id == id);
		return new ThreadCreateAuditLogData((channel == null) ? null : RestThreadChannel.Create(discord, null, channel), id, name, type, archived, autoArchiveDuration, locked, rateLimit);
	}
}
