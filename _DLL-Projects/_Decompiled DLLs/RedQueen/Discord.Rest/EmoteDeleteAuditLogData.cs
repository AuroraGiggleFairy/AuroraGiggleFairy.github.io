using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class EmoteDeleteAuditLogData : IAuditLogData
{
	public ulong EmoteId { get; }

	public string Name { get; }

	private EmoteDeleteAuditLogData(ulong id, string name)
	{
		EmoteId = id;
		Name = name;
	}

	internal static EmoteDeleteAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		string name = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "name").OldValue?.ToObject<string>(discord.ApiClient.Serializer);
		return new EmoteDeleteAuditLogData(entry.TargetId.Value, name);
	}
}
