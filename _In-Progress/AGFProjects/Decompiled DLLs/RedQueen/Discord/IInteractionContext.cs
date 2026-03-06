namespace Discord;

internal interface IInteractionContext
{
	IDiscordClient Client { get; }

	IGuild Guild { get; }

	IMessageChannel Channel { get; }

	IUser User { get; }

	IDiscordInteraction Interaction { get; }
}
