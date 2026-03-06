using System.Collections.Generic;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class ChannelDeleteAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public string ChannelName { get; }

	public ChannelType ChannelType { get; }

	public int? SlowModeInterval { get; }

	public bool? IsNsfw { get; }

	public int? Bitrate { get; }

	public IReadOnlyCollection<Overwrite> Overwrites { get; }

	private ChannelDeleteAuditLogData(ulong id, string name, ChannelType type, int? rateLimit, bool? nsfw, int? bitrate, IReadOnlyCollection<Overwrite> overwrites)
	{
		ChannelId = id;
		ChannelName = name;
		ChannelType = type;
		SlowModeInterval = rateLimit;
		IsNsfw = nsfw;
		Bitrate = bitrate;
		Overwrites = overwrites;
	}

	internal static ChannelDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "permission_overwrites");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "type");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "rate_limit_per_user");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "nsfw");
		AuditLogChange auditLogChange6 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "bitrate");
		List<Overwrite> source = (from x in auditLogChange.OldValue.ToObject<Discord.API.Overwrite[]>(discord.ApiClient.Serializer)
			select new Overwrite(x.TargetId, x.TargetType, new OverwritePermissions(x.Allow, x.Deny))).ToList();
		ChannelType type = auditLogChange2.OldValue.ToObject<ChannelType>(discord.ApiClient.Serializer);
		string name = auditLogChange3.OldValue.ToObject<string>(discord.ApiClient.Serializer);
		int? rateLimit = auditLogChange4?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		bool? nsfw = auditLogChange5?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		int? bitrate = auditLogChange6?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		return new ChannelDeleteAuditLogData(entry.TargetId.Value, name, type, rateLimit, nsfw, bitrate, source.ToReadOnlyCollection());
	}
}
