using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Discord.Commands;

internal class MessageTypeReader<T> : TypeReader where T : class, IMessage
{
	public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && await context.Channel.GetMessageAsync(result, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false) is T value)
		{
			return TypeReaderResult.FromSuccess(value);
		}
		return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Message not found.");
	}
}
