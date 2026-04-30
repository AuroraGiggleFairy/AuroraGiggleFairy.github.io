using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Discord.Commands;

internal class TimeSpanTypeReader : TypeReader
{
	private static readonly string[] Formats = new string[15]
	{
		"%d'd'%h'h'%m'm'%s's'", "%d'd'%h'h'%m'm'", "%d'd'%h'h'%s's'", "%d'd'%h'h'", "%d'd'%m'm'%s's'", "%d'd'%m'm'", "%d'd'%s's'", "%d'd'", "%h'h'%m'm'%s's'", "%h'h'%m'm'",
		"%h'h'%s's'", "%h'h'", "%m'm'%s's'", "%m'm'", "%s's'"
	};

	public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		if (string.IsNullOrEmpty(input))
		{
			throw new ArgumentException("input must not be null or empty.", "input");
		}
		bool flag = input[0] == '-';
		if (flag)
		{
			input = input.Substring(1);
		}
		if (TimeSpan.TryParseExact(input.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, out var result))
		{
			if (!flag)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(result));
			}
			return Task.FromResult(TypeReaderResult.FromSuccess(-result));
		}
		return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse TimeSpan"));
	}
}
