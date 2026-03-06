using System.Collections.Generic;

namespace Discord;

internal class GuildChannelProperties
{
	public Optional<string> Name { get; set; }

	public Optional<int> Position { get; set; }

	public Optional<ulong?> CategoryId { get; set; }

	public Optional<IEnumerable<Overwrite>> PermissionOverwrites { get; set; }
}
