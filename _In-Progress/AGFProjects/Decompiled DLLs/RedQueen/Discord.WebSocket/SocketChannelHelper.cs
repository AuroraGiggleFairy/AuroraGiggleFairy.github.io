using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.Rest;

namespace Discord.WebSocket;

internal static class SocketChannelHelper
{
	public static IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ISocketMessageChannel channel, DiscordSocketClient discord, MessageCache messages, ulong? fromMessageId, Direction dir, int limit, CacheMode mode, RequestOptions options)
	{
		if (dir == Direction.After && !fromMessageId.HasValue)
		{
			return AsyncEnumerable.Empty<IReadOnlyCollection<IMessage>>();
		}
		IReadOnlyCollection<SocketMessage> cachedMessages = GetCachedMessages(channel, discord, messages, fromMessageId, dir, limit);
		IAsyncEnumerable<IReadOnlyCollection<IMessage>> asyncEnumerable = ((IEnumerable<IReadOnlyCollection<IMessage>>)System.Collections.Immutable.ImmutableArray.Create(cachedMessages)).ToAsyncEnumerable();
		switch (dir)
		{
		case Direction.Before:
		{
			limit -= cachedMessages.Count;
			if (mode == CacheMode.CacheOnly || limit <= 0)
			{
				return asyncEnumerable;
			}
			ulong? fromMessageId2 = ((cachedMessages.Count > 0) ? new ulong?(cachedMessages.Min((SocketMessage x) => x.Id)) : fromMessageId);
			IAsyncEnumerable<IReadOnlyCollection<RestMessage>> messagesAsync2 = ChannelHelper.GetMessagesAsync(channel, discord, fromMessageId2, dir, limit, options);
			if (cachedMessages.Count != 0)
			{
				return asyncEnumerable.Concat(messagesAsync2);
			}
			return messagesAsync2;
		}
		case Direction.After:
		{
			limit -= cachedMessages.Count;
			if (mode == CacheMode.CacheOnly || limit <= 0)
			{
				return asyncEnumerable;
			}
			ulong value = ((cachedMessages.Count > 0) ? cachedMessages.Max((SocketMessage x) => x.Id) : fromMessageId.Value);
			IAsyncEnumerable<IReadOnlyCollection<RestMessage>> messagesAsync = ChannelHelper.GetMessagesAsync(channel, discord, value, dir, limit, options);
			if (cachedMessages.Count != 0)
			{
				return asyncEnumerable.Concat(messagesAsync);
			}
			return messagesAsync;
		}
		default:
			if (mode == CacheMode.CacheOnly || limit <= cachedMessages.Count)
			{
				return asyncEnumerable;
			}
			return ChannelHelper.GetMessagesAsync(channel, discord, fromMessageId, dir, limit, options);
		}
	}

	public static IReadOnlyCollection<SocketMessage> GetCachedMessages(ISocketMessageChannel channel, DiscordSocketClient discord, MessageCache messages, ulong? fromMessageId, Direction dir, int limit)
	{
		if (messages != null)
		{
			return messages.GetMany(fromMessageId, dir, limit);
		}
		return System.Collections.Immutable.ImmutableArray.Create<SocketMessage>();
	}

	public static void AddMessage(ISocketMessageChannel channel, DiscordSocketClient discord, SocketMessage msg)
	{
		if (!(channel is SocketDMChannel socketDMChannel))
		{
			if (!(channel is SocketGroupChannel socketGroupChannel))
			{
				if (!(channel is SocketThreadChannel socketThreadChannel))
				{
					if (!(channel is SocketTextChannel socketTextChannel))
					{
						throw new NotSupportedException("Unexpected ISocketMessageChannel type.");
					}
					socketTextChannel.AddMessage(msg);
				}
				else
				{
					socketThreadChannel.AddMessage(msg);
				}
			}
			else
			{
				socketGroupChannel.AddMessage(msg);
			}
		}
		else
		{
			socketDMChannel.AddMessage(msg);
		}
	}

	public static SocketMessage RemoveMessage(ISocketMessageChannel channel, DiscordSocketClient discord, ulong id)
	{
		if (!(channel is SocketDMChannel socketDMChannel))
		{
			if (!(channel is SocketGroupChannel socketGroupChannel))
			{
				if (channel is SocketTextChannel socketTextChannel)
				{
					return socketTextChannel.RemoveMessage(id);
				}
				throw new NotSupportedException("Unexpected ISocketMessageChannel type.");
			}
			return socketGroupChannel.RemoveMessage(id);
		}
		return socketDMChannel.RemoveMessage(id);
	}
}
