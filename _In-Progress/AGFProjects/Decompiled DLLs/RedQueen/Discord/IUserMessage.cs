using System;
using System.Threading.Tasks;

namespace Discord;

internal interface IUserMessage : IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	IUserMessage ReferencedMessage { get; }

	Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null);

	Task PinAsync(RequestOptions options = null);

	Task UnpinAsync(RequestOptions options = null);

	Task CrosspostAsync(RequestOptions options = null);

	string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name);
}
