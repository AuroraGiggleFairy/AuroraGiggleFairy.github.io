namespace Discord;

internal interface IMessageComponent
{
	ComponentType Type { get; }

	string CustomId { get; }
}
