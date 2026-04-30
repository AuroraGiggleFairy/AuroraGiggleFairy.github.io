using System.Diagnostics;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketSystemMessage : SocketMessage, ISystemMessage, IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private string DebuggerDisplay => $"{base.Author}: {base.Content} ({base.Id}, {base.Type})";

	internal SocketSystemMessage(DiscordSocketClient discord, ulong id, ISocketMessageChannel channel, SocketUser author)
		: base(discord, id, channel, author, MessageSource.System)
	{
	}

	internal new static SocketSystemMessage Create(DiscordSocketClient discord, ClientState state, SocketUser author, ISocketMessageChannel channel, Message model)
	{
		SocketSystemMessage socketSystemMessage = new SocketSystemMessage(discord, model.Id, channel, author);
		socketSystemMessage.Update(state, model);
		return socketSystemMessage;
	}

	internal override void Update(ClientState state, Message model)
	{
		base.Update(state, model);
	}

	internal new SocketSystemMessage Clone()
	{
		return MemberwiseClone() as SocketSystemMessage;
	}
}
