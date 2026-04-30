using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class ThreadUpdateAuditLogData : IAuditLogData
{
	public IThreadChannel Thread { get; }

	public ThreadType ThreadType { get; }

	public ThreadInfo Before { get; }

	public ThreadInfo After { get; }

	private ThreadUpdateAuditLogData(IThreadChannel thread, ThreadType type, ThreadInfo before, ThreadInfo after)
	{
		Thread = thread;
		ThreadType = type;
		Before = before;
		After = after;
	}

	internal static ThreadUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		ulong id = entry.TargetId.Value;
		AuditLogChange auditLogChange = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange2 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "type");
		AuditLogChange auditLogChange3 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "archived");
		AuditLogChange auditLogChange4 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "auto_archive_duration");
		AuditLogChange auditLogChange5 = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "locked");
		AuditLogChange auditLogChange6 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "rate_limit_per_user");
		ThreadType type = auditLogChange2.OldValue.ToObject<ThreadType>(discord.ApiClient.Serializer);
		string name = auditLogChange.OldValue.ToObject<string>(discord.ApiClient.Serializer);
		bool archived = auditLogChange3.OldValue.ToObject<bool>(discord.ApiClient.Serializer);
		ThreadArchiveDuration autoArchiveDuration = auditLogChange4.OldValue.ToObject<ThreadArchiveDuration>(discord.ApiClient.Serializer);
		bool locked = auditLogChange5.OldValue.ToObject<bool>(discord.ApiClient.Serializer);
		int? rateLimit = auditLogChange6?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		ThreadInfo before = new ThreadInfo(name, archived, autoArchiveDuration, locked, rateLimit);
		string name2 = auditLogChange.NewValue.ToObject<string>(discord.ApiClient.Serializer);
		bool archived2 = auditLogChange3.NewValue.ToObject<bool>(discord.ApiClient.Serializer);
		ThreadArchiveDuration autoArchiveDuration2 = auditLogChange4.NewValue.ToObject<ThreadArchiveDuration>(discord.ApiClient.Serializer);
		bool locked2 = auditLogChange5.NewValue.ToObject<bool>(discord.ApiClient.Serializer);
		int? rateLimit2 = auditLogChange6?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		ThreadInfo after = new ThreadInfo(name2, archived2, autoArchiveDuration2, locked2, rateLimit2);
		Channel channel = log.Threads.FirstOrDefault((Channel x) => x.Id == id);
		return new ThreadUpdateAuditLogData((channel == null) ? null : RestThreadChannel.Create(discord, null, channel), type, before, after);
	}
}
