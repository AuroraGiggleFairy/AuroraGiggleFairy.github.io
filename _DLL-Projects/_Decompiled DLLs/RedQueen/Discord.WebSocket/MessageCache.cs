using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord.WebSocket;

internal class MessageCache
{
	private readonly ConcurrentDictionary<ulong, SocketMessage> _messages;

	private readonly ConcurrentQueue<ulong> _orderedMessages;

	private readonly int _size;

	public IReadOnlyCollection<SocketMessage> Messages => _messages.ToReadOnlyCollection();

	public MessageCache(DiscordSocketClient discord)
	{
		_size = discord.MessageCacheSize;
		_messages = new ConcurrentDictionary<ulong, SocketMessage>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)((double)_size * 1.05));
		_orderedMessages = new ConcurrentQueue<ulong>();
	}

	public void Add(SocketMessage message)
	{
		if (_messages.TryAdd(message.Id, message))
		{
			_orderedMessages.Enqueue(message.Id);
			ulong result;
			while (_orderedMessages.Count > _size && _orderedMessages.TryDequeue(out result))
			{
				_messages.TryRemove(result, out var _);
			}
		}
	}

	public SocketMessage Remove(ulong id)
	{
		_messages.TryRemove(id, out var value);
		return value;
	}

	public SocketMessage Get(ulong id)
	{
		if (_messages.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public IReadOnlyCollection<SocketMessage> GetMany(ulong? fromMessageId, Direction dir, int limit = 100)
	{
		if (limit < 0)
		{
			throw new ArgumentOutOfRangeException("limit");
		}
		if (limit == 0)
		{
			return System.Collections.Immutable.ImmutableArray<SocketMessage>.Empty;
		}
		IEnumerable<ulong> source;
		if (!fromMessageId.HasValue)
		{
			source = _orderedMessages;
		}
		else
		{
			switch (dir)
			{
			case Direction.Before:
				source = _orderedMessages.Where((ulong x) => x < fromMessageId.Value);
				break;
			case Direction.After:
				source = _orderedMessages.Where((ulong x) => x > fromMessageId.Value);
				break;
			default:
			{
				if (!_messages.TryGetValue(fromMessageId.Value, out var value))
				{
					return System.Collections.Immutable.ImmutableArray<SocketMessage>.Empty;
				}
				int limit2 = limit / 2;
				IReadOnlyCollection<SocketMessage> many = GetMany(fromMessageId, Direction.Before, limit2);
				return GetMany(fromMessageId, Direction.After, limit2).Reverse().Concat(new SocketMessage[1] { value }).Concat(many)
					.ToImmutableArray();
			}
			}
		}
		if (dir == Direction.Before)
		{
			source = source.Reverse();
		}
		if (dir == Direction.Around)
		{
			limit = limit / 2 + 1;
		}
		return (from x in source
			select _messages.TryGetValue(x, out var value2) ? value2 : null into x
			where x != null
			select x).Take(limit).ToImmutableArray();
	}
}
