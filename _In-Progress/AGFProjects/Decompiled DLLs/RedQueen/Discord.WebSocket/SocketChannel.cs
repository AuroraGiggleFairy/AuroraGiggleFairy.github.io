using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal abstract class SocketChannel : SocketEntity<ulong>, IChannel, ISnowflakeEntity, IEntity<ulong>
{
	public virtual DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public IReadOnlyCollection<SocketUser> Users => GetUsersInternal();

	private string DebuggerDisplay => $"Unknown ({base.Id}, Channel)";

	string IChannel.Name => null;

	internal SocketChannel(DiscordSocketClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static ISocketPrivateChannel CreatePrivate(DiscordSocketClient discord, ClientState state, Channel model)
	{
		return model.Type switch
		{
			ChannelType.DM => SocketDMChannel.Create(discord, state, model), 
			ChannelType.Group => SocketGroupChannel.Create(discord, state, model), 
			_ => throw new InvalidOperationException($"Unexpected channel type: {model.Type}"), 
		};
	}

	internal abstract void Update(ClientState state, Channel model);

	public SocketUser GetUser(ulong id)
	{
		return GetUserInternal(id);
	}

	internal abstract SocketUser GetUserInternal(ulong id);

	internal abstract IReadOnlyCollection<SocketUser> GetUsersInternal();

	internal SocketChannel Clone()
	{
		return MemberwiseClone() as SocketChannel;
	}

	Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IUser>(null);
	}

	IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<IUser>>();
	}
}
