namespace Discord;

internal class MessageInteraction<TUser> : IMessageInteraction where TUser : IUser
{
	public ulong Id { get; }

	public InteractionType Type { get; }

	public string Name { get; }

	public TUser User { get; }

	IUser IMessageInteraction.User => User;

	internal MessageInteraction(ulong id, InteractionType type, string name, TUser user)
	{
		Id = id;
		Type = type;
		Name = name;
		User = user;
	}
}
