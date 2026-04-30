using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class WebhookUpdateAuditLogData : IAuditLogData
{
	public IWebhook Webhook { get; }

	public WebhookInfo Before { get; }

	public WebhookInfo After { get; }

	private WebhookUpdateAuditLogData(IWebhook webhook, WebhookInfo before, WebhookInfo after)
	{
		Webhook = webhook;
		Before = before;
		After = after;
	}

	internal static WebhookUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "avatar_hash");
		string name = auditLogChange?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		ulong? channelId = auditLogChange2?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		string avatar = auditLogChange3?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		WebhookInfo before = new WebhookInfo(name, channelId, avatar);
		string name2 = auditLogChange?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		ulong? channelId2 = auditLogChange2?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		string avatar2 = auditLogChange3?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		WebhookInfo after = new WebhookInfo(name2, channelId2, avatar2);
		Webhook webhook = log.Webhooks?.FirstOrDefault((Webhook x) => x.Id == entry.TargetId);
		return new WebhookUpdateAuditLogData((webhook != null) ? RestWebhook.Create(discord, (IGuild)null, webhook) : null, before, after);
	}
}
