using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class InviteDeleteAuditLogData : IAuditLogData
{
	public int MaxAge { get; }

	public string Code { get; }

	public bool Temporary { get; }

	public IUser Creator { get; }

	public ulong ChannelId { get; }

	public int Uses { get; }

	public int MaxUses { get; }

	private InviteDeleteAuditLogData(int maxAge, string code, bool temporary, IUser inviter, ulong channelId, int uses, int maxUses)
	{
		MaxAge = maxAge;
		Code = code;
		Temporary = temporary;
		Creator = inviter;
		ChannelId = channelId;
		Uses = uses;
		MaxUses = maxUses;
	}

	internal static InviteDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "max_age");
		AuditLogChange auditLogChange2 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "code");
		AuditLogChange auditLogChange3 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "temporary");
		AuditLogChange auditLogChange4 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "inviter_id");
		AuditLogChange auditLogChange5 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "channel_id");
		AuditLogChange auditLogChange6 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "uses");
		AuditLogChange auditLogChange7 = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "max_uses");
		int maxAge = auditLogChange.OldValue.ToObject<int>(discord.ApiClient.Serializer);
		string code = auditLogChange2.OldValue.ToObject<string>(discord.ApiClient.Serializer);
		bool temporary = auditLogChange3.OldValue.ToObject<bool>(discord.ApiClient.Serializer);
		ulong channelId = auditLogChange5.OldValue.ToObject<ulong>(discord.ApiClient.Serializer);
		int uses = auditLogChange6.OldValue.ToObject<int>(discord.ApiClient.Serializer);
		int maxUses = auditLogChange7.OldValue.ToObject<int>(discord.ApiClient.Serializer);
		RestUser inviter = null;
		if (auditLogChange4 != null)
		{
			ulong inviterId = auditLogChange4.OldValue.ToObject<ulong>(discord.ApiClient.Serializer);
			User user = log.Users.FirstOrDefault((User x) => x.Id == inviterId);
			inviter = ((user != null) ? RestUser.Create(discord, user) : null);
		}
		return new InviteDeleteAuditLogData(maxAge, code, temporary, inviter, channelId, uses, maxUses);
	}
}
