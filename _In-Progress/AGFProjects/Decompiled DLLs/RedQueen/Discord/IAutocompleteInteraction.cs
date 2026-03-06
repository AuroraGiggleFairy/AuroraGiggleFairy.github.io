namespace Discord;

internal interface IAutocompleteInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IAutocompleteInteractionData Data { get; }
}
