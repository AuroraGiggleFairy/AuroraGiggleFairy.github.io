using System.Runtime.CompilerServices;

namespace Discord;

internal struct DiscordError
{
	public string Code
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public string Message
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	internal DiscordError(string code, string message)
	{
		Code = code;
		Message = message;
	}
}
