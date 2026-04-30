using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class WebhookCreateAuditLogData : IAuditLogData
{
	public IWebhook Webhook { get; }

	public ulong WebhookId { get; }

	public WebhookType Type { get; }

	public string Name { get; }

	public ulong ChannelId { get; }

	private WebhookCreateAuditLogData(IWebhook webhook, ulong webhookId, WebhookType type, string name, ulong channelId)
	{
		Webhook = webhook;
		WebhookId = webhookId;
		Name = name;
		Type = type;
		ChannelId = channelId;
	}

	internal static WebhookCreateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "type");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		ulong channelId = auditLogChange.NewValue.ToObject<ulong>(discord.ApiClient.Serializer);
		WebhookType type = auditLogChange2.NewValue.ToObject<WebhookType>(discord.ApiClient.Serializer);
		string name = auditLogChange3.NewValue.ToObject<string>(discord.ApiClient.Serializer);
		Webhook webhook = log.Webhooks?.FirstOrDefault((Webhook x) => x.Id == entry.TargetId);
		return new WebhookCreateAuditLogData((webhook == null) ? null : RestWebhook.Create(discord, (IGuild)null, webhook), entry.TargetId.Value, type, name, channelId);
	}
}
