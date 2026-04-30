namespace Discord;

internal interface IMessageCommandInteractionData : IApplicationCommandInteractionData, IDiscordInteractionData
{
	IMessage Message { get; }
}
