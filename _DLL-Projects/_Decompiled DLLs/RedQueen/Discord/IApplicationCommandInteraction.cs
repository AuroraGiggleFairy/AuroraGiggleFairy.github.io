namespace Discord;

internal interface IApplicationCommandInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IApplicationCommandInteractionData Data { get; }
}
