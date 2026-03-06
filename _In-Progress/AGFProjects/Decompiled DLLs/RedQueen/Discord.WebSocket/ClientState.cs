using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Discord.WebSocket;

internal class ClientState
{
	private const double AverageChannelsPerGuild = 10.22;

	private const double AverageUsersPerGuild = 47.78;

	private const double CollectionMultiplier = 1.05;

	private readonly ConcurrentDictionary<ulong, SocketChannel> _channels;

	private readonly ConcurrentDictionary<ulong, SocketDMChannel> _dmChannels;

	private readonly ConcurrentDictionary<ulong, SocketGuild> _guilds;

	private readonly ConcurrentDictionary<ulong, SocketGlobalUser> _users;

	private readonly ConcurrentHashSet<ulong> _groupChannels;

	private readonly ConcurrentDictionary<ulong, SocketApplicationCommand> _commands;

	internal IReadOnlyCollection<SocketChannel> Channels => _channels.ToReadOnlyCollection();

	internal IReadOnlyCollection<SocketDMChannel> DMChannels => _dmChannels.ToReadOnlyCollection();

	internal IReadOnlyCollection<SocketGroupChannel> GroupChannels => _groupChannels.Select((ulong x) => GetChannel(x) as SocketGroupChannel).ToReadOnlyCollection(_groupChannels);

	internal IReadOnlyCollection<SocketGuild> Guilds => _guilds.ToReadOnlyCollection();

	internal IReadOnlyCollection<SocketGlobalUser> Users => _users.ToReadOnlyCollection();

	internal IReadOnlyCollection<SocketApplicationCommand> Commands => _commands.ToReadOnlyCollection();

	internal IReadOnlyCollection<ISocketPrivateChannel> PrivateChannels => ((IEnumerable<KeyValuePair<ulong, SocketDMChannel>>)_dmChannels).Select((Func<KeyValuePair<ulong, SocketDMChannel>, ISocketPrivateChannel>)((KeyValuePair<ulong, SocketDMChannel> x) => x.Value)).Concat(_groupChannels.Select((ulong x) => GetChannel(x) as ISocketPrivateChannel)).ToReadOnlyCollection(() => _dmChannels.Count + _groupChannels.Count);

	public ClientState(int guildCount, int dmChannelCount)
	{
		double num = (double)guildCount * 10.22 + (double)dmChannelCount;
		double num2 = (double)guildCount * 47.78;
		_channels = new ConcurrentDictionary<ulong, SocketChannel>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)(num * 1.05));
		_dmChannels = new ConcurrentDictionary<ulong, SocketDMChannel>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)dmChannelCount * 1.05));
		_guilds = new ConcurrentDictionary<ulong, SocketGuild>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)guildCount * 1.05));
		_users = new ConcurrentDictionary<ulong, SocketGlobalUser>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)(num2 * 1.05));
		_groupChannels = new ConcurrentHashSet<ulong>(ConcurrentHashSet.DefaultConcurrencyLevel, 10);
		_commands = new ConcurrentDictionary<ulong, SocketApplicationCommand>();
	}

	internal SocketChannel GetChannel(ulong id)
	{
		if (_channels.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketDMChannel GetDMChannel(ulong userId)
	{
		if (_dmChannels.TryGetValue(userId, out var value))
		{
			return value;
		}
		return null;
	}

	internal void AddChannel(SocketChannel channel)
	{
		_channels[channel.Id] = channel;
		if (!(channel is SocketDMChannel socketDMChannel))
		{
			if (channel is SocketGroupChannel socketGroupChannel)
			{
				_groupChannels.TryAdd(socketGroupChannel.Id);
			}
		}
		else
		{
			_dmChannels[socketDMChannel.Recipient.Id] = socketDMChannel;
		}
	}

	internal SocketChannel RemoveChannel(ulong id)
	{
		if (_channels.TryRemove(id, out var value))
		{
			if (!(value is SocketDMChannel socketDMChannel))
			{
				if (value is SocketGroupChannel)
				{
					_groupChannels.TryRemove(id);
				}
			}
			else
			{
				_dmChannels.TryRemove(socketDMChannel.Recipient.Id, out var _);
			}
			return value;
		}
		return null;
	}

	internal void PurgeAllChannels()
	{
		foreach (SocketGuild value in _guilds.Values)
		{
			value.PurgeChannelCache(this);
		}
		PurgeDMChannels();
	}

	internal void PurgeDMChannels()
	{
		foreach (SocketDMChannel value2 in _dmChannels.Values)
		{
			_channels.TryRemove(value2.Id, out var _);
		}
		_dmChannels.Clear();
	}

	internal SocketGuild GetGuild(ulong id)
	{
		if (_guilds.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal void AddGuild(SocketGuild guild)
	{
		_guilds[guild.Id] = guild;
	}

	internal SocketGuild RemoveGuild(ulong id)
	{
		if (_guilds.TryRemove(id, out var value))
		{
			value.PurgeChannelCache(this);
			value.PurgeUserCache();
			return value;
		}
		return null;
	}

	internal SocketGlobalUser GetUser(ulong id)
	{
		if (_users.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal SocketGlobalUser GetOrAddUser(ulong id, Func<ulong, SocketGlobalUser> userFactory)
	{
		return _users.GetOrAdd(id, userFactory);
	}

	internal SocketGlobalUser RemoveUser(ulong id)
	{
		if (_users.TryRemove(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal void PurgeUsers()
	{
		foreach (SocketGuild value in _guilds.Values)
		{
			value.PurgeUserCache();
		}
	}

	internal SocketApplicationCommand GetCommand(ulong id)
	{
		if (_commands.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal void AddCommand(SocketApplicationCommand command)
	{
		_commands[command.Id] = command;
	}

	internal SocketApplicationCommand GetOrAddCommand(ulong id, Func<ulong, SocketApplicationCommand> commandFactory)
	{
		return _commands.GetOrAdd(id, commandFactory);
	}

	internal SocketApplicationCommand RemoveCommand(ulong id)
	{
		if (_commands.TryRemove(id, out var value))
		{
			return value;
		}
		return null;
	}

	internal void PurgeCommands(Func<SocketApplicationCommand, bool> precondition)
	{
		foreach (ulong item in from x in _commands
			where precondition(x.Value)
			select x.Key)
		{
			_commands.TryRemove(item, out var _);
		}
	}
}
