using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;

namespace Discord.Net;

internal class HttpException : Exception
{
	public HttpStatusCode HttpCode { get; }

	public DiscordErrorCode? DiscordCode { get; }

	public string Reason { get; }

	public IRequest Request { get; }

	public IReadOnlyCollection<DiscordJsonError> Errors { get; }

	public HttpException(HttpStatusCode httpCode, IRequest request, DiscordErrorCode? discordCode = null, string reason = null, DiscordJsonError[] errors = null)
		: base(CreateMessage(httpCode, (int?)discordCode, reason, errors))
	{
		HttpCode = httpCode;
		Request = request;
		DiscordCode = discordCode;
		Reason = reason;
		Errors = errors?.ToImmutableArray() ?? System.Collections.Immutable.ImmutableArray<DiscordJsonError>.Empty;
	}

	private static string CreateMessage(HttpStatusCode httpCode, int? discordCode = null, string reason = null, DiscordJsonError[] errors = null)
	{
		string text = ((discordCode.HasValue && discordCode != 0) ? ((reason == null) ? $"The server responded with error {discordCode.Value}: {httpCode}" : $"The server responded with error {discordCode.Value}: {reason}") : ((reason == null) ? $"The server responded with error {(int)httpCode}: {httpCode}" : $"The server responded with error {(int)httpCode}: {reason}"));
		if (errors != null && errors.Length != 0)
		{
			text += "\nInner Errors:";
			for (int i = 0; i < errors.Length; i++)
			{
				DiscordJsonError discordJsonError = errors[i];
				IReadOnlyCollection<DiscordError> errors2 = discordJsonError.Errors;
				if (errors2 == null || errors2.Count <= 0)
				{
					continue;
				}
				foreach (DiscordError error in discordJsonError.Errors)
				{
					text = text + "\n" + error.Code + ": " + error.Message;
				}
			}
		}
		return text;
	}
}
