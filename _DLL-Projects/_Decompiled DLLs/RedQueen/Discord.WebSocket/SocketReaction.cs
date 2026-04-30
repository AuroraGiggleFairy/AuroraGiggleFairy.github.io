using Discord.API.Gateway;

namespace Discord.WebSocket;

internal class SocketReaction : IReaction
{
	public ulong UserId { get; }

	public Optional<IUser> User { get; }

	public ulong MessageId { get; }

	public Optional<SocketUserMessage> Message { get; }

	public ISocketMessageChannel Channel { get; }

	public IEmote Emote { get; }

	internal SocketReaction(ISocketMessageChannel channel, ulong messageId, Optional<SocketUserMessage> message, ulong userId, Optional<IUser> user, IEmote emoji)
	{
		Channel = channel;
		MessageId = messageId;
		Message = message;
		UserId = userId;
		User = user;
		Emote = emoji;
	}

	internal static SocketReaction Create(Reaction model, ISocketMessageChannel channel, Optional<SocketUserMessage> message, Optional<IUser> user)
	{
		return new SocketReaction(emoji: (!model.Emoji.Id.HasValue) ? ((IEmote)new Emoji(model.Emoji.Name)) : ((IEmote)new Emote(model.Emoji.Id.Value, model.Emoji.Name, model.Emoji.Animated == true)), channel: channel, messageId: model.MessageId, message: message, userId: model.UserId, user: user);
	}

	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!(other is SocketReaction socketReaction))
		{
			return false;
		}
		if (UserId == socketReaction.UserId && MessageId == socketReaction.MessageId)
		{
			return Emote.Equals(socketReaction.Emote);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((UserId.GetHashCode() * 397) ^ MessageId.GetHashCode()) * 397) ^ Emote.GetHashCode();
	}
}
