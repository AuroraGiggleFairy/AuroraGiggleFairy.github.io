using System;
using System.Diagnostics;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class SocketNewsChannel : SocketTextChannel, INewsChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	public override int SlowModeInterval
	{
		get
		{
			throw new NotSupportedException("News channels do not support Slow Mode.");
		}
	}

	internal SocketNewsChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketNewsChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketNewsChannel socketNewsChannel = new SocketNewsChannel(guild?.Discord, model.Id, guild);
		socketNewsChannel.Update(state, model);
		return socketNewsChannel;
	}
}
