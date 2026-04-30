using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class MessageHelper
{
	private static readonly Regex InlineCodeRegex = new Regex("[^\\\\]?(`).+?[^\\\\](`)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly Regex BlockCodeRegex = new Regex("[^\\\\]?(```).+?[^\\\\](```)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.Singleline);

	public static Task<Message> ModifyAsync(IMessage msg, BaseDiscordClient client, Action<MessageProperties> func, RequestOptions options)
	{
		return ModifyAsync(msg.Channel.Id, msg.Id, client, func, options);
	}

	public static async Task<Message> ModifyAsync(ulong channelId, ulong msgId, BaseDiscordClient client, Action<MessageProperties> func, RequestOptions options)
	{
		MessageProperties messageProperties = new MessageProperties();
		func(messageProperties);
		Optional<Embed> embed = messageProperties.Embed;
		Optional<Embed[]> embeds = messageProperties.Embeds;
		bool flag = messageProperties.Content.IsSpecified && string.IsNullOrEmpty(messageProperties.Content.Value);
		int num;
		if (!embed.IsSpecified || !(embed.Value != null))
		{
			if (embeds.IsSpecified)
			{
				Embed[] value = embeds.Value;
				num = ((value != null && value.Length != 0) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool flag2 = (byte)num != 0;
		bool num2 = messageProperties.Components.IsSpecified && messageProperties.Components.Value != null;
		bool isSpecified = messageProperties.Attachments.IsSpecified;
		bool isSpecified2 = messageProperties.Flags.IsSpecified;
		if (!num2 && !flag && !flag2 && !isSpecified && !isSpecified2)
		{
			Preconditions.NotNullOrEmpty(messageProperties.Content.IsSpecified ? messageProperties.Content.Value : string.Empty, "Content");
		}
		if (messageProperties.AllowedMentions.IsSpecified)
		{
			AllowedMentions value2 = messageProperties.AllowedMentions.Value;
			Preconditions.AtMost((value2?.RoleIds?.Count).GetValueOrDefault(), 100, "RoleIds", "A max of 100 role Ids are allowed.");
			Preconditions.AtMost((value2?.UserIds?.Count).GetValueOrDefault(), 100, "UserIds", "A max of 100 user Ids are allowed.");
			if (value2 != null && value2.AllowedTypes.HasValue)
			{
				if (value2.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Users) && value2.UserIds != null && value2.UserIds.Count > 0)
				{
					throw new ArgumentException("The Users flag is mutually exclusive with the list of User Ids.", "allowedMentions");
				}
				if (value2.AllowedTypes.Value.HasFlag(AllowedMentionTypes.Roles) && value2.RoleIds != null && value2.RoleIds.Count > 0)
				{
					throw new ArgumentException("The Roles flag is mutually exclusive with the list of Role Ids.", "allowedMentions");
				}
			}
		}
		List<Discord.API.Embed> list = ((embed.IsSpecified || embeds.IsSpecified) ? new List<Discord.API.Embed>() : null);
		if (embed.IsSpecified && embed.Value != null)
		{
			list.Add(embed.Value.ToModel());
		}
		if (embeds.IsSpecified && embeds.Value != null)
		{
			list.AddRange(embeds.Value.Select((Embed x) => x.ToModel()));
		}
		Preconditions.AtMost(list?.Count ?? 0, 10, "Embeds", "A max of 10 embeds are allowed.");
		Discord.API.Embed[] array;
		if (!messageProperties.Attachments.IsSpecified)
		{
			ModifyMessageParams obj = new ModifyMessageParams
			{
				Content = messageProperties.Content
			};
			array = list?.ToArray();
			obj.Embeds = ((array != null) ? ((Optional<Discord.API.Embed[]>)array) : Optional<Discord.API.Embed[]>.Unspecified);
			obj.Flags = (messageProperties.Flags.IsSpecified ? ((Optional<MessageFlags?>)messageProperties.Flags.Value) : Optional.Create<MessageFlags?>());
			obj.AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)messageProperties.AllowedMentions.Value.ToModel()) : Optional.Create<Discord.API.AllowedMentions>());
			obj.Components = (messageProperties.Components.IsSpecified ? ((Optional<Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray() ?? Array.Empty<Discord.API.ActionRowComponent>())) : Optional<Discord.API.ActionRowComponent[]>.Unspecified);
			ModifyMessageParams args = obj;
			return await client.ApiClient.ModifyMessageAsync(channelId, msgId, args, options).ConfigureAwait(continueOnCapturedContext: false);
		}
		UploadFileParams obj2 = new UploadFileParams(messageProperties.Attachments.Value.ToArray())
		{
			Content = messageProperties.Content
		};
		array = list?.ToArray();
		obj2.Embeds = ((array != null) ? ((Optional<Discord.API.Embed[]>)array) : Optional<Discord.API.Embed[]>.Unspecified);
		obj2.Flags = (messageProperties.Flags.IsSpecified ? ((Optional<MessageFlags?>)messageProperties.Flags.Value) : Optional.Create<MessageFlags?>());
		obj2.AllowedMentions = (messageProperties.AllowedMentions.IsSpecified ? ((Optional<Discord.API.AllowedMentions>)messageProperties.AllowedMentions.Value.ToModel()) : Optional.Create<Discord.API.AllowedMentions>());
		obj2.MessageComponent = (messageProperties.Components.IsSpecified ? ((Optional<Discord.API.ActionRowComponent[]>)(messageProperties.Components.Value?.Components.Select((ActionRowComponent x) => new Discord.API.ActionRowComponent(x)).ToArray() ?? Array.Empty<Discord.API.ActionRowComponent>())) : Optional<Discord.API.ActionRowComponent[]>.Unspecified);
		UploadFileParams args2 = obj2;
		return await client.ApiClient.ModifyMessageAsync(channelId, msgId, args2, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static Task DeleteAsync(IMessage msg, BaseDiscordClient client, RequestOptions options)
	{
		return DeleteAsync(msg.Channel.Id, msg.Id, client, options);
	}

	public static async Task DeleteAsync(ulong channelId, ulong msgId, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.DeleteMessageAsync(channelId, msgId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task AddReactionAsync(ulong channelId, ulong messageId, IEmote emote, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.AddReactionAsync(channelId, messageId, (emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task AddReactionAsync(IMessage msg, IEmote emote, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.AddReactionAsync(msg.Channel.Id, msg.Id, (emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveReactionAsync(ulong channelId, ulong messageId, ulong userId, IEmote emote, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemoveReactionAsync(channelId, messageId, userId, (emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveReactionAsync(IMessage msg, ulong userId, IEmote emote, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemoveReactionAsync(msg.Channel.Id, msg.Id, userId, (emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveAllReactionsAsync(ulong channelId, ulong messageId, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemoveAllReactionsAsync(channelId, messageId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveAllReactionsAsync(IMessage msg, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemoveAllReactionsAsync(msg.Channel.Id, msg.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveAllReactionsForEmoteAsync(ulong channelId, ulong messageId, IEmote emote, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemoveAllReactionsForEmoteAsync(channelId, messageId, (emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task RemoveAllReactionsForEmoteAsync(IMessage msg, IEmote emote, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemoveAllReactionsForEmoteAsync(msg.Channel.Id, msg.Id, (emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name), options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IMessage msg, IEmote emote, int? limit, BaseDiscordClient client, RequestOptions options)
	{
		Preconditions.NotNull(emote, "emote");
		string emoji = ((emote is Emote emote2) ? $"{emote2.Name}:{emote2.Id}" : UrlEncode(emote.Name));
		return new PagedAsyncEnumerable<IUser>(100, async delegate(PageInfo info, CancellationToken ct)
		{
			GetReactionUsersParams getReactionUsersParams = new GetReactionUsersParams
			{
				Limit = info.PageSize
			};
			if (info.Position.HasValue)
			{
				getReactionUsersParams.AfterUserId = info.Position.Value;
			}
			return (await client.ApiClient.GetReactionUsersAsync(msg.Channel.Id, msg.Id, emoji, getReactionUsersParams, options).ConfigureAwait(continueOnCapturedContext: false)).Select((User x) => RestUser.Create(client, x)).ToImmutableArray();
		}, delegate(PageInfo info, IReadOnlyCollection<IUser> lastPage)
		{
			if (lastPage.Count != 100)
			{
				return false;
			}
			info.Position = lastPage.Max((IUser x) => x.Id);
			return true;
		}, null, limit);
	}

	private static string UrlEncode(string text)
	{
		return WebUtility.UrlEncode(text);
	}

	public static string SanitizeMessage(IMessage message)
	{
		return Format.StripMarkDown(MentionUtils.Resolve(message, 0, TagHandling.FullName, TagHandling.FullName, TagHandling.FullName, TagHandling.FullName, TagHandling.FullName));
	}

	public static async Task PinAsync(IMessage msg, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.AddPinAsync(msg.Channel.Id, msg.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task UnpinAsync(IMessage msg, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.RemovePinAsync(msg.Channel.Id, msg.Id, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static System.Collections.Immutable.ImmutableArray<ITag> ParseTags(string text, IMessageChannel channel, IGuild guild, IReadOnlyCollection<IUser> userMentions)
	{
		System.Collections.Immutable.ImmutableArray<ITag>.Builder builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<ITag>();
		int index = 0;
		int codeIndex = 0;
		while (true)
		{
			index = text.IndexOf('<', index);
			if (index == -1)
			{
				break;
			}
			int num = text.IndexOf('>', index + 1);
			if (num == -1 || CheckWrappedCode())
			{
				break;
			}
			string text2 = text.Substring(index, num - index + 1);
			if (MentionUtils.TryParseUser(text2, out var userId))
			{
				IUser user = null;
				foreach (IUser userMention in userMentions)
				{
					if (userMention.Id == userId)
					{
						user = channel?.GetUserAsync(userId, CacheMode.CacheOnly).GetAwaiter().GetResult();
						if (user == null)
						{
							user = userMention;
						}
						break;
					}
				}
				builder.Add(new Tag<IUser>(TagType.UserMention, index, text2.Length, userId, user));
			}
			else if (MentionUtils.TryParseChannel(text2, out userId))
			{
				IChannel value = null;
				if (guild != null)
				{
					value = guild.GetChannelAsync(userId, CacheMode.CacheOnly).GetAwaiter().GetResult();
				}
				builder.Add(new Tag<IChannel>(TagType.ChannelMention, index, text2.Length, userId, value));
			}
			else if (MentionUtils.TryParseRole(text2, out userId))
			{
				IRole value2 = null;
				if (guild != null)
				{
					value2 = guild.GetRole(userId);
				}
				builder.Add(new Tag<IRole>(TagType.RoleMention, index, text2.Length, userId, value2));
			}
			else
			{
				if (!Emote.TryParse(text2, out var result))
				{
					index++;
					continue;
				}
				builder.Add(new Tag<Emote>(TagType.Emoji, index, text2.Length, result.Id, result));
			}
			index = num + 1;
		}
		index = 0;
		codeIndex = 0;
		while (true)
		{
			index = text.IndexOf("@everyone", index);
			if (index == -1 || CheckWrappedCode())
			{
				break;
			}
			int? num2 = FindIndex(builder, index);
			if (num2.HasValue)
			{
				builder.Insert(num2.Value, new Tag<IRole>(TagType.EveryoneMention, index, "@everyone".Length, 0uL, guild?.EveryoneRole));
			}
			index++;
		}
		index = 0;
		codeIndex = 0;
		while (true)
		{
			index = text.IndexOf("@here", index);
			if (index == -1 || CheckWrappedCode())
			{
				break;
			}
			int? num3 = FindIndex(builder, index);
			if (num3.HasValue)
			{
				builder.Insert(num3.Value, new Tag<IRole>(TagType.HereMention, index, "@here".Length, 0uL, guild?.EveryoneRole));
			}
			index++;
		}
		return builder.ToImmutable();
		bool CheckWrappedCode()
		{
			while (codeIndex < index)
			{
				Match match = BlockCodeRegex.Match(text, codeIndex);
				if (match.Success)
				{
					if (EnclosedInBlock(match))
					{
						return true;
					}
					codeIndex += match.Groups[2].Index + match.Groups[2].Length;
					if (codeIndex >= index)
					{
						return false;
					}
				}
				else
				{
					Match match2 = InlineCodeRegex.Match(text, codeIndex);
					if (!match2.Success)
					{
						return false;
					}
					if (EnclosedInBlock(match2))
					{
						return true;
					}
					codeIndex += match2.Groups[2].Index + match2.Groups[2].Length;
					if (codeIndex >= index)
					{
						return false;
					}
				}
			}
			return false;
		}
		bool EnclosedInBlock(Match m)
		{
			if (m.Groups[1].Index < index)
			{
				return index < m.Groups[2].Index;
			}
			return false;
		}
	}

	private static int? FindIndex(IReadOnlyList<ITag> tags, int index)
	{
		int i;
		for (i = 0; i < tags.Count; i++)
		{
			ITag tag = tags[i];
			if (index < tag.Index)
			{
				break;
			}
		}
		if (i > 0 && index < tags[i - 1].Index + tags[i - 1].Length)
		{
			return null;
		}
		return i;
	}

	public static System.Collections.Immutable.ImmutableArray<ulong> FilterTagsByKey(TagType type, System.Collections.Immutable.ImmutableArray<ITag> tags)
	{
		return (from x in tags
			where x.Type == type
			select x.Key).ToImmutableArray();
	}

	public static System.Collections.Immutable.ImmutableArray<T> FilterTagsByValue<T>(TagType type, System.Collections.Immutable.ImmutableArray<ITag> tags)
	{
		return (from x in tags
			where x.Type == type
			select (T)x.Value into x
			where x != null
			select x).ToImmutableArray();
	}

	public static MessageSource GetSource(Message msg)
	{
		if (msg.Type != MessageType.Default && msg.Type != MessageType.Reply)
		{
			return MessageSource.System;
		}
		if (msg.WebhookId.IsSpecified)
		{
			return MessageSource.Webhook;
		}
		User valueOrDefault = msg.Author.GetValueOrDefault();
		if (valueOrDefault != null && valueOrDefault.Bot.GetValueOrDefault(defaultValue: false))
		{
			return MessageSource.Bot;
		}
		return MessageSource.User;
	}

	public static Task CrosspostAsync(IMessage msg, BaseDiscordClient client, RequestOptions options)
	{
		return CrosspostAsync(msg.Channel.Id, msg.Id, client, options);
	}

	public static async Task CrosspostAsync(ulong channelId, ulong msgId, BaseDiscordClient client, RequestOptions options)
	{
		await client.ApiClient.CrosspostAsync(channelId, msgId, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static IUser GetAuthor(BaseDiscordClient client, IGuild guild, User model, ulong? webhookId)
	{
		IUser user = null;
		if (guild != null)
		{
			user = guild.GetUserAsync(model.Id, CacheMode.CacheOnly).Result;
		}
		if (user == null)
		{
			user = RestUser.Create(client, guild, model, webhookId);
		}
		return user;
	}
}
