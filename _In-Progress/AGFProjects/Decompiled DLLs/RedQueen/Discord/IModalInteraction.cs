namespace Discord;

internal interface IModalInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IModalInteractionData Data { get; }
}
