using System.IO;
using System.Threading.Tasks;

namespace Discord;

internal static class UserExtensions
{
	public static async Task<IUserMessage> SendMessageAsync(this IUser user, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed[] embeds = null)
	{
		return await (await user.CreateDMChannelAsync().ConfigureAwait(continueOnCapturedContext: false)).SendMessageAsync(text, isTTS, embed, options, allowedMentions, null, components, null, embeds).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IUserMessage> SendFileAsync(this IUser user, Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, MessageComponent components = null, Embed[] embeds = null)
	{
		return await (await user.CreateDMChannelAsync().ConfigureAwait(continueOnCapturedContext: false)).SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler: false, null, null, components, null, embeds).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<IUserMessage> SendFileAsync(this IUser user, string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, MessageComponent components = null, Embed[] embeds = null)
	{
		return await (await user.CreateDMChannelAsync().ConfigureAwait(continueOnCapturedContext: false)).SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler: false, null, null, components, null, embeds).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task BanAsync(this IGuildUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
	{
		return user.Guild.AddBanAsync(user, pruneDays, reason, options);
	}
}
