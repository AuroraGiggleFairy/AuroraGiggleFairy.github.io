namespace Discord;

internal class RoleTags
{
	public ulong? BotId { get; }

	public ulong? IntegrationId { get; }

	public bool IsPremiumSubscriberRole { get; }

	internal RoleTags(ulong? botId, ulong? integrationId, bool isPremiumSubscriber)
	{
		BotId = botId;
		IntegrationId = integrationId;
		IsPremiumSubscriberRole = isPremiumSubscriber;
	}
}
