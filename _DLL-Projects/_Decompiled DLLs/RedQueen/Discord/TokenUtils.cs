using System;
using System.Globalization;
using System.Text;

namespace Discord;

internal static class TokenUtils
{
	internal const int MinBotTokenLength = 58;

	internal const char Base64Padding = '=';

	internal static char[] IllegalTokenCharacters = new char[4] { ' ', '\t', '\r', '\n' };

	internal static string PadBase64String(string encodedBase64)
	{
		if (string.IsNullOrWhiteSpace(encodedBase64))
		{
			throw new ArgumentNullException(encodedBase64, "The supplied base64-encoded string was null or whitespace.");
		}
		if (encodedBase64.IndexOf('=') != -1)
		{
			return encodedBase64;
		}
		int num = (4 - encodedBase64.Length % 4) % 4;
		return num switch
		{
			3 => throw new FormatException("The provided base64 string is corrupt, as it requires an invalid amount of padding."), 
			0 => encodedBase64, 
			_ => encodedBase64.PadRight(encodedBase64.Length + num, '='), 
		};
	}

	internal static ulong? DecodeBase64UserId(string encoded)
	{
		if (string.IsNullOrWhiteSpace(encoded))
		{
			return null;
		}
		try
		{
			encoded = PadBase64String(encoded);
			byte[] bytes = Convert.FromBase64String(encoded);
			if (ulong.TryParse(Encoding.UTF8.GetString(bytes), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
			{
				return result;
			}
		}
		catch (DecoderFallbackException)
		{
		}
		catch (FormatException)
		{
		}
		catch (ArgumentException)
		{
		}
		return null;
	}

	internal static bool CheckBotTokenValidity(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return false;
		}
		string[] array = message.Split('.');
		if (array.Length != 3)
		{
			return false;
		}
		return DecodeBase64UserId(array[0]).HasValue;
	}

	internal static bool CheckContainsIllegalCharacters(string token)
	{
		return token.IndexOfAny(IllegalTokenCharacters) != -1;
	}

	public static void ValidateToken(TokenType tokenType, string token)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new ArgumentNullException("token", "A token cannot be null, empty, or contain only whitespace.");
		}
		if (CheckContainsIllegalCharacters(token))
		{
			throw new ArgumentException("The token contains a whitespace or newline character. Ensure that the token has been properly trimmed.", "token");
		}
		switch (tokenType)
		{
		case TokenType.Bot:
			if (token.Length < 58)
			{
				throw new ArgumentException($"A Bot token must be at least {58} characters in length. " + "Ensure that the Bot Token provided is not an OAuth client secret.", "token");
			}
			if (!CheckBotTokenValidity(token))
			{
				throw new ArgumentException("The Bot token was invalid. Ensure that the Bot Token provided is not an OAuth client secret.", "token");
			}
			break;
		default:
			throw new ArgumentException("Unrecognized TokenType.", "token");
		case TokenType.Bearer:
		case TokenType.Webhook:
			break;
		}
	}
}
