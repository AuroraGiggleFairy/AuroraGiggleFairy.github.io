using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

internal class ApplicationInstallParams
{
	public IReadOnlyCollection<string> Scopes { get; }

	public GuildPermission? Permission { get; }

	internal ApplicationInstallParams(string[] scopes, GuildPermission? permission)
	{
		Scopes = scopes.ToImmutableArray();
		Permission = permission;
	}
}
