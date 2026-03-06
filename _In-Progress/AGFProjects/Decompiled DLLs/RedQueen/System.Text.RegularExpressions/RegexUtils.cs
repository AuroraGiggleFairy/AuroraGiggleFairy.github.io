using System.Linq;

namespace System.Text.RegularExpressions;

internal static class RegexUtils
{
	internal const byte Q = 5;

	internal const byte S = 4;

	internal const byte Z = 3;

	internal const byte X = 2;

	internal const byte E = 1;

	internal static readonly byte[] _category = new byte[128]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 2,
		2, 0, 2, 2, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 2, 0, 0, 3, 4, 0, 0, 0,
		4, 4, 5, 5, 0, 0, 4, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 5, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 4, 4, 0, 4, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 5, 4, 0, 0, 0
	};

	internal static string EscapeExcluding(string input, params char[] exclude)
	{
		if (exclude == null)
		{
			throw new ArgumentNullException("exclude");
		}
		for (int i = 0; i < input.Length; i++)
		{
			if (!IsMetachar(input[i]) || exclude.Contains(input[i]))
			{
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder();
			char c = input[i];
			stringBuilder.Append(input, 0, i);
			do
			{
				stringBuilder.Append('\\');
				switch (c)
				{
				case '\n':
					c = 'n';
					break;
				case '\r':
					c = 'r';
					break;
				case '\t':
					c = 't';
					break;
				case '\f':
					c = 'f';
					break;
				}
				stringBuilder.Append(c);
				i++;
				int num = i;
				for (; i < input.Length; i++)
				{
					c = input[i];
					if (IsMetachar(c) && !exclude.Contains(input[i]))
					{
						break;
					}
				}
				stringBuilder.Append(input, num, i - num);
			}
			while (i < input.Length);
			return stringBuilder.ToString();
		}
		return input;
	}

	internal static bool IsMetachar(char ch)
	{
		if (ch <= '|')
		{
			return _category[(uint)ch] >= 1;
		}
		return false;
	}
}
