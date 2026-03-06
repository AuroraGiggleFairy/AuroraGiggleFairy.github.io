namespace Discord;

internal interface IMessageInteraction
{
	ulong Id { get; }

	InteractionType Type { get; }

	string Name { get; }

	IUser User { get; }
}
