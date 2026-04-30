using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class GuildUpdateAuditLogData : IAuditLogData
{
	public GuildInfo Before { get; }

	public GuildInfo After { get; }

	private GuildUpdateAuditLogData(GuildInfo before, GuildInfo after)
	{
		Before = before;
		After = after;
	}

	internal static GuildUpdateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "afk_timeout");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "default_message_notifications");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "afk_channel_id");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "region");
		AuditLogChange auditLogChange6 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "icon_hash");
		AuditLogChange auditLogChange7 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "verification_level");
		AuditLogChange auditLogChange8 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "owner_id");
		AuditLogChange auditLogChange9 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "mfa_level");
		AuditLogChange auditLogChange10 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "explicit_content_filter");
		AuditLogChange auditLogChange11 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "system_channel_id");
		AuditLogChange auditLogChange12 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "widget_channel_id");
		AuditLogChange auditLogChange13 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "widget_enabled");
		int? afkTimeout = auditLogChange?.OldValue?.ToObject<int>(discord.ApiClient.Serializer);
		int? afkTimeout2 = auditLogChange?.NewValue?.ToObject<int>(discord.ApiClient.Serializer);
		DefaultMessageNotifications? defaultNotifs = auditLogChange2?.OldValue?.ToObject<DefaultMessageNotifications>(discord.ApiClient.Serializer);
		DefaultMessageNotifications? defaultNotifs2 = auditLogChange2?.NewValue?.ToObject<DefaultMessageNotifications>(discord.ApiClient.Serializer);
		ulong? afkChannel = auditLogChange3?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? afkChannel2 = auditLogChange3?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		string name = auditLogChange4?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string name2 = auditLogChange4?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		string region = auditLogChange5?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string region2 = auditLogChange5?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		string icon = auditLogChange6?.OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		string icon2 = auditLogChange6?.NewValue?.ToObject<string>(discord.ApiClient.Serializer);
		VerificationLevel? verification = auditLogChange7?.OldValue?.ToObject<VerificationLevel>(discord.ApiClient.Serializer);
		VerificationLevel? verification2 = auditLogChange7?.NewValue?.ToObject<VerificationLevel>(discord.ApiClient.Serializer);
		ulong? oldOwnerId = auditLogChange8?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? newOwnerId = auditLogChange8?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		MfaLevel? mfa = auditLogChange9?.OldValue?.ToObject<MfaLevel>(discord.ApiClient.Serializer);
		MfaLevel? mfa2 = auditLogChange9?.NewValue?.ToObject<MfaLevel>(discord.ApiClient.Serializer);
		ExplicitContentFilterLevel? filter = auditLogChange10?.OldValue?.ToObject<ExplicitContentFilterLevel>(discord.ApiClient.Serializer);
		ExplicitContentFilterLevel? filter2 = auditLogChange10?.NewValue?.ToObject<ExplicitContentFilterLevel>(discord.ApiClient.Serializer);
		ulong? systemChannel = auditLogChange11?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? systemChannel2 = auditLogChange11?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? widgetChannel = auditLogChange12?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? widgetChannel2 = auditLogChange12?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		bool? widget = auditLogChange13?.OldValue?.ToObject<bool>(discord.ApiClient.Serializer);
		bool? widget2 = auditLogChange13?.NewValue?.ToObject<bool>(discord.ApiClient.Serializer);
		IUser owner = null;
		if (oldOwnerId.HasValue)
		{
			User model = log.Users.FirstOrDefault((User x) => x.Id == oldOwnerId.Value);
			owner = RestUser.Create(discord, model);
		}
		IUser owner2 = null;
		if (newOwnerId.HasValue)
		{
			User model2 = log.Users.FirstOrDefault((User x) => x.Id == newOwnerId.Value);
			owner2 = RestUser.Create(discord, model2);
		}
		GuildInfo before = new GuildInfo(afkTimeout, defaultNotifs, afkChannel, name, region, icon, verification, owner, mfa, filter, systemChannel, widgetChannel, widget);
		GuildInfo after = new GuildInfo(afkTimeout2, defaultNotifs2, afkChannel2, name2, region2, icon2, verification2, owner2, mfa2, filter2, systemChannel2, widgetChannel2, widget2);
		return new GuildUpdateAuditLogData(before, after);
	}
}
