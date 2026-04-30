using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class StageInstanceCreateAuditLogData : IAuditLogData
{
	public string Topic { get; }

	public StagePrivacyLevel PrivacyLevel { get; }

	public IUser User { get; }

	public ulong StageChannelId { get; }

	internal StageInstanceCreateAuditLogData(string topic, StagePrivacyLevel privacyLevel, IUser user, ulong channelId)
	{
		Topic = topic;
		PrivacyLevel = privacyLevel;
		User = user;
		StageChannelId = channelId;
	}

	internal static StageInstanceCreateAuditLogData Create(BaseDiscordClient discord, AuditLog log, AuditLogEntry entry)
	{
		string topic = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "topic").NewValue.ToObject<string>(discord.ApiClient.Serializer);
		StagePrivacyLevel privacyLevel = entry.Changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "privacy_level").NewValue.ToObject<StagePrivacyLevel>(discord.ApiClient.Serializer);
		User model = log.Users.FirstOrDefault((User x) => x.Id == entry.UserId);
		ulong? channelId = entry.Options.ChannelId;
		return new StageInstanceCreateAuditLogData(topic, privacyLevel, RestUser.Create(discord, model), channelId.GetValueOrDefault());
	}
}
