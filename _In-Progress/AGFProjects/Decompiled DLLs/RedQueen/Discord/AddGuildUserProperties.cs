using System.Collections.Generic;

namespace Discord;

internal class AddGuildUserProperties
{
	public Optional<string> Nickname { get; set; }

	public Optional<bool> Mute { get; set; }

	public Optional<bool> Deaf { get; set; }

	public Optional<IEnumerable<IRole>> Roles { get; set; }

	public Optional<IEnumerable<ulong>> RoleIds { get; set; }
}
