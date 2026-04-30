using System;

namespace Discord.Commands;

internal static class MessageExtensions
{
	public static bool HasCharPrefix(this IUserMessage msg, char c, ref int argPos)
	{
		string content = msg.Content;
		if (!string.IsNullOrEmpty(content) && content[0] == c)
		{
			argPos = 1;
			return true;
		}
		return false;
	}

	public static bool HasStringPrefix(this IUserMessage msg, string str, ref int argPos, StringComparison comparisonType = StringComparison.Ordinal)
	{
		string content = msg.Content;
		if (!string.IsNullOrEmpty(content) && content.StartsWith(str, comparisonType))
		{
			argPos = str.Length;
			return true;
		}
		return false;
	}

	public static bool HasMentionPrefix(this IUserMessage msg, IUser user, ref int argPos)
	{
		string content = msg.Content;
		if (string.IsNullOrEmpty(content) || content.Length <= 3 || content[0] != '<' || content[1] != '@')
		{
			return false;
		}
		int num = content.IndexOf('>');
		if (num == -1)
		{
			return false;
		}
		if (content.Length < num + 2 || content[num + 1] != ' ')
		{
			return false;
		}
		if (!MentionUtils.TryParseUser(content.Substring(0, num + 1), out var userId))
		{
			return false;
		}
		if (userId == user.Id)
		{
			argPos = num + 2;
			return true;
		}
		return false;
	}
}
