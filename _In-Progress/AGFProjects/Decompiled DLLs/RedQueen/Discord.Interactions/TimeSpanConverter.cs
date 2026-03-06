using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class TimeSpanConverter : TypeConverter<TimeSpan>
{
	private static readonly string[] Formats = new string[15]
	{
		"%d'd'%h'h'%m'm'%s's'", "%d'd'%h'h'%m'm'", "%d'd'%h'h'%s's'", "%d'd'%h'h'", "%d'd'%m'm'%s's'", "%d'd'%m'm'", "%d'd'%s's'", "%d'd'", "%h'h'%m'm'%s's'", "%h'h'%m'm'",
		"%h'h'%s's'", "%h'h'", "%m'm'%s's'", "%m'm'", "%s's'"
	};

	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.String;
	}

	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
	{
		if (!TimeSpan.TryParseExact((option.Value as string).ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, out var result))
		{
			return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Failed to parse TimeSpan"));
		}
		return Task.FromResult(TypeConverterResult.FromSuccess(result));
	}
}
