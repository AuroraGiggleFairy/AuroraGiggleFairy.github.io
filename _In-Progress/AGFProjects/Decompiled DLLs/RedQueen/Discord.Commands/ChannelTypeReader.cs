using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Commands;

internal class ChannelTypeReader<T> : TypeReader where T : class, IChannel
{
	public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		if (context.Guild != null)
		{
			Dictionary<ulong, TypeReaderValue> results = new Dictionary<ulong, TypeReaderValue>();
			IReadOnlyCollection<IGuildChannel> channels = await context.Guild.GetChannelsAsync(CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
			if (MentionUtils.TryParseChannel(input, out var channelId))
			{
				Dictionary<ulong, TypeReaderValue> results2 = results;
				AddResult(results2, (await context.Guild.GetChannelAsync(channelId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T, 1f);
			}
			if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out channelId))
			{
				Dictionary<ulong, TypeReaderValue> results2 = results;
				AddResult(results2, (await context.Guild.GetChannelAsync(channelId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T, 0.9f);
			}
			foreach (IGuildChannel item in channels.Where((IGuildChannel x) => string.Equals(input, x.Name, StringComparison.OrdinalIgnoreCase)))
			{
				AddResult(results, item as T, (item.Name == input) ? 0.8f : 0.7f);
			}
			if (results.Count > 0)
			{
				return TypeReaderResult.FromSuccess(results.Values.ToReadOnlyCollection());
			}
		}
		return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Channel not found.");
	}

	private void AddResult(Dictionary<ulong, TypeReaderValue> results, T channel, float score)
	{
		if (channel != null && !results.ContainsKey(channel.Id))
		{
			results.Add(channel.Id, new TypeReaderValue(channel, score));
		}
	}
}
