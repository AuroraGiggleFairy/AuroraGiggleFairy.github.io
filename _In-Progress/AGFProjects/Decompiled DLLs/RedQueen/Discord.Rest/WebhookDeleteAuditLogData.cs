using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class WebhookDeleteAuditLogData : IAuditLogData
{
	public ulong WebhookId { get; }

	public ulong ChannelId { get; }

	public WebhookType Type { get; }

	public string Name { get; }

	public string Avatar { get; }

	private WebhookDeleteAuditLogData(ulong id, ulong channel, WebhookType type, string name, string avatar)
	{
		WebhookId = id;
		ChannelId = channel;
		Name = name;
		Type = type;
		Avatar = avatar;
	}

	internal static WebhookDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "type");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "avatar_hash");
		ulong channel = auditLogChange.OldValue.ToObject<ulong>(discord.ApiClient.Serializer);
		WebhookType type = auditLogChange2.OldValue.ToObject<WebhookType>(discord.ApiClient.Serializer);
		string name = auditLogChange3.OldValue.ToObject<string>(discord.ApiClient.Serializer);
		string avatar = auditLogChange4?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new WebhookDeleteAuditLogData(entry.TargetId.Value, channel, type, name, avatar);
	}
}
