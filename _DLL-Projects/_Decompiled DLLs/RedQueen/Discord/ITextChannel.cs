using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord;

internal interface ITextChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable
{
	bool IsNsfw { get; }

	string Topic { get; }

	int SlowModeInterval { get; }

	ThreadArchiveDuration DefaultArchiveDuration { get; }

	Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null);

	Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null);

	Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null);

	Task<IWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null);

	Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

	Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null);

	Task<IThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null);
}
