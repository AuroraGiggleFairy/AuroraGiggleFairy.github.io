namespace Discord;

internal interface IUserCommandInteractionData : IApplicationCommandInteractionData, IDiscordInteractionData
{
	IUser User { get; }
}
