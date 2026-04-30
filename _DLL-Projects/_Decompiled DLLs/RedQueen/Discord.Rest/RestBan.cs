using System.Diagnostics;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestBan : IBan
{
	public RestUser User { get; }

	public string Reason { get; }

	private string DebuggerDisplay => $"{User}: {Reason}";

	IUser IBan.User => User;

	internal RestBan(RestUser user, string reason)
	{
		User = user;
		Reason = reason;
	}

	internal static RestBan Create(BaseDiscordClient client, Ban model)
	{
		return new RestBan(RestUser.Create(client, model.User), model.Reason);
	}

	public override string ToString()
	{
		return User.ToString();
	}
}
