using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Commands;

internal static class CommandParser
{
	private enum ParserPart
	{
		None,
		Parameter,
		QuotedParameter
	}

	public static async Task<ParseResult> ParseArgsAsync(CommandInfo command, ICommandContext context, bool ignoreExtraArgs, IServiceProvider services, string input, int startPos, IReadOnlyDictionary<char, char> aliasMap)
	{
		ParameterInfo curParam = null;
		StringBuilder argBuilder = new StringBuilder(input.Length);
		int endPos = input.Length;
		ParserPart curPart = ParserPart.None;
		int lastArgEndPos = int.MinValue;
		System.Collections.Immutable.ImmutableArray<TypeReaderResult>.Builder argList = System.Collections.Immutable.ImmutableArray.CreateBuilder<TypeReaderResult>();
		System.Collections.Immutable.ImmutableArray<TypeReaderResult>.Builder paramList = System.Collections.Immutable.ImmutableArray.CreateBuilder<TypeReaderResult>();
		bool isEscaping = false;
		char matchQuote = '\0';
		for (int curPos = startPos; curPos <= endPos; curPos++)
		{
			char c;
			if (curPos < endPos)
			{
				c = input[curPos];
			}
			else
			{
				c = '\0';
			}
			if (curParam != null && curParam.IsRemainder && curPos != endPos)
			{
				argBuilder.Append(c);
				continue;
			}
			if (isEscaping && curPos != endPos)
			{
				if (c != matchQuote)
				{
					argBuilder.Append('\\');
				}
				argBuilder.Append(c);
				isEscaping = false;
				continue;
			}
			if (c == '\\' && (curParam == null || !curParam.IsRemainder))
			{
				isEscaping = true;
				continue;
			}
			if (curPart == ParserPart.None)
			{
				if (char.IsWhiteSpace(c) || curPos == endPos)
				{
					continue;
				}
				if (curPos == lastArgEndPos)
				{
					return ParseResult.FromError(CommandError.ParseFailed, "There must be at least one character of whitespace between arguments.");
				}
				if (curParam == null)
				{
					curParam = ((command.Parameters.Count > argList.Count) ? command.Parameters[argList.Count] : null);
				}
				if (curParam != null && curParam.IsRemainder)
				{
					argBuilder.Append(c);
					continue;
				}
				if (IsOpenQuote(aliasMap, c))
				{
					curPart = ParserPart.QuotedParameter;
					matchQuote = GetMatch(aliasMap, c);
					continue;
				}
				curPart = ParserPart.Parameter;
			}
			string text = null;
			switch (curPart)
			{
			case ParserPart.Parameter:
				if (curPos == endPos || char.IsWhiteSpace(c))
				{
					text = argBuilder.ToString();
					lastArgEndPos = curPos;
				}
				else
				{
					argBuilder.Append(c);
				}
				break;
			case ParserPart.QuotedParameter:
				if (c == matchQuote)
				{
					text = argBuilder.ToString();
					lastArgEndPos = curPos + 1;
				}
				else
				{
					argBuilder.Append(c);
				}
				break;
			}
			if (text == null)
			{
				continue;
			}
			if (curParam == null)
			{
				if (command.IgnoreExtraArgs)
				{
					break;
				}
				return ParseResult.FromError(CommandError.BadArgCount, "The input text has too many parameters.");
			}
			TypeReaderResult typeReaderResult = await curParam.ParseAsync(context, text, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!typeReaderResult.IsSuccess && typeReaderResult.Error != CommandError.MultipleMatches)
			{
				return ParseResult.FromError(typeReaderResult, curParam);
			}
			if (curParam.IsMultiple)
			{
				paramList.Add(typeReaderResult);
				curPart = ParserPart.None;
			}
			else
			{
				argList.Add(typeReaderResult);
				curParam = null;
				curPart = ParserPart.None;
			}
			argBuilder.Clear();
			char GetMatch(IReadOnlyDictionary<char, char> dict, char ch)
			{
				if (dict.Count != 0 && dict.TryGetValue(c, out var value))
				{
					return value;
				}
				return '"';
			}
			bool IsOpenQuote(IReadOnlyDictionary<char, char> dict, char ch)
			{
				if (dict.Count != 0)
				{
					return dict.ContainsKey(ch);
				}
				return c == '"';
			}
		}
		if (curParam != null && curParam.IsRemainder)
		{
			TypeReaderResult typeReaderResult2 = await curParam.ParseAsync(context, argBuilder.ToString(), services).ConfigureAwait(continueOnCapturedContext: false);
			if (!typeReaderResult2.IsSuccess)
			{
				return ParseResult.FromError(typeReaderResult2, curParam);
			}
			argList.Add(typeReaderResult2);
		}
		if (isEscaping)
		{
			return ParseResult.FromError(CommandError.ParseFailed, "Input text may not end on an incomplete escape.");
		}
		if (curPart == ParserPart.QuotedParameter)
		{
			return ParseResult.FromError(CommandError.ParseFailed, "A quoted parameter is incomplete.");
		}
		for (int i = argList.Count; i < command.Parameters.Count; i++)
		{
			ParameterInfo parameterInfo = command.Parameters[i];
			if (!parameterInfo.IsMultiple)
			{
				if (!parameterInfo.IsOptional)
				{
					return ParseResult.FromError(CommandError.BadArgCount, "The input text has too few parameters.");
				}
				argList.Add(TypeReaderResult.FromSuccess(parameterInfo.DefaultValue));
			}
		}
		return ParseResult.FromSuccess(argList.ToImmutable(), paramList.ToImmutable());
	}
}
