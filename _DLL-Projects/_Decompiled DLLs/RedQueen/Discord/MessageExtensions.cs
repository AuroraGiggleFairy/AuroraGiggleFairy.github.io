using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

internal static class MessageExtensions
{
	public static string GetJumpUrl(this IMessage msg)
	{
		IMessageChannel channel = msg.Channel;
		return string.Format("https://discord.com/channels/{0}/{1}/{2}", (channel is IDMChannel) ? "@me" : $"{(channel as ITextChannel).GuildId}", channel.Id, msg.Id);
	}

	public static async Task AddReactionsAsync(this IUserMessage msg, IEnumerable<IEmote> reactions, RequestOptions options = null)
	{
		foreach (IEmote reaction in reactions)
		{
			await msg.AddReactionAsync(reaction, options).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task RemoveReactionsAsync(this IUserMessage msg, IUser user, IEnumerable<IEmote> reactions, RequestOptions options = null)
	{
		foreach (IEmote reaction in reactions)
		{
			await msg.RemoveReactionAsync(reaction, user, options).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static async Task<IUserMessage> ReplyAsync(this IUserMessage msg, string text = null, bool isTTS = false, Embed embed = null, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
	{
		return await msg.Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, new MessageReference(msg.Id), components, stickers, embeds).ConfigureAwait(continueOnCapturedContext: false);
	}
}
