using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Discord;

internal static class MentionUtils
{
	private const char SanitizeChar = '\u200b';

	internal static string MentionUser(string id, bool useNickname = true)
	{
		if (!useNickname)
		{
			return "<@" + id + ">";
		}
		return "<@!" + id + ">";
	}

	public static string MentionUser(ulong id)
	{
		return MentionUser(id.ToString());
	}

	internal static string MentionChannel(string id)
	{
		return "<#" + id + ">";
	}

	public static string MentionChannel(ulong id)
	{
		return MentionChannel(id.ToString());
	}

	internal static string MentionRole(string id)
	{
		return "<@&" + id + ">";
	}

	public static string MentionRole(ulong id)
	{
		return MentionRole(id.ToString());
	}

	public static ulong ParseUser(string text)
	{
		if (TryParseUser(text, out var userId))
		{
			return userId;
		}
		throw new ArgumentException("Invalid mention format.", "text");
	}

	public static bool TryParseUser(string text, out ulong userId)
	{
		if (text.Length >= 3 && text[0] == '<' && text[1] == '@' && text[text.Length - 1] == '>')
		{
			text = ((text.Length < 4 || text[2] != '!') ? text.Substring(2, text.Length - 3) : text.Substring(3, text.Length - 4));
			if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out userId))
			{
				return true;
			}
		}
		userId = 0uL;
		return false;
	}

	public static ulong ParseChannel(string text)
	{
		if (TryParseChannel(text, out var channelId))
		{
			return channelId;
		}
		throw new ArgumentException("Invalid mention format.", "text");
	}

	public static bool TryParseChannel(string text, out ulong channelId)
	{
		if (text.Length >= 3 && text[0] == '<' && text[1] == '#' && text[text.Length - 1] == '>')
		{
			text = text.Substring(2, text.Length - 3);
			if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out channelId))
			{
				return true;
			}
		}
		channelId = 0uL;
		return false;
	}

	public static ulong ParseRole(string text)
	{
		if (TryParseRole(text, out var roleId))
		{
			return roleId;
		}
		throw new ArgumentException("Invalid mention format.", "text");
	}

	public static bool TryParseRole(string text, out ulong roleId)
	{
		if (text.Length >= 4 && text[0] == '<' && text[1] == '@' && text[2] == '&' && text[text.Length - 1] == '>')
		{
			text = text.Substring(3, text.Length - 4);
			if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out roleId))
			{
				return true;
			}
		}
		roleId = 0uL;
		return false;
	}

	internal static string Resolve(IMessage msg, int startIndex, TagHandling userHandling, TagHandling channelHandling, TagHandling roleHandling, TagHandling everyoneHandling, TagHandling emojiHandling)
	{
		StringBuilder stringBuilder = new StringBuilder(msg.Content.Substring(startIndex));
		IReadOnlyCollection<ITag> tags = msg.Tags;
		int num = -startIndex;
		foreach (ITag item in tags)
		{
			if (item.Index < startIndex)
			{
				continue;
			}
			string text = "";
			switch (item.Type)
			{
			case TagType.UserMention:
				if (userHandling == TagHandling.Ignore)
				{
					continue;
				}
				text = ResolveUserMention(item, userHandling);
				break;
			case TagType.ChannelMention:
				if (channelHandling == TagHandling.Ignore)
				{
					continue;
				}
				text = ResolveChannelMention(item, channelHandling);
				break;
			case TagType.RoleMention:
				if (roleHandling == TagHandling.Ignore)
				{
					continue;
				}
				text = ResolveRoleMention(item, roleHandling);
				break;
			case TagType.EveryoneMention:
				if (everyoneHandling == TagHandling.Ignore)
				{
					continue;
				}
				text = ResolveEveryoneMention(item, everyoneHandling);
				break;
			case TagType.HereMention:
				if (everyoneHandling == TagHandling.Ignore)
				{
					continue;
				}
				text = ResolveHereMention(item, everyoneHandling);
				break;
			case TagType.Emoji:
				if (emojiHandling == TagHandling.Ignore)
				{
					continue;
				}
				text = ResolveEmoji(item, emojiHandling);
				break;
			}
			stringBuilder.Remove(item.Index + num, item.Length);
			stringBuilder.Insert(item.Index + num, text);
			num += text.Length - item.Length;
		}
		return stringBuilder.ToString();
	}

	internal static string ResolveUserMention(ITag tag, TagHandling mode)
	{
		if (mode != TagHandling.Remove)
		{
			IUser user = tag.Value as IUser;
			IGuildUser guildUser = user as IGuildUser;
			switch (mode)
			{
			case TagHandling.Name:
				if (user != null)
				{
					return "@" + (guildUser?.Nickname ?? user?.Username);
				}
				return "";
			case TagHandling.NameNoPrefix:
				if (user != null)
				{
					object obj = guildUser?.Nickname ?? user?.Username;
					if (obj == null)
					{
						obj = "";
					}
					return (string)obj;
				}
				return "";
			case TagHandling.FullName:
				if (user != null)
				{
					return "@" + user.Username + "#" + user.Discriminator;
				}
				return "";
			case TagHandling.FullNameNoPrefix:
				if (user != null)
				{
					return user.Username + "#" + user.Discriminator;
				}
				return "";
			case TagHandling.Sanitize:
				if (guildUser != null && guildUser.Nickname == null)
				{
					return MentionUser($"{'\u200b'}{tag.Key}", useNickname: false);
				}
				return MentionUser($"{'\u200b'}{tag.Key}");
			}
		}
		return "";
	}

	internal static string ResolveChannelMention(ITag tag, TagHandling mode)
	{
		if (mode != TagHandling.Remove)
		{
			IChannel channel = tag.Value as IChannel;
			switch (mode)
			{
			case TagHandling.Name:
			case TagHandling.FullName:
				if (channel != null)
				{
					return "#" + channel.Name;
				}
				return "";
			case TagHandling.NameNoPrefix:
			case TagHandling.FullNameNoPrefix:
				if (channel != null)
				{
					return channel.Name ?? "";
				}
				return "";
			case TagHandling.Sanitize:
				return MentionChannel($"{'\u200b'}{tag.Key}");
			}
		}
		return "";
	}

	internal static string ResolveRoleMention(ITag tag, TagHandling mode)
	{
		if (mode != TagHandling.Remove)
		{
			IRole role = tag.Value as IRole;
			switch (mode)
			{
			case TagHandling.Name:
			case TagHandling.FullName:
				if (role != null)
				{
					return "@" + role.Name;
				}
				return "";
			case TagHandling.NameNoPrefix:
			case TagHandling.FullNameNoPrefix:
				if (role != null)
				{
					return role.Name ?? "";
				}
				return "";
			case TagHandling.Sanitize:
				return MentionRole($"{'\u200b'}{tag.Key}");
			}
		}
		return "";
	}

	internal static string ResolveEveryoneMention(ITag tag, TagHandling mode)
	{
		switch (mode)
		{
		case TagHandling.Name:
		case TagHandling.NameNoPrefix:
		case TagHandling.FullName:
		case TagHandling.FullNameNoPrefix:
			return "everyone";
		case TagHandling.Sanitize:
			return $"@{'\u200b'}everyone";
		default:
			return "";
		}
	}

	internal static string ResolveHereMention(ITag tag, TagHandling mode)
	{
		switch (mode)
		{
		case TagHandling.Name:
		case TagHandling.NameNoPrefix:
		case TagHandling.FullName:
		case TagHandling.FullNameNoPrefix:
			return "here";
		case TagHandling.Sanitize:
			return $"@{'\u200b'}here";
		default:
			return "";
		}
	}

	internal static string ResolveEmoji(ITag tag, TagHandling mode)
	{
		if (mode != TagHandling.Remove)
		{
			Emote emote = (Emote)tag.Value;
			for (int i = 0; i < emote.Name.Length; i++)
			{
				char c = emote.Name[i];
				if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
				{
					return "";
				}
			}
			switch (mode)
			{
			case TagHandling.Name:
			case TagHandling.FullName:
				return ":" + emote.Name + ":";
			case TagHandling.NameNoPrefix:
			case TagHandling.FullNameNoPrefix:
				return emote.Name ?? "";
			case TagHandling.Sanitize:
				return $"<{emote.Id}{'\u200b'}:{'\u200b'}{emote.Name}>";
			}
		}
		return "";
	}
}
