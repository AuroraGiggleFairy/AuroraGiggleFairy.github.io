using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Commands;

internal class UserTypeReader<T> : TypeReader where T : class, IUser
{
	public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		Dictionary<ulong, TypeReaderValue> results = new Dictionary<ulong, TypeReaderValue>();
		IAsyncEnumerable<IUser> channelUsers = context.Channel.GetUsersAsync(CacheMode.CacheOnly).Flatten();
		IReadOnlyCollection<IGuildUser> guildUsers = System.Collections.Immutable.ImmutableArray.Create<IGuildUser>();
		if (context.Guild != null)
		{
			guildUsers = await context.Guild.GetUsersAsync(CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (MentionUtils.TryParseUser(input, out var userId))
		{
			if (context.Guild != null)
			{
				Dictionary<ulong, TypeReaderValue> results2 = results;
				AddResult(results2, (await context.Guild.GetUserAsync(userId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T, 1f);
			}
			else
			{
				Dictionary<ulong, TypeReaderValue> results2 = results;
				AddResult(results2, (await context.Channel.GetUserAsync(userId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T, 1f);
			}
		}
		if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out userId))
		{
			if (context.Guild != null)
			{
				Dictionary<ulong, TypeReaderValue> results2 = results;
				AddResult(results2, (await context.Guild.GetUserAsync(userId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T, 0.9f);
			}
			else
			{
				Dictionary<ulong, TypeReaderValue> results2 = results;
				AddResult(results2, (await context.Channel.GetUserAsync(userId, CacheMode.CacheOnly).ConfigureAwait(continueOnCapturedContext: false)) as T, 0.9f);
			}
		}
		int num = input.LastIndexOf('#');
		if (num >= 0)
		{
			string username = input.Substring(0, num);
			if (ushort.TryParse(input.Substring(num + 1), out var discriminator))
			{
				IUser user = await channelUsers.FirstOrDefaultAsync((IUser x) => x.DiscriminatorValue == discriminator && string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(continueOnCapturedContext: false);
				AddResult(results, user as T, (user?.Username == username) ? 0.85f : 0.75f);
				IGuildUser guildUser = guildUsers.FirstOrDefault((IGuildUser x) => x.DiscriminatorValue == discriminator && string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));
				AddResult(results, guildUser as T, (guildUser?.Username == username) ? 0.8f : 0.7f);
			}
		}
		await channelUsers.Where((IUser x) => string.Equals(input, x.Username, StringComparison.OrdinalIgnoreCase)).ForEachAsync(delegate(IUser channelUser)
		{
			AddResult(results, channelUser as T, (channelUser.Username == input) ? 0.65f : 0.55f);
		}).ConfigureAwait(continueOnCapturedContext: false);
		foreach (IGuildUser item in guildUsers.Where((IGuildUser x) => string.Equals(input, x.Username, StringComparison.OrdinalIgnoreCase)))
		{
			AddResult(results, item as T, (item.Username == input) ? 0.6f : 0.5f);
		}
		await channelUsers.Where((IUser x) => string.Equals(input, (x as IGuildUser)?.Nickname, StringComparison.OrdinalIgnoreCase)).ForEachAsync(delegate(IUser channelUser)
		{
			AddResult(results, channelUser as T, ((channelUser as IGuildUser).Nickname == input) ? 0.65f : 0.55f);
		}).ConfigureAwait(continueOnCapturedContext: false);
		foreach (IGuildUser item2 in guildUsers.Where((IGuildUser x) => string.Equals(input, x.Nickname, StringComparison.OrdinalIgnoreCase)))
		{
			AddResult(results, item2 as T, (item2.Nickname == input) ? 0.6f : 0.5f);
		}
		if (results.Count > 0)
		{
			return TypeReaderResult.FromSuccess(results.Values.ToImmutableArray());
		}
		return TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found.");
	}

	private void AddResult(Dictionary<ulong, TypeReaderValue> results, T user, float score)
	{
		if (user != null && !results.ContainsKey(user.Id))
		{
			results.Add(user.Id, new TypeReaderValue(user, score));
		}
	}
}
