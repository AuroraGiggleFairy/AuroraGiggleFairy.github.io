using System.Text;
using System.Text.RegularExpressions;

namespace Discord;

internal static class Format
{
	private static readonly string[] SensitiveCharacters = new string[10] { "\\", "*", "_", "~", "`", ".", ":", "/", ">", "|" };

	public static string Bold(string text)
	{
		return "**" + text + "**";
	}

	public static string Italics(string text)
	{
		return "*" + text + "*";
	}

	public static string Underline(string text)
	{
		return "__" + text + "__";
	}

	public static string Strikethrough(string text)
	{
		return "~~" + text + "~~";
	}

	public static string Spoiler(string text)
	{
		return "||" + text + "||";
	}

	public static string Url(string text, string url)
	{
		return "[" + text + "](" + url + ")";
	}

	public static string EscapeUrl(string url)
	{
		return "<" + url + ">";
	}

	public static string Code(string text, string language = null)
	{
		if (language != null || text.Contains("\n"))
		{
			return "```" + language + "\n" + text + "\n```";
		}
		return "`" + text + "`";
	}

	public static string Sanitize(string text)
	{
		if (text != null)
		{
			string[] sensitiveCharacters = SensitiveCharacters;
			foreach (string text2 in sensitiveCharacters)
			{
				text = text.Replace(text2, "\\" + text2);
			}
		}
		return text;
	}

	public static string Quote(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2;
		do
		{
			num2 = text.IndexOf('\n', num);
			if (num2 == -1)
			{
				string text2 = text.Substring(num);
				stringBuilder.Append("> " + text2);
			}
			else
			{
				string text3 = text.Substring(num, num2 - num);
				stringBuilder.Append("> " + text3 + "\n");
			}
			num = num2 + 1;
		}
		while (num2 != -1 && num != text.Length);
		return stringBuilder.ToString();
	}

	public static string BlockQuote(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return ">>> " + text;
	}

	public static string StripMarkDown(string text)
	{
		return Regex.Replace(text, "(\\*|_|`|~|>|\\\\)", "");
	}

	public static string UsernameAndDiscriminator(IUser user, bool doBidirectional)
	{
		if (!doBidirectional)
		{
			return user.Username + "#" + user.Discriminator;
		}
		return "\u2066" + user.Username + "\u2069#" + user.Discriminator;
	}
}
